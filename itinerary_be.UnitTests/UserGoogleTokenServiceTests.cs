namespace itinerary_be.UnitTests;

using System.Linq;
using Moq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Auth.Exceptions;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Models;
using itinerary_be.Modules.Auth.Services;

/// <summary>
/// Unit tests for UserGoogleTokenService class
/// </summary>
public class UserGoogleTokenServiceTests
{
    private readonly Mock<IUserGoogleTokenRepository> _mockRepository;
    private readonly Mock<IGoogleOAuthClient> _mockGoogleOAuthClient;
    private readonly Mock<IDataProtectionProvider> _mockDataProtectionProvider;
    private readonly Mock<IDataProtector> _mockProtector;
    private readonly Mock<ILogger<UserGoogleTokenService>> _mockLogger;
    private readonly UserGoogleTokenService _service;

    public UserGoogleTokenServiceTests()
    {
        _mockRepository = new Mock<IUserGoogleTokenRepository>();
        _mockGoogleOAuthClient = new Mock<IGoogleOAuthClient>();
        _mockProtector = new Mock<IDataProtector>();
        // Fake "encryption": reverse the bytes. Deterministic per input, never equal to plaintext,
        // and reversing twice round-trips, so the same setup backs both Protect and Unprotect.
        _mockProtector.Setup(p => p.Protect(It.IsAny<byte[]>()))
            .Returns<byte[]>(bytes => bytes.Reverse().ToArray());
        _mockProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>()))
            .Returns<byte[]>(bytes => bytes.Reverse().ToArray());

        _mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
        _mockDataProtectionProvider.Setup(p => p.CreateProtector(It.IsAny<string>()))
            .Returns(_mockProtector.Object);

        _mockLogger = new Mock<ILogger<UserGoogleTokenService>>();
        _service = new UserGoogleTokenService(
            _mockRepository.Object,
            _mockGoogleOAuthClient.Object,
            _mockDataProtectionProvider.Object,
            _mockLogger.Object);
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

    #region GetValidAccessTokenAsync Tests

    [Fact]
    public async Task GetValidAccessTokenAsync_NoStoredToken_ThrowsGoogleReauthorizationRequiredException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((UserGoogleToken?)null);

        // Act & Assert
        await Assert.ThrowsAsync<GoogleReauthorizationRequiredException>(() => _service.GetValidAccessTokenAsync(userId));
        _mockGoogleOAuthClient.Verify(c => c.RefreshAccessTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_TokenStillValid_ReturnsDecryptedAccessTokenWithoutRefreshing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var encryptedAccessToken = _mockProtector.Object.Protect("valid-access-token");
        var existing = new UserGoogleToken
        {
            UserId = userId,
            AccessToken = encryptedAccessToken,
            RefreshToken = "encrypted-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            Scope = "openid email profile",
            UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
        };
        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existing);

        // Act
        var result = await _service.GetValidAccessTokenAsync(userId);

        // Assert
        Assert.Equal("valid-access-token", result);
        _mockGoogleOAuthClient.Verify(c => c.RefreshAccessTokenAsync(It.IsAny<string>()), Times.Never);
        _mockRepository.Verify(r => r.UpsertAsync(It.IsAny<UserGoogleToken>()), Times.Never);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_TokenExpired_RefreshesAndPersistsNewAccessToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var encryptedRefreshToken = _mockProtector.Object.Protect("stored-refresh-token");
        var existing = new UserGoogleToken
        {
            UserId = userId,
            AccessToken = "old-encrypted-access-token",
            RefreshToken = encryptedRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(-5),
            Scope = "openid email profile",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existing);

        var refreshed = new GoogleRefreshTokenResponse("new-access-token", 3600, "openid email profile");
        _mockGoogleOAuthClient.Setup(c => c.RefreshAccessTokenAsync("stored-refresh-token")).ReturnsAsync(refreshed);

        UserGoogleToken? captured = null;
        _mockRepository.Setup(r => r.UpsertAsync(It.IsAny<UserGoogleToken>()))
            .Callback<UserGoogleToken>(t => captured = t)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetValidAccessTokenAsync(userId);

        // Assert
        Assert.Equal("new-access-token", result);
        Assert.NotNull(captured);
        Assert.Equal(encryptedRefreshToken, captured!.RefreshToken);
        Assert.Equal(_mockProtector.Object.Protect("new-access-token"), captured.AccessToken);
        Assert.True(captured.ExpiresAt > DateTime.UtcNow.AddMinutes(59) && captured.ExpiresAt < DateTime.UtcNow.AddMinutes(61));
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_TokenExpiredNoRefreshToken_ThrowsGoogleReauthorizationRequiredExceptionWithoutCallingGoogle()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existing = new UserGoogleToken
        {
            UserId = userId,
            AccessToken = "old-encrypted-access-token",
            RefreshToken = null,
            ExpiresAt = DateTime.UtcNow.AddSeconds(-5),
            Scope = "openid email profile",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existing);

        // Act & Assert
        await Assert.ThrowsAsync<GoogleReauthorizationRequiredException>(() => _service.GetValidAccessTokenAsync(userId));
        _mockGoogleOAuthClient.Verify(c => c.RefreshAccessTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_RefreshThrowsGoogleReauthorizationRequiredException_Propagates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var encryptedRefreshToken = _mockProtector.Object.Protect("revoked-refresh-token");
        var existing = new UserGoogleToken
        {
            UserId = userId,
            AccessToken = "old-encrypted-access-token",
            RefreshToken = encryptedRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(-5),
            Scope = "openid email profile",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existing);
        _mockGoogleOAuthClient.Setup(c => c.RefreshAccessTokenAsync("revoked-refresh-token"))
            .ThrowsAsync(new GoogleReauthorizationRequiredException("revoked"));

        // Act & Assert
        await Assert.ThrowsAsync<GoogleReauthorizationRequiredException>(() => _service.GetValidAccessTokenAsync(userId));
        _mockRepository.Verify(r => r.UpsertAsync(It.IsAny<UserGoogleToken>()), Times.Never);
    }

    #endregion
}
