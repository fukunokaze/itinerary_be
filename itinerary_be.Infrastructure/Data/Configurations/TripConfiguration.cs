namespace itinerary_be.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Core.Domain.Enums;

public class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("trips");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
               .IsRequired()
               .HasMaxLength(200);

        // Relationships
        // builder.HasMany(t => t.ItineraryDays)
        //        .WithOne(d => d.Trip)
        //        .HasForeignKey(d => d.TripId)
        //        .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(t => t.TripEvents)
               .WithOne(e => e.Trip)
               .HasForeignKey(e => e.TripId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Flights)
               .WithOne(f => f.Trip)
               .HasForeignKey(f => f.TripId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Lodgings)
               .WithOne(l => l.Trip)
               .HasForeignKey(l => l.TripId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ItineraryDayConfiguration : IEntityTypeConfiguration<ItineraryDay>
{
    public void Configure(EntityTypeBuilder<ItineraryDay> builder)
    {
        builder.ToTable("itinerary_days");

        builder.HasKey(d => d.Id);

        // Ensure we don't have duplicate dates for a single trip
        builder.HasIndex(d => new { d.TripId, d.Date }).IsUnique();

        builder.Property(d => d.Notes)
               .HasMaxLength(1000);

        builder.HasMany(d => d.Activities)
               .WithOne(a => a.ItineraryDay)
               .HasForeignKey(a => a.ItineraryDayId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("activities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(a => a.Location)
               .HasMaxLength(300);
    }
}

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.ToTable("flights");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FlightNumber)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(f => f.Airline)
               .HasMaxLength(100);

        builder.Property(f => f.Seat)
               .HasMaxLength(20);

        builder.Property(f => f.ConfirmationCode)
               .HasMaxLength(50);
    }
}

public class LodgingConfiguration : IEntityTypeConfiguration<Lodging>
{
    public void Configure(EntityTypeBuilder<Lodging> builder)
    {
        builder.ToTable("lodgings");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
               .IsRequired()
               .HasMaxLength(200);
    }
}

public class TripEventConfiguration : IEntityTypeConfiguration<TripEvent>
{
    public void Configure(EntityTypeBuilder<TripEvent> builder)
    {
        builder.ToTable("trip_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type)
               .IsRequired();

        // Relationships
        builder.HasOne(e => e.Trip)
               .WithMany(t => t.TripEvents)
               .HasForeignKey(e => e.TripId)
               .OnDelete(DeleteBehavior.Cascade);

    }
}