
using OpenDoors.Model.Authentication;

namespace OpenDoors.Model;

public class AccessGroup
{
    public Guid? Id { get; set; }

    public required string Name { get; set; }

    public required ICollection<Door> Doors { get; set; } = [];

    public required ICollection<TenantUser> Members { get; set; } = [];

    public required Tenant Tenant { get; set; }
}
