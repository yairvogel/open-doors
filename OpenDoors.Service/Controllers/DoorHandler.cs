using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;

namespace OpenDoors.Service.Handlers;

public class DoorHandler(OpenDoorsContext dbContext)
{
    public async Task CreateDoor(string location, Guid tenantId)
    {
        var tenant = await dbContext.Tenants.Where(t => t.Id == tenantId).FirstAsync();
        Door door = new Door { Location = location, Tenant = tenant };

        await dbContext.AddAsync(door);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Door>> ListDoorsForUser(string userId, Guid tenantId)
    {
        return await dbContext.Doors.Where(d => d.Tenant.Id == tenantId).ToListAsync();
    }
}

