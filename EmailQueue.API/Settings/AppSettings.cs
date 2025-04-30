using GaEpd.EmailService;

namespace EmailQueue.API.Settings;

public static class AppSettings
{
    public static QueueSettings QueueSettings { get; } = new();
    public static EmailServiceSettings EmailServiceSettings { get; } = new();
}

public static class AppSettingsExtensions
{
    public static void BindAppSettings(this WebApplicationBuilder builder)
    {
        // Bind app settings.
        builder.Services.AddOptions<List<ApiKey>>().BindConfiguration(configSectionPath: "ApiKeys");
        builder.Configuration.GetSection(nameof(AppSettings.QueueSettings)).Bind(AppSettings.QueueSettings);
        builder.Configuration.GetSection(nameof(AppSettings.EmailServiceSettings))
            .Bind(AppSettings.EmailServiceSettings);
    }
}

public record QueueSettings
{
    public int ProcessingDelaySeconds { get; [UsedImplicitly] init; } = 5; // Default value if not specified in config
}

public record ApiKey
{
    public string Key { get; init; } = null!;
    public string Owner { get; init; } = null!;
    public string[] Permissions { get; init; } = []; // Default to no access.
}
