namespace itinerary_be.WebAPI.DTOs;

public class CalendarDayDetailDto
{
    public DateOnly Date { get; set; }

    public List<CalendarEntryDto> Entries { get; set; } = new List<CalendarEntryDto>();
}
