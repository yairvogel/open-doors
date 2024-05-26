using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Exceptions;
using OpenDoors.Model;
using OpenDoors.Service.Authorization;
using OpenDoors.Service.DbOperations;

namespace OpenDoors.Service.Controllers;

[ApiController]
[Route("/groups")]
public class AccessGroupsController(OpenDoorsContext dbContext, ILogger<AccessGroupsController> logger) : ControllerBase
{
    [HttpGet]
    [Authorize(AuthorizationConstants.HasTenantPolicy)]
    public async Task<IActionResult> GetAccessGroups()
    {
        string userId = HttpContext.User.GetUserId();
        Guid tenantId = HttpContext.User.GetTenantId();
        bool isAdmin = HttpContext.User.IsInRole(AuthorizationConstants.AdminRole);

        IReadOnlyList<AccessGroup> groups = isAdmin 
            ? await dbContext.GetAccessGroupsForTenant(tenantId)
            : await dbContext.GetAccessGroupsForUser(userId);
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
        string name = request.Name;

        IReadOnlyList<AccessGroup> accessGroups = await dbContext.GetAccessGroupsForTenant(tenantId);

        if (accessGroups.Any(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest("Access group already exists");
        }

        await dbContext.CreateAccessGroup(name, tenantId);
        await dbContext.SaveChangesAsync();
        return Created();
    }

    [HttpGet("{groupId}")]
    [Authorize(AuthorizationConstants.HasTenantPolicy)]
    public async Task<IActionResult> GetAccessGroup(Guid groupId)
    {
        Guid tenantId = HttpContext.User.GetTenantId(); 
        string userId = HttpContext.User.GetUserId();
        bool isAdmin = HttpContext.User.IsInRole(AuthorizationConstants.AdminRole);

        AccessGroup? group = await dbContext.GetAccessGroup(groupId, detailed: true);
        if (group is null)
        {
            return NotFound();
        }

        if (group.TenantId != tenantId)
        {
            logger.LogWarning($"requested access group from different tenant. access group id: {group}, tenantId: {tenantId}");
            return NotFound();
        }

        if (!isAdmin && !group.Members.Any(m => m.Id == userId))
        {
            // user doesn't have permission to view this access group
            return NotFound();
        }

        return Ok(new AccessGroupDto(
                    Id: group!.Id.ToString()!,
                    Name: group.Name,
                    Doors: group.Doors.Select(d => new DoorDto(d.Id, d.Location)).ToList(),
                    MemberIds: group.Members.Select(m => m.Id).ToList()));
    }

    [HttpPut("{groupId}")]
    [Authorize(AuthorizationConstants.TenantAdminPolicy)]
    public async Task<IActionResult> UpdateAccessGroup([FromBody] AccessGroupUpdateRequest dto)
    {
        if (!Guid.TryParse(dto.Id, out var accessGroupId))
        {
            return BadRequest("access group id is not a valid id");
        }

        AccessGroup? accessGroup = await dbContext.GetAccessGroup(accessGroupId, detailed: true);
        if (accessGroup is null)
        {
            return NotFound(new AccessGroupNotFoundException("group with id {accessGroupId} was not found"));
        }

        if (!string.IsNullOrEmpty(dto.Name))
        {
            accessGroup.Name = dto.Name;
        }

        IEnumerable<string> droppedMemberIds = accessGroup.Members.Select(m => m.Id).Except(dto.MemberIds);
        foreach (string droppedMemberId in droppedMemberIds)
        {
            await dbContext.RemoveUserFromGroup(droppedMemberId, accessGroup);
        }


        IEnumerable<string> newMemberIds = dto.MemberIds.Except(accessGroup.Members.Select(m => m.Id));
        foreach (string newMemeberId in newMemberIds)
        {
            await dbContext.AddUserToAccessGroup(newMemeberId, accessGroup);
        }

        return Created();
    }
}

public record AccessGroupListItemDto(string Id, string Name, string Path);
public record AccessGroupDto(string Id, string Name, List<DoorDto> Doors, List<string> MemberIds);
public record AccessGroupUpdateRequest(string Id, string Name, List<string> MemberIds);
public record CreateAccessGroupRequest(string Name);
