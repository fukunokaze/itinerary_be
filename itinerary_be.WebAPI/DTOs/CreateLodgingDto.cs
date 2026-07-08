namespace itinerary_be.WebAPI.DTOs;

using System.ComponentModel.DataAnnotations;

public class CreateLodgingDto
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public DateTimeOffset CheckIn { get; set; }

    [Required]
    public DateTimeOffset CheckOut { get; set; }

    public string? Address { get; set; }

    public string? ConfirmationCode { get; set; }
}
