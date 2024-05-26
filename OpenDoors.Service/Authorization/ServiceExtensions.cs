using Microsoft.EntityFrameworkCore;

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

    public static IServiceCollection AddDbContext(this IServiceCollection services)
    {
        return services.AddDbContextFactory<OpenDoorsContext>(BuildDb);
    }

    private static void BuildDb(DbContextOptionsBuilder options)
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string dbPath = Path.Join(localAppData, "opendoors.db");
        options.UseSqlite($"Data Source={dbPath}");
    }
}
