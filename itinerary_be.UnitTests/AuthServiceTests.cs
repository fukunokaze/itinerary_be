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
    private readonly Mock<IGoogleTokenValidator> _mockGoogleTokenValidator;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockGoogleTokenValidator = new Mock<IGoogleTokenValidator>();
        _mockUserService = new Mock<IUserService>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _authService = new AuthService(
            _mockGoogleTokenValidator.Object,
            _mockUserService.Object,
            _mockJwtTokenService.Object,
            _mockLogger.Object);
    }

    #region LoginWithGoogleAsync Tests

    [Fact]
    public async Task LoginWithGoogleAsync_ValidTokenNewUser_CreatesUserAndReturnsToken()
    {
        // Arrange
        var idToken = "valid-id-token";
        var googleUser = new GoogleUserInfo("new@example.com", true, "New User");
        var user = new User { Id = Guid.NewGuid(), Email = googleUser.Email, Name = googleUser.Name, CreatedAt = DateTime.UtcNow };
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _mockGoogleTokenValidator.Setup(v => v.ValidateAsync(idToken)).ReturnsAsync(googleUser);
        _mockUserService.Setup(s => s.GetOrCreateUserAsync(googleUser.Email, googleUser.Name)).ReturnsAsync(user);
        _mockJwtTokenService.Setup(j => j.GenerateToken(user)).Returns(("jwt-token", expiresAt));

        // Act
        var result = await _authService.LoginWithGoogleAsync(idToken);

        // Assert
        Assert.Equal(user, result.User);
        Assert.Equal("jwt-token", result.AccessToken);
        Assert.Equal(expiresAt, result.ExpiresAtUtc);

        _mockUserService.Verify(s => s.GetOrCreateUserAsync(googleUser.Email, googleUser.Name), Times.Once);
        _mockJwtTokenService.Verify(j => j.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_ValidTokenExistingUser_ReturnsTokenWithoutDuplicateCreation()
    {
        // Arrange
        var idToken = "valid-id-token";
        var googleUser = new GoogleUserInfo("existing@example.com", true, "Existing User");
        var user = new User { Id = Guid.NewGuid(), Email = googleUser.Email, Name = googleUser.Name, CreatedAt = DateTime.UtcNow };

        _mockGoogleTokenValidator.Setup(v => v.ValidateAsync(idToken)).ReturnsAsync(googleUser);
        _mockUserService.Setup(s => s.GetOrCreateUserAsync(googleUser.Email, googleUser.Name)).ReturnsAsync(user);
        _mockJwtTokenService.Setup(j => j.GenerateToken(user)).Returns(("jwt-token", DateTime.UtcNow.AddHours(1)));

        // Act
        var result = await _authService.LoginWithGoogleAsync(idToken);

        // Assert
        Assert.Equal(user.Id, result.User.Id);
        _mockUserService.Verify(s => s.GetOrCreateUserAsync(googleUser.Email, googleUser.Name), Times.Once);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_EmailNotVerified_ThrowsInvalidGoogleTokenExceptionAndDoesNotIssueToken()
    {
        // Arrange
        var idToken = "unverified-token";
        var googleUser = new GoogleUserInfo("unverified@example.com", false, "Unverified User");

        _mockGoogleTokenValidator.Setup(v => v.ValidateAsync(idToken)).ReturnsAsync(googleUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidGoogleTokenException>(() =>
            _authService.LoginWithGoogleAsync(idToken));

        _mockUserService.Verify(s => s.GetOrCreateUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockJwtTokenService.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_GoogleValidatorThrows_ExceptionPropagates()
    {
        // Arrange
        var idToken = "garbage-token";

        _mockGoogleTokenValidator.Setup(v => v.ValidateAsync(idToken))
            .ThrowsAsync(new InvalidGoogleTokenException("Google ID token failed validation."));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidGoogleTokenException>(() =>
            _authService.LoginWithGoogleAsync(idToken));

        _mockUserService.Verify(s => s.GetOrCreateUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    #endregion
}
