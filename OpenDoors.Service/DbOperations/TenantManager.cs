using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
using OpenDoors.Service.Authorization;

namespace OpenDoors.Service.Services;

public class TenantManager(OpenDoorsContext dbContext, RoleManager<TenantRole> roleManager)
{
    public Task<Tenant?> GetByNameAsync(string tenantName, CancellationToken cancellationToken = default)
    {
        return dbContext.Tenants
            .Where(t => t.Name == tenantName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await dbContext.AddAsync(tenant, cancellationToken);
        await roleManager.CreateAsync(new TenantRole { Name = AuthorizationConstants.AdminRole, Tenant = tenant });

        await dbContext.AddAsync(new AccessGroup() { Tenant = tenant, Name = "default", Doors = [], Members = [] }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return tenant;
    }
}

