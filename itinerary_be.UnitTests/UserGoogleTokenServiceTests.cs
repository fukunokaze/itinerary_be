namespace itinerary_be.UnitTests;

using System.Linq;
using Moq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Models;
using itinerary_be.Modules.Auth.Services;

/// <summary>
/// Unit tests for UserGoogleTokenService class
/// </summary>
public class UserGoogleTokenServiceTests
{
    private readonly Mock<IUserGoogleTokenRepository> _mockRepository;
    private readonly Mock<IDataProtectionProvider> _mockDataProtectionProvider;
    private readonly Mock<IDataProtector> _mockProtector;
    private readonly Mock<ILogger<UserGoogleTokenService>> _mockLogger;
    private readonly UserGoogleTokenService _service;

    public UserGoogleTokenServiceTests()
    {
        _mockRepository = new Mock<IUserGoogleTokenRepository>();
        _mockProtector = new Mock<IDataProtector>();
        // Fake "encryption": reverse the bytes. Deterministic per input, never equal to plaintext.
        _mockProtector.Setup(p => p.Protect(It.IsAny<byte[]>()))
            .Returns<byte[]>(bytes => bytes.Reverse().ToArray());

        _mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
        _mockDataProtectionProvider.Setup(p => p.CreateProtector(It.IsAny<string>()))
            .Returns(_mockProtector.Object);

        _mockLogger = new Mock<ILogger<UserGoogleTokenService>>();
        _service = new UserGoogleTokenService(_mockRepository.Object, _mockDataProtectionProvider.Object, _mockLogger.Object);
    }

    #region SaveTokensAsync Tests

    [Fact]
    public async Task SaveTokensAsync_NewUser_EncryptsAndUpsertsTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenResponse = new GoogleTokenResponse("id-token", "access-token", "refresh-token", 3600, "openid email profile calendar.readonly");

        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((UserGoogleToken?)null);

        UserGoogleToken? captured = null;
        _mockRepository.Setup(r => r.UpsertAsync(It.IsAny<UserGoogleToken>()))
            .Callback<UserGoogleToken>(t => captured = t)
            .Returns(Task.CompletedTask);

        var expectedAccessToken = _mockProtector.Object.Protect(tokenResponse.AccessToken);
        var expectedRefreshToken = _mockProtector.Object.Protect(tokenResponse.RefreshToken!);

        // Act
        await _service.SaveTokensAsync(userId, tokenResponse);

        // Assert
        Assert.NotNull(captured);
        Assert.Equal(userId, captured!.UserId);
        Assert.Equal(expectedAccessToken, captured.AccessToken);
        Assert.Equal(expectedRefreshToken, captured.RefreshToken);
        Assert.NotEqual(tokenResponse.AccessToken, captured.AccessToken);
        Assert.Equal(tokenResponse.Scope, captured.Scope);
        Assert.True(captured.ExpiresAt > DateTime.UtcNow.AddMinutes(59) && captured.ExpiresAt < DateTime.UtcNow.AddMinutes(61));

        _mockRepository.Verify(r => r.UpsertAsync(It.IsAny<UserGoogleToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveTokensAsync_ExistingUserRefreshTokenOmitted_PreservesPreviousRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenResponse = new GoogleTokenResponse("id-token", "new-access-token", null, 3600, "openid email profile calendar.readonly");
        var existing = new UserGoogleToken
        {
            UserId = userId,
            AccessToken = "old-encrypted-access-token",
            RefreshToken = "old-encrypted-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5),
            Scope = "openid email profile",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existing);

        UserGoogleToken? captured = null;
        _mockRepository.Setup(r => r.UpsertAsync(It.IsAny<UserGoogleToken>()))
            .Callback<UserGoogleToken>(t => captured = t)
            .Returns(Task.CompletedTask);

        // Act
        await _service.SaveTokensAsync(userId, tokenResponse);

        // Assert
        Assert.NotNull(captured);
        Assert.Equal("old-encrypted-refresh-token", captured!.RefreshToken);
        Assert.NotEqual("old-encrypted-access-token", captured.AccessToken);
    }

    [Fact]
    public async Task SaveTokensAsync_ExistingUserRefreshTokenProvided_OverwritesPreviousRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenResponse = new GoogleTokenResponse("id-token", "new-access-token", "new-refresh-token", 3600, "openid email profile calendar.readonly");
        var existing = new UserGoogleToken
        {
            UserId = userId,
            AccessToken = "old-encrypted-access-token",
            RefreshToken = "old-encrypted-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5),
            Scope = "openid email profile",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existing);

        UserGoogleToken? captured = null;
        _mockRepository.Setup(r => r.UpsertAsync(It.IsAny<UserGoogleToken>()))
            .Callback<UserGoogleToken>(t => captured = t)
            .Returns(Task.CompletedTask);

        var expectedRefreshToken = _mockProtector.Object.Protect(tokenResponse.RefreshToken!);

        // Act
        await _service.SaveTokensAsync(userId, tokenResponse);

        // Assert
        Assert.NotNull(captured);
        Assert.Equal(expectedRefreshToken, captured!.RefreshToken);
        Assert.NotEqual("old-encrypted-refresh-token", captured.RefreshToken);
    }

    [Fact]
    public async Task SaveTokensAsync_RepositoryThrowsException_ExceptionPropagates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenResponse = new GoogleTokenResponse("id-token", "access-token", "refresh-token", 3600, "openid email profile");

        _mockRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SaveTokensAsync(userId, tokenResponse));
    }

    #endregion
}
