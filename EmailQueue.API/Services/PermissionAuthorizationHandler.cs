using Microsoft.AspNetCore.Authorization;

namespace EmailQueue.API.Services;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;

    public static PermissionRequirement ReadPermission => new("read");
    public static PermissionRequirement WritePermission => new("write");
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.HasClaim(claim =>
                claim.Type == ApiKeyAuthenticationHandler.PermissionClaimType &&
                claim.Value == requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public static class AuthorizationPolicyExtensions
{
    public static void AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(nameof(PermissionRequirement.ReadPermission), policy =>
                policy.Requirements.Add(PermissionRequirement.ReadPermission))
            .AddPolicy(nameof(PermissionRequirement.WritePermission), policy =>
                policy.Requirements.Add(PermissionRequirement.WritePermission));

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
    }
}
