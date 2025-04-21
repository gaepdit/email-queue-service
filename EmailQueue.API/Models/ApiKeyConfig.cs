using System.Text.Json.Serialization;

namespace EmailQueue.API.Models;

[UsedImplicitly]
public class ApiKeyConfig
{
    [JsonPropertyName("key")]
    public string Key { get; [UsedImplicitly] init; } = string.Empty;

    [JsonPropertyName("owner")]
    public string Owner { get; [UsedImplicitly] init; } = string.Empty;

    [JsonPropertyName("generatedAt")]
    public DateTimeOffset GeneratedAt { get; [UsedImplicitly] init; }

    [JsonPropertyName("permissions")]
    public string[] Permissions { get; [UsedImplicitly] init; } = []; // Default to no access.
}
