using System.Collections.Concurrent;
using EmailQueueService.Data;
using EmailQueueService.Models;
using Microsoft.EntityFrameworkCore;

namespace EmailQueueService.Services;

public interface IQueueService
{
    Task EnqueueItems(IEnumerable<EmailTask> items);
    Task<EmailTask?> DequeueAsync(CancellationToken cancellationToken);
    Task InitializeQueueFromDatabase();
}

public class QueueService : IQueueService
{
    private readonly ConcurrentQueue<EmailTask> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QueueService> _logger;
    private int _currentCounter;

    public QueueService(IServiceScopeFactory scopeFactory, ILogger<QueueService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task InitializeQueueFromDatabase()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmailQueueDbContext>();
        
        var pendingTasks = await dbContext.EmailTasks
            .Where(t => t.Status == "Queued")
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        foreach (var task in pendingTasks)
        {
            _queue.Enqueue(task);
            _signal.Release();
            _currentCounter = Math.Max(_currentCounter, task.Counter);
        }

        _logger.LogInformation("Initialized queue with {Count} pending tasks from database", pendingTasks.Count);
    }

    public async Task EnqueueItems(IEnumerable<EmailTask> items)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmailQueueDbContext>();

        foreach (var item in items)
        {
            // Initialize all task properties
            item.Id = Guid.NewGuid();
            item.Counter = Interlocked.Increment(ref _currentCounter);
            
            await dbContext.EmailTasks.AddAsync(item);
        }

        await dbContext.SaveChangesAsync();

        // Only enqueue items in memory after they're saved to the database
        foreach (var item in items)
        {
            _queue.Enqueue(item);
            _signal.Release();
        }
        
        _logger.LogInformation("Enqueued {Count} new tasks", items.Count());
    }

    public async Task<EmailTask?> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _queue.TryDequeue(out var item);
        return item;
    }
}