namespace itinerary_be.UnitTests;

using Microsoft.Extensions.Logging;
using Moq;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Itinerary.Interfaces;
using itinerary_be.Modules.Itinerary.Services;
using Xunit;

public class FlightServiceTests
{
    private readonly Mock<IFlightRepository> _mockFlightRepository;
    private readonly Mock<ITripRepository> _mockTripRepository;
    private readonly Mock<ILogger<FlightService>> _mockLogger;
    private readonly FlightService _service;

    public FlightServiceTests()
    {
        _mockFlightRepository = new Mock<IFlightRepository>();
        _mockTripRepository = new Mock<ITripRepository>();
        _mockLogger = new Mock<ILogger<FlightService>>();

        _service = new FlightService(_mockFlightRepository.Object, _mockTripRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsCreatedFlight()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var flightNumber = "AB123";
        var departureTime = DateTimeOffset.UtcNow;
        var arrivalTime = departureTime.AddHours(2);

        _mockTripRepository.Setup(r => r.GetByIdAsync(tripId)).ReturnsAsync(new Trip
        {
            Id = tripId,
            Title = "Test Trip",
            StartDate = DateOnly.FromDateTime(departureTime.Date),
            EndDate = DateOnly.FromDateTime(departureTime.Date).AddDays(5)
        });

        // Act
        var result = await _service.CreateAsync(tripId, flightNumber, departureTime, arrivalTime, "Airline", "1A", "CONF123", null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tripId, result.TripId);
        Assert.Equal(flightNumber, result.FlightNumber);
        _mockFlightRepository.Verify(r => r.CreateAsync(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_TripNotFound_ThrowsArgumentException()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var departureTime = DateTimeOffset.UtcNow;
        var arrivalTime = departureTime.AddHours(2);

        _mockTripRepository.Setup(r => r.GetByIdAsync(tripId)).ReturnsAsync((Trip?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(tripId, "AB123", departureTime, arrivalTime, null, null, null, null, null));
    }

    [Fact]
    public async Task CreateAsync_ArrivalBeforeDeparture_ThrowsArgumentException()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var departureTime = DateTimeOffset.UtcNow;
        var arrivalTime = departureTime.AddHours(-1);

        _mockTripRepository.Setup(r => r.GetByIdAsync(tripId)).ReturnsAsync(new Trip
        {
            Id = tripId,
            Title = "Test Trip",
            StartDate = DateOnly.FromDateTime(departureTime.Date),
            EndDate = DateOnly.FromDateTime(departureTime.Date).AddDays(5)
        });

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(tripId, "AB123", departureTime, arrivalTime, null, null, null, null, null));
    }

    [Fact]
    public async Task DeleteAsync_ExistingFlight_ReturnsTrue()
    {
        // Arrange
        var flightId = Guid.NewGuid();
        var flight = new Flight { Id = flightId, TripId = Guid.NewGuid(), FlightNumber = "AB123" };

        _mockFlightRepository.Setup(r => r.GetByIdAsync(flightId)).ReturnsAsync(flight);

        // Act
        var result = await _service.DeleteAsync(flightId);

        // Assert
        Assert.True(result);
        _mockFlightRepository.Verify(r => r.DeleteAsync(flight), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingFlight_ReturnsFalse()
    {
        // Arrange
        var flightId = Guid.NewGuid();
        _mockFlightRepository.Setup(r => r.GetByIdAsync(flightId)).ReturnsAsync((Flight?)null);

        // Act
        var result = await _service.DeleteAsync(flightId);

        // Assert
        Assert.False(result);
        _mockFlightRepository.Verify(r => r.DeleteAsync(It.IsAny<Flight>()), Times.Never);
    }
}
