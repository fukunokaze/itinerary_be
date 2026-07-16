namespace itinerary_be.Modules.Auth.Services;

using System.Net;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using itinerary_be.Modules.Auth.Exceptions;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Models;

public class GoogleCalendarClient : IGoogleCalendarClient
{
    private const string ApplicationName = "itinerary_be";
    private const string CalendarId = "primary";

    private readonly ILogger<GoogleCalendarClient> _logger;
    private readonly Google.Apis.Http.IHttpClientFactory? _httpClientFactory;

    public GoogleCalendarClient(ILogger<GoogleCalendarClient> logger, Google.Apis.Http.IHttpClientFactory? httpClientFactory = null)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<GoogleCalendarEvent>> GetEventsAsync(string accessToken, DateOnly startDate, DateOnly endDate)
    {
        try
        {
            var initializer = new BaseClientService.Initializer
            {
                ApplicationName = ApplicationName,
                HttpClientInitializer = GoogleCredential.FromAccessToken(accessToken)
            };

            if (_httpClientFactory != null)
            {
                initializer.HttpClientFactory = _httpClientFactory;
            }

            using var calendarService = new CalendarService(initializer);

            var request = calendarService.Events.List(CalendarId);
            request.TimeMinDateTimeOffset = ToStartOfDayUtc(startDate);
            request.TimeMaxDateTimeOffset = ToStartOfDayUtc(endDate.AddDays(1));
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var events = await request.ExecuteAsync();

            return (events.Items ?? new List<Event>())
                .Where(e => e.Status != "cancelled")
                .Select(MapEvent)
                .ToList();
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning(ex, "Google Calendar API rejected the access token as unauthorized.");
            throw new GoogleReauthorizationRequiredException("Google Calendar access token was rejected as unauthorized.", ex);
        }
        catch (Exception ex) when (ex is GoogleApiException or HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Google Calendar events request failed.");
            throw new GoogleCalendarApiException("Failed to fetch Google Calendar events.", ex);
        }
    }

    private static GoogleCalendarEvent MapEvent(Event calendarEvent)
    {
        var isAllDay = calendarEvent.Start?.DateTimeDateTimeOffset is null;

        var start = calendarEvent.Start?.DateTimeDateTimeOffset
            ?? ParseAllDayDate(calendarEvent.Start?.Date);
        var end = calendarEvent.End?.DateTimeDateTimeOffset
            ?? ParseAllDayDate(calendarEvent.End?.Date)
            ?? start;

        return new GoogleCalendarEvent(
            calendarEvent.Id ?? string.Empty,
            calendarEvent.Summary ?? "(no title)",
            start ?? DateTimeOffset.MinValue,
            end ?? DateTimeOffset.MinValue,
            isAllDay,
            calendarEvent.Description,
            calendarEvent.Location);
    }

    private static DateTimeOffset? ParseAllDayDate(string? date) =>
        DateOnly.TryParse(date, out var parsed) ? ToStartOfDayUtc(parsed) : null;

    private static DateTimeOffset ToStartOfDayUtc(DateOnly date) =>
        new(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
}
