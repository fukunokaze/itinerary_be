namespace itinerary_be.UnitTests;

using Microsoft.Extensions.Logging;
using Moq;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Core.Domain.Enums;
using itinerary_be.Modules.Auth.Exceptions;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Models;
using itinerary_be.Modules.Itinerary.Interfaces;
using itinerary_be.Modules.Utility.Models;
using itinerary_be.Modules.Utility.Services;

/// <summary>
/// Unit tests for CalendarService class
/// </summary>
public class CalendarServiceTests
{
    private const string TokyoTimeZoneId = "Asia/Tokyo";
    private const string HonoluluTimeZoneId = "Pacific/Honolulu";

    private readonly Mock<IGoogleCalendarService> _mockGoogleCalendarService;
    private readonly Mock<ITripService> _mockTripService;
    private readonly Mock<ITripEventService> _mockTripEventService;
    private readonly Mock<ILogger<CalendarService>> _mockLogger;
    private readonly CalendarService _service;

    public CalendarServiceTests()
    {
        _mockGoogleCalendarService = new Mock<IGoogleCalendarService>();
        _mockTripService = new Mock<ITripService>();
        _mockTripEventService = new Mock<ITripEventService>();
        _mockLogger = new Mock<ILogger<CalendarService>>();
        _service = new CalendarService(
            _mockGoogleCalendarService.Object,
            _mockTripService.Object,
            _mockTripEventService.Object,
            _mockLogger.Object);
    }

    #region GetCalendarAsync Tests

    [Fact]
    public async Task GetCalendarAsync_ValidRange_MergesAllThreeEntrySources()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 8, 1);
        var endDate = new DateOnly(2026, 8, 3);

        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Title = "Japan Trip",
            StartDate = new DateOnly(2026, 8, 2),
            EndDate = new DateOnly(2026, 8, 4),
            Destination = "Tokyo",
            UserId = userId
        };

        var tripEvent = new TripEvent
        {
            Id = Guid.NewGuid(),
            TripId = trip.Id,
            Type = EventTypes.activity,
            Title = "Museum Visit",
            Date = new DateOnly(2026, 8, 2),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(12, 0)
        };

        var googleEvent = new GoogleCalendarEvent(
            "evt1", "Dentist", new DateTimeOffset(2026, 8, 1, 1, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 8, 1, 2, 0, 0, TimeSpan.Zero), false, null, null);

        _mockGoogleCalendarService.Setup(s => s.GetEventsAsync(userId, startDate, endDate))
            .ReturnsAsync(new List<GoogleCalendarEvent> { googleEvent });
        _mockTripService.Setup(s => s.GetAllTripsAsync(userId)).ReturnsAsync(new List<Trip> { trip });
        _mockTripEventService.Setup(s => s.GetByTripIdAsync(trip.Id)).ReturnsAsync(new List<TripEvent> { tripEvent });

        // Act
        var result = await _service.GetCalendarAsync(userId, startDate, endDate, TokyoTimeZoneId);

        // Assert
        Assert.Equal(3, result.Count);

        var day1 = result.Single(d => d.Date == new DateOnly(2026, 8, 1));
        Assert.Single(day1.Entries);
        Assert.Equal(CalendarEntrySource.GoogleEvent, day1.Entries[0].Source);

        var day2 = result.Single(d => d.Date == new DateOnly(2026, 8, 2));
        Assert.Equal(2, day2.Entries.Count);
        Assert.Contains(day2.Entries, e => e.Source == CalendarEntrySource.TripEvent && e.TripEventId == tripEvent.Id);
        Assert.Contains(day2.Entries, e => e.Source == CalendarEntrySource.TripRange && e.TripId == trip.Id);

        var day3 = result.Single(d => d.Date == new DateOnly(2026, 8, 3));
        Assert.Single(day3.Entries);
        Assert.Equal(CalendarEntrySource.TripRange, day3.Entries[0].Source);
    }

    [Fact]
    public async Task GetCalendarAsync_DayWithNoActivity_StillPresentWithEmptyEntries()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 8, 1);
        var endDate = new DateOnly(2026, 8, 2);

        _mockGoogleCalendarService.Setup(s => s.GetEventsAsync(userId, startDate, endDate))
            .ReturnsAsync(new List<GoogleCalendarEvent>());
        _mockTripService.Setup(s => s.GetAllTripsAsync(userId)).ReturnsAsync(new List<Trip>());

        // Act
        var result = await _service.GetCalendarAsync(userId, startDate, endDate, TokyoTimeZoneId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, day => Assert.Empty(day.Entries));
    }

    [Fact]
    public async Task GetCalendarAsync_StartDateAfterEndDate_ThrowsArgumentExceptionWithoutCallingDependencies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 8, 5);
        var endDate = new DateOnly(2026, 8, 1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetCalendarAsync(userId, startDate, endDate, TokyoTimeZoneId));
        _mockGoogleCalendarService.Verify(s => s.GetEventsAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Never);
        _mockTripService.Verify(s => s.GetAllTripsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetCalendarAsync_UnrecognizedTimeZoneId_ThrowsArgumentExceptionWithoutCallingDependencies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 8, 1);
        var endDate = new DateOnly(2026, 8, 5);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetCalendarAsync(userId, startDate, endDate, "Not/A_Real_Zone"));
        _mockGoogleCalendarService.Verify(s => s.GetEventsAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Never);
        _mockTripService.Verify(s => s.GetAllTripsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetCalendarAsync_AllDayGoogleEvent_BucketsByUtcDateRegardlessOfTimeZone()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 7, 31);
        var endDate = new DateOnly(2026, 8, 1);

        // UTC midnight of Aug 1st; Honolulu is UTC-10, so a naive conversion would shift this to Jul 31st.
        var allDayEvent = new GoogleCalendarEvent(
            "evt-allday", "Birthday", new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 8, 2, 0, 0, 0, TimeSpan.Zero), true, null, null);

        _mockGoogleCalendarService.Setup(s => s.GetEventsAsync(userId, startDate, endDate))
            .ReturnsAsync(new List<GoogleCalendarEvent> { allDayEvent });
        _mockTripService.Setup(s => s.GetAllTripsAsync(userId)).ReturnsAsync(new List<Trip>());

        // Act
        var result = await _service.GetCalendarAsync(userId, startDate, endDate, HonoluluTimeZoneId);

        // Assert
        var day1 = result.Single(d => d.Date == new DateOnly(2026, 7, 31));
        Assert.Empty(day1.Entries);

        var day2 = result.Single(d => d.Date == new DateOnly(2026, 8, 1));
        Assert.Single(day2.Entries);
        Assert.Equal("evt-allday", day2.Entries[0].Id);
    }

    [Fact]
    public async Task GetCalendarAsync_TimedGoogleEvent_BucketsByConvertedLocalDate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 8, 1);
        var endDate = new DateOnly(2026, 8, 2);

        // 20:00 UTC on Aug 1st is 05:00 the next day in Tokyo (UTC+9).
        var timedEvent = new GoogleCalendarEvent(
            "evt-timed", "Late Call", new DateTimeOffset(2026, 8, 1, 20, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 8, 1, 21, 0, 0, TimeSpan.Zero), false, null, null);

        _mockGoogleCalendarService.Setup(s => s.GetEventsAsync(userId, startDate, endDate))
            .ReturnsAsync(new List<GoogleCalendarEvent> { timedEvent });
        _mockTripService.Setup(s => s.GetAllTripsAsync(userId)).ReturnsAsync(new List<Trip>());

        // Act
        var result = await _service.GetCalendarAsync(userId, startDate, endDate, TokyoTimeZoneId);

        // Assert
        var day1 = result.Single(d => d.Date == new DateOnly(2026, 8, 1));
        Assert.Empty(day1.Entries);

        var day2 = result.Single(d => d.Date == new DateOnly(2026, 8, 2));
        Assert.Single(day2.Entries);
        Assert.Equal("evt-timed", day2.Entries[0].Id);
    }

    [Fact]
    public async Task GetCalendarAsync_TripRange_PresentForEveryDayInRangeClippedToQueryRange()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 8, 1);
        var endDate = new DateOnly(2026, 8, 5);

        // Trip extends beyond both ends of the query range.
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Title = "Long Trip",
            StartDate = new DateOnly(2026, 7, 20),
            EndDate = new DateOnly(2026, 8, 10),
            Destination = "Bali",
            UserId = userId
        };

        _mockGoogleCalendarService.Setup(s => s.GetEventsAsync(userId, startDate, endDate))
            .ReturnsAsync(new List<GoogleCalendarEvent>());
        _mockTripService.Setup(s => s.GetAllTripsAsync(userId)).ReturnsAsync(new List<Trip> { trip });
        _mockTripEventService.Setup(s => s.GetByTripIdAsync(trip.Id)).ReturnsAsync(new List<TripEvent>());

        // Act
        var result = await _service.GetCalendarAsync(userId, startDate, endDate, TokyoTimeZoneId);

        // Assert
        Assert.Equal(5, result.Count);
        Assert.All(result, day =>
        {
            Assert.Single(day.Entries);
            Assert.Equal(CalendarEntrySource.TripRange, day.Entries[0].Source);
            Assert.Equal(trip.Id, day.Entries[0].TripId);
        });
    }

    [Fact]
    public async Task GetCalendarAsync_GoogleReauthorizationRequired_PropagatesWithoutCallingTripServices()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 8, 1);
        var endDate = new DateOnly(2026, 8, 5);

        _mockGoogleCalendarService.Setup(s => s.GetEventsAsync(userId, startDate, endDate))
            .ThrowsAsync(new GoogleReauthorizationRequiredException("No Google authorization found for user."));

        // Act & Assert
        await Assert.ThrowsAsync<GoogleReauthorizationRequiredException>(
            () => _service.GetCalendarAsync(userId, startDate, endDate, TokyoTimeZoneId));
        _mockTripService.Verify(s => s.GetAllTripsAsync(It.IsAny<Guid>()), Times.Never);
        _mockTripEventService.Verify(s => s.GetByTripIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetCalendarAsync_GoogleCalendarApiException_Propagates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 8, 1);
        var endDate = new DateOnly(2026, 8, 5);

        _mockGoogleCalendarService.Setup(s => s.GetEventsAsync(userId, startDate, endDate))
            .ThrowsAsync(new GoogleCalendarApiException("Failed to fetch Google Calendar events."));

        // Act & Assert
        await Assert.ThrowsAsync<GoogleCalendarApiException>(
            () => _service.GetCalendarAsync(userId, startDate, endDate, TokyoTimeZoneId));
        _mockTripService.Verify(s => s.GetAllTripsAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion
}
