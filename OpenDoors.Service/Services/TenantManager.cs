using Microsoft.EntityFrameworkCore;
using OpenDoors.Model.Authentication;

namespace OpenDoors.Service.Controllers;

public class TenantManager(OpenDoorsContext dbContext)
{
    public Task<Tenant?> GetByNameAsync(string tenantName, CancellationToken cancellationToken = default)
    {
        return dbContext.Tenants
            .Where(t => t.Name == tenantName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        var existingTenant = await GetByNameAsync(tenant.Name!);
        if (existingTenant != null)
        {
            return tenant;
        }

        await dbContext.AddAsync(tenant, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return tenant;
    }
}

