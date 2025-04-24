using GaEpd.EmailService;

namespace EmailQueue.API.Settings;

public static class AppSettings
{
    public static QueueSettings QueueSettings { get; } = new();
    public static List<ApiKeyConfig> ApiKeys { get; } = [];
    public static EmailServiceSettings EmailServiceSettings { get; } = new();
}

public static class AppSettingsExtensions
{
    public static void BindAppSettings(this WebApplicationBuilder builder)
    {
        // Bind app settings.
        builder.Configuration.GetSection(nameof(AppSettings.QueueSettings)).Bind(AppSettings.QueueSettings);
        builder.Configuration.GetSection(nameof(AppSettings.ApiKeys)).Bind(AppSettings.ApiKeys);
        builder.Configuration.GetSection(nameof(AppSettings.EmailServiceSettings))
            .Bind(AppSettings.EmailServiceSettings);
    }
}

public record QueueSettings
{
    public int ProcessingDelaySeconds { get; init; } = 5; // Default value if not specified in config
}

public class ApiKeyConfig
{
    public string Key { get; init; } = null!;
    public string Owner { get; init; } = null!;
    public string[] Permissions { get; init; } = []; // Default to no access.
    public DateTimeOffset GeneratedAt { get; init; }
}
