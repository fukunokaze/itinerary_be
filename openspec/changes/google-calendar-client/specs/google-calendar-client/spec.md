## ADDED Requirements

### Requirement: Fetch Google Calendar Events for a Date Range
The system SHALL fetch a user's Google Calendar events (primary calendar) for a given inclusive date range, using a currently-valid Google access token obtained on the user's behalf.

#### Scenario: Valid date range with a usable Google authorization
- **WHEN** `IGoogleCalendarService.GetEventsAsync(userId, startDate, endDate)` is called with `startDate <= endDate` for a user with a usable Google authorization
- **THEN** the system resolves a currently-valid access token via `IUserGoogleTokenService.GetValidAccessTokenAsync`, requests events on the user's primary calendar between `startDate` and `endDate` (inclusive), and returns them as `GoogleCalendarEvent` values ordered by start time, excluding cancelled events

#### Scenario: Start date after end date
- **WHEN** `GetEventsAsync` is called with `startDate` after `endDate`
- **THEN** the system throws `ArgumentException` without calling Google

#### Scenario: All-day event
- **WHEN** a Google Calendar event in the range has a `date`-only start/end (no time component)
- **THEN** the returned `GoogleCalendarEvent` has `IsAllDay = true` with `Start`/`End` set to UTC midnight of the corresponding dates

### Requirement: Distinct Error When the Access Token Is No Longer Valid
The system SHALL surface the existing `GoogleReauthorizationRequiredException` when the Google Calendar API itself rejects the access token, consistent with how a missing/revoked refresh token is reported elsewhere.

#### Scenario: No valid Google authorization for the user
- **WHEN** `IUserGoogleTokenService.GetValidAccessTokenAsync` throws `GoogleReauthorizationRequiredException` while resolving the access token
- **THEN** `GetEventsAsync` propagates the exception without calling the Calendar API

#### Scenario: Calendar API rejects the access token as unauthorized
- **WHEN** the Google Calendar API responds to the events request with an HTTP 401
- **THEN** the system throws `GoogleReauthorizationRequiredException`

#### Scenario: Calendar API fails for another reason
- **WHEN** the Google Calendar API responds with any other non-success status, or the request fails with a network/timeout error
- **THEN** the system throws `GoogleCalendarApiException`
