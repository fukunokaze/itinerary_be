namespace itinerary_be.Modules.Itinerary.Services;

using Microsoft.Extensions.Logging;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Itinerary.Interfaces;

/// <summary>
/// Service implementation for Trip business logic operations
/// </summary>
public class TripService : ITripService
{
    private readonly ITripRepository _repository;
    private readonly ILogger<TripService> _logger;

    /// <summary>
    /// Initializes a new instance of the TripService class
    /// </summary>
    /// <param name="repository">The Trip repository</param>
    /// <param name="logger">The logger instance</param>
    public TripService(ITripRepository repository, ILogger<TripService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Create a new Trip owned by the given user
    /// </summary>
    public async Task<Trip> CreateTripAsync(Guid userId, string title, DateOnly startDate, DateOnly endDate, string destination = "", string? description = "")
    {
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Destination = destination,
            Description = description,
            StartDate = startDate,
            EndDate = endDate
        };

        await _repository.CreateAsync(trip);
        _logger.LogInformation($"Trip created with ID: {trip.Id}");

        return trip;
    }

    /// <summary>
    /// Retrieve a Trip by its unique identifier
    /// </summary>
    public async Task<Trip?> GetTripByIdAsync(Guid id)
    {
        var trip = await _repository.GetByIdAsync(id);

        if (trip == null)
        {
            _logger.LogWarning($"Trip with ID {id} not found");
            return null;
        }

        return trip;
    }

    public async Task<Trip?> GetTripByIdAndUserIdAsync(Guid id, Guid userId)
    {
        var trip = await _repository.GetByIdAndUserIdAsync(id, userId);

        if (trip == null)
        {
            _logger.LogWarning($"Trip with ID {id} not found for User {userId}");
            return null;
        }

        return trip;
    }

    /// <summary>
    /// Retrieve all Trips belonging to the given user
    /// </summary>
    public async Task<List<Trip>> GetAllTripsAsync(Guid userId)
    {
        return await _repository.GetAllAsync(userId);
    }

    /// <summary>
    /// Update an existing Trip
    /// </summary>
    public async Task<Trip?> UpdateTripAsync(Guid id, string title, DateOnly startDate, DateOnly endDate, string destination = "", string? description = "")
    {
        var trip = await _repository.GetByIdAsync(id);

        if (trip == null)
        {
            _logger.LogWarning($"Trip with ID {id} not found for update");
            return null;
        }

        trip.Title = title;
        trip.StartDate = startDate;
        trip.EndDate = endDate;
        trip.Destination = destination;
        trip.Description = description;

        await _repository.UpdateAsync(trip);
        _logger.LogInformation($"Trip with ID {id} updated");

        return trip;
    }

    public async Task<Trip?> UpdateTripAsync(Guid id, string title, DateOnly startDate, DateOnly endDate, Guid userId, string destination = "", string? description = "")
    {
        var trip = await _repository.GetByIdAndUserIdAsync(id, userId);

        if (trip == null)
        {
            _logger.LogWarning($"Trip with ID {id} not found in User {userId} for update");
            return null;
        }

        return await UpdateTripAsync(id, title, startDate, endDate, destination, description);
    }

    /// <summary>
    /// Delete a Trip
    /// </summary>
    public async Task<bool> DeleteTripAsync(Guid id)
    {
        var trip = await _repository.GetByIdAsync(id);

        if (trip == null)
        {
            _logger.LogWarning($"Trip with ID {id} not found for deletion");
            return false;
        }

        await _repository.DeleteAsync(trip);
        _logger.LogInformation($"Trip with ID {id} deleted");

        return true;
    }

    public async Task<bool> DeleteTripAsync(Guid id, Guid userId)
    {
        var trip = await _repository.GetByIdAndUserIdAsync(id, userId);

        if (trip == null)
        {
            _logger.LogWarning($"Trip with ID {id} not found in User {userId} for deletion");
            return false;
        }

        await _repository.DeleteAsync(trip);
        _logger.LogInformation($"Trip with ID {id} deleted for User {userId}");

        return true;
    }
}
