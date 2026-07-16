## Why

`google-calendar-client` (KAN-13) made a user's Google Calendar events fetchable in-process via `IGoogleCalendarService.GetEventsAsync(userId, startDate, endDate)`, explicitly deferring any merging with Trip data to KAN-14. The "My Calendar" epic (KAN-7) needs a single day-by-day view for its Monthly calendar and day-detail dialog: Google events, days that fall inside a Trip's date range (marked locally, never synced back to Google), and the Trip's own `TripEvents` (flights/lodgings/activities) all need to show up together per day. No such merge exists yet.

## What Changes

- Add a new class library project `itinerary_be.Modules.Utility` for cross-cutting services that need both `itinerary_be.Modules.Auth` and `itinerary_be.Modules.Itinerary` — keeps neither feature module depending on the other.
- Add `CalendarDay`/`CalendarEntry`/`CalendarEntrySource` models: a day-by-day, timezone-bucketed view combining three entry sources — `GoogleEvent`, `TripEvent`, and `TripRange` (a local-only marker for days inside a Trip's `StartDate`/`EndDate`, never written back to Google).
- Add `ICalendarService`/`CalendarService`: `GetCalendarAsync(userId, startDate, endDate, timeZoneId)` — fetches Google events via `IGoogleCalendarService`, Trips/`TripEvents` via `ITripService`/`ITripEventService`, and buckets everything into one `CalendarDay` per date in the requested (inclusive) range, ordered ascending, in the caller-supplied IANA timezone.
- Register the new service in a `UtilityServiceRegistration.cs` (`AddUtilityServices()`), called once from `Program.cs`, mirroring `AddTripServices()`/`AddAuthServices()`.
- No new HTTP endpoint (that's KAN-15) — this is purely the merge/orchestration layer KAN-15 is expected to call.

## Capabilities

### Added Capabilities
- `calendar-day-merge`: produce a unified, timezone-bucketed day-by-day calendar view combining a user's Google Calendar events, their Trips' date-range markers, and their TripEvents.

## Impact

- **Code**: New project `itinerary_be.Modules.Utility` (references `itinerary_be.Core`, `itinerary_be.Modules.Auth`, `itinerary_be.Modules.Itinerary`); new `ICalendarService`/`CalendarService`, `CalendarDay`/`CalendarEntry`/`CalendarEntrySource` models, `UtilityServiceRegistration.cs`. `itinerary_be.slnx`, `itinerary_be.WebAPI.csproj`, and `itinerary_be.UnitTests.csproj` gain a reference to the new project; `Program.cs` calls `AddUtilityServices()`.
- **Database**: None.
- **APIs**: None — no controller/endpoint changes (KAN-15).
- **Dependencies**: None new (no additional NuGet packages).
