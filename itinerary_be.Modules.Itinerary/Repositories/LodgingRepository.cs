namespace itinerary_be.Modules.Itinerary.Repositories;

using Microsoft.EntityFrameworkCore;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Infrastructure.Data;
using itinerary_be.Modules.Itinerary.Interfaces;

public class LodgingRepository : ILodgingRepository
{
    private readonly ItineraryDbContext _context;

    public LodgingRepository(ItineraryDbContext context)
    {
        _context = context;
    }

    public async Task<Lodging> CreateAsync(Lodging lodging)
    {
        await _context.Lodgings.AddAsync(lodging);
        await _context.SaveChangesAsync();
        return lodging;
    }

    public async Task<Lodging?> GetByIdAsync(Guid id)
    {
        return await _context.Lodgings
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<List<Lodging>> GetByTripIdAsync(Guid tripId)
    {
        return await _context.Lodgings
            .Where(l => l.TripId == tripId)
            .OrderBy(l => l.CheckIn)
            .ToListAsync();
    }

    public async Task DeleteAsync(Lodging lodging)
    {
        _context.Lodgings.Remove(lodging);
        await _context.SaveChangesAsync();
    }
}
