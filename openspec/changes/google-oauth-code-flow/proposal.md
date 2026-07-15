## Why

The "My Calendar" feature (KAN-7) needs read access to a user's Google Calendar, which requires requesting the `calendar.readonly` scope and offline access at sign-in — Calendar consent is granted at login, not via a separate opt-in flow. The current login flow (`google-oauth-authentication`) only verifies an ID token the frontend already obtained via Google Identity Services; it never talks to Google's token endpoint and cannot request scopes beyond the default `openid`/profile claims baked into that token. This supersedes Decision 1 ("ID-token verification, not server-side redirect flow") and the refresh-token Non-Goal from that change's `design.md` — both were reasonable before Calendar access was in scope.

## What Changes

- The frontend moves to a full OAuth 2.0 authorization-code flow, requesting `openid email profile` + `https://www.googleapis.com/auth/calendar.readonly` with offline access, and sends the resulting authorization `code` to `POST /api/auth/google` instead of a pre-issued ID token.
- The backend exchanges the `code` for tokens at Google's token endpoint (`https://oauth2.googleapis.com/token`), then validates the `id_token` from that response for identity exactly as before (same `GoogleTokenValidator`), and issues the backend JWT unchanged.
- The Google `access_token`/`refresh_token` returned by the exchange are used transiently and then discarded — no persistence, no new database schema. Storing and encrypting these tokens per user is tracked separately (KAN-11); provisioning the real `ClientSecret` value and registering the redirect URI in Google Cloud Console is tracked separately (KAN-10). This change adds the structural config fields (`ClientSecret`, `RedirectUri`) those tickets need, with empty placeholder values.

## Capabilities

### Modified Capabilities
- `google-oauth-authentication`: the sign-in request contract changes from a Google ID token to a Google OAuth authorization code; the backend now performs a server-side token exchange before validating identity. JIT user provisioning and global authentication enforcement behavior are unchanged.

## Impact

- **Code**: `itinerary_be.Modules.Auth` — new `IGoogleOAuthClient`/`GoogleOAuthClient`/`GoogleTokenResponse`; `AuthService`/`IAuthService` now take an authorization code and orchestrate exchange → identity validation → JIT user → JWT; `GoogleAuthOptions` gains `ClientSecret`/`RedirectUri`. `itinerary_be.WebAPI` — `GoogleLoginDto`/`GoogleLoginDtoValidator`/`AuthController` updated for the `code` field.
- **Database**: None.
- **APIs**: `POST /api/auth/google` request body changes from `{ "idToken": "..." }` to `{ "code": "..." }` — breaking change, coordinated with a corresponding frontend change.
- **Config**: New `Google:ClientSecret` and `Google:RedirectUri` keys (empty placeholders; real values are KAN-10).
- **Dependencies**: `Microsoft.Extensions.Http` (for `AddHttpClient`).
