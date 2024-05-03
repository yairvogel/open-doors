
namespace OpenDoors.Service.Interfaces;

public interface IExternalDoorService
{
    Task<bool> OpenDoor(int doorId);
}