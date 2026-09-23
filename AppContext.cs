using Microsoft.EntityFrameworkCore;
using Api.Domain.Feed;
// using Microsoft.EntityFrameworkCore.Diagnostics;

public class AppContext : DbContext
{
    public DbSet<Feed> Feeds { get; set; }
    public DbSet<Item> Items { get; set; }

    public string DbPath { get; }

    public AppContext()
    {
        DbPath = Path.Combine(Directory.GetCurrentDirectory(), "app.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
        // Only sql commands executed
        // options.LogTo(Console.WriteLine, (eventId, logLevel) => eventId == RelationalEventId.CommandExecuted);
    } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>()
            .HasIndex(x => new { x.Id, x.Guid })
            .IsUnique();
    }
}