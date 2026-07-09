namespace itinerary_be.WebAPI.DTOs;

using System.ComponentModel.DataAnnotations;

public class CreateFlightDto
{
    [Required]
    public required string FlightNumber { get; set; }

    [Required]
    public DateTimeOffset DepartureTime { get; set; }

    [Required]
    public DateTimeOffset ArrivalTime { get; set; }

    public string? Airline { get; set; }

    public string? Seat { get; set; }

    public string? ConfirmationCode { get; set; }

    public string? Route { get; set; }
}
