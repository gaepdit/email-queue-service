using EmailQueue.API.Validation;
using System.Text.Json.Serialization;

namespace EmailQueue.API.Models;

public record NewEmailTask
{
    [JsonRequired]
    [MinLength(1)]
    [NoEmptyStrings]
    public List<string> Recipients { get; init; } = [];

    [NoEmptyStrings]
    public List<string>? CopyRecipients { get; init; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string From { get; init; } = null!;

    [Required(AllowEmptyStrings = false)]
    [StringLength(200)]
    public string Subject { get; init; } = null!;

    [Required(AllowEmptyStrings = false)]
    [StringLength(20000)]
    public string Body { get; init; } = null!;

    [JsonRequired]
    public bool IsHtml { get; init; }
}
