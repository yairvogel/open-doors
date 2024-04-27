namespace OpenDoors.Model.Authentication;

public class Tenant
{
    public Guid? Id { get; set; }

    public string? Name { get; set; }

    ICollection<TenantUser> Tenants { get; } = new List<TenantUser>();
}


