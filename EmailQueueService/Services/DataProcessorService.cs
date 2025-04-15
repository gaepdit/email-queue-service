using EmailQueueService.Models;
using EmailQueueService.Data;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace EmailQueueService.Services;

public class DataProcessorService : BackgroundService
{
    private readonly IQueueService _queueService;
    private readonly ILogger<DataProcessorService> _logger;
    private readonly EmailQueueSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;

    public DataProcessorService(
        IQueueService queueService,
        ILogger<DataProcessorService> logger,
        IOptions<EmailQueueSettings> settings,
        IServiceScopeFactory scopeFactory)
    {
        _queueService = queueService;
        _logger = logger;
        _settings = settings.Value;
        _scopeFactory = scopeFactory;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // Initialize queue with pending tasks from database
        await _queueService.InitializeQueueFromDatabase();
        _logger.LogInformation("DataProcessorService started and queue initialized.");
        await base.StartAsync(cancellationToken);
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
        
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmailQueueDbContext>();
        
        // Get a fresh instance of the task that is tracked by this context
        var dbTask = await dbContext.EmailTasks.FindAsync(task.Id);
        if (dbTask == null)
        {
            _logger.LogError("Task {Id} not found in database", task.Id);
            return;
        }

        dbTask.Status = "Processing";
        await dbContext.SaveChangesAsync();
        
        try
        {
            await task.SendEmailAsync(task);
            dbTask.Status = "Sent";
            dbTask.SentAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Successfully sent email task: {Counter}", task.Counter);
        }
        catch (Exception ex)
        {
            dbTask.Status = "Failed";
            await dbContext.SaveChangesAsync();
            _logger.LogError(ex, "Failed to send email task: {Counter}", task.Counter);
            throw;
        }
    }
}