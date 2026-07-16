namespace itinerary_be.Modules.Auth.Interfaces;

using itinerary_be.Modules.Auth.Models;

public interface IGoogleCalendarService
{
    /// <exception cref="itinerary_be.Modules.Auth.Exceptions.GoogleReauthorizationRequiredException">
    /// No valid Google access token could be obtained for the user.
    /// </exception>
    /// <exception cref="itinerary_be.Modules.Auth.Exceptions.GoogleCalendarApiException">
    /// The Google Calendar API call failed for any other reason.
    /// </exception>
    Task<IReadOnlyList<GoogleCalendarEvent>> GetEventsAsync(Guid userId, DateOnly startDate, DateOnly endDate);
}
