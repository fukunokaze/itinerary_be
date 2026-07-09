namespace itinerary_be.Modules.Itinerary.Interfaces;

using itinerary_be.Core.Domain.Entities;
using itinerary_be.Core.Domain.Enums;

public interface ITripEventService
{
    Task<TripEvent> CreateAsync(
        Guid tripId, 
        EventTypes type, 
        string title, 
        DateOnly date, 
        TimeOnly? startTime, 
        TimeOnly? endTime, 
        string? location, 
        string? notes, 
        string? bookingCode, 
        string? imageUrl, 
        string? tags,
        decimal? cost);
    
    Task<TripEvent?> GetByIdAsync(Guid id);
    
    Task<List<TripEvent>> GetByTripIdAsync(Guid tripId);
    
    Task<bool> DeleteAsync(Guid id);
}
