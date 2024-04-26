using OpenDoors.Model;
using OpenDoors.Model.Authentication;

namespace OpenDoors.Service.Interfaces;

public interface ITenantManager
{
    Task<Tenant?> GetByNameAsync(string tenantName, CancellationToken cancellationToken = default);

    Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default);

    Task<AccessGroup> GetDefaultAccessGroup(Guid tenantId, CancellationToken cancellationToken = default);

    Task<AccessGroup?> GetAccessGroup(Guid accessGroupId, CancellationToken cancellationToken = default);

    Task AddUserToAccessGroup(TenantUser user, AccessGroup defaultAccessGroup, CancellationToken cancellationToken = default);
}
