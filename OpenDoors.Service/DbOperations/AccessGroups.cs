using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;

namespace OpenDoors.Service.DbOperations;

public static class AccessGroupsDbOperations
{
    public static async Task<AccessGroup> GetDefaultAccessGroup(this OpenDoorsContext dbContext, Guid tenantId)
    {
        return await dbContext.AccessGroups
            .Where(g => g.Tenant.Id == tenantId && g.Name == "default")
            .FirstAsync();
    }

    public static async Task<AccessGroup?> GetAccessGroup(this OpenDoorsContext dbContext, Guid accessGroupId, bool detailed = false)
    {
        IQueryable<AccessGroup> query = dbContext.AccessGroups.Where(g => g.Id == accessGroupId);
        if (detailed)
        {
            query = query.Include(g => g.Members).Include(g => g.Doors);
        }

        return await query.FirstOrDefaultAsync();
    }

    public static Task<AccessGroup?> GetAccessGroupByName(this OpenDoorsContext dbContext, string groupName, Guid tenantId)
    {
        return dbContext.AccessGroups
            .Where(g => g.Name == groupName && g.Tenant.Id == tenantId)
            .FirstOrDefaultAsync();
    }

    public static async Task<IReadOnlyList<AccessGroup>> GetAccessGroupsForTenant(this OpenDoorsContext dbContext, Guid tenantId)
    {
        return await dbContext.AccessGroups.Where(g => g.Tenant.Id == tenantId).ToListAsync();
    }

    public static async Task<IReadOnlyList<AccessGroup>> GetAccessGroupsForUser(this OpenDoorsContext dbContext, string userId)
    {
        return await dbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.AccessGroups)
            .SelectMany(u => u.AccessGroups)
            .ToListAsync();
    }

    public static async Task CreateAccessGroup(this OpenDoorsContext dbContext, string name, Guid tenantId)
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

    public static async Task AddUserToAccessGroup(this OpenDoorsContext dbContext, string userId, AccessGroup accessGroup)
    {
        TenantUser user = await dbContext.Users.Where(u => u.Id == userId).FirstAsync();
        accessGroup.Members.Add(user);
        await dbContext.SaveChangesAsync();
    }

    public static async Task RemoveUserFromGroup(this OpenDoorsContext dbContext, string droppedMemberId, AccessGroup accessGroup)
    {
        TenantUser member = accessGroup.Members.First(m => m.Id == droppedMemberId);
        accessGroup.Members.Remove(member);
        await dbContext.SaveChangesAsync();
    }
}

