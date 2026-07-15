## 1. Core / Infrastructure / Migration

- [x] 1.1 Add `UserGoogleToken` entity (`itinerary_be.Core/Domain/Entities`)
- [x] 1.2 Add `UserGoogleTokenConfiguration` (`itinerary_be.Infrastructure/Data/Configurations`) + `DbSet<UserGoogleToken>` on `ItineraryDbContext`
- [x] 1.3 Add FluentMigrator migration creating `user_google_tokens` (FK to `users`, cascade delete)

## 2. Auth Module

- [x] 2.1 Add `IUserGoogleTokenRepository`/`UserGoogleTokenRepository` (EF upsert by `user_id`)
- [x] 2.2 Add `IUserGoogleTokenService`/`UserGoogleTokenService` (encrypts via `IDataProtectionProvider`, preserves prior refresh token when the new one is null)
- [x] 2.3 Register both in `AuthServiceRegistration.cs`
- [x] 2.4 Add `Microsoft.AspNetCore.DataProtection.Abstractions` package reference
- [x] 2.5 Wire `AuthService.LoginWithGoogleAsync` to call `SaveTokensAsync` post-JWT-issuance (best-effort, logs on failure, does not throw)

## 3. WebAPI Wiring

- [x] 3.1 `builder.Services.AddDataProtection()` in `Program.cs`

## 4. Tests

- [x] 4.1 `UserGoogleTokenServiceTests` — encrypts before save; preserves existing refresh token when new one is null; overwrites when new one is present
- [x] 4.2 Update `AuthServiceTests` for the new `IUserGoogleTokenService` collaborator; verify a save failure doesn't prevent the JWT result from being returned

## 5. Spec

- [x] 5.1 `## ADDED Requirements` delta for new `google-token-storage` capability

## 6. Verification

- [x] 6.1 `dotnet build itinerary_be.slnx` succeeds
- [x] 6.2 `dotnet test itinerary_be.UnitTests` passes with no regressions
- [ ] 6.3 Manual/DB check: run the migrator, confirm `user_google_tokens` row is created/updated on login and that stored `access_token`/`refresh_token` values are not plaintext (requires a real Postgres connection + real Google OAuth credentials — not run here)
