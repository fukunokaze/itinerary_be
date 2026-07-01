namespace itinerary_be.Core.Domain.Entities;

public class ItineraryDay
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }

    public Trip? Trip { get; set; }
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}