using System.ComponentModel.DataAnnotations.Schema;

namespace itinerary_be.Core.Domain.Entities;
public class Trip
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("title")]
    public required string Title { get; set; }
    [Column("start_date")]
    public DateOnly StartDate { get; set; }
    [Column("end_date")]
    public DateOnly EndDate { get; set; }
    [Column("destination")]
    public string Destination { get; set; } = string.Empty;
    [Column("description")]
    public string? Description { get; set; } = string.Empty;
    [Column("user_id")]
    public Guid UserId { get; set; }

    // Navigation Properties
    // public ICollection<ItineraryDay> ItineraryDays { get; set; } = new List<ItineraryDay>();
    public User? User { get; set; }
    public ICollection<Flight> Flights { get; set; } = new List<Flight>();
    public ICollection<Lodging> Lodgings { get; set; } = new List<Lodging>();
    public ICollection<TripEvent> TripEvents { get; set; } = new List<TripEvent>();
}
