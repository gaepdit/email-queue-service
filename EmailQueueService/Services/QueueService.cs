using EmailQueueService.Data;
using EmailQueueService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace EmailQueueService.Services;

public interface IQueueService
{
    Task EnqueueItems(IEnumerable<EmailTask> items, string apiKeyOwner);
    Task<EmailTask?> DequeueAsync(CancellationToken cancellationToken);
    Task InitializeQueueFromDatabase();
}

public class QueueService(IServiceScopeFactory scopeFactory, ILogger<QueueService> logger) : IQueueService
{
    private readonly ConcurrentQueue<EmailTask> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private int _currentCounter;

    public async Task InitializeQueueFromDatabase()
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmailQueueDbContext>();

        var pendingTasks = await dbContext.EmailTasks
            .Where(t => t.Status == "Queued")
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        foreach (var task in pendingTasks)
        {
            _queue.Enqueue(task);
            _signal.Release();
        }

        _currentCounter = await dbContext.EmailTasks.DefaultIfEmpty().MaxAsync(t => t == null ? 0 : t.Counter);
        logger.LogInformation("Initialized queue with {Count} pending tasks from database", pendingTasks.Count);
    }

    public async Task EnqueueItems(IEnumerable<EmailTask> items, string apiKeyOwner)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmailQueueDbContext>();

        var emailTasks = items as EmailTask[] ?? items.ToArray();

        foreach (var item in emailTasks)
        {
            // Initialize all task properties
            item.Id = Guid.NewGuid();
            item.Counter = Interlocked.Increment(ref _currentCounter);
            item.ApiKeyOwner = apiKeyOwner;
        }

        await dbContext.EmailTasks.AddRangeAsync(emailTasks);
        await dbContext.SaveChangesAsync();

        // Enqueue items in memory after they're saved to the database
        foreach (var item in emailTasks)
        {
            _queue.Enqueue(item);
            _signal.Release();
        }

        logger.LogInformation("Enqueued {Count} new tasks", emailTasks.Length);
    }

    public async Task<EmailTask?> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _queue.TryDequeue(out var item);
        return item;
    }
}
