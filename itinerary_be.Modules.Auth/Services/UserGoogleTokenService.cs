namespace itinerary_be.Modules.Auth.Services;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Models;

public class UserGoogleTokenService : IUserGoogleTokenService
{
    private const string ProtectorPurpose = "itinerary_be.Modules.Auth.UserGoogleToken";

    private readonly IUserGoogleTokenRepository _repository;
    private readonly IDataProtector _protector;
    private readonly ILogger<UserGoogleTokenService> _logger;

    public UserGoogleTokenService(
        IUserGoogleTokenRepository repository,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<UserGoogleTokenService> logger)
    {
        _repository = repository;
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        _logger = logger;
    }

    public async Task SaveTokensAsync(Guid userId, GoogleTokenResponse tokenResponse)
    {
        // Google only returns a refresh_token on a user's first consent; preserve the
        // previously stored one rather than overwriting it with null on later logins.
        var existing = await _repository.GetByUserIdAsync(userId);
        var encryptedRefreshToken = tokenResponse.RefreshToken != null
            ? _protector.Protect(tokenResponse.RefreshToken)
            : existing?.RefreshToken;

        var token = new UserGoogleToken
        {
            UserId = userId,
            AccessToken = _protector.Protect(tokenResponse.AccessToken),
            RefreshToken = encryptedRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
            Scope = tokenResponse.Scope,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.UpsertAsync(token);
        _logger.LogInformation("Saved Google tokens for user {UserId}", userId);
    }
}
