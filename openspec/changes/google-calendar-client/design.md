## Context

`google-token-refresh` (KAN-12) left "any Calendar API call" as an explicit Non-Goal, naming KAN-13/14/15 as the consumers of `GetValidAccessTokenAsync`. This change is KAN-13: the first actual call to Google's Calendar API. The existing Auth module (`GoogleOAuthClient`) hand-rolls raw `HttpClient` calls against Google's OAuth token endpoint rather than pulling in a Google client library — but the KAN-13 ticket explicitly asks for the `Google.Apis.Calendar.v3` NuGet package, so this change intentionally introduces the official SDK rather than extending the hand-rolled-HTTP convention.

## Goals / Non-Goals

**Goals:**
- Fetch a user's Google Calendar events (primary calendar) for a given day range, using an access token already obtained via `IUserGoogleTokenService.GetValidAccessTokenAsync`.
- Return a clean domain model, not the raw Google SDK `Event` type, so downstream code (KAN-14) doesn't take a direct dependency on `Google.Apis.Calendar.v3.Data`.
- Let a Calendar-API-specific failure (rejected/unauthorized access token) surface as the existing `GoogleReauthorizationRequiredException`, matching the error contract KAN-12 already established for "user must re-consent."

**Non-Goals:**
- Any HTTP endpoint (KAN-15).
- Merging Google events with Trip date ranges or `TripEvents` (KAN-14).
- Writing to Google Calendar (this integration is read-only).
- Pagination beyond a single date-range query — a single `Events.List` call with `SingleEvents = true` is assumed sufficient for typical per-day/per-trip ranges; revisit if a range regularly returns more events than one page.

## Decisions

**Decision 1: Use `Google.Apis.Calendar.v3.CalendarService` directly, not a hand-rolled HTTP client**
- **Chosen**: `GoogleCalendarClient` constructs a `CalendarService` per call, authorized via `GoogleCredential.FromAccessToken(accessToken)`.
- **Rationale**: The ticket explicitly asks for the `Google.Apis.Calendar.v3` package. `GoogleCredential.FromAccessToken` is the SDK's supported way to authorize a service with a bearer token that's already been obtained and refreshed elsewhere (no separate OAuth flow needed inside the Calendar client itself).

**Decision 2: `IGoogleCalendarClient` (token in, SDK out) vs. `IGoogleCalendarService` (userId in, orchestration)**
- **Chosen**: Same split as `IGoogleOAuthClient`/`IUserGoogleTokenService` — `GoogleCalendarClient` is a pure wrapper over the Calendar API given a bearer token; `GoogleCalendarService` owns calling `IUserGoogleTokenService.GetValidAccessTokenAsync(userId)` and delegating to the client.
- **Rationale**: Mirrors the existing token-refresh split (see `google-token-refresh`'s design Decision 1) and keeps `GoogleCalendarClient` testable/reusable independent of user/token concerns. `IGoogleCalendarService` is the entry point KAN-14/15 call.

**Decision 3: Day-range parameters as `DateOnly`, not `DateTime`/`DateTimeOffset`**
- **Chosen**: `GetEventsAsync(Guid userId, DateOnly startDate, DateOnly endDate)` (and the client's `GetEventsAsync(string accessToken, DateOnly startDate, DateOnly endDate)`).
- **Rationale**: `Trip.StartDate`/`Trip.EndDate` are already `DateOnly` — KAN-14 will merge Calendar events with Trip ranges, so matching that type avoids a conversion at the merge boundary. Internally, the range is converted to UTC `DateTimeOffset` bounds (`startDate` 00:00 UTC through `endDate + 1 day` 00:00 UTC, exclusive) for Google's `TimeMin`/`TimeMax` request parameters.

**Decision 4: Unauthorized Calendar API response reuses `GoogleReauthorizationRequiredException`; other failures use a new `GoogleCalendarApiException`**
- **Chosen**: `GoogleCalendarClient` catches `Google.GoogleApiException` — an HTTP 401 (`HttpStatusCode.Unauthorized`) throws the existing `GoogleReauthorizationRequiredException`; any other API failure, network error, or timeout throws a new `GoogleCalendarApiException`.
- **Rationale**: A 401 from the Calendar API means the access token itself is no longer valid (e.g. revoked between the refresh check and this call, or scope insufficient) — semantically identical to "the user must re-consent," which is exactly what `GoogleReauthorizationRequiredException` already communicates to callers. Other failures (5xx, quota, network) are a different kind of problem and get their own type so callers don't conflate "needs re-auth" with "transient Calendar API failure."

**Decision 5: All-day events are mapped to UTC midnight with `IsAllDay = true`, not kept as bare dates**
- **Chosen**: `GoogleCalendarEvent.Start`/`End` are always `DateTimeOffset`; for all-day events (which Google represents as a `Date` string with no time/timezone), the mapper parses the date and treats it as UTC midnight, setting `IsAllDay = true` so callers can distinguish it from a real UTC-midnight timed event.
- **Rationale**: Keeps `GoogleCalendarEvent` a single consistent shape rather than a union of "timed" vs. "all-day" representations, which is simpler for KAN-14's merge logic to consume; the `IsAllDay` flag preserves the information a caller needs to render/group all-day events correctly.

## Risks / Trade-offs

- **No pagination**: if a date range's event count exceeds one page from Google, results are silently truncated to the first page. Acceptable for expected per-day/per-trip range sizes; revisit if KAN-14/15 usage patterns request wide multi-month ranges.
- **A new `CalendarService`/`HttpClient` is constructed per call** rather than reused across requests. Consistent with the SDK's typical usage pattern for per-request bearer-token authorization (the credential is tied to a specific already-obtained token, not a reusable service-level identity); acceptable given Calendar API call volume is expected to be low relative to other backend traffic.
