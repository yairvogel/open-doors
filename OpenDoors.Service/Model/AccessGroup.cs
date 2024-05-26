
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenDoors.Model;

public class AccessGroup
{
    public Guid? Id { get; set; }

    public required string Name { get; set; }

    public required ICollection<Door> Doors { get; set; } = [];

    public required ICollection<TenantUser> Members { get; set; } = [];

    public required Tenant Tenant { get; set; }

    [ForeignKey("Tenant")]
    public Guid? TenantId { get; set; }

}
