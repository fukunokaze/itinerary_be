namespace itinerary_be.WebAPI.DTOs;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
}
