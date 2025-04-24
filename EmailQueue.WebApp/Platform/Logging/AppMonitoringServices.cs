using Mindscape.Raygun4Net;
using Mindscape.Raygun4Net.AspNetCore;
using Mindscape.Raygun4Net.Extensions.Logging;

namespace EmailQueue.WebApp.Platform.Logging;

public static class AppMonitoringServices
{
    public static void AddErrorLogging(this WebApplicationBuilder builder)
    {
        if (string.IsNullOrEmpty(AppSettings.RaygunSettings.ApiKey)) return;

        builder.Services
            .AddSingleton(provider =>
            {
                var client = new RaygunClient(provider.GetService<RaygunSettings>()!,
                    provider.GetService<IRaygunUserProvider>()!);
                client.SendingMessage += (_, eventArgs) =>
                    eventArgs.Message.Details.Tags.Add(builder.Environment.EnvironmentName);
                return client;
            })
            .AddRaygun(opts =>
            {
                opts.ApiKey = AppSettings.RaygunSettings.ApiKey;
                opts.ApplicationVersion = AppSettings.InformationalVersion;
                opts.EnvironmentVariables.Add("ASPNETCORE_*");
            })
            .AddRaygunUserProvider()
            .AddHttpContextAccessor(); // needed by RaygunScriptPartial

        builder.Logging.AddRaygunLogger(options =>
        {
            options.MinimumLogLevel = LogLevel.Warning;
            options.OnlyLogExceptions = false;
        });
    }
}
