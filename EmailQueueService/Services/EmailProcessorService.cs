using EmailQueueService.Data;
using EmailQueueService.Models;
using Microsoft.Extensions.Options;

namespace EmailQueueService.Services;

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
        // Initialize queue with pending tasks from database
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
                var item = await queueService.DequeueAsync(stoppingToken);
                if (item == null) continue;

                await ProcessItemAsync(item);
                logger.LogInformation("Waiting {Delay} seconds before processing next task", _processingDelaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(_processingDelaySeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing queue item");
            }
        }
    }

    private async Task ProcessItemAsync(EmailTask task)
    {
        logger.LogInformation("Processing email task: {Counter}", task.Counter);

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmailQueueDbContext>();

        // Get a fresh instance of the task that is tracked by this context
        var dbTask = await dbContext.EmailTasks.FindAsync(task.Id);
        if (dbTask == null)
        {
            logger.LogError("Task {Id} not found in database", task.Id);
            return;
        }

        dbTask.Status = "Processing";
        await dbContext.SaveChangesAsync();

        try
        {
            await EmailTask.SendEmailAsync(task);
            dbTask.Status = "Sent";
            dbTask.SentAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Successfully sent email task: {Counter}", task.Counter);
        }
        catch (Exception ex)
        {
            dbTask.Status = "Failed";
            await dbContext.SaveChangesAsync();
            logger.LogError(ex, "Failed to send email task: {Counter}", task.Counter);
            throw;
        }
    }
}
