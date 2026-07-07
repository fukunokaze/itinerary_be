using System.ComponentModel.DataAnnotations.Schema;
using itinerary_be.Core.Domain.Enums;

namespace itinerary_be.Core.Domain.Entities;

/*
export interface TripEvent {
  id: string;
  type: TripEventType;
  title: string;
  date: string;
  startTime?: string;
  endTime?: string;
  location?: string;
  notes?: string;
  bookingCode?: string;
  imageUrl?: string;
  tags?: string[];
  travelerIds?: string[];
  relatedItemIds?: string[];
}
*/

public class TripEvent
{
    [Column ("id")]
    public Guid Id { get; set; }
    [Column ("trip_id")]
    public Guid TripId { get; set; }
    [Column ("type")]
    public required EventTypes Type { get; set; }
    [Column ("title")]
    public required string Title { get; set; }
    [Column ("date")]
    public DateOnly Date { get; set; }
    [Column ("start_time")]
    public TimeOnly? StartTime { get; set; }
    [Column ("end_time")]
    public TimeOnly? EndTime { get; set; }
    [Column ("location")]
    public string? Location { get; set; } = string.Empty;
    [Column ("notes")]
    public string? Notes { get; set; } = string.Empty;
    [Column ("booking_code")]
    public string? BookingCode { get; set; } = string.Empty;
    [Column ("image_url")]
    public string? ImageUrl { get; set; } = string.Empty;
    [Column ("tags")]
    public string? Tags { get; set; } = string.Empty;
    // public List<Guid>? TravelerIds { get; set; } = new List<Guid>();
    // public List<Guid>? RelatedItemIds { get; set; } = new List<Guid>();

    public Trip? Trip { get; set; } 
}