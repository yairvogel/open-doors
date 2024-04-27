using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Services;

public class DoorService(OpenDoorsContext dbContext, IExternalDoorService externalDoorService) : IDoorService
{
    private static readonly OpenDoorResult _successResult = new(true, null);

    public async Task CreateDoor(string location, AccessGroup accessGroup)
    {
        Door door = new Door { Location = location, AccessGroups = [accessGroup] };

        await dbContext.AddAsync(door);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Door>> ListDoorsForUser(string userId)
    {
        var doors = await dbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.AccessGroups).AsQueryable()
            .SelectMany(u => u.AccessGroups)
            .Include(g => g.Doors).AsQueryable()
            .SelectMany(g => g.Doors)
            .ToListAsync();

        return doors.Distinct().ToList();
    }

    public async Task<OpenDoorResult> OpenDoor(int doorId)
    {
        try
        {
            await externalDoorService.OpenDoor(doorId);
            return _successResult;
        }
        catch (Exception e)
        {
            return new OpenDoorResult(false, e);
        }
    }
}
