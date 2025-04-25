using System.Text.Json.Serialization;

namespace EmailQueue.API.Models;

[UsedImplicitly]
public record NewEmailTask
{
    [Required(AllowEmptyStrings = false)]
    public List<string> Recipients { get; [UsedImplicitly] init; } = [];

    [Required(AllowEmptyStrings = false)]
    [StringLength(200)]
    public string Subject { get; [UsedImplicitly] init; } = null!;

    [Required(AllowEmptyStrings = false)]
    [StringLength(20000)]
    public string Body { get; [UsedImplicitly] init; } = null!;

    [JsonRequired]
    public bool IsHtml { get; [UsedImplicitly] init; }
}
