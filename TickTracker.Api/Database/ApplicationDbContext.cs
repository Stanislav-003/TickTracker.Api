using Microsoft.EntityFrameworkCore;
using TickTracker.Api.DatabaseConfigurations;
using TickTracker.Api.Entities;

namespace TickTracker.Api.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AssetConfiguration());
    }

    public DbSet<Asset> Assets { get; set; }
}
