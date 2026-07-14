## 1. Domain and Data Layer

- [x] 1.1 Add `User` entity to `itinerary_be.Core`
- [x] 1.2 Add `UserConfiguration` (EF Core) in `itinerary_be.Infrastructure`
- [x] 1.3 Add `Users` DbSet to `ItineraryDbContext`
- [x] 1.4 Add `CreateUsersTable` FluentMigrator migration (version `20260713120000`)

## 2. Auth Module

- [x] 2.1 Create `itinerary_be.Modules.Auth` project and add it to the solution
- [x] 2.2 Add `GoogleAuthOptions` / `JwtOptions`
- [x] 2.3 Add `IGoogleTokenValidator` / `GoogleTokenValidator` (wraps `Google.Apis.Auth`)
- [x] 2.4 Add `IJwtTokenService` / `JwtTokenService` (issues backend JWT)
- [x] 2.5 Add `IUserRepository` / `UserRepository` and `IUserService` / `UserService` (JIT provisioning)
- [x] 2.6 Add `IAuthService` / `AuthService` (orchestrates login)
- [x] 2.7 Add `AuthServiceRegistration.AddAuthServices()` DI extension

## 3. WebAPI Wiring

- [x] 3.1 Reference `Modules.Auth` and add `Microsoft.AspNetCore.Authentication.JwtBearer` package
- [x] 3.2 Wire `AddAuthentication().AddJwtBearer()` and `AddAuthorization()` fallback policy in `Program.cs`
- [x] 3.3 Insert `UseAuthentication()` / `UseAuthorization()` into the middleware pipeline
- [x] 3.4 Add `GoogleLoginDto` / `UserResponseDto` / `AuthResponseDto` + `GoogleLoginDtoValidator`
- [x] 3.5 Add `AuthController` with `POST /api/auth/google` (`[AllowAnonymous]`)
- [x] 3.6 Add `Google`/`Jwt` config placeholders to `appsettings.json`; initialize user-secrets for local dev

## 4. Tests

- [x] 4.1 `UserServiceTests` (JIT provisioning logic)
- [x] 4.2 `AuthServiceTests` (login orchestration, mocked collaborators)

## 5. Documentation

- [x] 5.1 `docs/google-oauth-setup.md` — Google Cloud Console setup guide
- [x] 5.2 Link the new doc from root `README.md`

## 6. Verification

- [ ] 6.1 `dotnet build itinerary_be.slnx` succeeds
- [ ] 6.2 `dotnet run --project itinerary_be.Migration` applies the new migration
- [ ] 6.3 `dotnet test itinerary_be.UnitTests` passes with no regressions
- [ ] 6.4 Manual check: unauthenticated request to `/api/trips` returns 401; Google login returns a usable token; authenticated request to `/api/trips` returns 200
