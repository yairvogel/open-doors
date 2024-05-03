using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
using OpenDoors.Model.Authentication;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Services;

public class AccessGroupRepository(OpenDoorsContext dbContext) : IAccessGroupRepository
{
    public async Task<AccessGroup> GetDefaultAccessGroup(Guid tenantId)
    {
        return await dbContext.AccessGroups
            .Where(g => g.Tenant.Id == tenantId && g.Name == "default")
            .FirstAsync();
    }

    public async Task<AccessGroup?> GetAccessGroup(Guid accessGroupId, bool detailed = false)
    {
        IQueryable<AccessGroup> query = dbContext.AccessGroups.Where(g => g.Id == accessGroupId);
        if (detailed)
        {
            query = query.Include(g => g.Members).Include(g => g.Doors);
        }

        return await query.FirstOrDefaultAsync();
    }

    public Task<AccessGroup?> GetAccessGroupByName(string groupName, Guid tenantId)
    {
        return dbContext.AccessGroups
            .Where(g => g.Name == groupName && g.Tenant.Id == tenantId)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<AccessGroup>> GetAccessGroupsForTenant(Guid tenantId)
    {
        return await dbContext.AccessGroups.Where(g => g.Tenant.Id == tenantId).ToListAsync();
    }

    public async Task<IReadOnlyList<AccessGroup>> GetAccessGroupsForUser(string userId)
    {
        return await dbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.AccessGroups)
            .SelectMany(u => u.AccessGroups)
            .ToListAsync();
    }

    public async Task CreateAccessGroup(string name, Guid tenantId)
    {
        Tenant? tenant = await dbContext.Tenants.Where(t => t.Id == tenantId).FirstOrDefaultAsync();
        if (tenant is null)
        {
            throw new ArgumentException("Tenant was not found");
        }

        await dbContext.AddAsync(new AccessGroup
            {
                Name = name,
                Tenant = tenant,
                Doors = [],
                Members = [],
            });

        await dbContext.SaveChangesAsync();
    }

    public async Task AddUserToAccessGroup(string userId, AccessGroup accessGroup)
    {
        TenantUser user = await dbContext.Users.Where(u => u.Id == userId).FirstAsync();
        accessGroup.Members.Add(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveUserFromGroup(string droppedMemberId, AccessGroup accessGroup)
    {
        TenantUser member = accessGroup.Members.First(m => m.Id == droppedMemberId);
        accessGroup.Members.Remove(member);
        await dbContext.SaveChangesAsync();
    }
}

