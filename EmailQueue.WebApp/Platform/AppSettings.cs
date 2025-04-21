namespace EmailQueue.WebApp.Platform;

public static class AppSettings
{
    public static EmailQueueApi EmailQueueApi { get; set; } = new();
    public static bool UseEntraId { get; set; } = true;
    public static string DataProtectionKeysFolder { get; set; } = null!;
    public static bool DevAuthFails { get; set; }

    public static void BindSettings(this WebApplicationBuilder builder)
    {
        builder.Configuration.GetSection(nameof(EmailQueueApi)).Bind(EmailQueueApi);
        UseEntraId = builder.Configuration.GetValue<bool>(nameof(UseEntraId));
        DevAuthFails = builder.Configuration.GetValue<bool>(nameof(DevAuthFails));
        DataProtectionKeysFolder = builder.Configuration.GetValue<string>(nameof(DataProtectionKeysFolder)) ??
                                   $"../{nameof(DataProtectionKeysFolder)}";
    }
}

public record EmailQueueApi
{
    public string BaseUrl { get; init; } = null!;
    public string ApiKey { get; init; } = null!;
}
