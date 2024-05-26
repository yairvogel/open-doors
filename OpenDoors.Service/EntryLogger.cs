using System.Threading.Tasks.Dataflow;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;

namespace OpenDoors.Service.DbOperations;

public class EntryLogger : IAsyncDisposable
{
    private IDbContextFactory<OpenDoorsContext> contextFactory;
    private BatchBlock<EntryLog> batchBlock;
    private ActionBlock<EntryLog[]> loggerBlock;
    private IDisposable link;
    private Timer timer;

    public EntryLogger(IDbContextFactory<OpenDoorsContext> contextFactory)
    {
        this.contextFactory = contextFactory;
        batchBlock = new BatchBlock<EntryLog>(50);
        loggerBlock = new ActionBlock<EntryLog[]>(LogEntriesAsync);
        link = batchBlock.LinkTo(loggerBlock, new() { PropagateCompletion = true });
        timer = new Timer(_ => batchBlock.TriggerBatch(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    private async Task LogEntriesAsync(EntryLog[] logs)
    {
        var dbContext = await contextFactory.CreateDbContextAsync();
        foreach (var log in logs)
        {
            await dbContext.AddAsync(log);
        }

        await dbContext.SaveChangesAsync();
    }

    public Task LogFailure(int doorId, string userId, Guid tenantId)
    {
        return batchBlock.SendAsync(new()
        {
            DoorId = doorId,
            UserId = userId,
            Success = false,
            FailureReason = "External Error",
            TenantId = tenantId
        });
    }

    public Task LogSuccess(int doorId, string userId, Guid tenantId)
    {
        return batchBlock.SendAsync(new()
        {
            DoorId = doorId,
            UserId = userId,
            Success = true,
            TenantId = tenantId
        });
    }

    public Task LogUnauthorized(int doorId, string userId, Guid tenantId)
    {
        return batchBlock.SendAsync(new()
        {
            DoorId = doorId,
            UserId = userId,
            Success = false,
            FailureReason = "Unauthorized",
            TenantId = tenantId
        });
    }

    public async Task<IReadOnlyList<EntryLog>> ReadLog(Guid tenantId)
    {
        var dbContext = await contextFactory.CreateDbContextAsync();
        return await dbContext.Log
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await timer.DisposeAsync();
        batchBlock.Complete();
        await loggerBlock.Completion;
        link.Dispose();
    }
}
