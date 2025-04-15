namespace EmailQueueService.Models;

public class EmailTask
{
    public Guid Id { get; set; }
    public int Counter { get; set; }
    public string Status { get; set; } = "Queued";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SentAt { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public Task SendEmailAsync(EmailTask task)
    {
        // Simulate sending an email
        Console.WriteLine($"Email {task.Counter} sent at {DateTimeOffset.UtcNow}");
        return Task.CompletedTask;
    }
}
