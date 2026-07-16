namespace itinerary_be.Modules.Utility.Models;

public record CalendarDay(
    DateOnly Date,
    IReadOnlyList<CalendarEntry> Entries);
