namespace itinerary_be.Modules.Auth.Interfaces;

using itinerary_be.Modules.Auth.Models;

public interface IAuthService
{
    Task<AuthResult> LoginWithGoogleAsync(string idToken);
}
