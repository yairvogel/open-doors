namespace OpenDoors.Model.Authentication;

using Microsoft.AspNetCore.Identity;

public class TenantUser : IdentityUser
{
    public required Tenant Tenant { get; set; }
}

public class Tenant
{
    public int Id { get; set; }

    public string? Name { get; set; }
}
