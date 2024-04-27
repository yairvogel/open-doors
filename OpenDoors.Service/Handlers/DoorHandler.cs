using OpenDoors.Exceptions;
using OpenDoors.Model;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Handlers;

public class DoorHandler(IAccessGroupManager accessGroupManager, IDoorService doorService, IEntryLogger entryLogger)
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

    public async Task<OpenDoorResult> OpenDoor(int doorId, string userId, Guid tenantId)
    {
        bool allowedEntry = await IsAllowedEntry(doorId, userId);
        if (!allowedEntry)
        {
            await entryLogger.LogUnauthorized(doorId, userId, tenantId);
            return new OpenDoorResult(false, doorId, userId, FailureReason.Unauthorized);
        }

        bool success = await doorService.OpenDoor(doorId);

        if (!success)
        {
            await entryLogger.LogFailure(doorId, userId, tenantId);
            return new OpenDoorResult(false, doorId, userId, FailureReason.ExternalError);
        }

        await entryLogger.LogSuccess(doorId, userId, tenantId);
        return new OpenDoorResult(true, doorId, userId, null);
    }

    public async Task<bool> IsAllowedEntry(int doorId, string userId)
    {
        IReadOnlyList<Door> doors = await doorService.ListDoorsForUser(userId);
        return doors.Any(d => d.Id == doorId);
    }
}
