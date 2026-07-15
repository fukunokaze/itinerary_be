namespace itinerary_be.UnitTests;

using Moq;
using Microsoft.Extensions.Logging;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Auth.Exceptions;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Models;
using itinerary_be.Modules.Auth.Services;

/// <summary>
/// Unit tests for AuthService class
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IGoogleOAuthClient> _mockGoogleOAuthClient;
    private readonly Mock<IGoogleTokenValidator> _mockGoogleTokenValidator;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockGoogleOAuthClient = new Mock<IGoogleOAuthClient>();
        _mockGoogleTokenValidator = new Mock<IGoogleTokenValidator>();
        _mockUserService = new Mock<IUserService>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _authService = new AuthService(
            _mockGoogleOAuthClient.Object,
            _mockGoogleTokenValidator.Object,
            _mockUserService.Object,
            _mockJwtTokenService.Object,
            _mockLogger.Object);
    }

    #region LoginWithGoogleAsync Tests

    [Fact]
    public async Task LoginWithGoogleAsync_ValidCodeNewUser_CreatesUserAndReturnsToken()
    {
        // Arrange
        var code = "valid-code";
        var idToken = "id-token-from-exchange";
        var tokenResponse = new GoogleTokenResponse(idToken, "access-token", "refresh-token", 3600, "openid email profile https://www.googleapis.com/auth/calendar.readonly");
        var googleUser = new GoogleUserInfo("new@example.com", true, "New User");
        var user = new User { Id = Guid.NewGuid(), Email = googleUser.Email, Name = googleUser.Name, CreatedAt = DateTime.UtcNow };
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _mockGoogleOAuthClient.Setup(c => c.ExchangeCodeAsync(code)).ReturnsAsync(tokenResponse);
        _mockGoogleTokenValidator.Setup(v => v.ValidateAsync(idToken)).ReturnsAsync(googleUser);
        _mockUserService.Setup(s => s.GetOrCreateUserAsync(googleUser.Email, googleUser.Name)).ReturnsAsync(user);
        _mockJwtTokenService.Setup(j => j.GenerateToken(user)).Returns(("jwt-token", expiresAt));

        // Act
        var result = await _authService.LoginWithGoogleAsync(code);

        // Assert
        Assert.Equal(user, result.User);
        Assert.Equal("jwt-token", result.AccessToken);
        Assert.Equal(expiresAt, result.ExpiresAtUtc);

        _mockUserService.Verify(s => s.GetOrCreateUserAsync(googleUser.Email, googleUser.Name), Times.Once);
        _mockJwtTokenService.Verify(j => j.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_ValidCodeExistingUser_ReturnsTokenWithoutDuplicateCreation()
    {
        // Arrange
        var code = "valid-code";
        var idToken = "id-token-from-exchange";
        var tokenResponse = new GoogleTokenResponse(idToken, "access-token", "refresh-token", 3600, "openid email profile https://www.googleapis.com/auth/calendar.readonly");
        var googleUser = new GoogleUserInfo("existing@example.com", true, "Existing User");
        var user = new User { Id = Guid.NewGuid(), Email = googleUser.Email, Name = googleUser.Name, CreatedAt = DateTime.UtcNow };

        _mockGoogleOAuthClient.Setup(c => c.ExchangeCodeAsync(code)).ReturnsAsync(tokenResponse);
        _mockGoogleTokenValidator.Setup(v => v.ValidateAsync(idToken)).ReturnsAsync(googleUser);
        _mockUserService.Setup(s => s.GetOrCreateUserAsync(googleUser.Email, googleUser.Name)).ReturnsAsync(user);
        _mockJwtTokenService.Setup(j => j.GenerateToken(user)).Returns(("jwt-token", DateTime.UtcNow.AddHours(1)));

        // Act
        var result = await _authService.LoginWithGoogleAsync(code);

        // Assert
        Assert.Equal(user.Id, result.User.Id);
        _mockUserService.Verify(s => s.GetOrCreateUserAsync(googleUser.Email, googleUser.Name), Times.Once);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_EmailNotVerified_ThrowsInvalidGoogleTokenExceptionAndDoesNotIssueToken()
    {
        // Arrange
        var code = "valid-code";
        var idToken = "id-token-from-exchange";
        var tokenResponse = new GoogleTokenResponse(idToken, "access-token", "refresh-token", 3600, "openid email profile");
        var googleUser = new GoogleUserInfo("unverified@example.com", false, "Unverified User");

        _mockGoogleOAuthClient.Setup(c => c.ExchangeCodeAsync(code)).ReturnsAsync(tokenResponse);
        _mockGoogleTokenValidator.Setup(v => v.ValidateAsync(idToken)).ReturnsAsync(googleUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidGoogleTokenException>(() =>
            _authService.LoginWithGoogleAsync(code));

        _mockUserService.Verify(s => s.GetOrCreateUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockJwtTokenService.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_GoogleValidatorThrows_ExceptionPropagates()
    {
        // Arrange
        var code = "valid-code";
        var idToken = "garbage-id-token";
        var tokenResponse = new GoogleTokenResponse(idToken, "access-token", "refresh-token", 3600, "openid email profile");

        _mockGoogleOAuthClient.Setup(c => c.ExchangeCodeAsync(code)).ReturnsAsync(tokenResponse);
        _mockGoogleTokenValidator.Setup(v => v.ValidateAsync(idToken))
            .ThrowsAsync(new InvalidGoogleTokenException("Google ID token failed validation."));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidGoogleTokenException>(() =>
            _authService.LoginWithGoogleAsync(code));

        _mockUserService.Verify(s => s.GetOrCreateUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_CodeExchangeThrows_ExceptionPropagatesAndDoesNotValidateToken()
    {
        // Arrange
        var code = "bad-code";

        _mockGoogleOAuthClient.Setup(c => c.ExchangeCodeAsync(code))
            .ThrowsAsync(new InvalidGoogleTokenException("Google token exchange failed."));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidGoogleTokenException>(() =>
            _authService.LoginWithGoogleAsync(code));

        _mockGoogleTokenValidator.Verify(v => v.ValidateAsync(It.IsAny<string>()), Times.Never);
        _mockUserService.Verify(s => s.GetOrCreateUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockJwtTokenService.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    #endregion
}
