## Context

`google-token-storage` (KAN-11) added `user_google_tokens` (encrypted `access_token`/`refresh_token`, keyed by `user_id`) but named "Refreshing an expired access token (KAN-12)" as an explicit Non-Goal. Its access tokens are short-lived (Google's default is ~1 hour, per `expires_in`), so anything that later calls the Calendar API (KAN-13/14/15) needs a valid access token on demand, not just whatever was captured at login.

## Goals / Non-Goals

**Goals:**
- Give callers a single entry point that returns a currently-valid access token for a user, refreshing transparently via the stored refresh token when the cached one is expired or about to expire.
- Let a caller distinguish "no valid access token could be obtained, and the user must re-consent with Google" from other failure modes.

**Non-Goals:**
- Any Calendar API call (KAN-13/14/15).
- Any new HTTP endpoint — this is an in-process service method for other backend code to call.
- Handling concurrent refresh races across multiple requests for the same user (accepted risk, see below).

## Decisions

**Decision 1: `GetValidAccessTokenAsync` lives on `IUserGoogleTokenService`, refresh HTTP call lives on `IGoogleOAuthClient`**
- **Chosen**: `IGoogleOAuthClient.RefreshAccessTokenAsync(string refreshToken)` is a thin addition next to the existing `ExchangeCodeAsync` — same Google token endpoint, same raw-`HttpClient`-plus-manual-JSON style, no new abstraction. `IUserGoogleTokenService.GetValidAccessTokenAsync(Guid userId)` owns the orchestration: read the stored row, decrypt, check expiry, call the OAuth client if needed, re-encrypt, persist, return plaintext.
- **Rationale**: Mirrors the existing split — `GoogleOAuthClient` is a pure wrapper over Google's token endpoint, `UserGoogleTokenService` already owns the `IDataProtector` and the encrypt/decrypt boundary for this table. Keeping that boundary means the raw HTTP client never needs to know about encryption, and the encryption-owning service never needs to know Google's wire format beyond the response shape.

**Decision 2: `GoogleReauthorizationRequiredException`, not a reuse of `InvalidGoogleTokenException`**
- **Chosen**: A new exception type, distinct from the login-flow's `InvalidGoogleTokenException`.
- **Rationale**: The ticket explicitly asks for "a distinct error so the frontend can prompt the user to re-authenticate" — a Calendar-API caller catching this needs to tell "your Google connection needs re-consent" apart from "this login attempt itself failed," which is what `InvalidGoogleTokenException` means today (`AuthController` maps it to 401 on `POST /api/auth/google`). Reusing it would conflate two different HTTP-facing behaviors once a Calendar controller exists.
- Thrown from `GetValidAccessTokenAsync` when: no `user_google_tokens` row exists, the row has no `refresh_token`, or a refresh attempt gets `error=invalid_grant` back from Google (Google's actual response for a revoked/expired/invalid refresh token — HTTP 400 in practice, though the ticket also mentions 401; both are treated as revoked/invalid).

**Decision 3: A 60-second expiry buffer, refresh triggered proactively**
- **Chosen**: `GetValidAccessTokenAsync` treats a token as needing refresh when `ExpiresAt <= DateTime.UtcNow.AddSeconds(60)`, not only when already expired.
- **Rationale**: Avoids a race where the token is valid at the check but expires by the time the caller's outbound Calendar API request lands.

**Decision 4: On successful refresh, only `AccessToken`/`ExpiresAt` are updated; `RefreshToken` is left as-is**
- **Chosen**: `UpsertAsync` is called with the existing (still-encrypted) `RefreshToken` value unchanged, new encrypted `AccessToken`, and new `ExpiresAt`/`Scope`/`UpdatedAt`.
- **Rationale**: Google's `grant_type=refresh_token` response normally does not include a new `refresh_token` — only a full authorization-code exchange does. Treating a missing `refresh_token` in the refresh response as "clear it" would break future refreshes, the same failure mode KAN-11's Decision 3 already guards against for login.

## Risks / Trade-offs

- **No distributed lock around refresh**: two concurrent calls to `GetValidAccessTokenAsync` for the same user with an expired token will both call Google's refresh endpoint. Google tolerates this (each valid refresh token can be exchanged repeatedly), so the outcome is a harmless duplicate HTTP call, not a correctness bug. Acceptable for current traffic; revisit only if this becomes a hot path with real concurrency.
- **Revoked-grant detection depends on Google's error body shape** (`{"error":"invalid_grant",...}`), not just status code, since Google returns 400 (not 401) for this in practice. If Google changes its error contract this silently stops being detected as "must re-auth" and instead surfaces as a generic failure — acceptable given this mirrors how `GoogleOAuthClient.ExchangeCodeAsync` already parses Google's responses.
