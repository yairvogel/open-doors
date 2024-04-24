
namespace OpenDoors.Model;

using System;
using Microsoft.EntityFrameworkCore;

public class OpenDoorsContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public string DbPath { get; }

    public OpenDoorsContext()
    {
        var localAppData = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(localAppData);
        DbPath = Path.Join(path, "opendoors.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}
