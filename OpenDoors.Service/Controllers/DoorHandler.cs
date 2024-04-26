using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Handlers;

public class DoorHandler(OpenDoorsContext dbContext, ITenantManager tenantManager)
{
    public async Task CreateDoor(string location, Guid tenantId, Guid? accessGroupId = null)
    {
        AccessGroup accessGroup = accessGroupId is null 
            ? await tenantManager.GetDefaultAccessGroup(tenantId)
            : await tenantManager.GetAccessGroup(accessGroupId!.Value) ?? throw new ArgumentException("access group does not exist");

        Door door = new Door { Location = location, AccessGroups = [accessGroup] };

        await dbContext.AddAsync(door);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Door>> ListDoorsForUser(string userId)
    {
        return await dbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.AccessGroups)
            .SelectMany(u => u.AccessGroups)
            .Include(g => g.Doors)
            .SelectMany(g => g.Doors)
            .DistinctBy(d => d.Id)
            .ToListAsync();
    }
}

