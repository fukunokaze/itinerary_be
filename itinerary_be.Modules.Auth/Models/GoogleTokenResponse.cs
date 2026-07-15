namespace itinerary_be.Modules.Auth.Models;

public record GoogleTokenResponse(string IdToken, string AccessToken, string? RefreshToken, int ExpiresIn, string Scope);
