namespace itinerary_be.Modules.Itinerary.Interfaces;

using itinerary_be.Core.Domain.Entities;

/// <summary>
/// Service interface for Trip business logic operations
/// </summary>
public interface ITripService
{
    /// <summary>
    /// Create a new Trip
    /// </summary>
    Task<Trip> CreateTripAsync(string title, DateOnly startDate, DateOnly endDate, string destination = "", string? description = "");

    /// <summary>
    /// Retrieve a Trip by its unique identifier
    /// </summary>
    Task<Trip?> GetTripByIdAsync(Guid id);

    /// <summary>
    /// Retrieve all Trips
    /// </summary>
    Task<List<Trip>> GetAllTripsAsync();

    /// <summary>
    /// Update an existing Trip
    /// </summary>
    Task<Trip?> UpdateTripAsync(Guid id, string title, DateOnly startDate, DateOnly endDate, string destination = "", string? description = "");

    /// <summary>
    /// Delete a Trip
    /// </summary>
    Task<bool> DeleteTripAsync(Guid id);
}
