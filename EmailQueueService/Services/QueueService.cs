using EmailQueueService.Data;
using EmailQueueService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace EmailQueueService.Services;

public interface IQueueService
{
    Task<Guid?> EnqueueItems(IEnumerable<EmailTask> emailTasks, string apiKeyOwner);
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

    public async Task<Guid?> EnqueueItems(IEnumerable<EmailTask> emailTasks, string apiKeyOwner)
    {
        var emailTasksArray = emailTasks as EmailTask[] ?? emailTasks.ToArray();
        if (emailTasksArray.Length == 0) return null;

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmailQueueDbContext>();

        var batchId = Guid.NewGuid();

        foreach (var item in emailTasksArray)
        {
            // Initialize all task properties
            item.Id = Guid.NewGuid();
            item.BatchId = batchId;
            item.Counter = Interlocked.Increment(ref _currentCounter);
            item.ApiKeyOwner = apiKeyOwner;
        }

        await dbContext.EmailTasks.AddRangeAsync(emailTasksArray);
        await dbContext.SaveChangesAsync();

        // Enqueue items in memory after they're saved to the database
        foreach (var item in emailTasksArray)
        {
            _queue.Enqueue(item);
            _signal.Release();
        }

        logger.LogInformation("Enqueued {Count} new email tasks", emailTasksArray.Length);
        return batchId;
    }

    public async Task<EmailTask?> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _queue.TryDequeue(out var item);
        return item;
    }
}
