using OpenDoors.Model;

namespace OpenDoors.Service.Interfaces;

public interface IAccessGroupManager
{
    Task<AccessGroup> GetDefaultAccessGroup(Guid tenantId);

    Task<AccessGroup?> GetAccessGroup(Guid accessGroupId, bool detailed = false);

    Task<AccessGroup?> GetAccessGroupByName(string groupName, Guid tenantId);

    Task<IReadOnlyList<AccessGroup>> GetAccessGroupsForTenant(Guid tenantId);

    Task<IReadOnlyList<AccessGroup>> GetAccessGroupsForUser(string userId);

    Task CreateAccessGroup(string name, Guid tenantId);

    Task AddUserToAccessGroup(string userId, AccessGroup defaultAccessGroup);

    Task RemoveUserFromGroup(string droppedMemberId, AccessGroup accessGroup);
}

