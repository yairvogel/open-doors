using Microsoft.AspNetCore.Authorization;
using OpenDoors.Model;
using OpenDoors.Service.DbOperations;

namespace OpenDoors.Service.Authorization;

public class AllowedEntryAuthorizationHandler(OpenDoorsContext dbContext) : AuthorizationHandler<AllowedEntryAuthorizationRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AllowedEntryAuthorizationRequirement requirement)
    {
        string userId = context.User.GetUserId();
        if (context.Resource is not int doorId)
        {
            context.Fail(new AuthorizationFailureReason(this, "resource received is not a doorId"));
            return;
        }

        if (!await IsAllowedEntry(doorId, userId))
        {
            context.Fail(new AuthorizationFailureReason(this, "user is not allowed to access the door"));
            return;
        }

        context.Succeed(requirement);
    }

    private async Task<bool> IsAllowedEntry(int doorId, string userId)
    {
        IReadOnlyList<Door> doors = await dbContext.ListDoorsForUser(userId);
        return doors.Any(d => d.Id == doorId);
    }
}

public class AllowedEntryAuthorizationRequirement(int doorId) : IAuthorizationRequirement
{
    public int DoorId => doorId;
}
