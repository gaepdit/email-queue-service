using System.ComponentModel.DataAnnotations;

namespace EmailQueueService.Models;

public class EmailTask
{
    public Guid Id { get; set; }
    public int Counter { get; set; }

    [StringLength(15)]
    [Required(AllowEmptyStrings = false)]
    public string Status { get; set; } = "Queued";

    [StringLength(25)]
    public string? ApiKeyOwner { get; set; } 
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SentAt { get; set; }

    [StringLength(150)]
    [Required(AllowEmptyStrings = false)]
    public string EmailAddress { get; init; } = string.Empty;

    [StringLength(7000)]
    [Required(AllowEmptyStrings = false)]
    public string Body { get; init; } = string.Empty;

    public static Task SendEmailAsync(EmailTask task)
    {
        // TODO: Replace with code to send an email.
        Console.WriteLine($"Email {task.Counter} sent at {DateTimeOffset.UtcNow}");
        return Task.CompletedTask;
    }
}
