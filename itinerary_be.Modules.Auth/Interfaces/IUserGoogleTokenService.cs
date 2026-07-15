namespace itinerary_be.Modules.Auth.Interfaces;

using itinerary_be.Modules.Auth.Models;

public interface IUserGoogleTokenService
{
    Task SaveTokensAsync(Guid userId, GoogleTokenResponse tokenResponse);
}
