
namespace OpenDoors.Service.Interfaces;

public interface IExternalDoorService
{
    Task OpenDoor(int doorId);
}

public class ExternalDoorServiceMock : IExternalDoorService
{
    public Task OpenDoor(int doorId)
    {
        return Task.CompletedTask;
    }
}
