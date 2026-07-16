namespace itinerary_be.Modules.Utility.Models;

public record CalendarEntry(
    string Id,
    CalendarEntrySource Source,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End,
    bool IsAllDay,
    string? Description,
    string? Location,
    Guid? TripId,
    Guid? TripEventId);
