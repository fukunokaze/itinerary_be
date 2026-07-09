namespace itinerary_be.Modules.Itinerary.Services;

using Microsoft.Extensions.Logging;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Itinerary.Interfaces;

public class FlightService : IFlightService
{
    private readonly IFlightRepository _repository;
    private readonly ITripRepository _tripRepository;
    private readonly ILogger<FlightService> _logger;

    public FlightService(IFlightRepository repository, ITripRepository tripRepository, ILogger<FlightService> logger)
    {
        _repository = repository;
        _tripRepository = tripRepository;
        _logger = logger;
    }

    public async Task<Flight> CreateAsync(
        Guid tripId,
        string flightNumber,
        DateTimeOffset departureTime,
        DateTimeOffset arrivalTime,
        string? airline,
        string? seat,
        string? confirmationCode,
        string? route,
        decimal? cost)
    {
        var trip = await _tripRepository.GetByIdAsync(tripId);
        if (trip == null)
        {
            throw new ArgumentException($"Trip with ID {tripId} not found");
        }

        if (arrivalTime <= departureTime)
        {
            throw new ArgumentException("ArrivalTime must be after DepartureTime.");
        }

        var flight = new Flight
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            FlightNumber = flightNumber,
            DepartureTime = departureTime.ToUniversalTime(),
            ArrivalTime = arrivalTime.ToUniversalTime(),
            Airline = airline,
            Seat = seat,
            ConfirmationCode = confirmationCode,
            Route = route,
            Cost = cost
        };

        await _repository.CreateAsync(flight);
        _logger.LogInformation($"Flight created with ID: {flight.Id}");

        return flight;
    }

    public async Task<Flight?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<Flight>> GetByTripIdAsync(Guid tripId)
    {
        return await _repository.GetByTripIdAsync(tripId);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var flight = await _repository.GetByIdAsync(id);
        if (flight == null)
        {
            _logger.LogWarning($"Flight with ID {id} not found for deletion");
            return false;
        }

        await _repository.DeleteAsync(flight);
        _logger.LogInformation($"Flight with ID {id} deleted");

        return true;
    }
}
