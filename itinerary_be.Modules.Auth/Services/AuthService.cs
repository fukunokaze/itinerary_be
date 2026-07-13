namespace itinerary_be.Modules.Auth.Services;

using Microsoft.Extensions.Logging;
using itinerary_be.Modules.Auth.Exceptions;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Models;

public class AuthService : IAuthService
{
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IGoogleTokenValidator googleTokenValidator,
        IUserService userService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _googleTokenValidator = googleTokenValidator;
        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<AuthResult> LoginWithGoogleAsync(string idToken)
    {
        var googleUser = await _googleTokenValidator.ValidateAsync(idToken);

        if (!googleUser.EmailVerified)
        {
            _logger.LogWarning($"Google login rejected: email {googleUser.Email} not verified");
            throw new InvalidGoogleTokenException("Google email is not verified.");
        }

        var user = await _userService.GetOrCreateUserAsync(googleUser.Email, googleUser.Name);
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);

        _logger.LogInformation($"Issued JWT for user {user.Id}");

        return new AuthResult(user, token, expiresAt);
    }
}
