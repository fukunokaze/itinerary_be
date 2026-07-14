using System.ComponentModel.DataAnnotations.Schema;

namespace itinerary_be.Core.Domain.Entities;
public class User
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("email")]
    public required string Email { get; set; }
    [Column("name")]
    public required string Name { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
