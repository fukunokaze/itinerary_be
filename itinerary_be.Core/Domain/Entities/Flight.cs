namespace itinerary_be.Core.Domain.Entities;

public class Flight
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public required string FlightNumber { get; set; }

    // DateTimeOffset is highly recommended for PostgreSQL timezone handling
    public DateTimeOffset DepartureTime { get; set; }
    public DateTimeOffset ArrivalTime { get; set; }

    public Trip? Trip { get; set; }
}