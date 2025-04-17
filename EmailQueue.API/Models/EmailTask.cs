using System.ComponentModel.DataAnnotations;

namespace EmailQueue.API.Models;

public class EmailTask
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public int Counter { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(15)]
    public string Status { get; set; } = "Queued";

    [StringLength(50)]
    public string? ApiKeyOwner { get; set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SentAt { get; set; }

    // User-supplied data

    [Required(AllowEmptyStrings = false)]
    public List<string> Recipients { get; init; } = [];

    [Required(AllowEmptyStrings = false)]
    [StringLength(200)]
    public string Subject { get; init; } = null!;

    [Required(AllowEmptyStrings = false)]
    [StringLength(20000)]
    public string Body { get; init; } = null!;

    public bool IsHtml { get; init; }

    public static Task SendEmailAsync(EmailTask task)
    {
        // TODO: Replace with code to send an email.
        Console.WriteLine($"Email {task.Counter} sent at {DateTimeOffset.UtcNow}");
        return Task.CompletedTask;
    }
}
