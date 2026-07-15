## Context

`google-oauth-code-flow`'s design.md flagged this as a Non-Goal and left a risk note: because Google only issues a `refresh_token` on a user's first authorization for a client+scope combination, some users who logged in before this change may not get a fresh refresh token on their next login unless the frontend forces re-consent. That risk is inherited here, not solved — an upsert that blindly overwrote `refresh_token` with `null` on every subsequent login would actively make it worse by erasing a token that was captured once.

## Goals / Non-Goals

**Goals:**
- Persist the Google `access_token`/`refresh_token` pair per user, encrypted at rest, after a successful login.
- Never regress a previously-captured refresh token to `null` just because a later login didn't get a new one from Google.

**Non-Goals:**
- Reading tokens back out via any API endpoint (no consumer exists yet).
- Refreshing an expired access token (KAN-12).
- Any Calendar API usage (KAN-13/14/15).
- Key management for the Data Protection key ring beyond the ASP.NET Core default (file-system key store) — revisit if/when this ships to a multi-instance production deployment where key sharing across instances matters.

## Decisions

**Decision 1: One row per user (`user_id` as primary key), not a history table**
- **Chosen**: `user_google_tokens.user_id` is both the primary key and the FK to `users.id`; each login upserts that single row.
- **Rationale**: Only the latest token is ever useful for calling Google's APIs on the user's behalf — there's no use case for keeping historical tokens, and a 1:1 table keeps the upsert trivial (no "find the current row among many" query).

**Decision 2: ASP.NET Data Protection API for encryption, not a custom AES implementation**
- **Chosen**: `IDataProtectionProvider.CreateProtector("...")` wraps encryption/decryption of both token strings.
- **Rationale**: Matches the ticket's explicit instruction and is the standard .NET mechanism for exactly this use case (encrypt-before-persist, decrypt-on-read) — it handles key rotation and versioning so a custom scheme doesn't have to.

**Decision 3: Preserve the existing refresh token when a new exchange doesn't return one**
- **Chosen**: The upsert only overwrites `refresh_token` when the incoming `GoogleTokenResponse.RefreshToken` is non-null; `access_token`, `expires_at`, `scope`, and `updated_at` are always overwritten.
- **Rationale**: Directly addresses the risk flagged in KAN-9's design — a returning user's second login (no `prompt=consent`) legitimately gets `refresh_token: null` from Google, and losing the one already on file would silently break any later Calendar-API use for that user.

**Decision 4: Token persistence failures do not fail the login request**
- **Chosen**: `AuthService.LoginWithGoogleAsync` still returns the JWT even if saving the Google tokens throws; the failure is logged as a warning.
- **Rationale**: Login (JWT issuance) is the primary, already-shipped contract of this endpoint; a database hiccup on the token-storage side-effect shouldn't turn into a 500 that blocks sign-in for a feature (Calendar) the user may not even use yet.

## Risks / Trade-offs

- **Data Protection default key storage isn't guaranteed durable/shared across instances.** Acceptable for the current single-instance dev/staging setup; must be revisited (e.g. `PersistKeysToDbContext` or a shared key ring) before a multi-instance production deployment, or previously-encrypted tokens become undecryptable after a key loss.
- **Silent best-effort persistence** (Decision 4) means a failed save is only visible in logs, not to the caller. Acceptable because nothing yet depends on the stored token; revisit once KAN-12/13 land and a missing token becomes user-visible.
