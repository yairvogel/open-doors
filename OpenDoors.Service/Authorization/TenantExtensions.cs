using System.Security.Claims;

namespace OpenDoors.Service.Authorization;

public static class UserExtensions
{
    public static Guid GetTenantId(this ClaimsPrincipal user)
    {
        return Guid.Parse(user.FindFirst(AuthorizationConstants.TenantClaimType)!.Value);
    }

    public static string GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)!.Value;
    }
}
