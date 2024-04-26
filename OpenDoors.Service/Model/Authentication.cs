namespace OpenDoors.Model.Authentication;

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

public class Tenant
{
    public Guid? Id { get; set; }

    public string? Name { get; set; }

    ICollection<TenantUser> Tenants { get; } = new List<TenantUser>();
}

