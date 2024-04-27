using OpenDoors.Model;

namespace OpenDoors.Service.Interfaces;

public interface IDoorService
{
    Task CreateDoor(string location, AccessGroup accessGroup);

    Task<IReadOnlyList<Door>> ListDoorsForUser(string userId);

    Task<OpenDoorResult> OpenDoor(int doorId);
}

public class OpenDoorResult(bool Succeeded, Exception? Error);
