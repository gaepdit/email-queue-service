using System.Collections.Concurrent;
using EmailQueueService.Models;

namespace EmailQueueService.Services;

public interface IQueueService
{
    void EnqueueItems(IEnumerable<EmailTask> items);
    Task<EmailTask?> DequeueAsync(CancellationToken cancellationToken);
}

public class QueueService : IQueueService
{
    private readonly ConcurrentQueue<EmailTask> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private int _currentCounter;

    public void EnqueueItems(IEnumerable<EmailTask> items)
    {
        foreach (var item in items)
        {
            // Initialize all task properties
            item.Id = Guid.NewGuid();
            item.Counter = Interlocked.Increment(ref _currentCounter);
            item.Status = "Queued";
            item.CreatedAt = DateTimeOffset.UtcNow;
            
            _queue.Enqueue(item);
            _signal.Release();
        }
    }

    public async Task<EmailTask?> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _queue.TryDequeue(out var item);
        return item;
    }
}