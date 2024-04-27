using OpenDoors.Model;

namespace OpenDoors.Service.Interfaces;

public interface IDoorService
{
    Task CreateDoor(string location, AccessGroup accessGroup);

    Task<IReadOnlyList<Door>> ListDoorsForUser(string userId);

    Task<bool> OpenDoor(int doorId);
}

public record OpenDoorResult(bool Succeeded, int DoorId, string UserId, FailureReason? FailureReason);

public enum FailureReason
{
    Unauthorized,
    ExternalError
}
