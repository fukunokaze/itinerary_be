namespace itinerary_be.WebAPI.DTOs;

public class TripResponseDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
