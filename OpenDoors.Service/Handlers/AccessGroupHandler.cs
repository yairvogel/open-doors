using OpenDoors.Exceptions;
using OpenDoors.Model;
using OpenDoors.Service.Controllers;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Handlers;

public class AccessGroupsHandler(IAccessGroupRepository accessGroupRepository, ILogger<AccessGroupsHandler> logger)
{
    public Task<IReadOnlyList<AccessGroup>> GetAccessGroupsForUser(string userId, Guid tenantId, bool isAdmin)
    {
        return isAdmin 
            ? accessGroupRepository.GetAccessGroupsForTenant(tenantId) 
            : accessGroupRepository.GetAccessGroupsForUser(userId);
    }

    public async Task<bool> CreateAccessGroup(string name, Guid tenantId)
    {
        IReadOnlyList<AccessGroup> accessGroups = await accessGroupRepository.GetAccessGroupsForTenant(tenantId);
        if (accessGroups.Any(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        await accessGroupRepository.CreateAccessGroup(name, tenantId);
        return true;
    }

    public async Task<AccessGroup?> GetAccessGroup(Guid groupId, string userId, Guid tenantId, bool isAdmin)
    {
        AccessGroup? accessGroup = await accessGroupRepository.GetAccessGroup(groupId, detailed: true);
        if (accessGroup is null)
        {
            return null;
        }

        if (accessGroup.TenantId != tenantId)
        {
            logger.LogWarning($"requested access group from different tenant. access group id: {accessGroup}, tenantId: {tenantId}");
            return null;
        }

        if (!isAdmin && !accessGroup.Members.Any(m => m.Id == userId))
        {
            // user doesn't have permission to view this access group
            return null;
        }

        return accessGroup;
    }

    public async Task UpdateAccessGroup(AccessGroupUpdateRequest dto)
    {
        if (!Guid.TryParse(dto.Id, out var accessGroupId))
        {
            throw new ArgumentException("access group id is not a valid id");
        }

        AccessGroup? accessGroup = await accessGroupRepository.GetAccessGroup(accessGroupId, detailed: true);
        if (accessGroup is null)
        {
            throw new AccessGroupNotFoundException("group with id {accessGroupId} was not found");
        }

        if (!string.IsNullOrEmpty(dto.Name))
        {
            accessGroup.Name = dto.Name;
        }

        IEnumerable<string> droppedMemberIds = accessGroup.Members.Select(m => m.Id).Except(dto.MemberIds);
        foreach (string droppedMemberId in droppedMemberIds)
        {
            await accessGroupRepository.RemoveUserFromGroup(droppedMemberId, accessGroup);
        }


        IEnumerable<string> newMemberIds = dto.MemberIds.Except(accessGroup.Members.Select(m => m.Id));
        foreach (string newMemeberId in newMemberIds)
        {
            await accessGroupRepository.AddUserToAccessGroup(newMemeberId, accessGroup);
        }
    }
}
