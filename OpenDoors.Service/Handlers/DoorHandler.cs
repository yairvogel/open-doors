using OpenDoors.Exceptions;
using OpenDoors.Model;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Handlers;

public class DoorHandler(IAccessGroupRepository accessGroupRepository, IDoorRepository doorRepository, IExternalDoorService externalDoorService, IEntryLogger entryLogger)
{
    public async Task CreateDoor(string location, Guid tenantId, string? accessGroupName = null)
    {
        AccessGroup accessGroup = accessGroupName is null 
            ? await accessGroupRepository.GetDefaultAccessGroup(tenantId)
            : await accessGroupRepository.GetAccessGroupByName(accessGroupName, tenantId) ?? throw new AccessGroupNotFoundException("access group does was not found");

        await doorRepository.CreateDoor(location, accessGroup);
    }

    public Task<IReadOnlyList<Door>> ListDoors(string userId, Guid tenantId, bool isAdmin)
    {
        return isAdmin 
            ? doorRepository.ListDoorsForTenant(tenantId)
            : doorRepository.ListDoorsForUser(userId);
    }

    public async Task<OpenDoorResult> OpenDoor(int doorId, string userId)
    {
        return await externalDoorService.OpenDoor(doorId)
            ? new OpenDoorResult(false, doorId, userId, FailureReason.ExternalError)
            : new OpenDoorResult(true, doorId, userId, null);
    }
}
