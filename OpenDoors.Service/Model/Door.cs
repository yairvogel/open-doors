namespace OpenDoors.Model;

public class Door
{
    public int Id { get; set; }

    public required string Location { get; set; }

    public required ICollection<AccessGroup> AccessGroups { get; set; } = [];
}

