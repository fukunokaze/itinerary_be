namespace itinerary_be.Modules.Auth.Services;

using Microsoft.Extensions.Logging;
using itinerary_be.Modules.Auth.Exceptions;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Models;

public class AuthService : IAuthService
{
    private readonly IGoogleOAuthClient _googleOAuthClient;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IGoogleOAuthClient googleOAuthClient,
        IGoogleTokenValidator googleTokenValidator,
        IUserService userService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _googleOAuthClient = googleOAuthClient;
        _googleTokenValidator = googleTokenValidator;
        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<AuthResult> LoginWithGoogleAsync(string code)
    {
        var tokenResponse = await _googleOAuthClient.ExchangeCodeAsync(code);
        var googleUser = await _googleTokenValidator.ValidateAsync(tokenResponse.IdToken);

        if (!googleUser.EmailVerified)
        {
            _logger.LogWarning("Google login rejected: email {Email} not verified", googleUser.Email);
            throw new InvalidGoogleTokenException("Google email is not verified.");
        }

        var user = await _userService.GetOrCreateUserAsync(googleUser.Email, googleUser.Name);
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);
        _logger.LogInformation("Issued JWT for user {UserId}", user.Id);

        // tokenResponse.AccessToken / RefreshToken are intentionally discarded here —
        // persistence and encryption of Google tokens is tracked separately (KAN-11).
        return new AuthResult(user, token, expiresAt);
    }
}
