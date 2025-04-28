namespace EmailQueue.API.Models;

public record EmailTask : NewEmailTask
{
    // Constructors
    [UsedImplicitly]
    private EmailTask() { } // Used by ORM.

    private EmailTask(Guid id) => Id = id;

    // Properties
    public Guid Id { get; }

    [StringLength(10)]
    public string BatchId { get; private init; } = null!;

    public int Counter { get; private init; }

    [StringLength(50)]
    public string? ApiKeyOwner { get; private init; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(15)]
    public string Status { get; private set; } = "Queued";

    public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;
    public DateTime? AttemptedAt { [UsedImplicitly] get; private set; }

    // Methods
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

    public static EmailTask Create(NewEmailTask resource, string batchId, string apiKeyOwner, int counter) =>
        new(id: Guid.NewGuid())
        {
            BatchId = batchId,
            Counter = counter,
            ApiKeyOwner = apiKeyOwner,
            Recipients = resource.Recipients,
            From = resource.From,
            Subject = resource.Subject,
            Body = resource.Body,
            IsHtml = resource.IsHtml,
        };
}
