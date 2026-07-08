namespace itinerary_be.WebAPI.DTOs;

using itinerary_be.Core.Domain.Enums;

public class TripEventResponseDto
{
    public Guid Id { get; set; }
    
    public Guid TripId { get; set; }
    
    public string Type { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    
    public DateOnly Date { get; set; }
    
    public TimeOnly? StartTime { get; set; }
    
    public TimeOnly? EndTime { get; set; }
    
    public string? Location { get; set; }
    
    public string? Notes { get; set; }
    
    public string? BookingCode { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public string? Tags { get; set; }
}
