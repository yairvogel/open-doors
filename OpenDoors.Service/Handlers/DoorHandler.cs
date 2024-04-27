using OpenDoors.Exceptions;
using OpenDoors.Model;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Handlers;

public class DoorHandler(IAccessGroupManager accessGroupManager, IDoorService doorService)
{
    public async Task CreateDoor(string location, Guid tenantId, string? accessGroupName = null)
    {
        AccessGroup accessGroup = accessGroupName is null 
            ? await accessGroupManager.GetDefaultAccessGroup(tenantId)
            : await accessGroupManager.GetAccessGroupByName(accessGroupName, tenantId) ?? throw new AccessGroupNotFoundException("access group does was not found");

        await doorService.CreateDoor(location, accessGroup);
    }

    public Task<IReadOnlyList<Door>> ListDoorsForUser(string userId)
    {
        return doorService.ListDoorsForUser(userId);
    }

    public async Task<OpenDoorResult> OpenDoor(int doorId)
    {
        OpenDoorResult result = await doorService.OpenDoor(doorId);
        // log
        return result;
    }
}
