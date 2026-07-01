namespace itinerary_be.Core.Domain.Entities;

public class Activity
{
    public Guid Id { get; set; }
    public Guid ItineraryDayId { get; set; }
    public required string Name { get; set; }
    public TimeOnly? StartTime { get; set; }
    public string? Location { get; set; }

    public ItineraryDay? ItineraryDay { get; set; }
}