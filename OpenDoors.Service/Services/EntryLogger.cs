using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Services;

public class EntryLogger(OpenDoorsContext dbContext) : IEntryLogger
{
    public Task LogFailure(int doorId, string userId, Guid tenantId)
    {
        EntryLog entry = new()
        {
            DoorId = doorId,
            UserId = userId,
            Success = false,
            FailureReason = FailureReason.ExternalError.ToString(),
            TenantId = tenantId
        };

        return LogResult(entry);
    }

    public Task LogSuccess(int doorId, string userId, Guid tenantId)
    {
        EntryLog entry = new()
        {
            DoorId = doorId,
            UserId = userId,
            Success = true,
            TenantId = tenantId
        };

        return LogResult(entry);
    }

    public Task LogUnauthorized(int doorId, string userId, Guid tenantId)
    {
        EntryLog entry = new()
        {
            DoorId = doorId,
            UserId = userId,
            Success = false,
            FailureReason = FailureReason.Unauthorized.ToString(),
            TenantId = tenantId
        };
        
        return LogResult(entry);
    }

    public async Task<IReadOnlyList<EntryLog>> ReadLog(Guid tenantId)
    {
        return await dbContext.Log
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    private async Task LogResult(EntryLog log)
    {
        await dbContext.AddAsync(log);
    }
}
