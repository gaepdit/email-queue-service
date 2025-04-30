using System.Security.Claims;

namespace EmailQueue.API.AuthHandlers;

internal static class PrincipalExtensions
{
    public static string? ApiKeyOwner(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Name);
}
