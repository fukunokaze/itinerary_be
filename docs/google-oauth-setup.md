# Google OAuth Setup

This API authenticates users via **Google Sign-In using ID token verification**. There is no server-side redirect/consent-screen flow on the backend, and no Google client secret is used anywhere.

## How the flow works

1. The frontend (SPA or mobile app) integrates Google Identity Services and performs the sign-in itself, obtaining a **Google ID token** for the signed-in user.
2. The frontend sends that ID token to the backend: `POST /api/auth/google` with body `{ "idToken": "<google id token>" }`.
3. The backend verifies the token's signature, issuer, expiry, and audience against Google's public keys (via `Google.Apis.Auth`).
4. If valid, the backend looks up a `User` by the token's email claim. If none exists, one is created automatically (email + Google's `name` claim) — first sign-in doubles as registration.
5. The backend returns its own signed JWT (`accessToken`) and the user profile. The frontend sends this JWT as `Authorization: Bearer <accessToken>` on every subsequent request. All other endpoints require this header.

No "Authorized redirect URIs" are involved, because the backend never redirects to Google — only the frontend talks to Google directly to obtain the ID token.

## Google Cloud Console setup

1. **Create or select a project** at [console.cloud.google.com](https://console.cloud.google.com).
2. **Configure the OAuth consent screen** (APIs & Services > OAuth consent screen):
   - User type: **External** (unless this is an internal-only Workspace app).
   - App name, support email, and developer contact info.
   - Scopes: the defaults (`openid`, `email`, `profile`) are sufficient — no additional scopes are needed for this flow.
   - While the app is in "Testing" publishing status, add any Google accounts you want to sign in with as test users.
3. **Create an OAuth Client ID** (APIs & Services > Credentials > Create Credentials > OAuth client ID):
   - Application type: **Web application**. This is correct even if your actual frontend is a single-page app or mobile app using Google Identity Services' ID-token/One Tap flow — "Web application" is the client type Google's Identity Services JS SDK expects. If you later add a native mobile app using a platform-specific Google Sign-In SDK, that requires its own additional Android/iOS OAuth client ID (out of scope here).
   - **Authorized JavaScript origins**: add every origin the frontend is served from (e.g. `http://localhost:5173` for local dev, plus your deployed frontend's origin). Leave "Authorized redirect URIs" empty — this flow doesn't use them.
4. **One Client ID, two consumers**: the same Web application Client ID is used by both:
   - The **frontend**, to initialize Google Identity Services and request an ID token.
   - The **backend**, as `Google:ClientId` in configuration — the backend validates the token's `aud` claim matches this exact Client ID, to make sure the token was actually issued for this app.

   There is no client *secret* involved anywhere in this flow, since the backend never exchanges an authorization code with Google.

## Backend configuration

The app reads two configuration sections (see the placeholders in `itinerary_be.WebAPI/appsettings.json`):

```json
{
  "Google": { "ClientId": "" },
  "Jwt": { "Secret": "", "Issuer": "itinerary-be", "Audience": "itinerary-be-client", "ExpiryMinutes": 60 }
}
```

Never commit real values for `Google:ClientId` or `Jwt:Secret`. For local development, use `dotnet user-secrets` (already initialized for `itinerary_be.WebAPI`):

```
dotnet user-secrets set "Google:ClientId" "<your-client-id>.apps.googleusercontent.com" --project itinerary_be.WebAPI
dotnet user-secrets set "Jwt:Secret" "<a long random string>" --project itinerary_be.WebAPI
```

Generate a strong `Jwt:Secret` with something like `openssl rand -base64 48`. This secret signs the backend's own session tokens — it has nothing to do with Google. Rotating it invalidates every previously issued token (there's no refresh-token/revocation mechanism yet), which is expected at this stage.

In non-local environments, set these same two values through your deployment platform's environment variables or secret manager instead of `appsettings.json`.

## Manual end-to-end test

1. Obtain a real Google ID token for a test account — the simplest way locally is a small HTML page using [Google Identity Services](https://developers.google.com/identity/gsi/web/guides/overview) with your Client ID, or the [OAuth 2.0 Playground](https://developers.google.com/oauthplayground/).
2. Call the login endpoint:
   ```
   curl -X POST https://localhost:<port>/api/auth/google \
     -H "Content-Type: application/json" \
     -d '{"idToken":"<the id token>"}'
   ```
   Expect a `200` with `accessToken`, `expiresAt`, and `user` in the response, and a new row in `itinerary.users` on first call (no duplicate on repeat calls with the same account).
3. Use the returned token against a protected endpoint:
   ```
   curl -H "Authorization: Bearer <accessToken>" https://localhost:<port>/api/trips
   ```
   Expect `200`. Omitting the header, or calling `/api/trips` before ever authenticating, returns `401`.
