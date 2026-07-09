namespace itinerary_be.WebAPI.DTOs;

public class TripResponseDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;

    public ICollection<TripEventResponseDto> Events { get; set; } = new List<TripEventResponseDto>();

    public ICollection<FlightResponseDto> Flights { get; set; } = new List<FlightResponseDto>();

    public ICollection<LodgingResponseDto> Lodgings { get; set; } = new List<LodgingResponseDto>();
    public ICollection<DocumentResponseDto> Documents { get; set; } = new List<DocumentResponseDto>();
    public ICollection<TravelerResponseDto> Travelers { get; set; } = new List<TravelerResponseDto>()
    {
        new TravelerResponseDto { Id = Guid.NewGuid(), Name = "John Doe" },
        new TravelerResponseDto { Id = Guid.NewGuid(), Name = "Jane Smith" }
    };
    
    public string? Notes { get; set; } = string.Empty;
}

public class TravelerResponseDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}

public class DocumentResponseDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Icon { get; set; }
}

public class LodgingResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int Nights { get; set; }
    public string? ConfirmationCode { get; set; }
}

public class FlightResponseDto
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public DateTimeOffset DepartureTime { get; set; }
    public DateTimeOffset ArrivalTime { get; set; }
    public string? Airline { get; set; }
    public string? Seat { get; set; }
    public string? ConfirmationCode { get; set; }
    public string? Route { get; set; }
}