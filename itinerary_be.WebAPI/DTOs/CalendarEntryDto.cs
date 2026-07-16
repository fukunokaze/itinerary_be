namespace itinerary_be.WebAPI.DTOs;

public class CalendarEntryDto
{
    public string Id { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public DateTimeOffset Start { get; set; }

    public DateTimeOffset End { get; set; }

    public bool IsAllDay { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public Guid? TripId { get; set; }

    public Guid? TripEventId { get; set; }
}
