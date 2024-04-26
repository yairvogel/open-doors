
namespace OpenDoors.Service;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
using OpenDoors.Model.Authentication;

public class OpenDoorsContext(DbContextOptions options) : IdentityDbContext<TenantUser, TenantRole, string>(options)
{
    public DbSet<Tenant> Tenants { get; set; } = null!;

    public DbSet<Door> Doors { get; set; } = null!;
}
