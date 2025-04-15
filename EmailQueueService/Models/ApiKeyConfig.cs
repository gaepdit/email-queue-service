using System.Text.Json.Serialization;

namespace EmailQueueService.Models;

public class ApiKeyConfig
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
    
    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;
    
    [JsonPropertyName("generatedAt")]
    public DateTimeOffset GeneratedAt { get; set; }
}