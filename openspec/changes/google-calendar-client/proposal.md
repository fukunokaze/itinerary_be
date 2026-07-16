## Why

`google-token-refresh` (KAN-12) made a currently-valid Google access token obtainable in-process via `IUserGoogleTokenService.GetValidAccessTokenAsync(Guid userId)`, explicitly deferring any actual Calendar API call to KAN-13/14/15. No Google Calendar integration exists in the codebase yet. This change (KAN-13) adds that integration: the `Google.Apis.Calendar.v3` client library plus a wrapper service that fetches a user's Google Calendar events for a given date range, so KAN-14 (merging Google events with Trip date ranges) and KAN-15 (Calendar API endpoints) have something to build on.

## What Changes

- Add the `Google.Apis.Calendar.v3` NuGet package to `itinerary_be.Modules.Auth`.
- Add `IGoogleCalendarClient`/`GoogleCalendarClient`: a thin wrapper around `Google.Apis.Calendar.v3.CalendarService` that lists events on the user's primary calendar within a `[startDate, endDate]` day range (inclusive), given a bearer access token.
- Add `IGoogleCalendarService`/`GoogleCalendarService`: the caller-facing wrapper — resolves a valid access token via `IUserGoogleTokenService.GetValidAccessTokenAsync(userId)`, then delegates to `IGoogleCalendarClient`. This is the entry point KAN-14/15 are expected to call.
- Add `GoogleCalendarEvent`: a clean domain model (`Id`, `Title`, `Start`, `End`, `IsAllDay`, `Description`, `Location`) decoupled from the Google SDK's `Event` type.
- Add `GoogleCalendarApiException`: thrown for Calendar API failures other than an unauthorized/rejected access token (which instead surfaces the existing `GoogleReauthorizationRequiredException`, consistent with KAN-12's error contract).
- No new HTTP endpoint (that's KAN-15) and no merging with Trip data (that's KAN-14) — this is purely the Calendar API integration layer.

## Capabilities

### Added Capabilities
- `google-calendar-client`: fetch a user's Google Calendar events for a given date range using their stored (and transparently refreshed) access token.

## Impact

- **Code**: `itinerary_be.Modules.Auth` — new `IGoogleCalendarClient`/`GoogleCalendarClient`, `IGoogleCalendarService`/`GoogleCalendarService`, `GoogleCalendarEvent` model, `GoogleCalendarApiException`; both new services registered in `AuthServiceRegistration.cs`.
- **Database**: None.
- **APIs**: None — no controller/endpoint changes (KAN-15).
- **Dependencies**: New NuGet package `Google.Apis.Calendar.v3` in `itinerary_be.Modules.Auth`.
