## 1. Auth Module

- [x] 1.1 Add `GoogleRefreshTokenResponse` model (`itinerary_be.Modules.Auth/Models`)
- [x] 1.2 Add `GoogleReauthorizationRequiredException` (`itinerary_be.Modules.Auth/Exceptions`)
- [x] 1.3 Add `IGoogleOAuthClient.RefreshAccessTokenAsync(string refreshToken)` + `GoogleOAuthClient` implementation (detects `error=invalid_grant`/401 → `GoogleReauthorizationRequiredException`, other failures → `InvalidGoogleTokenException`)
- [x] 1.4 Add `IUserGoogleTokenService.GetValidAccessTokenAsync(Guid userId)` + `UserGoogleTokenService` implementation (decrypt, 60s expiry buffer, refresh + re-persist on expiry, preserve `refresh_token`)

## 2. Tests

- [x] 2.1 `GoogleOAuthClientTests` (new) — `RefreshAccessTokenAsync`: success parses response; `invalid_grant` body throws `GoogleReauthorizationRequiredException`; other non-2xx/network/parse failures throw `InvalidGoogleTokenException`
- [x] 2.2 `UserGoogleTokenServiceTests` — add `GetValidAccessTokenAsync` cases: no stored row throws `GoogleReauthorizationRequiredException`; non-expired token returned decrypted without calling refresh; expired token refreshes, re-persists (refresh token unchanged), returns new decrypted access token; no stored refresh token throws `GoogleReauthorizationRequiredException`; refresh call throwing `GoogleReauthorizationRequiredException` propagates

## 3. Spec

- [x] 3.1 `## ADDED Requirements` delta for new `google-token-refresh` capability

## 4. Verification

- [x] 4.1 `dotnet build itinerary_be.slnx` succeeds
- [x] 4.2 `dotnet test itinerary_be.UnitTests` passes with no regressions (64/64 passing, 0 regressions)
