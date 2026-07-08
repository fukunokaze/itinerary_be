namespace itinerary_be.WebAPI.DTOs;

using itinerary_be.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


public class CreateTripEventDto
{
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required EventTypes Type { get; set; }

    [Required]
    public required string Title { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string? Location { get; set; }

    public string? Notes { get; set; }

    public string? BookingCode { get; set; }

    public List<string>? TravelerIds { get; set; } = new List<string>();

    public List<string>? RelatedItemIds { get; set; } = new List<string>();

}
