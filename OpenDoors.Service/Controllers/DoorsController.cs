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
        Guid tenantId = HttpContext.User.GetTenantId();
        string userId = HttpContext.User.GetUserId();
        OpenDoorResult result = await doorHandler.OpenDoor(id, userId, tenantId);
        if (result.Succeeded)
        {
            return Ok();
        }

        return result.FailureReason switch 
        {
            FailureReason.Unauthorized => Unauthorized(),
            FailureReason.ExternalError => BadRequest(),
            _ => throw new ArgumentOutOfRangeException("unreachable")
        };
    }
}

public record CreateDoorRequest(string Location, string? AccessGroupName);
public record DoorDto(int Id, string Location);
