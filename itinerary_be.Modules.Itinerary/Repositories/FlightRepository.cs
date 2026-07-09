namespace itinerary_be.Modules.Itinerary.Repositories;

using Microsoft.EntityFrameworkCore;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Infrastructure.Data;
using itinerary_be.Modules.Itinerary.Interfaces;

public class FlightRepository : IFlightRepository
{
    private readonly ItineraryDbContext _context;

    public FlightRepository(ItineraryDbContext context)
    {
        _context = context;
    }

    public async Task<Flight> CreateAsync(Flight flight)
    {
        await _context.Flights.AddAsync(flight);
        await _context.SaveChangesAsync();
        return flight;
    }

    public async Task<Flight?> GetByIdAsync(Guid id)
    {
        return await _context.Flights
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<List<Flight>> GetByTripIdAsync(Guid tripId)
    {
        return await _context.Flights
            .Where(f => f.TripId == tripId)
            .OrderBy(f => f.DepartureTime)
            .ToListAsync();
    }

    public async Task DeleteAsync(Flight flight)
    {
        _context.Flights.Remove(flight);
        await _context.SaveChangesAsync();
    }
}
