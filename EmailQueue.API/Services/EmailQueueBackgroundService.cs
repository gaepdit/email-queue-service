using EmailQueue.API.Models;
using Microsoft.Extensions.Options;

namespace EmailQueue.API.Services;

public class EmailQueueBackgroundService(
    IQueueService queueService,
    ILogger<EmailQueueBackgroundService> logger,
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

                using var scope = scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailProcessorService>();
                await emailService.ProcessEmailAsync(emailTask);

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
}
