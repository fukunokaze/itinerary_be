namespace itinerary_be.Modules.Itinerary.Interfaces;

using itinerary_be.Core.Domain.Entities;

public interface ILodgingService
{
    Task<Lodging> CreateAsync(
        Guid tripId,
        string name,
        DateTimeOffset checkIn,
        DateTimeOffset checkOut,
        string? address,
        string? confirmationCode);

    Task<Lodging?> GetByIdAsync(Guid id);

    Task<List<Lodging>> GetByTripIdAsync(Guid tripId);

    Task<bool> DeleteAsync(Guid id);
}
