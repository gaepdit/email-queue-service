using System.Text.Json.Serialization;

namespace EmailQueue.API.Models;

public record NewEmailTask
{
    [Required(AllowEmptyStrings = false)]
    [MaxLength(7000)]
    public List<string> Recipients { get; init; } = [];

    [Required(AllowEmptyStrings = true)]
    [StringLength(150)]
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
