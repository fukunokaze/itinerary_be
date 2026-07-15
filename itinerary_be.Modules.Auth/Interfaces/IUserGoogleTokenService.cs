namespace itinerary_be.Modules.Auth.Interfaces;

using itinerary_be.Modules.Auth.Models;

public interface IUserGoogleTokenService
{
    Task SaveTokensAsync(Guid userId, GoogleTokenResponse tokenResponse);

    /// <summary>
    /// Returns a currently-valid, decrypted Google access token for the user, transparently
    /// refreshing it via the stored refresh token if the cached one is expired or about to expire.
    /// </summary>
    /// <exception cref="itinerary_be.Modules.Auth.Exceptions.GoogleReauthorizationRequiredException">
    /// No Google authorization is on file for the user, or Google reports the refresh token is invalid/revoked.
    /// </exception>
    Task<string> GetValidAccessTokenAsync(Guid userId);
}
