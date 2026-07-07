namespace itinerary_be.Modules.Itinerary.Interfaces;

using itinerary_be.Core.Domain.Entities;

/// <summary>
/// Repository interface for Trip data access operations
/// </summary>
public interface ITripRepository
{
    /// <summary>
    /// Create a new Trip in the database
    /// </summary>
    Task CreateAsync(Trip trip);

    /// <summary>
    /// Retrieve a Trip by its unique identifier
    /// </summary>
    Task<Trip?> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieve all Trips from the database
    /// </summary>
    Task<List<Trip>> GetAllAsync();

    /// <summary>
    /// Update an existing Trip
    /// </summary>
    Task UpdateAsync(Trip trip);

    /// <summary>
    /// Delete a Trip from the database
    /// </summary>
    Task DeleteAsync(Trip trip);
}
