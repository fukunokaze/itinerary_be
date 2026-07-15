## Context

`openspec/changes/google-oauth-authentication/design.md` built the current login flow and made two decisions this change explicitly supersedes:

- **Decision 1** chose ID-token verification over a server-side authorization-code flow, calling the latter "unnecessary complexity for a stateless JSON API."
- The **Non-Goals** section excluded refresh tokens entirely: "expired/compromised tokens are handled solely by short expiry and secret rotation."

Both were correct calls at the time — there was no need to request anything beyond basic profile/email claims. That's no longer true: KAN-7 ("Implement My Calendar feature") needs `calendar.readonly` access, which is only obtainable via the standard OAuth consent + authorization-code + token-exchange flow with offline access requested.

## Goals / Non-Goals

**Goals:**
- Support requesting the `calendar.readonly` scope (and offline access, so Google issues a refresh token) at login time, without changing the "one call handles login and signup" UX.
- Keep the backend's role in the exchange minimal: swap the code for tokens, validate identity, issue the app JWT — same shape of responsibility as before.

**Non-Goals (still, deferred to sibling KAN-7 subtasks):**
- Persisting or encrypting the Google access/refresh tokens (KAN-11).
- Provisioning the real `ClientSecret` value or registering the redirect URI in Google Cloud Console (KAN-10).
- Any actual use of the Calendar API (KAN-13/KAN-14/KAN-15).
- Refreshing expired Google access tokens (KAN-12).

## Decisions

**Decision 1: Server-side authorization-code exchange, not pure ID-token verification**
- **Chosen**: The frontend performs the OAuth consent flow and sends the resulting authorization `code`; the backend exchanges it for tokens via a direct POST to Google's token endpoint.
- **Rationale**: `calendar.readonly` and offline access are only obtainable through the standard consent + code + exchange flow — a bare ID token has no mechanism to carry additional scope grants or a refresh token. This directly supersedes the prior change's Decision 1.

**Decision 2: Hand-rolled `HttpClient` token exchange, not `Google.Apis.Auth.OAuth2.GoogleAuthorizationCodeFlow`**
- **Chosen**: A thin typed `HttpClient` (`GoogleOAuthClient`) posts directly to `https://oauth2.googleapis.com/token` and parses the JSON response.
- **Rationale**: `GoogleAuthorizationCodeFlow`/`IAuthorizationCodeFlow` is built around a persistent `IDataStore` for the installed-app/offline-store pattern — it assumes tokens are saved and later retrieved by user id. That's a mismatch for this change, which explicitly discards the tokens after login. A direct HTTP call is simpler, has no new abstraction to learn, and matches the existing `GoogleTokenValidator`'s style of being a thin wrapper over one Google primitive — consistent with the original design's "keep the implementation minimal" goal.

**Decision 3: Request offline access now, but still discard the refresh token**
- **Chosen**: The frontend requests `access_type=offline` (so Google returns a `refresh_token`) even though the backend does nothing with it yet.
- **Rationale**: Google generally only issues a `refresh_token` on a user's *first* authorization for a given client+scope combination, unless the frontend forces `prompt=consent` on every login. Requesting offline access now means the plumbing is in place; if KAN-11 needs to guarantee a refresh token is available for already-authorized users, that's a frontend consent-prompt decision to revisit at that time, not a backend concern here.

**Decision 4: Reuse `InvalidGoogleTokenException` for code-exchange failures**
- **Chosen**: A failed exchange (non-2xx response, missing `id_token`, network/parse failure) throws the same `InvalidGoogleTokenException` that ID-token validation failures already throw.
- **Rationale**: `AuthController` needs no new catch block, and the client-facing contract stays "401 means the Google login step failed" without needing to distinguish exchange failure from token-validity failure.

## Risks / Trade-offs

- **Discarding tokens has a re-consent cost later**: since no refresh token is persisted, when KAN-11 lands, some already-migrated users may not get a fresh refresh token on their next login unless Google is asked to re-prompt for consent. Flagged for whoever picks up KAN-11, not solved here per the confirmed KAN-9 scope.
- **Config coupling to KAN-10**: `GoogleAuthOptions.ClientSecret`/`RedirectUri` are validated at startup (`ValidateOnStart`) but shipped with empty placeholders — the app will fail to start in any environment until KAN-10 supplies real values, same as the existing `ClientId` requirement today.
- **Verification gap**: the full happy path (real consent screen → code → exchange → JWT) can't be exercised end-to-end until KAN-10 (Console config) and the frontend's authorization-code flow both exist. This change verifies what it can in isolation (validation errors, exchange-failure handling, unit tests with mocked collaborators) and explicitly flags the rest as unverified until then.
