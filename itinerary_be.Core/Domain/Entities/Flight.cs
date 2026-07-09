using System.ComponentModel.DataAnnotations.Schema;
namespace itinerary_be.Core.Domain.Entities;

public class Flight
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("trip_id")]
    public Guid TripId { get; set; }
    [Column("flight_number")]
    public required string FlightNumber { get; set; }

    // DateTimeOffset is highly recommended for PostgreSQL timezone handling
    [Column("departure_time")]
    public DateTimeOffset DepartureTime { get; set; }
    
    [Column("arrival_time")]
    public DateTimeOffset ArrivalTime { get; set; }

    [Column("airline")]
    public string? Airline { get; set; }

    [Column("seat")]
    public string? Seat { get; set; }

    [Column("confirmation_code")]
    public string? ConfirmationCode { get; set; }

    [Column("route")]
    public string? Route { get; set; }

    public Trip? Trip { get; set; }
}