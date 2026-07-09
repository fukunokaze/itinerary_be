namespace itinerary_be.Modules.Itinerary.Services;

using Microsoft.Extensions.Logging;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Core.Domain.Enums;
using itinerary_be.Modules.Itinerary.Interfaces;

public class TripEventService : ITripEventService
{
    private readonly ITripEventRepository _repository;
    private readonly ITripRepository _tripRepository;
    private readonly ILogger<TripEventService> _logger;

    public TripEventService(ITripEventRepository repository, ITripRepository tripRepository, ILogger<TripEventService> logger)
    {
        _repository = repository;
        _tripRepository = tripRepository;
        _logger = logger;
    }

    public async Task<TripEvent> CreateAsync(
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
        decimal? cost)
    {
        // Verify the trip exists
        var trip = await _tripRepository.GetByIdAsync(tripId);
        if (trip == null)
        {
            throw new ArgumentException($"Trip with ID {tripId} not found");
        }

        // Validate the event date falls within the trip's date range
        if (date < trip.StartDate || date > trip.EndDate)
        {
            throw new ArgumentException($"Event date must be between {trip.StartDate:yyyy-MM-dd} and {trip.EndDate:yyyy-MM-dd}.");
        }

        // Validate overlapping times if both start and end times are provided
        if (startTime.HasValue && endTime.HasValue)
        {
            var existingEvents = await _repository.GetByTripIdAsync(tripId);
            var overlappingEvents = existingEvents.Where(e => 
                e.Date == date && 
                e.StartTime.HasValue && 
                e.EndTime.HasValue &&
                startTime.Value < e.EndTime.Value && 
                endTime.Value > e.StartTime.Value);

            if (overlappingEvents.Any())
            {
                throw new ArgumentException("The event overlaps with an existing event on the same day.");
            }
        }

        var tripEvent = new TripEvent
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            Type = type,
            Title = title,
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            Location = location,
            Notes = notes,
            BookingCode = bookingCode,
            ImageUrl = imageUrl,
            Tags = tags,
            Cost = cost
        };

        await _repository.CreateAsync(tripEvent);
        _logger.LogInformation($"Trip Event created with ID: {tripEvent.Id}");

        return tripEvent;
    }

    public async Task<TripEvent?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<TripEvent>> GetByTripIdAsync(Guid tripId)
    {
        return await _repository.GetByTripIdAsync(tripId);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tripEvent = await _repository.GetByIdAsync(id);
        if (tripEvent == null)
        {
            _logger.LogWarning($"Trip Event with ID {id} not found for deletion");
            return false;
        }

        await _repository.DeleteAsync(tripEvent);
        _logger.LogInformation($"Trip Event with ID {id} deleted");
        
        return true;
    }
}
