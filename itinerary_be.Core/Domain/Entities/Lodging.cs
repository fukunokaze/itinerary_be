using System.ComponentModel.DataAnnotations.Schema;
namespace itinerary_be.Core.Domain.Entities;

public class Lodging
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("trip_id")]
    public Guid TripId { get; set; }
    [Column("name")]
    public required string Name { get; set; }
    [Column("check_in")]
    public DateTimeOffset CheckIn { get; set; }
    [Column("check_out")]
    public DateTimeOffset CheckOut { get; set; }

    public Trip? Trip { get; set; }
}