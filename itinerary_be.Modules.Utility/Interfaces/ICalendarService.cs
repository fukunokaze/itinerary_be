namespace itinerary_be.Modules.Utility.Interfaces;

using itinerary_be.Modules.Utility.Models;

public interface ICalendarService
{
    /// <exception cref="ArgumentException">
    /// <paramref name="startDate"/> is after <paramref name="endDate"/>, or <paramref name="timeZoneId"/> does not resolve to a known time zone.
    /// </exception>
    /// <exception cref="itinerary_be.Modules.Auth.Exceptions.GoogleReauthorizationRequiredException">
    /// No valid Google access token could be obtained for the user.
    /// </exception>
    /// <exception cref="itinerary_be.Modules.Auth.Exceptions.GoogleCalendarApiException">
    /// The Google Calendar API call failed for any other reason.
    /// </exception>
    Task<IReadOnlyList<CalendarDay>> GetCalendarAsync(Guid userId, DateOnly startDate, DateOnly endDate, string timeZoneId);
}
