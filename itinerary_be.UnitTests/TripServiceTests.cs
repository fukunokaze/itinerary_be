namespace itinerary_be.UnitTests;

using Moq;
using Microsoft.Extensions.Logging;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Itinerary.Interfaces;
using itinerary_be.Modules.Itinerary.Services;

/// <summary>
/// Unit tests for TripService class
/// </summary>
public class TripServiceTests
{
    private readonly Mock<ITripRepository> _mockRepository;
    private readonly Mock<ILogger<TripService>> _mockLogger;
    private readonly TripService _tripService;

    public TripServiceTests()
    {
        _mockRepository = new Mock<ITripRepository>();
        _mockLogger = new Mock<ILogger<TripService>>();
        _tripService = new TripService(_mockRepository.Object, _mockLogger.Object);
    }

    #region CreateTripAsync Tests

    [Fact]
    public async Task CreateTripAsync_WithValidInput_CreatesAndReturnsTripSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var title = "Summer Vacation";
        var startDate = new DateOnly(2026, 06, 01);
        var endDate = new DateOnly(2026, 06, 15);
        var destination = "Hawaii";
        var description = "A beautiful summer trip";

        // Act
        var result = await _tripService.CreateTripAsync(userId, title, startDate, endDate, destination, description);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(title, result.Title);
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(endDate, result.EndDate);
        Assert.Equal(destination, result.Destination);
        Assert.Equal(description, result.Description);

        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Trip>()), Times.Once);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateTripAsync_WithMinimalInput_CreatesWithDefaultValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var title = "Quick Trip";
        var startDate = new DateOnly(2026, 07, 01);
        var endDate = new DateOnly(2026, 07, 02);

        // Act
        var result = await _tripService.CreateTripAsync(userId, title, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(title, result.Title);
        Assert.Equal(string.Empty, result.Destination);
        Assert.Equal(string.Empty, result.Description);
        
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Trip>()), Times.Once);
    }

    [Fact]
    public async Task CreateTripAsync_RepositoryThrowsException_ExceptionPropagates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var title = "Test Trip";
        var startDate = new DateOnly(2026, 07, 01);
        var endDate = new DateOnly(2026, 07, 02);

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Trip>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _tripService.CreateTripAsync(userId, title, startDate, endDate));
    }

    #endregion

    #region GetTripByIdAsync Tests

    [Fact]
    public async Task GetTripByIdAsync_WithValidId_ReturnsTripSuccessfully()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var trip = new Trip
        {
            Id = tripId,
            Title = "Test Trip",
            StartDate = new DateOnly(2026, 07, 01),
            EndDate = new DateOnly(2026, 07, 10),
            Destination = "Paris",
            Description = "A trip to Paris"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(tripId))
            .ReturnsAsync(trip);

        // Act
        var result = await _tripService.GetTripByIdAsync(tripId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(trip.Id, result.Id);
        Assert.Equal(trip.Title, result.Title);
        Assert.Equal(trip.Destination, result.Destination);
        
        _mockRepository.Verify(r => r.GetByIdAsync(tripId), Times.Once);
    }

    [Fact]
    public async Task GetTripByIdAsync_WithNonExistentId_ReturnsNullAndLogsWarning()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(tripId))
            .ReturnsAsync((Trip?)null);

        // Act
        var result = await _tripService.GetTripByIdAsync(tripId);

        // Assert
        Assert.Null(result);
        
        _mockRepository.Verify(r => r.GetByIdAsync(tripId), Times.Once);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTripByIdAsync_RepositoryThrowsException_ExceptionPropagates()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(tripId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _tripService.GetTripByIdAsync(tripId));
    }

    #endregion

    #region GetTripByIdAndUserIdAsync Tests

    [Fact]
    public async Task GetTripByIdAndUserIdAsync_WithValidIdAndUserId_ReturnsTripSuccessfully()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var trip = new Trip
        {
            Id = tripId,
            UserId = userId,
            Title = "Test Trip",
            StartDate = new DateOnly(2026, 07, 01),
            EndDate = new DateOnly(2026, 07, 10),
            Destination = "Paris",
            Description = "A trip to Paris"
        };

        _mockRepository.Setup(r => r.GetByIdAndUserIdAsync(tripId, userId))
            .ReturnsAsync(trip);

        // Act
        var result = await _tripService.GetTripByIdAndUserIdAsync(tripId, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(trip.Id, result.Id);
        Assert.Equal(trip.UserId, result.UserId);
        Assert.Equal(trip.Title, result.Title);

        _mockRepository.Verify(r => r.GetByIdAndUserIdAsync(tripId, userId), Times.Once);
    }

    [Fact]
    public async Task GetTripByIdAndUserIdAsync_WithNonExistentIdOrUserId_ReturnsNullAndLogsWarning()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAndUserIdAsync(tripId, userId))
            .ReturnsAsync((Trip?)null);

        // Act
        var result = await _tripService.GetTripByIdAndUserIdAsync(tripId, userId);

        // Assert
        Assert.Null(result);

        _mockRepository.Verify(r => r.GetByIdAndUserIdAsync(tripId, userId), Times.Once);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTripByIdAndUserIdAsync_RepositoryThrowsException_ExceptionPropagates()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAndUserIdAsync(tripId, userId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _tripService.GetTripByIdAndUserIdAsync(tripId, userId));
    }

    #endregion

    #region GetAllTripsAsync Tests

    [Fact]
    public async Task GetAllTripsAsync_WithExistingTrips_ReturnsAllTripsForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var trips = new List<Trip>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Title = "Trip 1", StartDate = new DateOnly(2026, 07, 01), EndDate = new DateOnly(2026, 07, 10) },
            new() { Id = Guid.NewGuid(), UserId = userId, Title = "Trip 2", StartDate = new DateOnly(2026, 08, 01), EndDate = new DateOnly(2026, 08, 10) },
            new() { Id = Guid.NewGuid(), UserId = userId, Title = "Trip 3", StartDate = new DateOnly(2026, 09, 01), EndDate = new DateOnly(2026, 09, 10) }
        };

        _mockRepository.Setup(r => r.GetAllAsync(userId))
            .ReturnsAsync(trips);

        // Act
        var result = await _tripService.GetAllTripsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, t => t.Title == "Trip 1");
        Assert.Contains(result, t => t.Title == "Trip 2");
        Assert.Contains(result, t => t.Title == "Trip 3");

        _mockRepository.Verify(r => r.GetAllAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetAllTripsAsync_WithNoTrips_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetAllAsync(userId))
            .ReturnsAsync(new List<Trip>());

        // Act
        var result = await _tripService.GetAllTripsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _mockRepository.Verify(r => r.GetAllAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetAllTripsAsync_RepositoryThrowsException_ExceptionPropagates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetAllAsync(userId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _tripService.GetAllTripsAsync(userId));
    }

    #endregion

    #region UpdateTripAsync Tests

    [Fact]
    public async Task UpdateTripAsync_WithValidIdAndInput_UpdatesAndReturnsTripSuccessfully()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var originalTrip = new Trip
        {
            Id = tripId,
            Title = "Old Title",
            StartDate = new DateOnly(2026, 07, 01),
            EndDate = new DateOnly(2026, 07, 10),
            Destination = "Old Destination",
            Description = "Old Description"
        };

        var newTitle = "Updated Title";
        var newStartDate = new DateOnly(2026, 08, 01);
        var newEndDate = new DateOnly(2026, 08, 15);
        var newDestination = "New Destination";
        var newDescription = "New Description";

        _mockRepository.Setup(r => r.GetByIdAsync(tripId))
            .ReturnsAsync(originalTrip);

        // Act
        var result = await _tripService.UpdateTripAsync(tripId, newTitle, newStartDate, newEndDate, newDestination, newDescription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tripId, result.Id);
        Assert.Equal(newTitle, result.Title);
        Assert.Equal(newStartDate, result.StartDate);
        Assert.Equal(newEndDate, result.EndDate);
        Assert.Equal(newDestination, result.Destination);
        Assert.Equal(newDescription, result.Description);
        
        _mockRepository.Verify(r => r.GetByIdAsync(tripId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Trip>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTripAsync_WithNonExistentId_ReturnsNullAndLogsWarning()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(tripId))
            .ReturnsAsync((Trip?)null);

        // Act
        var result = await _tripService.UpdateTripAsync(tripId, "Title", new DateOnly(2026, 07, 01), new DateOnly(2026, 07, 10));

        // Assert
        Assert.Null(result);
        
        _mockRepository.Verify(r => r.GetByIdAsync(tripId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Trip>()), Times.Never);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateTripAsync_RepositoryThrowsException_ExceptionPropagates()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(tripId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _tripService.UpdateTripAsync(tripId, "Title", new DateOnly(2026, 07, 01), new DateOnly(2026, 07, 10)));
    }

    [Fact]
    public async Task UpdateTripAsync_WithPartialUpdate_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var originalTrip = new Trip
        {
            Id = tripId,
            Title = "Original Title",
            StartDate = new DateOnly(2026, 07, 01),
            EndDate = new DateOnly(2026, 07, 10),
            Destination = "Original Destination",
            Description = "Original Description"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(tripId))
            .ReturnsAsync(originalTrip);

        // Act
        var result = await _tripService.UpdateTripAsync(tripId, "New Title", new DateOnly(2026, 07, 01), new DateOnly(2026, 07, 10));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Title", result.Title);
        Assert.Equal(string.Empty, result.Destination);
        Assert.Equal(string.Empty, result.Description);
        
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Trip>()), Times.Once);
    }

    #endregion

    #region UpdateTripAsync (with userId) Tests

    [Fact]
    public async Task UpdateTripAsync_WithValidIdAndUserId_UpdatesAndReturnsTripSuccessfully()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var originalTrip = new Trip
        {
            Id = tripId,
            UserId = userId,
            Title = "Old Title",
            StartDate = new DateOnly(2026, 07, 01),
            EndDate = new DateOnly(2026, 07, 10),
            Destination = "Old Destination",
            Description = "Old Description"
        };

        var newTitle = "Updated Title";
        var newStartDate = new DateOnly(2026, 08, 01);
        var newEndDate = new DateOnly(2026, 08, 15);
        var newDestination = "New Destination";
        var newDescription = "New Description";

        _mockRepository.Setup(r => r.GetByIdAndUserIdAsync(tripId, userId))
            .ReturnsAsync(originalTrip);
        _mockRepository.Setup(r => r.GetByIdAsync(tripId))
            .ReturnsAsync(originalTrip);

        // Act
        var result = await _tripService.UpdateTripAsync(tripId, newTitle, newStartDate, newEndDate, userId, newDestination, newDescription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tripId, result.Id);
        Assert.Equal(newTitle, result.Title);
        Assert.Equal(newStartDate, result.StartDate);
        Assert.Equal(newEndDate, result.EndDate);
        Assert.Equal(newDestination, result.Destination);
        Assert.Equal(newDescription, result.Description);

        _mockRepository.Verify(r => r.GetByIdAndUserIdAsync(tripId, userId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Trip>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTripAsync_WithNonExistentIdForUser_ReturnsNullAndLogsWarning()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAndUserIdAsync(tripId, userId))
            .ReturnsAsync((Trip?)null);

        // Act
        var result = await _tripService.UpdateTripAsync(tripId, "Title", new DateOnly(2026, 07, 01), new DateOnly(2026, 07, 10), userId);

        // Assert
        Assert.Null(result);

        _mockRepository.Verify(r => r.GetByIdAndUserIdAsync(tripId, userId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Trip>()), Times.Never);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateTripAsync_WithUserId_RepositoryThrowsException_ExceptionPropagates()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAndUserIdAsync(tripId, userId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _tripService.UpdateTripAsync(tripId, "Title", new DateOnly(2026, 07, 01), new DateOnly(2026, 07, 10), userId));
    }

    #endregion

    #region DeleteTripAsync Tests

    [Fact]
    public async Task DeleteTripAsync_WithValidId_DeletesAndReturnsTrueSuccessfully()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var trip = new Trip
        {
            Id = tripId,
            Title = "Trip to Delete",
            StartDate = new DateOnly(2026, 07, 01),
            EndDate = new DateOnly(2026, 07, 10)
        };

        _mockRepository.Setup(r => r.GetByIdAsync(tripId))
            .ReturnsAsync(trip);

        // Act
        var result = await _tripService.DeleteTripAsync(tripId);

        // Assert
        Assert.True(result);
        
        _mockRepository.Verify(r => r.GetByIdAsync(tripId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(trip), Times.Once);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteTripAsync_WithNonExistentId_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(tripId))
            .ReturnsAsync((Trip?)null);

        // Act
        var result = await _tripService.DeleteTripAsync(tripId);

        // Assert
        Assert.False(result);
        
        _mockRepository.Verify(r => r.GetByIdAsync(tripId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Trip>()), Times.Never);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteTripAsync_RepositoryThrowsException_ExceptionPropagates()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(tripId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _tripService.DeleteTripAsync(tripId));
    }

    #endregion

    #region DeleteTripAsync (with userId) Tests

    [Fact]
    public async Task DeleteTripAsync_WithValidIdAndUserId_DeletesAndReturnsTrueSuccessfully()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var trip = new Trip
        {
            Id = tripId,
            UserId = userId,
            Title = "Trip to Delete",
            StartDate = new DateOnly(2026, 07, 01),
            EndDate = new DateOnly(2026, 07, 10)
        };

        _mockRepository.Setup(r => r.GetByIdAndUserIdAsync(tripId, userId))
            .ReturnsAsync(trip);

        // Act
        var result = await _tripService.DeleteTripAsync(tripId, userId);

        // Assert
        Assert.True(result);

        _mockRepository.Verify(r => r.GetByIdAndUserIdAsync(tripId, userId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(trip), Times.Once);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteTripAsync_WithNonExistentIdForUser_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAndUserIdAsync(tripId, userId))
            .ReturnsAsync((Trip?)null);

        // Act
        var result = await _tripService.DeleteTripAsync(tripId, userId);

        // Assert
        Assert.False(result);

        _mockRepository.Verify(r => r.GetByIdAndUserIdAsync(tripId, userId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Trip>()), Times.Never);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteTripAsync_WithUserId_RepositoryThrowsException_ExceptionPropagates()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAndUserIdAsync(tripId, userId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _tripService.DeleteTripAsync(tripId, userId));
    }

    #endregion
}
