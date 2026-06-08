using Microsoft.EntityFrameworkCore;
using WeatherApp.Api.Models;

namespace WeatherApp.Api.Data;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options) { }

    public DbSet<City> Cities => Set<City>();
    public DbSet<TemperatureRecord> TemperatureRecords => Set<TemperatureRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Latitude).HasPrecision(9, 6);
            e.Property(c => c.Longitude).HasPrecision(9, 6);
            e.HasIndex(c => c.Name);
            e.HasIndex(c => new { c.Latitude, c.Longitude });
        });

        modelBuilder.Entity<TemperatureRecord>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Temperature).HasPrecision(6, 2).IsRequired();
            e.Property(r => r.FeelsLike).HasPrecision(6, 2);
            e.Property(r => r.Description).HasMaxLength(500);
            e.Property(r => r.Provider).HasMaxLength(100);
            e.HasOne(r => r.City)
             .WithMany(c => c.TemperatureRecords)
             .HasForeignKey(r => r.CityId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(r => r.RecordedAt);
            e.HasIndex(r => new { r.CityId, r.RecordedAt });
        });
    }
}
