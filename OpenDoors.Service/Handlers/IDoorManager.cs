using OpenDoors.Model;

namespace OpenDoors.Service.Handlers;

public interface IDoorManager
{
    Task CreateDoor(string location, AccessGroup accessGroup);
}

