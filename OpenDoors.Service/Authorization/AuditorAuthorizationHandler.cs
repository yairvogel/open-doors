using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using static OpenDoors.Service.Authorization.AuthorizationConstants;

namespace OpenDoors.Service.Authorization;

public class AuditorAuthorizationHandler(ILogger<AuditorAuthorizationHandler> logger) : AuthorizationHandler<AuditorRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuditorRequirement requirement)
    {
        bool hasAccess = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Any(c => c.Value.Equals(AdminRole, StringComparison.OrdinalIgnoreCase) || c.Value.Equals(AuditorRole, StringComparison.OrdinalIgnoreCase));

        logger.LogInformation($"hasAccess: {hasAccess}");

        if (hasAccess)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        context.Fail();
        return Task.CompletedTask;
    }
}

public class AuditorRequirement : IAuthorizationRequirement {}
