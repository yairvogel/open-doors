using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Model;
using OpenDoors.Service.Authorization;
using OpenDoors.Service.Handlers;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Controllers;

[ApiController]
[Route("/doors")]
public class DoorsController(DoorHandler doorHandler, IAuthorizationService authorizationService, IEntryLogger entryLogger) : ControllerBase
{
    [HttpPost]
    [Authorize(AuthorizationConstants.TenantAdminPolicy)]
    public async Task<IActionResult> CreateDoor([FromBody] CreateDoorRequest createDoorRequest)
    {
        Guid tenantId = HttpContext.User.GetTenantId();
        await doorHandler.CreateDoor(createDoorRequest.Location, tenantId, createDoorRequest.AccessGroupName);
        return Created();
    }

    [HttpGet]
    [Authorize(AuthorizationConstants.HasTenantPolicy)]
    public async Task<IActionResult> ListDoors()
    {
        string userId = HttpContext.User.GetUserId();
        Guid tenantId = HttpContext.User.GetTenantId();
        bool isAdmin = HttpContext.User.IsInRole(AuthorizationConstants.AdminRole);
        IReadOnlyList<Door> doors = await doorHandler.ListDoors(userId, tenantId, isAdmin);
        IReadOnlyList<DoorDto> doorDtos = doors.Select(d => new DoorDto(d.Id, d.Location)).ToList();
        return Ok(doorDtos);
    }

    [HttpGet("open/{id}")]
    [Authorize(AuthorizationConstants.HasTenantPolicy)]
    public async Task<IActionResult> OpenDoor(int id)
    {
        string userId = HttpContext.User.GetUserId();
        Guid tenantId = HttpContext.User.GetTenantId();
        var authResult = await authorizationService.AuthorizeAsync(HttpContext.User, null, [ new AllowedEntryAuthorizationRequirement(id) ]);
        if (!authResult.Succeeded)
        {
            await entryLogger.LogUnauthorized(id, userId, tenantId);
            return Unauthorized();
        }

        OpenDoorResult result = await doorHandler.OpenDoor(id, userId);
        if (result.Succeeded)
        {
            await entryLogger.LogSuccess(id, userId, tenantId);
            return Ok();
        }

        await entryLogger.LogFailure(id, userId, tenantId);
        // gaslighting the user
        return BadRequest();
    }
}

public record CreateDoorRequest(string Location, string? AccessGroupName);
public record DoorDto(int Id, string Location);
