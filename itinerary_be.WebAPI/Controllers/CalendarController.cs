namespace itinerary_be.WebAPI.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using itinerary_be.WebAPI.DTOs;
using itinerary_be.Modules.Utility.Interfaces;
using itinerary_be.Modules.Utility.Models;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Exceptions;

[ApiController]
[Route("api/[controller]")]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(ICalendarService calendarService, IUserRepository userRepository, ILogger<CalendarController> logger)
    {
        _calendarService = calendarService;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get compact per-day markers for a month, for the calendar grid view.
    /// </summary>
    [HttpGet("month")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<IEnumerable<CalendarDayMarkerDto>>> GetMonth([FromQuery] int year, [FromQuery] int month, [FromQuery] string timeZoneId)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return Unauthorized();
        }

        if (month < 1 || month > 12)
        {
            return BadRequest(new { message = "Month must be between 1 and 12." });
        }

        DateOnly startDate;
        try
        {
            startDate = new DateOnly(year, month, 1);
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest(new { message = "Invalid year or month." });
        }

        var endDate = startDate.AddMonths(1).AddDays(-1);

        try
        {
            var days = await _calendarService.GetCalendarAsync(currentUser.Id, startDate, endDate, timeZoneId);
            return Ok(days.Select(MapToMarkerDto));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (GoogleReauthorizationRequiredException ex)
        {
            _logger.LogWarning(ex, "Google reauthorization required for user {UserId}.", currentUser.Id);
            return Unauthorized(new { message = "Google Calendar authorization has expired. Please reconnect your Google account." });
        }
        catch (GoogleCalendarApiException ex)
        {
            _logger.LogError(ex, "Google Calendar API call failed for user {UserId}.", currentUser.Id);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Failed to retrieve Google Calendar events." });
        }
    }

    /// <summary>
    /// Get full event details for a single day, for the day-detail dialog.
    /// </summary>
    [HttpGet("day")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<CalendarDayDetailDto>> GetDay([FromQuery] DateOnly date, [FromQuery] string timeZoneId)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return Unauthorized();
        }

        try
        {
            var days = await _calendarService.GetCalendarAsync(currentUser.Id, date, date, timeZoneId);
            return Ok(MapToDetailDto(days[0]));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (GoogleReauthorizationRequiredException ex)
        {
            _logger.LogWarning(ex, "Google reauthorization required for user {UserId}.", currentUser.Id);
            return Unauthorized(new { message = "Google Calendar authorization has expired. Please reconnect your Google account." });
        }
        catch (GoogleCalendarApiException ex)
        {
            _logger.LogError(ex, "Google Calendar API call failed for user {UserId}.", currentUser.Id);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Failed to retrieve Google Calendar events." });
        }
    }

    private async Task<itinerary_be.Core.Domain.Entities.User?> GetCurrentUserAsync()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return await _userRepository.GetByEmailAsync(email);
    }

    private static CalendarDayMarkerDto MapToMarkerDto(CalendarDay day)
    {
        return new CalendarDayMarkerDto
        {
            Date = day.Date,
            HasEvents = day.Entries.Any(e => e.Source != CalendarEntrySource.TripRange),
            IsTripDay = day.Entries.Any(e => e.Source == CalendarEntrySource.TripRange)
        };
    }

    private static CalendarDayDetailDto MapToDetailDto(CalendarDay day)
    {
        return new CalendarDayDetailDto
        {
            Date = day.Date,
            Entries = day.Entries.Select(MapToEntryDto).ToList()
        };
    }

    private static CalendarEntryDto MapToEntryDto(CalendarEntry entry)
    {
        return new CalendarEntryDto
        {
            Id = entry.Id,
            Source = entry.Source.ToString(),
            Title = entry.Title,
            Start = entry.Start,
            End = entry.End,
            IsAllDay = entry.IsAllDay,
            Description = entry.Description,
            Location = entry.Location,
            TripId = entry.TripId,
            TripEventId = entry.TripEventId
        };
    }
}
