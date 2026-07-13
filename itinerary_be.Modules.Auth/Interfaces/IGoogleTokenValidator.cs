namespace itinerary_be.Modules.Auth.Interfaces;

using itinerary_be.Modules.Auth.Models;

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo> ValidateAsync(string idToken);
}
