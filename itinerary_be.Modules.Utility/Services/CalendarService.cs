namespace itinerary_be.Modules.Utility.Services;

using Microsoft.Extensions.Logging;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Models;
using itinerary_be.Modules.Itinerary.Interfaces;
using itinerary_be.Modules.Utility.Interfaces;
using itinerary_be.Modules.Utility.Models;

public class CalendarService : ICalendarService
{
    private readonly IGoogleCalendarService _googleCalendarService;
    private readonly ITripService _tripService;
    private readonly ITripEventService _tripEventService;
    private readonly ILogger<CalendarService> _logger;

    public CalendarService(
        IGoogleCalendarService googleCalendarService,
        ITripService tripService,
        ITripEventService tripEventService,
        ILogger<CalendarService> logger)
    {
        _googleCalendarService = googleCalendarService;
        _tripService = tripService;
        _tripEventService = tripEventService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CalendarDay>> GetCalendarAsync(Guid userId, DateOnly startDate, DateOnly endDate, string timeZoneId)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("startDate must not be after endDate.", nameof(startDate));
        }

        var timeZone = ResolveTimeZone(timeZoneId);

        var googleEvents = await _googleCalendarService.GetEventsAsync(userId, startDate, endDate);

        var trips = await _tripService.GetAllTripsAsync(userId);
        var overlappingTrips = trips
            .Where(t => t.StartDate <= endDate && t.EndDate >= startDate)
            .ToList();

        var entriesByDate = new Dictionary<DateOnly, List<CalendarEntry>>();
        foreach (var date in EachDate(startDate, endDate))
        {
            entriesByDate[date] = new List<CalendarEntry>();
        }

        AddGoogleEventEntries(entriesByDate, googleEvents, timeZone);
        await AddTripEventEntriesAsync(entriesByDate, overlappingTrips, startDate, endDate, timeZone);
        AddTripRangeEntries(entriesByDate, overlappingTrips, startDate, endDate, timeZone);

        var result = EachDate(startDate, endDate)
            .Select(date => new CalendarDay(date, entriesByDate[date].OrderBy(e => e.Start).ToList()))
            .ToList();

        _logger.LogInformation(
            "Built calendar for user {UserId} between {StartDate} and {EndDate} in {TimeZoneId}",
            userId, startDate, endDate, timeZoneId);

        return result;
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new ArgumentException($"'{timeZoneId}' is not a recognized time zone id.", nameof(timeZoneId), ex);
        }
    }

    private static void AddGoogleEventEntries(
        Dictionary<DateOnly, List<CalendarEntry>> entriesByDate,
        IReadOnlyList<GoogleCalendarEvent> googleEvents,
        TimeZoneInfo timeZone)
    {
        foreach (var googleEvent in googleEvents)
        {
            var localDate = googleEvent.IsAllDay
                ? DateOnly.FromDateTime(googleEvent.Start.UtcDateTime)
                : DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(googleEvent.Start, timeZone).DateTime);

            if (!entriesByDate.TryGetValue(localDate, out var bucket))
            {
                continue;
            }

            bucket.Add(new CalendarEntry(
                googleEvent.Id,
                CalendarEntrySource.GoogleEvent,
                googleEvent.Title,
                googleEvent.Start,
                googleEvent.End,
                googleEvent.IsAllDay,
                googleEvent.Description,
                googleEvent.Location,
                TripId: null,
                TripEventId: null));
        }
    }

    private async Task AddTripEventEntriesAsync(
        Dictionary<DateOnly, List<CalendarEntry>> entriesByDate,
        List<Trip> overlappingTrips,
        DateOnly startDate,
        DateOnly endDate,
        TimeZoneInfo timeZone)
    {
        foreach (var trip in overlappingTrips)
        {
            var tripEvents = await _tripEventService.GetByTripIdAsync(trip.Id);

            foreach (var tripEvent in tripEvents)
            {
                if (tripEvent.Date < startDate || tripEvent.Date > endDate)
                {
                    continue;
                }

                var bucket = entriesByDate[tripEvent.Date];
                var start = ToLocalDateTimeOffset(tripEvent.Date, tripEvent.StartTime ?? TimeOnly.MinValue, timeZone);
                var end = tripEvent.EndTime.HasValue
                    ? ToLocalDateTimeOffset(tripEvent.Date, tripEvent.EndTime.Value, timeZone)
                    : start;

                bucket.Add(new CalendarEntry(
                    tripEvent.Id.ToString(),
                    CalendarEntrySource.TripEvent,
                    tripEvent.Title,
                    start,
                    end,
                    IsAllDay: !tripEvent.StartTime.HasValue,
                    tripEvent.Notes,
                    tripEvent.Location,
                    trip.Id,
                    tripEvent.Id));
            }
        }
    }

    private static void AddTripRangeEntries(
        Dictionary<DateOnly, List<CalendarEntry>> entriesByDate,
        List<Trip> overlappingTrips,
        DateOnly startDate,
        DateOnly endDate,
        TimeZoneInfo timeZone)
    {
        foreach (var trip in overlappingTrips)
        {
            var rangeStart = trip.StartDate > startDate ? trip.StartDate : startDate;
            var rangeEnd = trip.EndDate < endDate ? trip.EndDate : endDate;

            foreach (var date in EachDate(rangeStart, rangeEnd))
            {
                var dayStart = ToLocalDateTimeOffset(date, TimeOnly.MinValue, timeZone);

                entriesByDate[date].Add(new CalendarEntry(
                    $"trip-range-{trip.Id}",
                    CalendarEntrySource.TripRange,
                    trip.Title,
                    dayStart,
                    dayStart.AddDays(1),
                    IsAllDay: true,
                    trip.Description,
                    trip.Destination,
                    trip.Id,
                    TripEventId: null));
            }
        }
    }

    private static DateTimeOffset ToLocalDateTimeOffset(DateOnly date, TimeOnly time, TimeZoneInfo timeZone)
    {
        var local = date.ToDateTime(time);
        return new DateTimeOffset(local, timeZone.GetUtcOffset(local));
    }

    private static IEnumerable<DateOnly> EachDate(DateOnly start, DateOnly end)
    {
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            yield return date;
        }
    }
}
