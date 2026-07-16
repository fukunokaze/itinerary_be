## 1. Project Scaffolding

- [x] 1.1 Create `itinerary_be.Modules.Utility` class library (references `itinerary_be.Core`, `itinerary_be.Modules.Auth`, `itinerary_be.Modules.Itinerary`)
- [x] 1.2 Add it to `itinerary_be.slnx`, `itinerary_be.WebAPI.csproj`, and `itinerary_be.UnitTests.csproj`

## 2. Models

- [x] 2.1 `CalendarEntrySource` enum (`GoogleEvent`, `TripEvent`, `TripRange`) (`itinerary_be.Modules.Utility/Models`)
- [x] 2.2 `CalendarEntry` record (`Id`, `Source`, `Title`, `Start`, `End`, `IsAllDay`, `Description`, `Location`, `TripId`, `TripEventId`)
- [x] 2.3 `CalendarDay` record (`Date`, `Entries`)

## 3. Service

- [x] 3.1 `ICalendarService.GetCalendarAsync(Guid userId, DateOnly startDate, DateOnly endDate, string timeZoneId)` (`itinerary_be.Modules.Utility/Interfaces`)
- [x] 3.2 `CalendarService` implementation (`itinerary_be.Modules.Utility/Services`): validates range and timezone, fetches Google events via `IGoogleCalendarService`, Trips via `ITripService`, `TripEvents` via `ITripEventService`; buckets all three into one `CalendarDay` per date per the design decisions
- [x] 3.3 Register `ICalendarService`/`CalendarService` in new `UtilityServiceRegistration.cs` (`AddUtilityServices()`), called from `Program.cs`

## 4. Tests

- [x] 4.1 `CalendarServiceTests` (new) — full range with all three entry sources merges correctly; empty days still present; `startDate > endDate` throws `ArgumentException` without calling dependencies; unrecognized `timeZoneId` throws `ArgumentException` without calling dependencies; all-day Google event buckets by UTC date regardless of timezone; timed Google event buckets by converted local date; Trip-range marker present for every day in a Trip's range clipped to the query range; `GoogleReauthorizationRequiredException`/`GoogleCalendarApiException` propagate without calling `ITripService`/`ITripEventService`

## 5. Spec

- [x] 5.1 `## ADDED Requirements` delta for new `calendar-day-merge` capability

## 6. Verification

- [x] 6.1 `dotnet build itinerary_be.slnx` succeeds
- [x] 6.2 `dotnet test itinerary_be.UnitTests` passes with no regressions (79/79 passing, 0 regressions)
