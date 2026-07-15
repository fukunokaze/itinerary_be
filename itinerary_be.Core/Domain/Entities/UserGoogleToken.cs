using System.ComponentModel.DataAnnotations.Schema;

namespace itinerary_be.Core.Domain.Entities;
public class UserGoogleToken
{
    [Column("user_id")]
    public Guid UserId { get; set; }
    [Column("access_token")]
    public required string AccessToken { get; set; }
    [Column("refresh_token")]
    public string? RefreshToken { get; set; }
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }
    [Column("scope")]
    public required string Scope { get; set; }
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
