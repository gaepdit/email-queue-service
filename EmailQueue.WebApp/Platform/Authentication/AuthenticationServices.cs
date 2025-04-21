using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

namespace EmailQueue.WebApp.Platform.Authentication;

public static class AuthenticationServices
{
    public static void AddAuthenticationServices(this WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment() || AppSettings.UseEntraId)
        {
            // Requires a Microsoft Entra ID account when running in production or when set in the app settings.
            // (Entra ID account settings are configured in the app settings file.)
            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration);
        }
        else
        {
            // Optionally use a dev auth handler.
            builder.Services.AddAuthentication(DevAuthenticationHandler.BasicAuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, DevAuthenticationHandler>(
                    DevAuthenticationHandler.BasicAuthenticationScheme, null);
        }

        builder.Services.AddAuthorization();
    }
}
