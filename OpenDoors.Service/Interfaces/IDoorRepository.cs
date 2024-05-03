using OpenDoors.Model;

namespace OpenDoors.Service.Interfaces;

public interface IDoorRepository
{
    Task CreateDoor(string location, AccessGroup accessGroup);

    Task<IReadOnlyList<Door>> ListDoorsForUser(string userId);

    Task<IReadOnlyList<Door>> ListDoorsForTenant(Guid tenantId);
}

public record OpenDoorResult(bool Succeeded, int DoorId, string UserId, FailureReason? FailureReason);

public enum FailureReason
{
    Unauthorized,
    ExternalError
}
