using Microsoft.AspNetCore.Authorization;

namespace OpenDoors.Service.Authorization;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationConstants.HasTenantPolicy,
                    policy => policy.RequireClaim(AuthorizationConstants.TenantClaimType))
            .AddPolicy(AuthorizationConstants.TenantAdminPolicy,
                    policy => policy
                        .RequireRole(AuthorizationConstants.AdminRole)
                        .RequireClaim(AuthorizationConstants.TenantClaimType))
            .AddPolicy(AuthorizationConstants.AuditorPolicy,
                    policy => policy
                        .AddRequirements(new AuditorRequirement()));
        return services;
    }

    public static IServiceCollection AddAuthorizationHandlers(this IServiceCollection services)
    {
        return services;
    }
}
