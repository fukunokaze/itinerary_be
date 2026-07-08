namespace itinerary_be.Modules.Itinerary.Repositories;

using Microsoft.EntityFrameworkCore;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Infrastructure.Data;
using itinerary_be.Modules.Itinerary.Interfaces;

/// <summary>
/// Repository implementation for Trip data access operations
/// </summary>
public class TripRepository : ITripRepository
{
    private readonly ItineraryDbContext _context;

    /// <summary>
    /// Initializes a new instance of the TripRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public TripRepository(ItineraryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Create a new Trip in the database
    /// </summary>
    public async Task CreateAsync(Trip trip)
    {
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieve a Trip by its unique identifier
    /// </summary>
    public async Task<Trip?> GetByIdAsync(Guid id)
    {
        return await _context.Trips
            .Include(t => t.TripEvents)
            .Include(t => t.Lodgings)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <summary>
    /// Retrieve all Trips from the database
    /// </summary>
    public async Task<List<Trip>> GetAllAsync()
    {
        return await _context.Trips.ToListAsync();
    }

    /// <summary>
    /// Update an existing Trip
    /// </summary>
    public async Task UpdateAsync(Trip trip)
    {
        _context.Trips.Update(trip);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Delete a Trip from the database
    /// </summary>
    public async Task DeleteAsync(Trip trip)
    {
        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync();
    }
}
