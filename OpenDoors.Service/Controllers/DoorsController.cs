using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Model;
using OpenDoors.Service.Authorization;
using OpenDoors.Service.DbOperations;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Service.Controllers;

[ApiController]
[Route("/doors")]
public class DoorsController(OpenDoorsContext dbContext, IAuthorizationService authorizationService, EntryLogger entryLogger, IExternalDoorService externalDoorService) : ControllerBase
{
    [HttpPost]
    [Authorize(AuthorizationConstants.TenantAdminPolicy)]
    public async Task<IActionResult> CreateDoor([FromBody] CreateDoorRequest createDoorRequest)
    {
        (string location, string? accessGroupName) = createDoorRequest;
        Guid tenantId = HttpContext.User.GetTenantId();
        AccessGroup? accessGroup = accessGroupName is null 
            ? await dbContext.GetDefaultAccessGroup(tenantId)
            : await dbContext.GetAccessGroupByName(accessGroupName, tenantId);

        if (accessGroup is null)
        {
            return BadRequest($"Access Group with name {accessGroupName} was not found");
        }

        await dbContext.CreateDoor(createDoorRequest.Location, accessGroup);
        await dbContext.SaveChangesAsync();
        return Created();
    }

    [HttpGet]
    [Authorize(AuthorizationConstants.HasTenantPolicy)]
    public async Task<IActionResult> ListDoors()
    {
        string userId = HttpContext.User.GetUserId();
        Guid tenantId = HttpContext.User.GetTenantId();
        bool isAdmin = HttpContext.User.IsInRole(AuthorizationConstants.AdminRole);

        IReadOnlyList<Door> doors = isAdmin 
            ? await dbContext.ListDoorsForTenant(tenantId)
            : await dbContext.ListDoorsForUser(userId);

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

        bool success = await externalDoorService.OpenDoor(id);
        if (success)
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
