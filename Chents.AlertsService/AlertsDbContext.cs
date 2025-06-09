using Chents.Models;
using Chents.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Chents.AlertsService;

public class AlertsDbContext : DbContext
{
    public AlertsDbContext(DbContextOptions<AlertsDbContext> options) : base(options)
    {
    }

    public DbSet<Alert> Alerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Severity).IsRequired();
        });
    }
}