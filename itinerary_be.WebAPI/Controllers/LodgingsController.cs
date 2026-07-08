namespace itinerary_be.WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using itinerary_be.WebAPI.DTOs;
using itinerary_be.Modules.Itinerary.Interfaces;
using itinerary_be.Core.Domain.Entities;

[ApiController]
[Route("api/trips/{tripId}/lodgings")]
public class LodgingsController : ControllerBase
{
    private readonly ILodgingService _lodgingService;
    private readonly ILogger<LodgingsController> _logger;

    public LodgingsController(ILodgingService lodgingService, ILogger<LodgingsController> logger)
    {
        _lodgingService = lodgingService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new Lodging for a Trip
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LodgingResponseDto>> CreateLodging(Guid tripId, [FromBody] CreateLodgingDto createLodgingDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var lodging = await _lodgingService.CreateAsync(
                tripId,
                createLodgingDto.Name,
                createLodgingDto.CheckIn,
                createLodgingDto.CheckOut,
                createLodgingDto.Address,
                createLodgingDto.ConfirmationCode
            );

            var lodgingResponseDto = MapToResponseDto(lodging);
            return CreatedAtAction(nameof(GetLodgings), new { tripId }, lodgingResponseDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed when creating lodging.");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all Lodgings for a Trip
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LodgingResponseDto>>> GetLodgings(Guid tripId)
    {
        var lodgings = await _lodgingService.GetByTripIdAsync(tripId);
        var lodgingDtos = lodgings.Select(MapToResponseDto).ToList();
        return Ok(lodgingDtos);
    }

    /// <summary>
    /// Delete a Lodging
    /// </summary>
    [HttpDelete("{lodgingId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLodging(Guid tripId, Guid lodgingId)
    {
        var success = await _lodgingService.DeleteAsync(lodgingId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static LodgingResponseDto MapToResponseDto(Lodging lodging)
    {
        return new LodgingResponseDto
        {
            Id = lodging.Id,
            Name = lodging.Name,
            Address = lodging.Address,
            CheckIn = lodging.CheckIn.DateTime,
            CheckOut = lodging.CheckOut.DateTime,
            Nights = (lodging.CheckOut.Date - lodging.CheckIn.Date).Days,
            ConfirmationCode = lodging.ConfirmationCode
        };
    }
}
