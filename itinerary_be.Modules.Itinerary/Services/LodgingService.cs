namespace itinerary_be.Modules.Itinerary.Services;

using Microsoft.Extensions.Logging;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Itinerary.Interfaces;

public class LodgingService : ILodgingService
{
    private readonly ILodgingRepository _repository;
    private readonly ITripRepository _tripRepository;
    private readonly ILogger<LodgingService> _logger;

    public LodgingService(ILodgingRepository repository, ITripRepository tripRepository, ILogger<LodgingService> logger)
    {
        _repository = repository;
        _tripRepository = tripRepository;
        _logger = logger;
    }

    public async Task<Lodging> CreateAsync(
        Guid tripId,
        string name,
        DateTimeOffset checkIn,
        DateTimeOffset checkOut,
        string? address,
        string? confirmationCode,
        decimal? cost)
    {
        var trip = await _tripRepository.GetByIdAsync(tripId);
        if (trip == null)
        {
            throw new ArgumentException($"Trip with ID {tripId} not found");
        }

        if (checkOut <= checkIn)
        {
            throw new ArgumentException("CheckOut must be after CheckIn.");
        }

        var lodging = new Lodging
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            Name = name,
            CheckIn = checkIn.ToUniversalTime(),
            CheckOut = checkOut.ToUniversalTime(),
            Address = address ?? string.Empty,
            ConfirmationCode = confirmationCode ?? string.Empty,
            Cost = cost
        };

        await _repository.CreateAsync(lodging);
        _logger.LogInformation($"Lodging created with ID: {lodging.Id}");

        return lodging;
    }

    public async Task<Lodging?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<Lodging>> GetByTripIdAsync(Guid tripId)
    {
        return await _repository.GetByTripIdAsync(tripId);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var lodging = await _repository.GetByIdAsync(id);
        if (lodging == null)
        {
            _logger.LogWarning($"Lodging with ID {id} not found for deletion");
            return false;
        }

        await _repository.DeleteAsync(lodging);
        _logger.LogInformation($"Lodging with ID {id} deleted");

        return true;
    }
}
