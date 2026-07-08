namespace itinerary_be.Modules.Itinerary.Interfaces;

using itinerary_be.Core.Domain.Entities;

public interface ILodgingRepository
{
    Task<Lodging> CreateAsync(Lodging lodging);
    Task<Lodging?> GetByIdAsync(Guid id);
    Task<List<Lodging>> GetByTripIdAsync(Guid tripId);
    Task DeleteAsync(Lodging lodging);
}
