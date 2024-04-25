using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
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
        await dbContext.Tenants.AddAsync(tenant, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return tenant;
    }
}

