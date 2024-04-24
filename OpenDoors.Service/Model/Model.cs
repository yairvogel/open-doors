
namespace OpenDoors.Model;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

class OpenDoorsContext(DbContextOptions options) : IdentityDbContext(options)
{
    public DbSet<Door> Doors { get; set; }
}

public class Door
{
    public int Id { get; set; }

    public string Location { get; set; }
}
