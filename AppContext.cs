using Microsoft.EntityFrameworkCore;
using Api.Domain.Feed;

public class AppContext : DbContext
{
    public DbSet<Feed> Feeds { get; set; }

    public string DbPath { get; }

    public AppContext()
    {
        DbPath = Path.Combine(Directory.GetCurrentDirectory(), "app.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}