## ADDED Requirements

### Requirement: Encrypted Persistence of Google OAuth Tokens
The system SHALL persist the Google `access_token` and `refresh_token` obtained during login, encrypted at rest, keyed one-to-one with the local User.

#### Scenario: Successful login persists tokens
- **WHEN** `LoginWithGoogleAsync` completes a successful code exchange and JWT issuance for a user
- **THEN** the system encrypts the returned `access_token` and `refresh_token` (if present) and upserts them into that user's `user_google_tokens` row along with `expires_at`, `scope`, and `updated_at`

#### Scenario: Returning login without a new refresh token preserves the prior one
- **WHEN** a user logs in again and Google's exchange response omits `refresh_token` (returns null)
- **THEN** the system overwrites `access_token`, `expires_at`, `scope`, and `updated_at` but leaves the previously stored encrypted `refresh_token` unchanged

#### Scenario: Token storage failure does not block login
- **WHEN** persisting the encrypted tokens throws (e.g. a database error)
- **THEN** the system logs the failure and still returns the issued JWT and user profile to the caller
