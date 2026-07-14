namespace itinerary_be.WebAPI.DTOs;

public class AuthResponseDto
{
    public required string AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public required UserResponseDto User { get; set; }
}
