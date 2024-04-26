using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
using OpenDoors.Model.Authentication;
using OpenDoors.Service.Authorization;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Services;

public class TenantManager(OpenDoorsContext dbContext, RoleManager<TenantRole> roleManager) : ITenantManager
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

    public async Task<AccessGroup> GetDefaultAccessGroup(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await dbContext.AccessGroups.Where(g => g.Tenant.Id == tenantId).Where(g => g.Name == "default").FirstAsync(cancellationToken);
    }

    public Task<AccessGroup?> GetAccessGroup(Guid accessGroupId, CancellationToken cancellationToken = default)
    {
        return dbContext.AccessGroups.Where(g => g.Id == accessGroupId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddUserToAccessGroup(TenantUser user, AccessGroup accessGroup, CancellationToken cancellationToken = default)
    {
        accessGroup.Members.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

