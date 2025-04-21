using EmailQueue.API.Data;
using EmailQueue.API.Models;
using Microsoft.Extensions.Options;

namespace EmailQueue.API.Services;

public class EmailProcessorService(
    IQueueService queueService,
    ILogger<EmailProcessorService> logger,
    IOptions<EmailQueueSettings> settings,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    private readonly int _processingDelaySeconds = settings.Value.ProcessingDelaySeconds;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // Initialize the queue with pending tasks from the database.
        await queueService.InitializeQueueFromDatabase();
        logger.LogInformation("DataProcessorService started and queue initialized.");
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var emailTask = await queueService.DequeueAsync(stoppingToken);
                if (emailTask == null) continue;

                await ProcessItemAsync(emailTask);

                logger.LogInformation("Waiting {Delay} seconds before processing next task", _processingDelaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(_processingDelaySeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email task: {Counter}", ex.Data["Counter"] ?? "Unknown");
            }
        }
    }

    private async Task ProcessItemAsync(EmailTask emailTask)
    {
        logger.LogInformation("Processing email task: {Counter}", emailTask.Counter);

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmailQueueDbContext>();

        // Get a fresh instance of the task that is tracked by this context.
        var dbTask = await dbContext.EmailTasks.FindAsync(emailTask.Id);
        if (dbTask == null)
        {
            logger.LogError("Task {Id} not found in database", emailTask.Id);
            return;
        }

        dbTask.MarkAsProcessing();
        await dbContext.SaveChangesAsync();

        try
        {
            await EmailTask.SendEmailAsync(emailTask);
            dbTask.MarkAsSent();
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Successfully sent email task: {Counter}", emailTask.Counter);
        }
        catch (Exception ex)
        {
            dbTask.MarkAsFailed();
            await dbContext.SaveChangesAsync();
            ex.Data.Add("Counter", emailTask.Counter);
            throw;
        }
    }
}
