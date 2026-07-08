namespace itinerary_be.Modules.Itinerary.Interfaces;

using itinerary_be.Core.Domain.Entities;

public interface ITripEventRepository
{
    Task<TripEvent> CreateAsync(TripEvent tripEvent);
    Task<TripEvent?> GetByIdAsync(Guid id);
    Task<List<TripEvent>> GetByTripIdAsync(Guid tripId);
    Task DeleteAsync(TripEvent tripEvent);
}
