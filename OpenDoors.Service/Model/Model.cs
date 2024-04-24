
namespace OpenDoors.Model;

using Microsoft.EntityFrameworkCore;

public class OpenDoorsContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public OpenDoorsContext(DbContextOptions options)
        : base(options)
    {
    }
}

public class User
{
    public int Id { get; set; }

    public string Name { get; set; }
}
