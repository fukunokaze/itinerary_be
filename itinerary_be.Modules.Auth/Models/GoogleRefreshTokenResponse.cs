namespace itinerary_be.Modules.Auth.Models;

public record GoogleRefreshTokenResponse(string AccessToken, int ExpiresIn, string Scope);
