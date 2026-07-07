namespace itinerary_be.WebAPI.DTOs;

using System.ComponentModel.DataAnnotations;

public class UpdateTripDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public required string Title { get; set; }

    [Required(ErrorMessage = "StartDate is required")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "EndDate is required")]
    public DateOnly EndDate { get; set; }
}
