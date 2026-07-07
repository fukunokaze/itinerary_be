using System.ComponentModel.DataAnnotations.Schema;
namespace itinerary_be.Core.Domain.Entities;

public class ItineraryDay
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("trip_id")]
    public Guid TripId { get; set; }
    [Column("date")]
    public DateOnly Date { get; set; }
    [Column("notes")]
    public string? Notes { get; set; }

    public Trip? Trip { get; set; }
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}