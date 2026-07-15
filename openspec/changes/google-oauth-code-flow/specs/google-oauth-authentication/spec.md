## MODIFIED Requirements

### Requirement: Google Sign-In and JIT User Registration
The system SHALL allow clients to authenticate by sending a Google OAuth authorization code, and SHALL automatically create a local User profile on first sign-in.

#### Scenario: First-time sign-in creates a new user
- **WHEN** a POST request is sent to `/api/auth/google` with a valid Google authorization code for an email that has no existing User
- **THEN** the system exchanges the code for tokens, validates the returned ID token, creates a new User record using the token's email and name claims, and returns HTTP 200 with an access token and the user profile

#### Scenario: Returning user signs in without duplication
- **WHEN** a POST request is sent to `/api/auth/google` with a valid Google authorization code for an email that already has a User
- **THEN** the system returns HTTP 200 with an access token and the existing user profile, without creating a duplicate User record

#### Scenario: Invalid or expired authorization code
- **WHEN** a POST request is sent to `/api/auth/google` with an authorization code that Google's token endpoint rejects (invalid, expired, or already redeemed)
- **THEN** the system returns HTTP 401 Unauthorized and does not create or return any user/token

#### Scenario: Invalid or expired Google ID token in exchange response
- **WHEN** a POST request is sent to `/api/auth/google` with a code that exchanges successfully but whose returned ID token fails signature, issuer, audience, or expiry validation
- **THEN** the system returns HTTP 401 Unauthorized and does not create or return any user/token

#### Scenario: Unverified email
- **WHEN** a POST request is sent to `/api/auth/google` with a code that exchanges to an ID token whose `email_verified` claim is false
- **THEN** the system returns HTTP 401 Unauthorized and does not create a user or issue a token
