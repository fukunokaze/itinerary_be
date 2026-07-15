## 1. Auth Module

- [x] 1.1 Add `GoogleTokenResponse` model (`itinerary_be.Modules.Auth/Models`)
- [x] 1.2 Add `IGoogleOAuthClient` / `GoogleOAuthClient` (token-exchange HTTP client)
- [x] 1.3 Add `ClientSecret` / `RedirectUri` to `GoogleAuthOptions`
- [x] 1.4 Update `IAuthService` / `AuthService.LoginWithGoogleAsync` to exchange the code, validate the returned ID token, and issue the JWT as before
- [x] 1.5 Register `IGoogleOAuthClient` via `AddHttpClient<>()` and extend `GoogleAuthOptions` validation in `AuthServiceRegistration.cs`
- [x] 1.6 Add `Microsoft.Extensions.Http` package reference

## 2. WebAPI Wiring

- [x] 2.1 Rename `GoogleLoginDto.IdToken` -> `Code`; update `GoogleLoginDtoValidator`
- [x] 2.2 Update `AuthController` to pass `Code` through to `LoginWithGoogleAsync`
- [x] 2.3 Add `Google:ClientSecret` / `Google:RedirectUri` placeholders to `appsettings.json`

## 3. Tests

- [x] 3.1 Update `AuthServiceTests` for the new `IGoogleOAuthClient` collaborator and code-based flow
- [x] 3.2 Add `LoginWithGoogleAsync_CodeExchangeThrows_ExceptionPropagatesAndDoesNotValidateToken`

## 4. Spec

- [x] 4.1 `## MODIFIED Requirements` delta for `google-oauth-authentication` (code-exchange scenarios)

## 5. Verification

- [ ] 5.1 `dotnet build itinerary_be.slnx` succeeds
- [ ] 5.2 `dotnet test itinerary_be.UnitTests` passes with no regressions
- [ ] 5.3 Manual check: missing `code` returns 400; a garbage `code` against real `ClientId`/`ClientSecret`/`RedirectUri` returns 401
- [ ] 5.4 Full happy-path verification (real consent -> code -> exchange -> Calendar scope granted -> JWT issued) deferred until KAN-10 (Console config) and the companion frontend change land
