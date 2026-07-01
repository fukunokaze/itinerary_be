namespace itinerary_be.Core.Domain.Entities;

public class Lodging
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CheckIn { get; set; }
    public DateTimeOffset CheckOut { get; set; }

    public Trip? Trip { get; set; }
}