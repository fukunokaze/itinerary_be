namespace itinerary_be.Modules.Auth.Interfaces;

using itinerary_be.Modules.Auth.Models;

public interface IGoogleCalendarClient
{
    /// <exception cref="itinerary_be.Modules.Auth.Exceptions.GoogleReauthorizationRequiredException">
    /// Google rejected the access token as unauthorized.
    /// </exception>
    /// <exception cref="itinerary_be.Modules.Auth.Exceptions.GoogleCalendarApiException">
    /// The Google Calendar API call failed for any other reason.
    /// </exception>
    Task<IReadOnlyList<GoogleCalendarEvent>> GetEventsAsync(string accessToken, DateOnly startDate, DateOnly endDate);
}
