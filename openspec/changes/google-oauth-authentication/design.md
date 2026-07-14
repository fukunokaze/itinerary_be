## Context

No authentication, authorization, or user concept exists in the codebase today. `Trip` has no owner/UserId — trips are not scoped to any user. This change introduces identity as a new bounded context without yet tying existing domain data (trips, lodgings, flights, events) to a specific owner.

## Goals / Non-Goals

**Goals:**
- Let a frontend/mobile client authenticate end users via their Google account.
- Auto-create a local `User` profile (email + name) on first sign-in, keyed by email.
- Require a valid session on every existing endpoint by default.
- Keep the implementation minimal: authentication only, no authorization tiers.

**Non-Goals:**
- Role-based access control or per-user permissions.
- Refresh tokens, logout, or token revocation — expired/compromised tokens are handled solely by short expiry and secret rotation (which invalidates all sessions).
- Associating existing `Trip`/`Lodging`/`Flight`/`TripEvent` rows with a `UserId` — trips remain globally readable/writable by any authenticated user. Likely a fast-follow, explicitly out of scope here.
- Native mobile-specific OAuth client configuration (Android/iOS client IDs) — only the Web application client ID (used by Google Identity Services for ID-token issuance) is covered.

## Decisions

**Decision 1: ID-token verification, not server-side redirect flow**
- **Chosen**: Frontend performs Google Sign-In and sends the resulting ID token to `POST /api/auth/google`; backend verifies it via `Google.Apis.Auth`.
- **Rationale**: This is an API-only backend with a separate frontend/mobile client — the frontend controls the sign-in UI. A cookie-based `AddGoogle()` redirect handler is designed for server-rendered apps and doesn't fit this architecture or the existing wide-open CORS setup.
- **Alternative considered**: Server-side OAuth authorization-code flow — rejected as unnecessary complexity for a stateless JSON API.

**Decision 2: Backend-issued JWT, not re-validating the Google token on every request**
- **Chosen**: After verifying the Google ID token once at login, the backend mints its own HMAC-signed JWT for the client to use on all subsequent requests.
- **Rationale**: Decouples the API from Google's token lifetime/audience on every call, and gives a uniform bearer-token scheme that could support other auth providers later without touching every endpoint.

**Decision 3: JIT (just-in-time) user provisioning, single endpoint for login and signup**
- **Chosen**: `POST /api/auth/google` creates the `User` row automatically if none exists for the verified email.
- **Rationale**: Matches standard "Continue with Google" UX — one call from the client handles both new and returning users. Email is the natural unique identifier since it comes verified from Google.

**Decision 4: New `itinerary_be.Modules.Auth` project, not a folder inside `Modules.Itinerary`**
- **Chosen**: Authentication is its own bounded context, in its own class-library project, mirroring the existing `itinerary_be.Modules.Itinerary` / `itinerary_be.Modules.Logistics` convention (the latter already exists as an empty placeholder for a future non-Itinerary context, confirming this is a deliberate repo convention).

**Decision 5: Global fallback authorization policy, not per-controller `[Authorize]`**
- **Chosen**: `AddAuthorization(options => options.FallbackPolicy = RequireAuthenticatedUser())` in `Program.cs`, with `[AllowAnonymous]` only on the Google login action.
- **Rationale**: Guarantees every existing and future controller is protected by default with zero per-controller edits, rather than relying on remembering to add `[Authorize]` everywhere.

## Risks / Trade-offs

- **No token revocation**: a compromised JWT signing secret requires rotating the secret, which invalidates every active session and forces re-login for all users. Acceptable at this stage given the small scope; a refresh/revocation scheme is a likely future addition.
- **Trips remain unowned**: any authenticated user can read/write any trip — there is no per-user data isolation yet. Explicitly flagged as a known gap for a follow-up change.
- **Testing boundary**: `GoogleTokenValidator` and `JwtTokenService` wrap real cryptography/network calls and are intentionally left without unit tests — `AuthService` is tested against mocked interfaces instead, consistent with how the rest of the codebase avoids testing thin infrastructure adapters directly.
