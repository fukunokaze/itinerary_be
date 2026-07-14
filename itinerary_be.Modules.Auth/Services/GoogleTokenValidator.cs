namespace itinerary_be.Modules.Auth.Services;

using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using itinerary_be.Modules.Auth.Exceptions;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Models;
using itinerary_be.Modules.Auth.Options;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleAuthOptions _options;

    public GoogleTokenValidator(IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleUserInfo> ValidateAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _options.ClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleUserInfo(payload.Email, payload.EmailVerified, payload.Name);
        }
        catch (Exception ex) when (ex is InvalidJwtException or ArgumentException)
        {
            throw new InvalidGoogleTokenException("Google ID token failed validation.", ex);
        }
    }
}
