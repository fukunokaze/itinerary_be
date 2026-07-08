namespace itinerary_be.Modules.Itinerary.Repositories;

using Microsoft.EntityFrameworkCore;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Infrastructure.Data;
using itinerary_be.Modules.Itinerary.Interfaces;

public class TripEventRepository : ITripEventRepository
{
    private readonly ItineraryDbContext _context;

    public TripEventRepository(ItineraryDbContext context)
    {
        _context = context;
    }

    public async Task<TripEvent> CreateAsync(TripEvent tripEvent)
    {
        await _context.TripEvents.AddAsync(tripEvent);
        await _context.SaveChangesAsync();
        return tripEvent;
    }

    public async Task<TripEvent?> GetByIdAsync(Guid id)
    {
        return await _context.TripEvents
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<TripEvent>> GetByTripIdAsync(Guid tripId)
    {
        return await _context.TripEvents
            .Where(e => e.TripId == tripId)
            .OrderBy(e => e.Date)
            .ThenBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task DeleteAsync(TripEvent tripEvent)
    {
        _context.TripEvents.Remove(tripEvent);
        await _context.SaveChangesAsync();
    }
}
