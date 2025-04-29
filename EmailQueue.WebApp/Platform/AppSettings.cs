using System.Reflection;

namespace EmailQueue.WebApp.Platform;

public static class AppSettings
{
    public static RaygunClientSettings RaygunSettings { get; } = new();
    public static bool UseEntraId { get; private set; } = true;
    public static bool DevAuthFails { get; private set; }
    public static string? InformationalVersion { get; private set; }
    public static string? InformationalBuild { get; private set; }
    public const string DateTimeFormat = "d\u2011MMM\u2011yyyy h:mm:ss\u00a0tt";

    public static void BindSettings(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<EmailQueueApi>().BindConfiguration(configSectionPath: nameof(EmailQueueApi));
        builder.Configuration.GetSection(nameof(RaygunSettings)).Bind(RaygunSettings);
        UseEntraId = builder.Configuration.GetValue<bool>(nameof(UseEntraId));
        DevAuthFails = builder.Configuration.GetValue<bool>(nameof(DevAuthFails));

        // App version
        var segments = (Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "").Split('+');
        InformationalVersion = segments[0];
        if (segments.Length > 0 && segments[1].Length > 0)
            InformationalBuild = segments[1][..Math.Min(7, segments[1].Length)];
    }
}

public record EmailQueueApi
{
    public string BaseUrl { get; [UsedImplicitly] init; } = null!;
    public string ApiKey { get; [UsedImplicitly] init; } = null!;
}

public class RaygunClientSettings
{
    public string? ApiKey { get; [UsedImplicitly] init; }
}
