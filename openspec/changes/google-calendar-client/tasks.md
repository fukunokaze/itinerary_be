## 1. Auth Module

- [x] 1.1 Add `Google.Apis.Calendar.v3` NuGet package reference (`itinerary_be.Modules.Auth`)
- [x] 1.2 Add `GoogleCalendarEvent` model (`itinerary_be.Modules.Auth/Models`)
- [x] 1.3 Add `GoogleCalendarApiException` (`itinerary_be.Modules.Auth/Exceptions`)
- [x] 1.4 Add `IGoogleCalendarClient.GetEventsAsync(string accessToken, DateOnly startDate, DateOnly endDate)` + `GoogleCalendarClient` implementation (SDK `CalendarService` via `GoogleCredential.FromAccessToken`, 401 → `GoogleReauthorizationRequiredException`, other failures → `GoogleCalendarApiException`)
- [x] 1.5 Add `IGoogleCalendarService.GetEventsAsync(Guid userId, DateOnly startDate, DateOnly endDate)` + `GoogleCalendarService` implementation (resolves access token via `IUserGoogleTokenService.GetValidAccessTokenAsync`, delegates to `IGoogleCalendarClient`)
- [x] 1.6 Register `IGoogleCalendarClient`/`GoogleCalendarClient` and `IGoogleCalendarService`/`GoogleCalendarService` in `AuthServiceRegistration.cs`

## 2. Tests

- [x] 2.1 `GoogleCalendarClientTests` (new) — success maps timed and all-day events, filters cancelled events; 401 throws `GoogleReauthorizationRequiredException`; other non-2xx throws `GoogleCalendarApiException`
- [x] 2.2 `GoogleCalendarServiceTests` (new) — valid range delegates to client with the resolved access token; `startDate > endDate` throws `ArgumentException` without calling dependencies; `GoogleReauthorizationRequiredException` from the token service propagates without calling the client

## 3. Spec

- [x] 3.1 `## ADDED Requirements` delta for new `google-calendar-client` capability

## 4. Verification

- [x] 4.1 `dotnet build itinerary_be.slnx` succeeds
- [x] 4.2 `dotnet test itinerary_be.UnitTests` passes with no regressions (70/70 passing, 0 regressions)
