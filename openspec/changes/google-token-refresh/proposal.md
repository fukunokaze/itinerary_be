## Why

`openspec/changes/google-token-storage` (KAN-11) persists the Google `access_token`/`refresh_token` pair per user but stops short of anything reading them back out — the `access_token` it stores is only valid for ~1 hour. KAN-7's Calendar API subtasks (KAN-13/14/15) need a way to get a currently-valid access token for a user on demand, transparently refreshing an expired one via the stored `refresh_token` rather than pushing that logic into every future Calendar-API call site. This change (KAN-12) adds that capability, plus a way for callers to distinguish "refresh worked" from "the user must re-consent" so the frontend can react accordingly.

## What Changes

- Add `IUserGoogleTokenService.GetValidAccessTokenAsync(Guid userId)`: returns a decrypted, currently-valid Google access token for the user, transparently refreshing via Google's token endpoint when the stored one is expired (or within a short buffer of expiring).
- Add `IGoogleOAuthClient.RefreshAccessTokenAsync(string refreshToken)`: posts a `grant_type=refresh_token` request to Google's token endpoint and returns the new access token/expiry/scope.
- Add `GoogleReauthorizationRequiredException`: thrown when there is no usable path to a valid access token — no stored token/refresh token for the user, or Google reports the refresh token is invalid/revoked (`error=invalid_grant`, or an HTTP 401 from the refresh call). Distinct from `InvalidGoogleTokenException` so callers can tell "user must re-authenticate with Google" apart from a login-time failure.
- A successful refresh updates the stored `access_token` and `expires_at` (re-encrypted) in place; the stored `refresh_token` is left untouched (Google's refresh grant does not normally return a new one).
- No new endpoint and no Calendar API call — this only makes a valid access token obtainable in-process for KAN-13/14/15 to consume.

## Capabilities

### Added Capabilities
- `google-token-refresh`: on-demand, transparent refresh of an expired Google access token using the stored refresh token, with a distinct error for revoked/invalid grants.

## Impact

- **Code**: `itinerary_be.Modules.Auth` — `IGoogleOAuthClient`/`GoogleOAuthClient` gain `RefreshAccessTokenAsync`; `IUserGoogleTokenService`/`UserGoogleTokenService` gain `GetValidAccessTokenAsync`; new `GoogleReauthorizationRequiredException`; new `GoogleRefreshTokenResponse` model.
- **Database**: None — reuses the existing `user_google_tokens` table from KAN-11.
- **APIs**: None — no controller/endpoint changes.
- **Dependencies**: None new.
