using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Exceptions;
using OpenDoors.Model;
using OpenDoors.Service.Authorization;
using OpenDoors.Service.Handlers;

namespace OpenDoors.Service.Controllers;

[ApiController]
[Route("/groups")]
public class AccessGroupsController(AccessGroupsHandler accessGroupsHandler) : ControllerBase
{
    [HttpGet]
    [Authorize(AuthorizationConstants.HasTenantPolicy)]
    public async Task<IActionResult> GetAccessGroups()
    {
        string userId = HttpContext.User.GetUserId();
        Guid tenantId = HttpContext.User.GetTenantId();
        bool isAdmin = HttpContext.User.IsInRole(AuthorizationConstants.AdminRole);
        IReadOnlyList<AccessGroup> groups = await accessGroupsHandler.GetAccessGroupsForUser(userId, tenantId, isAdmin);
        if (groups.Count == 0)
        {
            return NotFound();
        }

        List<AccessGroupListItemDto> groupDtos = groups.Select(g => new AccessGroupListItemDto(g.Id.ToString()!, g.Name, $"/groups/{g.Id}")).ToList();
        return Ok(groupDtos);
    }


    [HttpPost]
    [Authorize(AuthorizationConstants.TenantAdminPolicy)]
    public async Task<IActionResult> CreateAccessGroup([FromBody] CreateAccessGroupRequest request)
    {
        Guid tenantId = HttpContext.User.GetTenantId();

        try
        {
            await accessGroupsHandler.CreateAccessGroup(request.Name, tenantId);
            return Created();
        }
        catch (TenantNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{groupId}")]
    [Authorize(AuthorizationConstants.HasTenantPolicy)]
    public async Task<IActionResult> GetAccessGroup(Guid groupId)
    {
        Guid tenantId = HttpContext.User.GetTenantId(); 
        string userId = HttpContext.User.GetUserId();
        bool isAdmin = HttpContext.User.IsInRole(AuthorizationConstants.AdminRole);
        AccessGroup? group = await accessGroupsHandler.GetAccessGroup(groupId, userId, tenantId, isAdmin);
        AccessGroupDto dto = new AccessGroupDto(
                Id: group!.Id.ToString()!,
                Name: group.Name,
                Doors: group.Doors.Select(d => new DoorDto(d.Id, d.Location)).ToList(),
                MemberIds: group.Members.Select(m => m.Id).ToList());

        return group is null ? NotFound() : Ok(dto);
    }

    [HttpPut("{groupId}")]
    [Authorize(AuthorizationConstants.TenantAdminPolicy)]
    public async Task<IActionResult> UpdateAccessGroup([FromBody] AccessGroupUpdateRequest dto)
    {
        await accessGroupsHandler.UpdateAccessGroup(dto);
        return Created();
    }
}

public record AccessGroupListItemDto(string Id, string Name, string Path);
public record AccessGroupDto(string Id, string Name, List<DoorDto> Doors, List<string> MemberIds);
public record AccessGroupUpdateRequest(string Id, string Name, List<string> MemberIds);
public record CreateAccessGroupRequest(string Name);
