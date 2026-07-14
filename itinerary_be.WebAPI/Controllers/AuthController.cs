namespace itinerary_be.WebAPI.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using itinerary_be.Modules.Auth.Exceptions;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.WebAPI.DTOs;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Logs in with a Google ID token, auto-registering a new user profile on first login.
    /// </summary>
    [HttpPost("google")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> LoginWithGoogle([FromBody] GoogleLoginDto googleLoginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(googleLoginDto.IdToken))
        {
            return BadRequest(new { message = "IdToken is required." });
        }
        try
        {
            var result = await _authService.LoginWithGoogleAsync(googleLoginDto.IdToken);

            return Ok(new AuthResponseDto
            {
                AccessToken = result.AccessToken,
                ExpiresAt = result.ExpiresAtUtc,
                User = new UserResponseDto
                {
                    Id = result.User.Id,
                    Email = result.User.Email,
                    Name = result.User.Name
                }
            });
        }
        catch (InvalidGoogleTokenException ex)
        {
            _logger.LogWarning(ex, "Google login failed.");
            return Unauthorized(new { message = "Invalid Google ID token." });
        }
    }
}
