namespace itinerary_be.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using itinerary_be.Core.Domain.Entities;
using System.Reflection;
using itinerary_be.Core.Domain.Enums;

public class ItineraryDbContext(DbContextOptions<ItineraryDbContext> options) : DbContext(options)
{
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<ItineraryDay> ItineraryDays => Set<ItineraryDay>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<Lodging> Lodgings => Set<Lodging>();
    public DbSet<TripEvent> TripEvents => Set<TripEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("itinerary");

        modelBuilder.HasPostgresEnum<EventTypes>("event_type");

        base.OnModelCreating(modelBuilder);

        // Scans the current assembly and applies all IEntityTypeConfiguration implementations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}