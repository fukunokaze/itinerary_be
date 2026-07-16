namespace itinerary_be.Modules.Auth.Services;

using Microsoft.Extensions.Logging;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Models;

public class GoogleCalendarService : IGoogleCalendarService
{
    private readonly IUserGoogleTokenService _userGoogleTokenService;
    private readonly IGoogleCalendarClient _googleCalendarClient;
    private readonly ILogger<GoogleCalendarService> _logger;

    public GoogleCalendarService(
        IUserGoogleTokenService userGoogleTokenService,
        IGoogleCalendarClient googleCalendarClient,
        ILogger<GoogleCalendarService> logger)
    {
        _userGoogleTokenService = userGoogleTokenService;
        _googleCalendarClient = googleCalendarClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<GoogleCalendarEvent>> GetEventsAsync(Guid userId, DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("startDate must not be after endDate.", nameof(startDate));
        }

        var accessToken = await _userGoogleTokenService.GetValidAccessTokenAsync(userId);
        var events = await _googleCalendarClient.GetEventsAsync(accessToken, startDate, endDate);

        _logger.LogInformation(
            "Fetched {Count} Google Calendar events for user {UserId} between {StartDate} and {EndDate}",
            events.Count, userId, startDate, endDate);

        return events;
    }
}
