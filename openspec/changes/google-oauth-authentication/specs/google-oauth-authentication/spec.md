## ADDED Requirements

### Requirement: Google Sign-In and JIT User Registration
The system SHALL allow clients to authenticate by sending a Google ID token, and SHALL automatically create a local User profile on first sign-in.

#### Scenario: First-time sign-in creates a new user
- **WHEN** a POST request is sent to `/api/auth/google` with a valid Google ID token for an email that has no existing User
- **THEN** the system creates a new User record using the token's email and name claims, and returns HTTP 200 with an access token and the user profile

#### Scenario: Returning user signs in without duplication
- **WHEN** a POST request is sent to `/api/auth/google` with a valid Google ID token for an email that already has a User
- **THEN** the system returns HTTP 200 with an access token and the existing user profile, without creating a duplicate User record

#### Scenario: Invalid or expired Google token
- **WHEN** a POST request is sent to `/api/auth/google` with a Google ID token that fails signature, issuer, audience, or expiry validation
- **THEN** the system returns HTTP 401 Unauthorized and does not create or return any user/token

#### Scenario: Unverified email
- **WHEN** a POST request is sent to `/api/auth/google` with a Google ID token whose `email_verified` claim is false
- **THEN** the system returns HTTP 401 Unauthorized and does not create a user or issue a token

### Requirement: Global Authentication Enforcement
The system SHALL require a valid backend-issued bearer token on every endpoint except the Google sign-in endpoint.

#### Scenario: Unauthenticated request to a protected endpoint
- **WHEN** a request is sent to any existing endpoint (e.g. `/api/trips`) without an `Authorization` header, or with an invalid/expired bearer token
- **THEN** the system returns HTTP 401 Unauthorized

#### Scenario: Authenticated request to a protected endpoint
- **WHEN** a request is sent to any existing endpoint with a valid bearer token issued by `/api/auth/google`
- **THEN** the system processes the request normally

#### Scenario: Sign-in endpoint remains accessible without a token
- **WHEN** a request is sent to `/api/auth/google` without any `Authorization` header
- **THEN** the system processes the request (subject only to Google token validation, not to the bearer-token requirement)
