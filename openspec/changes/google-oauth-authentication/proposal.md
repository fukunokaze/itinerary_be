## Why

The API currently has no authentication at all — every endpoint (Trips, Lodgings, Flights, Trip Events) is publicly accessible to anyone, and there is no concept of a user. To move toward a real multi-user product, clients need to sign in with their Google account, the backend needs a minimal local user profile keyed by email, and every existing endpoint needs to require a valid session before it can be used.

## What Changes

- Add Google Sign-In support via ID token verification: the frontend performs Google Sign-In itself and sends the resulting ID token to a new `POST /api/auth/google` endpoint.
- Auto-provision a `User` profile (email + name) the first time a given Google account signs in — one endpoint handles both login and registration.
- Issue a backend-signed JWT on successful login; require a valid bearer token on every other endpoint via a global fallback authorization policy.
- No roles/RBAC — authentication only, "valid token or not."
- Add a setup guide for the Google Cloud Console configuration this requires.

## Capabilities

### New Capabilities
- `google-oauth-authentication`: Google ID-token login with just-in-time user registration, and global JWT-based authentication enforcement across the API.

### Modified Capabilities
- `trip-crud-api`: all existing Trip/Lodging/Flight/TripEvent endpoints now require an `Authorization: Bearer <token>` header; no route or payload shapes change, only the access-control behavior.

## Impact

- **Code**: New `itinerary_be.Modules.Auth` project (interfaces/services/repository for Google token validation, JWT issuance, user lookup/creation); new `AuthController` in `itinerary_be.WebAPI`; new `User` entity in `itinerary_be.Core` + EF configuration in `itinerary_be.Infrastructure`.
- **Database**: New `itinerary.users` table (migration `20260713120000`).
- **APIs**: New `POST /api/auth/google` (anonymous). All other existing endpoints become authenticated-only — this is a breaking change for any current API consumer.
- **Config**: New `Google:ClientId` and `Jwt:*` configuration keys; local secrets via `dotnet user-secrets`.
- **Dependencies**: `Google.Apis.Auth`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`.
