namespace OpenDoors.Service.Interfaces;

public class ExternalDoorServiceMock : IExternalDoorService
{
    public Task<bool> OpenDoor(int doorId)
    {
        try
        {
            // Some computations here
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}