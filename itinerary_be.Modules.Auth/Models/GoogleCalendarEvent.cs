namespace itinerary_be.Modules.Auth.Models;

public record GoogleCalendarEvent(
    string Id,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End,
    bool IsAllDay,
    string? Description,
    string? Location);
