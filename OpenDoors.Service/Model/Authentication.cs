namespace OpenDoors.Model;

using Microsoft.AspNetCore.Identity;

public class TenantUser : IdentityUser
{
    public required Tenant Tenant { get; set; } = null!;

    public ICollection<AccessGroup> AccessGroups { get; set; } = [];
}

public class TenantRole : IdentityRole
{
    public required Tenant Tenant { get; set; } = null!;
}

