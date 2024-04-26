using Microsoft.AspNetCore.Authorization;

namespace OpenDoors.Service.Authorization;

public class AllowedEntryHandler(ILogger<AllowedEntryHandler> logger) : AuthorizationHandler<AllowedEntryRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AllowedEntryRequirement requirement)
    {
        var tenantId = context.User.GetTenantId();
        var userId = context.User.GetUserId();
        logger.LogInformation($"got user {userId} on tenant {tenantId}");
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public class AllowedEntryRequirement : IAuthorizationRequirement {}
