## ADDED Requirements

### Requirement: Merge Google Events, Trip Ranges, and TripEvents into a Day-by-Day Model
The system SHALL produce one `CalendarDay` per date in a given inclusive date range, each containing the `CalendarEntry` values — from Google Calendar events, Trip date-range markers, and `TripEvents` — that land on that date once bucketed into a caller-supplied timezone.

#### Scenario: Valid range and timezone with events from all three sources
- **WHEN** `ICalendarService.GetCalendarAsync(userId, startDate, endDate, timeZoneId)` is called with `startDate <= endDate` and a valid IANA `timeZoneId`
- **THEN** the system returns exactly one `CalendarDay` per date in `[startDate, endDate]`, ordered ascending by `Date`, where each day's `Entries` include: Google Calendar events whose local date (per `timeZoneId`) falls on that day, `TripEvent`s whose `Date` falls on that day, and one `TripRange` entry per Trip whose `[StartDate, EndDate]` covers that day

#### Scenario: Day with no activity
- **WHEN** a date in the requested range has no Google event, `TripEvent`, or overlapping Trip
- **THEN** the corresponding `CalendarDay` is still present in the result with an empty `Entries` list

#### Scenario: Start date after end date
- **WHEN** `GetCalendarAsync` is called with `startDate` after `endDate`
- **THEN** the system throws `ArgumentException` without calling any dependency

#### Scenario: Unrecognized timezone id
- **WHEN** `GetCalendarAsync` is called with a `timeZoneId` that does not resolve to a known time zone
- **THEN** the system throws `ArgumentException` without calling any dependency

### Requirement: All-Day Google Events Bucket by Their Original Date, Not a Timezone-Shifted One
The system SHALL bucket an all-day Google Calendar event onto its original UTC-midnight date rather than converting it through the caller's timezone.

#### Scenario: All-day Google event
- **WHEN** a Google Calendar event has `IsAllDay = true` (per KAN-13's mapping, `Start` is UTC midnight of the event's calendar date)
- **THEN** the resulting `CalendarEntry` is bucketed onto `DateOnly.FromDateTime(Start.UtcDateTime)`, unaffected by `timeZoneId`

### Requirement: Timed Google Events Bucket by Local Date in the Caller's Timezone
The system SHALL convert a timed Google Calendar event's start instant into the caller-supplied timezone before determining which day it lands on.

#### Scenario: Timed Google event
- **WHEN** a Google Calendar event has `IsAllDay = false`
- **THEN** the resulting `CalendarEntry` is bucketed onto the date obtained by converting `Start` into `timeZoneId` and taking its date component

### Requirement: Trip-Range Markers Are Local-Only and Never Synced to Google
The system SHALL represent each day inside a Trip's `[StartDate, EndDate]` as a local `CalendarEntry` with `Source = TripRange`, distinct from any Google-sourced entry, and SHALL NOT write it to the user's Google Calendar.

#### Scenario: Date inside a Trip's range
- **WHEN** a date in the requested range falls within `[Trip.StartDate, Trip.EndDate]` for a Trip belonging to the user
- **THEN** the corresponding `CalendarDay.Entries` includes a `CalendarEntry` with `Source = TripRange` and `TripId` set to that Trip's id

### Requirement: Errors from the Google Calendar Layer Propagate Unchanged
The system SHALL propagate `GoogleReauthorizationRequiredException` and `GoogleCalendarApiException` from the underlying `IGoogleCalendarService` call without catching or wrapping them.

#### Scenario: No valid Google authorization for the user
- **WHEN** `IGoogleCalendarService.GetEventsAsync` throws `GoogleReauthorizationRequiredException`
- **THEN** `GetCalendarAsync` propagates the exception without calling `ITripService`/`ITripEventService`

#### Scenario: Google Calendar API failure
- **WHEN** `IGoogleCalendarService.GetEventsAsync` throws `GoogleCalendarApiException`
- **THEN** `GetCalendarAsync` propagates the exception
