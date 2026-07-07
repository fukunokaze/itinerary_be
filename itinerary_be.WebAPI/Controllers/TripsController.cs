namespace itinerary_be.WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using itinerary_be.WebAPI.DTOs;
using itinerary_be.Modules.Itinerary.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;
    private readonly ILogger<TripsController> _logger;

    public TripsController(ITripService tripService, ILogger<TripsController> logger)
    {
        _tripService = tripService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new Trip
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TripResponseDto>> CreateTrip(CreateTripDto createTripDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var trip = await _tripService.CreateTripAsync(createTripDto.Title, createTripDto.StartDate, createTripDto.EndDate);
        var tripResponseDto = MapToResponseDto(trip);
        return CreatedAtAction(nameof(GetTripById), new { id = trip.Id }, tripResponseDto);
    }

    /// <summary>
    /// Get a Trip by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TripResponseDto>> GetTripById(Guid id)
    {
        var trip = await _tripService.GetTripByIdAsync(id);

        if (trip == null)
        {
            return NotFound();
        }

        return Ok(MapToResponseDto(trip));
    }

    /// <summary>
    /// Get all Trips
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TripResponseDto>>> GetAllTrips()
    {
        var trips = await _tripService.GetAllTripsAsync();
        var tripDtos = trips.Select(MapToResponseDto).ToList();
        return Ok(tripDtos);
    }

    /// <summary>
    /// Update a Trip
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TripResponseDto>> UpdateTrip(Guid id, UpdateTripDto updateTripDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var trip = await _tripService.UpdateTripAsync(id, updateTripDto.Title, updateTripDto.StartDate, updateTripDto.EndDate);

        if (trip == null)
        {
            return NotFound();
        }

        return Ok(MapToResponseDto(trip));
    }

    /// <summary>
    /// Delete a Trip
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTrip(Guid id)
    {
        var success = await _tripService.DeleteTripAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static TripResponseDto MapToResponseDto(itinerary_be.Core.Domain.Entities.Trip trip)
    {
        return new TripResponseDto
        {
            Id = trip.Id,
            Title = trip.Title,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate
        };
    }
}
