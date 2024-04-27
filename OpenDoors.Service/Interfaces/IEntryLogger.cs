using OpenDoors.Model;

namespace OpenDoors.Service.Interfaces;

public interface IEntryLogger
{
    Task LogUnauthorized(int doorId, string userId, Guid tenantId);

    Task LogSuccess(int doorId, string userId, Guid tenantId);

    Task LogFailure(int doorId, string userId, Guid tenantId);

    Task<IReadOnlyList<EntryLog>> ReadLog(Guid tenantId);
}
