## ADDED Requirements

### Requirement: On-Demand Valid Google Access Token
The system SHALL provide a way to obtain a currently-valid Google access token for a user, transparently refreshing an expired one via the stored refresh token.

#### Scenario: Stored access token is still valid
- **WHEN** `GetValidAccessTokenAsync` is called for a user whose stored `access_token` expires more than 60 seconds from now
- **THEN** the system decrypts and returns the stored `access_token` without calling Google

#### Scenario: Stored access token is expired or expiring soon
- **WHEN** `GetValidAccessTokenAsync` is called for a user whose stored `access_token` expires within 60 seconds or has already expired, and a `refresh_token` is on file
- **THEN** the system exchanges the decrypted `refresh_token` for a new access token via Google's token endpoint, persists the new encrypted `access_token` and `expires_at` (leaving `refresh_token` unchanged), and returns the new decrypted `access_token`

### Requirement: Distinct Error for Revoked or Missing Google Authorization
The system SHALL surface a distinct error when no valid access token can be obtained because the user must re-authenticate with Google.

#### Scenario: No stored Google authorization for the user
- **WHEN** `GetValidAccessTokenAsync` is called for a user with no `user_google_tokens` row
- **THEN** the system throws `GoogleReauthorizationRequiredException`

#### Scenario: Expired access token with no refresh token on file
- **WHEN** `GetValidAccessTokenAsync` is called for a user whose stored `access_token` is expired and `refresh_token` is null
- **THEN** the system throws `GoogleReauthorizationRequiredException` without calling Google

#### Scenario: Google reports the refresh token is invalid or revoked
- **WHEN** a refresh request to Google's token endpoint fails with `error=invalid_grant` (or an HTTP 401)
- **THEN** the system throws `GoogleReauthorizationRequiredException` instead of a generic failure
