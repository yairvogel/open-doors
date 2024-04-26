using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Model;
using OpenDoors.Service.Authorization;
using OpenDoors.Service.Handlers;

namespace OpenDoors.Service.Controllers;

[ApiController]
public class DoorsController(DoorHandler doorHandler) : ControllerBase
{
    [HttpPost]
    [Route("/doors")]
    [Authorize(AuthorizationConstants.TenantAdminPolicy)]
    public async Task<IActionResult> CreateDoor([FromBody] CreateDoorRequest createDoorRequest)
    {
        Guid tenantId = HttpContext.User.GetTenantId();
        await doorHandler.CreateDoor(createDoorRequest.location, tenantId);
        return Created();
    }

    [HttpGet]
    [Route("/doors")]
    [Authorize(AuthorizationConstants.HasTenantPolicy)]
    public async Task<IActionResult> ListDoors()
    {
        string userId = HttpContext.User.GetUserId();
        IReadOnlyList<Door> doors = await doorHandler.ListDoorsForUser(userId);
        IReadOnlyList<DoorDto> doorDtos = doors.Select(d => new DoorDto(d.Id, d.Location)).ToList();
        return Ok(doorDtos);
    }
}

public record CreateDoorRequest(string location);
public record DoorDto(int Id, string Location);
