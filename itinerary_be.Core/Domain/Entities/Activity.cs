using System.ComponentModel.DataAnnotations.Schema;

namespace itinerary_be.Core.Domain.Entities;

public class Activity
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("itinerary_day_id")]
    public Guid ItineraryDayId { get; set; }
    [Column("name")]
    public required string Name { get; set; }
    [Column("start_time")]
    public TimeOnly? StartTime { get; set; }
    [Column("location")]
    public string? Location { get; set; }

    public ItineraryDay? ItineraryDay { get; set; }
}