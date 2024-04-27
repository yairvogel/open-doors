using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Model;
using OpenDoors.Service.Authorization;
using OpenDoors.Service.Handlers;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Controllers;

[ApiController]
[Route("/doors")]
public class DoorsController(DoorHandler doorHandler) : ControllerBase
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
        IReadOnlyList<Door> doors = await doorHandler.ListDoorsForUser(userId);
        IReadOnlyList<DoorDto> doorDtos = doors.Select(d => new DoorDto(d.Id, d.Location)).ToList();
        return Ok(doorDtos);
    }

    [HttpGet("open/{id}")]
    [Authorize(AuthorizationConstants.HasTenantPolicy)]
    public async Task<IActionResult> OpenDoor(int id)
    {
        Guid tenantId = HttpContext.User.GetTenantId();
        string userId = HttpContext.User.GetUserId();
        OpenDoorResult result = await doorHandler.OpenDoor(id, userId, tenantId);
        if (result.Succeeded)
        {
            return Ok();
        }

        return BadRequest(result.FailureReason.ToString());
    }
}

public record CreateDoorRequest(string Location, string? AccessGroupName);
public record DoorDto(int Id, string Location);
