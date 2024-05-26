using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;

namespace OpenDoors.Service.DbOperations;

public static class DoorRepository
{
    public static async Task CreateDoor(this OpenDoorsContext dbContext, string location, AccessGroup accessGroup)
    {
        Door door = new Door { Location = location, AccessGroups = [accessGroup] };

        await dbContext.AddAsync(door);
        await dbContext.SaveChangesAsync();
    }

    public static async Task<IReadOnlyList<Door>> ListDoorsForTenant(this OpenDoorsContext dbContext, Guid tenantId)
    {
        List<Door> doors = await dbContext.AccessGroups
            .Where(g => g.TenantId == tenantId)
            .Include(g => g.Doors)
            .SelectMany(g => g.Doors)
            .ToListAsync();

        return doors.Distinct().ToList();
    }

    public static async Task<IReadOnlyList<Door>> ListDoorsForUser(this OpenDoorsContext dbContext, string userId)
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
