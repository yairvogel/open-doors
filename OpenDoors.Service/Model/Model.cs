
namespace OpenDoors.Model;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Model.Authentication;

public class OpenDoorsContext(DbContextOptions options) : IdentityDbContext<TenantUser>(options)
{
    public DbSet<Tenant> Tenants { get; set; } = null!;

    public DbSet<Door> Doors { get; set; } = null!;
}

public class Door
{
    public int Id { get; set; }

    public string? Location { get; set; }
}
