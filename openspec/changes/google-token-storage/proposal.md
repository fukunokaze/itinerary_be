## Why

`openspec/changes/google-oauth-code-flow` (KAN-9) moved login to a full authorization-code exchange so Google returns an `access_token`/`refresh_token` alongside the `id_token`, but explicitly discards both after issuing the JWT — "no persistence, no new database schema." KAN-7 ("My Calendar") needs to call the Calendar API on the user's behalf outside of the login request, which requires those tokens to still exist afterward. This change (KAN-11) adds that persistence, encrypted at rest, since the `users` table currently has no token storage at all.

## What Changes

- Add a `user_google_tokens` table (one row per user) storing `access_token`, `refresh_token`, `expires_at`, `scope`, `updated_at`, keyed by `user_id` (FK to `users`, cascade delete).
- `AccessToken`/`RefreshToken` are encrypted at rest using the ASP.NET Data Protection API before being written, and decrypted on read. No plaintext token is ever persisted.
- `AuthService.LoginWithGoogleAsync` saves (upserts) the exchanged tokens after issuing the JWT, instead of discarding them. Since Google only returns a `refresh_token` on a user's first consent, an upsert must preserve the previously stored refresh token when the new exchange response omits one.
- This change does not add any endpoint to read tokens back out, refresh an expired access token (KAN-12), or call the Calendar API (KAN-13/14/15) — it only makes tokens available for those later changes to consume.

## Capabilities

### Added Capabilities
- `google-token-storage`: encrypted per-user persistence of Google OAuth tokens obtained during login.

## Impact

- **Code**: `itinerary_be.Core` — new `UserGoogleToken` entity. `itinerary_be.Infrastructure` — new `UserGoogleTokenConfiguration`, `DbSet<UserGoogleToken>`. `itinerary_be.Modules.Auth` — new `IUserGoogleTokenRepository`/`UserGoogleTokenRepository`, `IUserGoogleTokenService`/`UserGoogleTokenService`; `AuthService.LoginWithGoogleAsync` now persists tokens post-login. `itinerary_be.WebAPI` — `Program.cs` registers `AddDataProtection()`.
- **Database**: New `itinerary_be.user_google_tokens` table (FluentMigrator migration).
- **APIs**: None — `POST /api/auth/google` response contract is unchanged.
- **Dependencies**: `Microsoft.AspNetCore.DataProtection.Abstractions` in `itinerary_be.Modules.Auth`.
