using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
using OpenDoors.Service.Authorization;

namespace OpenDoors.Service.Controllers;

[ApiController]
public class DoorsController(DoorHandler doorHandler) : ControllerBase
{
    [HttpPost]
    [Route("/doors")]
    [Authorize(Policy = AuthorizationConstants.TenantAdminPolicy)]
    public async Task<IActionResult> CreateDoor([FromBody] CreateDoorRequest createDoorRequest)
    {
        Guid tenantId = HttpContext.User.GetTenantId();
        await doorHandler.CreateDoor(createDoorRequest.location, tenantId);
        return Created();
    }

    [HttpGet]
    [Route("/doors")]
    [Authorize(Policy = "Test")]
    public async Task<IActionResult> ListDoors()
    {
        Guid tenantId = HttpContext.User.GetTenantId();
        Guid userId = HttpContext.User.GetUserId();
        IReadOnlyList<Door> doors = await doorHandler.ListDoorsForUser(userId, tenantId);
        IReadOnlyList<DoorDto> doorDtos = doors.Select(d => new DoorDto(d.Id, d.Location)).ToList();
        return Ok(doorDtos);
    }
}

public record CreateDoorRequest(string location);
public record DoorDto(int Id, string Location);


public class DoorHandler(DoorRepository doorRepository)
{
    public Task CreateDoor(string location, Guid tenant)
    {
        return doorRepository.CreateDoor(location, tenant);
    }

    public Task<IReadOnlyList<Door>> ListDoorsForUser(Guid userId, Guid tenantId)
    {
        return doorRepository.ListDoorsForUser(userId, tenantId);
    }
}

public class DoorRepository(OpenDoorsContext dbContext)
{
    public async Task CreateDoor(string location, Guid tenantId)
    {
        var tenant = await dbContext.Tenants.Where(t => t.Id == tenantId).FirstAsync();
        Door door = new Door { Location = location, Tenant = tenant };

        await dbContext.AddAsync(door);
        await dbContext.SaveChangesAsync();
    }

    public Task<IReadOnlyList<Door>> ListDoorsForUser(Guid userId, Guid tenantId)
    {
        throw new NotImplementedException();
    }
}
