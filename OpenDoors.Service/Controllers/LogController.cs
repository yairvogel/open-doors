using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Model;
using OpenDoors.Service.Authorization;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Controllers;

[ApiController]
public class LogController(IEntryLogger entryLogger) : ControllerBase
{
    [HttpGet("log")]
    [Authorize(AuthorizationConstants.AuditorPolicy)]
    public async Task<IActionResult> ReadLog()
    {
        Guid tenantId = HttpContext.User.GetTenantId();
        IReadOnlyList<EntryLog> logs = await entryLogger.ReadLog(tenantId);

        return Ok(logs.Select(l => new EntryLogDto(l.Timestamp, l.DoorId, l.UserId, l.Success, l.FailureReason)).ToList());
    }
}

public record EntryLogDto(DateTime Timestamp, int DoorId, string UserId, bool Success, string? FailureReason);
