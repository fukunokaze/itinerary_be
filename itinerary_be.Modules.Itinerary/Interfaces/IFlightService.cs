namespace itinerary_be.Modules.Itinerary.Interfaces;

using itinerary_be.Core.Domain.Entities;

public interface IFlightService
{
    Task<Flight> CreateAsync(
        Guid tripId,
        string flightNumber,
        DateTimeOffset departureTime,
        DateTimeOffset arrivalTime,
        string? airline,
        string? seat,
        string? confirmationCode);

    Task<Flight?> GetByIdAsync(Guid id);

    Task<List<Flight>> GetByTripIdAsync(Guid tripId);

    Task<bool> DeleteAsync(Guid id);
}
