using EmailQueueService.Models;
using Microsoft.Extensions.Options;

namespace EmailQueueService.Services;

public class DataProcessorService : BackgroundService
{
    private readonly IQueueService _queueService;
    private readonly ILogger<DataProcessorService> _logger;
    private readonly EmailQueueSettings _settings;

    public DataProcessorService(
        IQueueService queueService, 
        ILogger<DataProcessorService> logger,
        IOptions<EmailQueueSettings> settings)
    {
        _queueService = queueService;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var item = await _queueService.DequeueAsync(stoppingToken);
                if (item != null)
                {
                    await ProcessItemAsync(item);
                    _logger.LogInformation("Waiting {Delay} seconds before processing next task", _settings.ProcessingDelaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(_settings.ProcessingDelaySeconds), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing queue item");
            }
        }
    }

    private async Task ProcessItemAsync(EmailTask task)
    {
        _logger.LogInformation("Processing email task: {Counter} for {Email}", task.Counter, task.EmailAddress);
        task.Status = "Processing";
        
        try
        {
            await task.SendEmailAsync(task);
            task.Status = "Sent";
            task.SentAt = DateTimeOffset.UtcNow;
            _logger.LogInformation("Successfully sent email task: {Counter}", task.Counter);
        }
        catch (Exception ex)
        {
            task.Status = "Failed";
            _logger.LogError(ex, "Failed to send email task: {Counter}", task.Counter);
            throw;
        }
    }
}