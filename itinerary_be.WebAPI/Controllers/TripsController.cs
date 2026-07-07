namespace itinerary_be.WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using itinerary_be.Infrastructure.Data;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.WebAPI.DTOs;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ItineraryDbContext _context;
    private readonly ILogger<TripsController> _logger;

    public TripsController(ItineraryDbContext context, ILogger<TripsController> logger)
    {
        _context = context;
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

        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Title = createTripDto.Title,
            StartDate = createTripDto.StartDate,
            EndDate = createTripDto.EndDate
        };

        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();

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
        var trip = await _context.Trips.FindAsync(id);

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
        var trips = await _context.Trips.ToListAsync();
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

        var trip = await _context.Trips.FindAsync(id);

        if (trip == null)
        {
            return NotFound();
        }

        trip.Title = updateTripDto.Title;
        trip.StartDate = updateTripDto.StartDate;
        trip.EndDate = updateTripDto.EndDate;

        _context.Trips.Update(trip);
        await _context.SaveChangesAsync();

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
        var trip = await _context.Trips.FindAsync(id);

        if (trip == null)
        {
            return NotFound();
        }

        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static TripResponseDto MapToResponseDto(Trip trip)
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
