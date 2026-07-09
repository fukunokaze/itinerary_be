namespace itinerary_be.WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using itinerary_be.WebAPI.DTOs;
using itinerary_be.Modules.Itinerary.Interfaces;
using itinerary_be.Core.Domain.Enums;


[ApiController]
[Route("api/trips/{tripId}/events")]
public class TripEventsController : ControllerBase
{
    private readonly ITripEventService _tripEventService;
    private readonly ILogger<TripEventsController> _logger;

    public TripEventsController(ITripEventService tripEventService, ILogger<TripEventsController> logger)
    {
        _tripEventService = tripEventService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new Trip Event
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TripEventResponseDto>> CreateTripEvent(Guid tripId, [FromBody] CreateTripEventDto createTripEventDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var tripEvent = await _tripEventService.CreateAsync(
                tripId,
                createTripEventDto.Type,
                createTripEventDto.Title,
                createTripEventDto.Date,
                createTripEventDto.StartTime,
                createTripEventDto.EndTime,
                createTripEventDto.Location,
                createTripEventDto.Notes,
                createTripEventDto.BookingCode,
                null,
                null,
                createTripEventDto.Cost
            );

            var tripEventResponseDto = MapToResponseDto(tripEvent);
            return CreatedAtAction(nameof(GetTripEvents), new { tripId = tripId }, tripEventResponseDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed when creating trip event.");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all Trip Events for a Trip
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TripEventResponseDto>>> GetTripEvents(Guid tripId)
    {
        var events = await _tripEventService.GetByTripIdAsync(tripId);
        var eventDtos = events.Select(MapToResponseDto).ToList();
        return Ok(eventDtos);
    }

    /// <summary>
    /// Delete a Trip Event
    /// </summary>
    [HttpDelete("{eventId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTripEvent(Guid tripId, Guid eventId)
    {
        var success = await _tripEventService.DeleteAsync(eventId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static TripEventResponseDto MapToResponseDto(itinerary_be.Core.Domain.Entities.TripEvent tripEvent)
    {
        return new TripEventResponseDto
        {
            Id = tripEvent.Id,
            TripId = tripEvent.TripId,
            Type = tripEvent.Type.ToFriendlyString(),
            Title = tripEvent.Title,
            Date = tripEvent.Date,
            StartTime = tripEvent.StartTime,
            EndTime = tripEvent.EndTime,
            Location = tripEvent.Location,
            Notes = tripEvent.Notes,
            BookingCode = tripEvent.BookingCode,
            ImageUrl = tripEvent.ImageUrl,
            Tags = tripEvent.Tags,
            Cost = tripEvent.Cost
        };
    }
}
