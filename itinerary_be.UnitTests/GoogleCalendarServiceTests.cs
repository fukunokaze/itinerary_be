namespace itinerary_be.UnitTests;

using Microsoft.Extensions.Logging;
using Moq;
using itinerary_be.Modules.Auth.Exceptions;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Models;
using itinerary_be.Modules.Auth.Services;

/// <summary>
/// Unit tests for GoogleCalendarService class
/// </summary>
public class GoogleCalendarServiceTests
{
    private readonly Mock<IUserGoogleTokenService> _mockUserGoogleTokenService;
    private readonly Mock<IGoogleCalendarClient> _mockGoogleCalendarClient;
    private readonly Mock<ILogger<GoogleCalendarService>> _mockLogger;
    private readonly GoogleCalendarService _service;

    public GoogleCalendarServiceTests()
    {
        _mockUserGoogleTokenService = new Mock<IUserGoogleTokenService>();
        _mockGoogleCalendarClient = new Mock<IGoogleCalendarClient>();
        _mockLogger = new Mock<ILogger<GoogleCalendarService>>();
        _service = new GoogleCalendarService(_mockUserGoogleTokenService.Object, _mockGoogleCalendarClient.Object, _mockLogger.Object);
    }

    #region GetEventsAsync Tests

    [Fact]
    public async Task GetEventsAsync_ValidRange_ReturnsEventsFromClientUsingValidAccessToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 8, 1);
        var endDate = new DateOnly(2026, 8, 5);
        var expectedEvents = new List<GoogleCalendarEvent>
        {
            new("evt1", "Flight", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2), false, null, null)
        };

        _mockUserGoogleTokenService.Setup(s => s.GetValidAccessTokenAsync(userId)).ReturnsAsync("valid-access-token");
        _mockGoogleCalendarClient.Setup(c => c.GetEventsAsync("valid-access-token", startDate, endDate)).ReturnsAsync(expectedEvents);

        // Act
        var result = await _service.GetEventsAsync(userId, startDate, endDate);

        // Assert
        Assert.Same(expectedEvents, result);
        _mockGoogleCalendarClient.Verify(c => c.GetEventsAsync("valid-access-token", startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task GetEventsAsync_StartDateAfterEndDate_ThrowsArgumentExceptionWithoutCallingDependencies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 8, 5);
        var endDate = new DateOnly(2026, 8, 1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetEventsAsync(userId, startDate, endDate));
        _mockUserGoogleTokenService.Verify(s => s.GetValidAccessTokenAsync(It.IsAny<Guid>()), Times.Never);
        _mockGoogleCalendarClient.Verify(c => c.GetEventsAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public async Task GetEventsAsync_TokenServiceRequiresReauthorization_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 8, 1);
        var endDate = new DateOnly(2026, 8, 5);

        _mockUserGoogleTokenService.Setup(s => s.GetValidAccessTokenAsync(userId))
            .ThrowsAsync(new GoogleReauthorizationRequiredException("No Google authorization found for user."));

        // Act & Assert
        await Assert.ThrowsAsync<GoogleReauthorizationRequiredException>(() => _service.GetEventsAsync(userId, startDate, endDate));
        _mockGoogleCalendarClient.Verify(c => c.GetEventsAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Never);
    }

    #endregion
}
