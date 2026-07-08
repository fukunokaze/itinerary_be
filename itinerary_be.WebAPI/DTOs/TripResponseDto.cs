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

/*
 id: string;
  name: string;
  address: string;
  startDate: string;
  endDate: string;
  nights: number;
  confirmationCode?: string;
*/
public class LodgingResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Nights { get; set; }
    public string? ConfirmationCode { get; set; }
}

/*
 id: string;
  route: string;
  airline: string;
  flightNumber: string;
  date: string;
  seat?: string;
  confirmationCode?: string;
*/
public class FlightResponseDto
{
    public Guid Id { get; set; }
    public string Route { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Seat { get; set; }
    public string? ConfirmationCode { get; set; }
}