
namespace OpenDoors.Service;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
using OpenDoors.Model.Authentication;

public class OpenDoorsContext(DbContextOptions options) : IdentityDbContext<TenantUser, TenantRole, string>(options)
{
    public DbSet<Tenant> Tenants { get; set; } = null!;

    public DbSet<AccessGroup> AccessGroups { get; set; } = null!;

    public DbSet<Door> Doors { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<TenantUser>()
            .HasMany(u => u.AccessGroups)
            .WithMany(g => g.Members)
            .UsingEntity(j => j.ToTable("AccessGroupMemberships"));

        builder.Entity<Door>()
            .HasMany(d => d.AccessGroups)
            .WithMany(g => g.Doors)
            .UsingEntity(j => j.ToTable("AccessGroupDoors"));
    }
}
