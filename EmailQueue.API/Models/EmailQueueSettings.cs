namespace EmailQueue.API.Models;

public class EmailQueueSettings
{
    public int ProcessingDelaySeconds { get; init; } = 2; // Default value if not specified in config
}
