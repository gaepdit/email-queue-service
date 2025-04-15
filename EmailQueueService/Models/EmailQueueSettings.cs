namespace EmailQueueService.Models;

public class EmailQueueSettings
{
    public const string SectionName = "EmailQueueSettings";
    public int ProcessingDelaySeconds { get; set; } = 2; // Default value if not specified in config
}