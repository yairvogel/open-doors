using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Services;

public class DoorRepository(OpenDoorsContext dbContext) : IDoorRepository
{
    public async Task CreateDoor(string location, AccessGroup accessGroup)
    {
        Door door = new Door { Location = location, AccessGroups = [accessGroup] };

        await dbContext.AddAsync(door);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Door>> ListDoorsForTenant(Guid tenantId)
    {
        List<Door> doors = await dbContext.AccessGroups
            .Where(g => g.TenantId == tenantId)
            .Include(g => g.Doors)
            .SelectMany(g => g.Doors)
            .ToListAsync();

        return doors.Distinct().ToList();
    }

    public async Task<IReadOnlyList<Door>> ListDoorsForUser(string userId)
    {
        List<Door> doors = await dbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.AccessGroups)
            .SelectMany(u => u.AccessGroups)
            .Include(g => g.Doors)
            .SelectMany(g => g.Doors)
            .ToListAsync();

        return doors.Distinct().ToList();
    }
}
