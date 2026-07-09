namespace itinerary_be.Modules.Itinerary.Interfaces;

using itinerary_be.Core.Domain.Entities;

public interface IFlightRepository
{
    Task<Flight> CreateAsync(Flight flight);
    Task<Flight?> GetByIdAsync(Guid id);
    Task<List<Flight>> GetByTripIdAsync(Guid tripId);
    Task DeleteAsync(Flight flight);
}
