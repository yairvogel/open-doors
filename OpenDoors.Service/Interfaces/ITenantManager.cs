using OpenDoors.Model.Authentication;

namespace OpenDoors.Service.Interfaces;

public interface ITenantManager
{
    Task<Tenant?> GetByNameAsync(string tenantName, CancellationToken cancellationToken = default);

    Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
