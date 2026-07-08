namespace itinerary_be.UnitTests;

using Microsoft.Extensions.Logging;
using Moq;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Itinerary.Interfaces;
using itinerary_be.Modules.Itinerary.Services;
using Xunit;

public class LodgingServiceTests
{
    private readonly Mock<ILodgingRepository> _mockLodgingRepository;
    private readonly Mock<ITripRepository> _mockTripRepository;
    private readonly Mock<ILogger<LodgingService>> _mockLogger;
    private readonly LodgingService _service;

    public LodgingServiceTests()
    {
        _mockLodgingRepository = new Mock<ILodgingRepository>();
        _mockTripRepository = new Mock<ITripRepository>();
        _mockLogger = new Mock<ILogger<LodgingService>>();

        _service = new LodgingService(_mockLodgingRepository.Object, _mockTripRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsCreatedLodging()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var name = "Test Hotel";
        var checkIn = DateTimeOffset.UtcNow;
        var checkOut = checkIn.AddDays(2);

        _mockTripRepository.Setup(r => r.GetByIdAsync(tripId)).ReturnsAsync(new Trip
        {
            Id = tripId,
            Title = "Test Trip",
            StartDate = DateOnly.FromDateTime(checkIn.DateTime.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(checkOut.DateTime.AddDays(1))
        });

        // Act
        var result = await _service.CreateAsync(tripId, name, checkIn, checkOut, "123 Main St", "CONF123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tripId, result.TripId);
        Assert.Equal(name, result.Name);
        Assert.Equal("123 Main St", result.Address);
        Assert.Equal("CONF123", result.ConfirmationCode);
        _mockLodgingRepository.Verify(r => r.CreateAsync(It.IsAny<Lodging>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_TripNotFound_ThrowsArgumentException()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var checkIn = DateTimeOffset.UtcNow;
        var checkOut = checkIn.AddDays(1);

        _mockTripRepository.Setup(r => r.GetByIdAsync(tripId)).ReturnsAsync((Trip?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(tripId, "Test Hotel", checkIn, checkOut, null, null));
    }

    [Fact]
    public async Task CreateAsync_CheckOutBeforeCheckIn_ThrowsArgumentException()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var checkIn = DateTimeOffset.UtcNow;
        var checkOut = checkIn.AddDays(-1);

        _mockTripRepository.Setup(r => r.GetByIdAsync(tripId)).ReturnsAsync(new Trip
        {
            Id = tripId,
            Title = "Test Trip",
            StartDate = DateOnly.FromDateTime(checkIn.DateTime.AddDays(-2)),
            EndDate = DateOnly.FromDateTime(checkIn.DateTime.AddDays(2))
        });

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(tripId, "Test Hotel", checkIn, checkOut, null, null));
    }

    [Fact]
    public async Task DeleteAsync_ExistingLodging_ReturnsTrue()
    {
        // Arrange
        var lodgingId = Guid.NewGuid();
        var lodging = new Lodging { Id = lodgingId, TripId = Guid.NewGuid(), Name = "Test Hotel" };

        _mockLodgingRepository.Setup(r => r.GetByIdAsync(lodgingId)).ReturnsAsync(lodging);

        // Act
        var result = await _service.DeleteAsync(lodgingId);

        // Assert
        Assert.True(result);
        _mockLodgingRepository.Verify(r => r.DeleteAsync(lodging), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingLodging_ReturnsFalse()
    {
        // Arrange
        var lodgingId = Guid.NewGuid();
        _mockLodgingRepository.Setup(r => r.GetByIdAsync(lodgingId)).ReturnsAsync((Lodging?)null);

        // Act
        var result = await _service.DeleteAsync(lodgingId);

        // Assert
        Assert.False(result);
        _mockLodgingRepository.Verify(r => r.DeleteAsync(It.IsAny<Lodging>()), Times.Never);
    }
}
