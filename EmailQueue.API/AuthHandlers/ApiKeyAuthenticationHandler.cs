using EmailQueue.API.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EmailQueue.API.AuthHandlers;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    IOptionsSnapshot<List<ApiKey>> apiKeys,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    internal const string ApiKeyHeaderName = "X-API-Key";
    internal const string PermissionClaimType = "permission";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key is missing"));
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key is empty"));
        }

        var matchingKey = apiKeys.Value.FirstOrDefault(k => k.Key == providedApiKey);

        if (matchingKey == null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, matchingKey.Owner),
            new(ClaimTypes.NameIdentifier, matchingKey.Key),
        };

        claims.AddRange(matchingKey.Permissions.Select(permission => new Claim(PermissionClaimType, permission)));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
