namespace EmailQueue.API.Models;

public class EmailTask
{
    // Constructors
    [UsedImplicitly]
    private EmailTask() { } // Used by ORM.

    internal EmailTask(Guid id) => Id = id;

    // Properties
    public Guid Id { get; }

    public Guid BatchId { get; init; }
    public int Counter { get; init; }

    [StringLength(50)]
    public string? ApiKeyOwner { get; init; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(15)]
    public string Status { get; private set; } = "Queued";

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? AttemptedAt { [UsedImplicitly] get; private set; }

    // User-supplied properties

    [Required(AllowEmptyStrings = false)]
    [MaxLength(7000)]
    public List<string> Recipients { get; init; } = [];

    [Required(AllowEmptyStrings = false)]
    [StringLength(200)]
    public string Subject { get; init; } = null!;

    [Required(AllowEmptyStrings = false)]
    [StringLength(20000)]
    public string Body { get; init; } = null!;

    public bool IsHtml { get; init; }

    public void MarkAsSent()
    {
        Status = "Sent";
        AttemptedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = "Failed";
        AttemptedAt = DateTime.UtcNow;
    }

    public static EmailTask Create(NewEmailTask resource, Guid batchId, string apiKeyOwner, int counter) =>
        new(Guid.NewGuid())
        {
            BatchId = batchId,
            Counter = counter,
            ApiKeyOwner = apiKeyOwner,
            Recipients = resource.Recipients,
            Subject = resource.Subject,
            Body = resource.Body,
            IsHtml = resource.IsHtml,
        };
}
