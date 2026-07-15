namespace itinerary_be.Modules.Auth.Interfaces;

using itinerary_be.Modules.Auth.Models;

public interface IGoogleOAuthClient
{
    Task<GoogleTokenResponse> ExchangeCodeAsync(string code);
    Task<GoogleRefreshTokenResponse> RefreshAccessTokenAsync(string refreshToken);
}
