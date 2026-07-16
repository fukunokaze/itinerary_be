namespace itinerary_be.UnitTests;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Auth.Exceptions;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Utility.Interfaces;
using itinerary_be.Modules.Utility.Models;
using itinerary_be.WebAPI.Controllers;
using itinerary_be.WebAPI.DTOs;

/// <summary>
/// Unit tests for CalendarController class
/// </summary>
public class CalendarControllerTests
{
    private const string TimeZoneId = "Asia/Tokyo";
    private const string UserEmail = "traveler@example.com";

    private readonly Mock<ICalendarService> _mockCalendarService;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<CalendarController>> _mockLogger;
    private readonly CalendarController _controller;
    private readonly User _currentUser;

    public CalendarControllerTests()
    {
        _mockCalendarService = new Mock<ICalendarService>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<CalendarController>>();
        _controller = new CalendarController(_mockCalendarService.Object, _mockUserRepository.Object, _mockLogger.Object);

        _currentUser = new User { Id = Guid.NewGuid(), Email = UserEmail, Name = "Traveler" };
        SetAuthenticatedUser(UserEmail);
        _mockUserRepository.Setup(r => r.GetByEmailAsync(UserEmail)).ReturnsAsync(_currentUser);
    }

    private void SetAuthenticatedUser(string? email)
    {
        var claims = new List<Claim>();
        if (email != null)
        {
            claims.Add(new Claim(ClaimTypes.Email, email));
        }

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    #region GetMonth Tests

    [Fact]
    public async Task GetMonth_ValidRequest_ReturnsOkWithDayMarkers()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var days = new List<CalendarDay>
        {
            new CalendarDay(new DateOnly(2026, 8, 1), new List<CalendarEntry>()),
            new CalendarDay(new DateOnly(2026, 8, 2), new List<CalendarEntry>
            {
                new CalendarEntry("evt1", CalendarEntrySource.GoogleEvent, "Dentist",
                    new DateTimeOffset(2026, 8, 2, 9, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2026, 8, 2, 10, 0, 0, TimeSpan.Zero),
                    false, null, null, null, null)
            }),
            new CalendarDay(new DateOnly(2026, 8, 3), new List<CalendarEntry>
            {
                new CalendarEntry($"trip-range-{tripId}", CalendarEntrySource.TripRange, "Bali Trip",
                    new DateTimeOffset(2026, 8, 3, 0, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2026, 8, 4, 0, 0, 0, TimeSpan.Zero),
                    true, null, null, tripId, null)
            })
        };

        _mockCalendarService
            .Setup(s => s.GetCalendarAsync(_currentUser.Id, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31), TimeZoneId))
            .ReturnsAsync(days);

        // Act
        var result = await _controller.GetMonth(2026, 8, TimeZoneId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var markers = Assert.IsAssignableFrom<IEnumerable<CalendarDayMarkerDto>>(okResult.Value).ToList();

        Assert.Equal(3, markers.Count);

        var day1 = markers.Single(d => d.Date == new DateOnly(2026, 8, 1));
        Assert.False(day1.HasEvents);
        Assert.False(day1.IsTripDay);

        var day2 = markers.Single(d => d.Date == new DateOnly(2026, 8, 2));
        Assert.True(day2.HasEvents);
        Assert.False(day2.IsTripDay);

        var day3 = markers.Single(d => d.Date == new DateOnly(2026, 8, 3));
        Assert.False(day3.HasEvents);
        Assert.True(day3.IsTripDay);
    }

    [Fact]
    public async Task GetMonth_UserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        SetAuthenticatedUser(null);

        // Act
        var result = await _controller.GetMonth(2026, 8, TimeZoneId);

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
        _mockCalendarService.Verify(s => s.GetCalendarAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public async Task GetMonth_MonthOutOfRange_ReturnsBadRequest(int month)
    {
        // Act
        var result = await _controller.GetMonth(2026, month, TimeZoneId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockCalendarService.Verify(s => s.GetCalendarAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetMonth_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        _mockCalendarService
            .Setup(s => s.GetCalendarAsync(_currentUser.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), TimeZoneId))
            .ThrowsAsync(new ArgumentException("'Not/A_Real_Zone' is not a recognized time zone id."));

        // Act
        var result = await _controller.GetMonth(2026, 8, TimeZoneId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetMonth_ServiceThrowsGoogleReauthorizationRequired_ReturnsUnauthorized()
    {
        // Arrange
        _mockCalendarService
            .Setup(s => s.GetCalendarAsync(_currentUser.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), TimeZoneId))
            .ThrowsAsync(new GoogleReauthorizationRequiredException("No Google authorization found for user."));

        // Act
        var result = await _controller.GetMonth(2026, 8, TimeZoneId);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetMonth_ServiceThrowsGoogleCalendarApiException_ReturnsBadGateway()
    {
        // Arrange
        _mockCalendarService
            .Setup(s => s.GetCalendarAsync(_currentUser.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), TimeZoneId))
            .ThrowsAsync(new GoogleCalendarApiException("Failed to fetch Google Calendar events."));

        // Act
        var result = await _controller.GetMonth(2026, 8, TimeZoneId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status502BadGateway, objectResult.StatusCode);
    }

    #endregion

    #region GetDay Tests

    [Fact]
    public async Task GetDay_ValidRequest_ReturnsOkWithDayDetail()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var tripEventId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 2);
        var day = new CalendarDay(date, new List<CalendarEntry>
        {
            new CalendarEntry(tripEventId.ToString(), CalendarEntrySource.TripEvent, "Museum Visit",
                new DateTimeOffset(2026, 8, 2, 10, 0, 0, TimeSpan.FromHours(9)),
                new DateTimeOffset(2026, 8, 2, 12, 0, 0, TimeSpan.FromHours(9)),
                false, "Notes", "Tokyo Museum", tripId, tripEventId)
        });

        _mockCalendarService
            .Setup(s => s.GetCalendarAsync(_currentUser.Id, date, date, TimeZoneId))
            .ReturnsAsync(new List<CalendarDay> { day });

        // Act
        var result = await _controller.GetDay(date, TimeZoneId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var detail = Assert.IsType<CalendarDayDetailDto>(okResult.Value);

        Assert.Equal(date, detail.Date);
        Assert.Single(detail.Entries);

        var entry = detail.Entries[0];
        Assert.Equal(tripEventId.ToString(), entry.Id);
        Assert.Equal(nameof(CalendarEntrySource.TripEvent), entry.Source);
        Assert.Equal("Museum Visit", entry.Title);
        Assert.Equal(tripId, entry.TripId);
        Assert.Equal(tripEventId, entry.TripEventId);
    }

    [Fact]
    public async Task GetDay_UserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        SetAuthenticatedUser(null);

        // Act
        var result = await _controller.GetDay(new DateOnly(2026, 8, 2), TimeZoneId);

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
        _mockCalendarService.Verify(s => s.GetCalendarAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetDay_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 2);
        _mockCalendarService
            .Setup(s => s.GetCalendarAsync(_currentUser.Id, date, date, TimeZoneId))
            .ThrowsAsync(new ArgumentException("'Not/A_Real_Zone' is not a recognized time zone id."));

        // Act
        var result = await _controller.GetDay(date, TimeZoneId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetDay_ServiceThrowsGoogleReauthorizationRequired_ReturnsUnauthorized()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 2);
        _mockCalendarService
            .Setup(s => s.GetCalendarAsync(_currentUser.Id, date, date, TimeZoneId))
            .ThrowsAsync(new GoogleReauthorizationRequiredException("No Google authorization found for user."));

        // Act
        var result = await _controller.GetDay(date, TimeZoneId);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetDay_ServiceThrowsGoogleCalendarApiException_ReturnsBadGateway()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 2);
        _mockCalendarService
            .Setup(s => s.GetCalendarAsync(_currentUser.Id, date, date, TimeZoneId))
            .ThrowsAsync(new GoogleCalendarApiException("Failed to fetch Google Calendar events."));

        // Act
        var result = await _controller.GetDay(date, TimeZoneId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status502BadGateway, objectResult.StatusCode);
    }

    #endregion
}
