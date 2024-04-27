using System.ComponentModel.DataAnnotations.Schema;
using OpenDoors.Model.Authentication;

namespace OpenDoors.Model;

public class EntryLog
{
    public int Id { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public required int DoorId { get; set; }

    public required string UserId { get; set; } = null!;

    public bool Success { get; set; }

    public string? FailureReason { get; set; }

    public Tenant Tenant { get; set; } = null!;

    [ForeignKey(nameof(Tenant))]
    public Guid? TenantId { get; set; } = null!;
}
