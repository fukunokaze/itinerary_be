namespace itinerary_be.WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using itinerary_be.WebAPI.DTOs;
using itinerary_be.Modules.Itinerary.Interfaces;

[ApiController]
[Route("api/trips/{tripId}/flights")]
public class FlightsController : ControllerBase
{
    private readonly IFlightService _flightService;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(IFlightService flightService, ILogger<FlightsController> logger)
    {
        _flightService = flightService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new Flight
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FlightResponseDto>> CreateFlight(Guid tripId, [FromBody] CreateFlightDto createFlightDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var flight = await _flightService.CreateAsync(
                tripId,
                createFlightDto.FlightNumber,
                createFlightDto.DepartureTime,
                createFlightDto.ArrivalTime,
                createFlightDto.Airline,
                createFlightDto.Seat,
                createFlightDto.ConfirmationCode,
                createFlightDto.Route
            );

            var flightResponseDto = MapToResponseDto(flight);
            return CreatedAtAction(nameof(GetFlights), new { tripId = tripId }, flightResponseDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed when creating flight.");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all Flights for a Trip
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FlightResponseDto>>> GetFlights(Guid tripId)
    {
        var flights = await _flightService.GetByTripIdAsync(tripId);
        var flightDtos = flights.Select(MapToResponseDto).ToList();
        return Ok(flightDtos);
    }

    /// <summary>
    /// Delete a Flight
    /// </summary>
    [HttpDelete("{flightId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFlight(Guid tripId, Guid flightId)
    {
        var success = await _flightService.DeleteAsync(flightId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static FlightResponseDto MapToResponseDto(itinerary_be.Core.Domain.Entities.Flight flight)
    {
        return new FlightResponseDto
        {
            Id = flight.Id,
            TripId = flight.TripId,
            FlightNumber = flight.FlightNumber,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            Airline = flight.Airline,
            Seat = flight.Seat,
            ConfirmationCode = flight.ConfirmationCode,
            Route = flight.Route
        };
    }
}
