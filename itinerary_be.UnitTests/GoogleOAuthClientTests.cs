namespace itinerary_be.UnitTests;

using System.Net;
using System.Net.Http;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using itinerary_be.Modules.Auth.Exceptions;
using itinerary_be.Modules.Auth.Options;
using itinerary_be.Modules.Auth.Services;

/// <summary>
/// Unit tests for GoogleOAuthClient class
/// </summary>
public class GoogleOAuthClientTests
{
    private readonly Mock<ILogger<GoogleOAuthClient>> _mockLogger;
    private readonly GoogleAuthOptions _options;

    public GoogleOAuthClientTests()
    {
        _mockLogger = new Mock<ILogger<GoogleOAuthClient>>();
        _options = new GoogleAuthOptions
        {
            ClientId = "client-id",
            ClientSecret = "client-secret",
            RedirectUri = "https://app.example.com/callback"
        };
    }

    private GoogleOAuthClient CreateClient(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseBody)
            });

        var httpClient = new HttpClient(handler.Object);
        return new GoogleOAuthClient(httpClient, Options.Create(_options), _mockLogger.Object);
    }

    #region RefreshAccessTokenAsync Tests

    [Fact]
    public async Task RefreshAccessTokenAsync_Success_ReturnsParsedResponse()
    {
        // Arrange
        var client = CreateClient(HttpStatusCode.OK, """{"access_token":"new-access-token","expires_in":3600,"scope":"openid email profile"}""");

        // Act
        var result = await client.RefreshAccessTokenAsync("refresh-token");

        // Assert
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.Equal("openid email profile", result.Scope);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_InvalidGrantError_ThrowsGoogleReauthorizationRequiredException()
    {
        // Arrange
        var client = CreateClient(HttpStatusCode.BadRequest, """{"error":"invalid_grant","error_description":"Token has been expired or revoked."}""");

        // Act & Assert
        await Assert.ThrowsAsync<GoogleReauthorizationRequiredException>(() => client.RefreshAccessTokenAsync("revoked-refresh-token"));
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_UnauthorizedStatus_ThrowsGoogleReauthorizationRequiredException()
    {
        // Arrange
        var client = CreateClient(HttpStatusCode.Unauthorized, """{"error":"unauthorized_client"}""");

        // Act & Assert
        await Assert.ThrowsAsync<GoogleReauthorizationRequiredException>(() => client.RefreshAccessTokenAsync("refresh-token"));
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_OtherServerError_ThrowsInvalidGoogleTokenException()
    {
        // Arrange
        var client = CreateClient(HttpStatusCode.InternalServerError, """{"error":"server_error"}""");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidGoogleTokenException>(() => client.RefreshAccessTokenAsync("refresh-token"));
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_MalformedResponseBody_ThrowsInvalidGoogleTokenException()
    {
        // Arrange
        var client = CreateClient(HttpStatusCode.OK, "not-json");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidGoogleTokenException>(() => client.RefreshAccessTokenAsync("refresh-token"));
    }

    #endregion
}
