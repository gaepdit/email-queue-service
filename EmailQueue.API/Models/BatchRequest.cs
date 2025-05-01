namespace EmailQueue.API.Models;

public record BatchRequest
{
    [MaxLength(10), MinLength(1)]
    public required string BatchId { get; [UsedImplicitly] init; }
}
