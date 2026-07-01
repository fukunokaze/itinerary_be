namespace itinerary_be.Core.Domain.Entities;
public class Trip
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    // Navigation Properties
    public ICollection<ItineraryDay> ItineraryDays { get; set; } = new List<ItineraryDay>();
    public ICollection<Flight> Flights { get; set; } = new List<Flight>();
    public ICollection<Lodging> Lodgings { get; set; } = new List<Lodging>();
}
