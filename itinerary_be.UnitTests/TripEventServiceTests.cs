namespace itinerary_be.UnitTests;

using Microsoft.Extensions.Logging;
using Moq;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Core.Domain.Enums;
using itinerary_be.Modules.Itinerary.Interfaces;
using itinerary_be.Modules.Itinerary.Services;
using Xunit;

public class TripEventServiceTests
{
    private readonly Mock<ITripEventRepository> _mockEventRepository;
    private readonly Mock<ITripRepository> _mockTripRepository;
    private readonly Mock<ILogger<TripEventService>> _mockLogger;
    private readonly TripEventService _service;

    public TripEventServiceTests()
    {
        _mockEventRepository = new Mock<ITripEventRepository>();
        _mockTripRepository = new Mock<ITripRepository>();
        _mockLogger = new Mock<ILogger<TripEventService>>();
        
        _service = new TripEventService(_mockEventRepository.Object, _mockTripRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsCreatedTripEvent()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var type = EventTypes.activity;
        var title = "Test Event";
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockTripRepository.Setup(r => r.GetByIdAsync(tripId)).ReturnsAsync(new Trip { Id = tripId, Title = "Test Trip", StartDate = date.AddDays(-1), EndDate = date.AddDays(1) });
        _mockEventRepository.Setup(r => r.GetByTripIdAsync(tripId)).ReturnsAsync(new List<TripEvent>());

        // Act
        var result = await _service.CreateAsync(tripId, type, title, date, null, null, null, null, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tripId, result.TripId);
        Assert.Equal(title, result.Title);
        _mockEventRepository.Verify(r => r.CreateAsync(It.IsAny<TripEvent>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_OverlappingEvents_ThrowsArgumentException()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var type = EventTypes.activity;
        var title = "Overlapping Event";
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var startTime = new TimeOnly(10, 0);
        var endTime = new TimeOnly(12, 0);

        _mockTripRepository.Setup(r => r.GetByIdAsync(tripId)).ReturnsAsync(new Trip { Id = tripId, Title = "Test Trip", StartDate = date.AddDays(-1), EndDate = date.AddDays(1) });

        var existingEvents = new List<TripEvent>
        {
            new TripEvent {
                Id = Guid.NewGuid(),
                TripId = tripId,
                Type = EventTypes.activity,
                Title = "Existing",
                Date = date,
                StartTime = new TimeOnly(11, 0),
                EndTime = new TimeOnly(13, 0)
            }
        };

        _mockEventRepository.Setup(r => r.GetByTripIdAsync(tripId)).ReturnsAsync(existingEvents);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(tripId, type, title, date, startTime, endTime, null, null, null, null, null));
    }

    [Fact]
    public async Task CreateAsync_DateBeforeTripStartDate_ThrowsArgumentException()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var type = EventTypes.activity;
        var title = "Test Event";
        var tripStartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var tripEndDate = tripStartDate.AddDays(5);
        var eventDate = tripStartDate.AddDays(-1);

        _mockTripRepository.Setup(r => r.GetByIdAsync(tripId)).ReturnsAsync(new Trip { Id = tripId, Title = "Test Trip", StartDate = tripStartDate, EndDate = tripEndDate });

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(tripId, type, title, eventDate, null, null, null, null, null, null, null));
    }

    [Fact]
    public async Task CreateAsync_DateAfterTripEndDate_ThrowsArgumentException()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var type = EventTypes.activity;
        var title = "Test Event";
        var tripStartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var tripEndDate = tripStartDate.AddDays(5);
        var eventDate = tripEndDate.AddDays(1);

        _mockTripRepository.Setup(r => r.GetByIdAsync(tripId)).ReturnsAsync(new Trip { Id = tripId, Title = "Test Trip", StartDate = tripStartDate, EndDate = tripEndDate });

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(tripId, type, title, eventDate, null, null, null, null, null, null, null));
    }

    [Fact]
    public async Task DeleteAsync_ExistingEvent_ReturnsTrue()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var tripEvent = new TripEvent { Id = eventId, TripId = Guid.NewGuid(), Type = EventTypes.activity, Title = "Test" };
        
        _mockEventRepository.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(tripEvent);

        // Act
        var result = await _service.DeleteAsync(eventId);

        // Assert
        Assert.True(result);
        _mockEventRepository.Verify(r => r.DeleteAsync(tripEvent), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingEvent_ReturnsFalse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mockEventRepository.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync((TripEvent?)null);

        // Act
        var result = await _service.DeleteAsync(eventId);

        // Assert
        Assert.False(result);
        _mockEventRepository.Verify(r => r.DeleteAsync(It.IsAny<TripEvent>()), Times.Never);
    }
}
