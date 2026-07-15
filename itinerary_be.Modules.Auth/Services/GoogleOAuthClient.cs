namespace itinerary_be.Modules.Auth.Services;

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using itinerary_be.Modules.Auth.Exceptions;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Models;
using itinerary_be.Modules.Auth.Options;

public class GoogleOAuthClient : IGoogleOAuthClient
{
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";

    private readonly HttpClient _httpClient;
    private readonly GoogleAuthOptions _options;
    private readonly ILogger<GoogleOAuthClient> _logger;

    public GoogleOAuthClient(HttpClient httpClient, IOptions<GoogleAuthOptions> options, ILogger<GoogleOAuthClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<GoogleTokenResponse> ExchangeCodeAsync(string code)
    {
        try
        {
            var form = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["redirect_uri"] = _options.RedirectUri,
                ["grant_type"] = "authorization_code"
            };

            using var response = await _httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(form));
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google token exchange failed with status {StatusCode}: {Body}", response.StatusCode, body);
                throw new InvalidGoogleTokenException($"Google token exchange failed with status {(int)response.StatusCode}.");
            }

            var payload = JsonSerializer.Deserialize<GoogleTokenExchangePayload>(body)
                ?? throw new InvalidGoogleTokenException("Google token exchange returned an empty response.");

            if (string.IsNullOrWhiteSpace(payload.IdToken))
            {
                throw new InvalidGoogleTokenException("Google token exchange response did not include an id_token.");
            }

            return new GoogleTokenResponse(payload.IdToken, payload.AccessToken, payload.RefreshToken, payload.ExpiresIn, payload.Scope);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Google token exchange request failed.");
            throw new InvalidGoogleTokenException("Google token exchange failed.", ex);
        }
    }

    public async Task<GoogleRefreshTokenResponse> RefreshAccessTokenAsync(string refreshToken)
    {
        try
        {
            var form = new Dictionary<string, string>
            {
                ["refresh_token"] = refreshToken,
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["grant_type"] = "refresh_token"
            };

            using var response = await _httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(form));
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || IsInvalidGrantError(body))
                {
                    _logger.LogWarning("Google refresh token is invalid or revoked: {Body}", body);
                    throw new GoogleReauthorizationRequiredException("Google refresh token is invalid or has been revoked.");
                }

                _logger.LogWarning("Google token refresh failed with status {StatusCode}: {Body}", response.StatusCode, body);
                throw new InvalidGoogleTokenException($"Google token refresh failed with status {(int)response.StatusCode}.");
            }

            var payload = JsonSerializer.Deserialize<GoogleTokenRefreshPayload>(body)
                ?? throw new InvalidGoogleTokenException("Google token refresh returned an empty response.");

            return new GoogleRefreshTokenResponse(payload.AccessToken, payload.ExpiresIn, payload.Scope);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Google token refresh request failed.");
            throw new InvalidGoogleTokenException("Google token refresh failed.", ex);
        }
    }

    private static bool IsInvalidGrantError(string body)
    {
        try
        {
            var error = JsonSerializer.Deserialize<GoogleTokenErrorPayload>(body);
            return error?.Error == "invalid_grant";
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private class GoogleTokenExchangePayload
    {
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; } = string.Empty;

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
    }

    private class GoogleTokenRefreshPayload
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
    }

    private class GoogleTokenErrorPayload
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
