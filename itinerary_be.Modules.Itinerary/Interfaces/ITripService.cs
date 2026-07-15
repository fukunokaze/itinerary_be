namespace itinerary_be.Modules.Itinerary.Interfaces;

using itinerary_be.Core.Domain.Entities;

/// <summary>
/// Service interface for Trip business logic operations
/// </summary>
public interface ITripService
{
    /// <summary>
    /// Create a new Trip owned by the given user
    /// </summary>
    Task<Trip> CreateTripAsync(Guid userId, string title, DateOnly startDate, DateOnly endDate, string destination = "", string? description = "");

    /// <summary>
    /// Retrieve a Trip by its unique identifier
    /// </summary>
    Task<Trip?> GetTripByIdAsync(Guid id);

    /// <summary>
    /// Retrieve all Trips belonging to the given user
    /// </summary>
    Task<List<Trip>> GetAllTripsAsync(Guid userId);

    /// <summary>
    /// Update an existing Trip
    /// </summary>
    Task<Trip?> UpdateTripAsync(Guid id, string title, DateOnly startDate, DateOnly endDate, string destination = "", string? description = "");

    /// <summary>
    /// Delete a Trip
    /// </summary>
    Task<bool> DeleteTripAsync(Guid id);
    Task<Trip?> UpdateTripAsync(Guid id, string title, DateOnly startDate, DateOnly endDate, Guid userId, string destination = "", string? description = "");
    Task<bool> DeleteTripAsync(Guid id, Guid userId);
}
