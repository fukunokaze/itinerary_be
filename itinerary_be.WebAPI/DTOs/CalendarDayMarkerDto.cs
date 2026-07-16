namespace itinerary_be.WebAPI.DTOs;

public class CalendarDayMarkerDto
{
    public DateOnly Date { get; set; }

    public bool HasEvents { get; set; }

    public bool IsTripDay { get; set; }
}
