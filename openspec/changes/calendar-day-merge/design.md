## Context

KAN-13 left "merging Google events with Trip date ranges" as an explicit Non-Goal, naming KAN-14 as the consumer of `IGoogleCalendarService.GetEventsAsync`. The parent epic (KAN-7, "My Calendar") needs: a Monthly view that marks which days have something on them, and a day-detail dialog "detailing the events within the day" when a date is clicked. KAN-14 builds the merge logic both views will read from; KAN-15 wraps it in an endpoint.

The KAN-14 ticket body flagged an open question — whether `TripEvents` should feed into day-detail results — and referenced "the user's local timezone (per decision)" without that decision existing anywhere in the schema (no `User.Timezone` column). Both were resolved with the user before implementation:
1. **TripEvents are included** in the merged day model, not just Google events + the Trip-range marker.
2. **The caller supplies an IANA timezone id per call** (no schema change) — the frontend already knows the browser/user's timezone; storing one on `User` can be revisited later if a server-side default is ever needed.
3. **The merge service lives in a new project, `itinerary_be.Modules.Utility`**, rather than adding a `Modules.Itinerary → Modules.Auth` (or reverse) project reference — keeps the two feature modules independent of each other; a service that inherently needs both lives in a project that depends on both instead.

## Goals / Non-Goals

**Goals:**
- Produce one `CalendarDay` per date in a requested inclusive `[startDate, endDate]` range (including empty days), each holding the `CalendarEntry` values that land on that date once bucketed into the caller's timezone.
- Combine three entry sources into that per-day bucket: Google Calendar events (KAN-13), Trip `TripEvents`, and a `TripRange` marker entry for every day a Trip's `[StartDate, EndDate]` covers — the marker is local-only and is never written back to Google.
- Keep `CalendarService` as pure orchestration over existing service interfaces (`IGoogleCalendarService`, `ITripService`, `ITripEventService`) — no direct repository/DbContext access, no new persistence.

**Non-Goals:**
- Any HTTP endpoint (KAN-15).
- Writing anything to Google Calendar (still read-only, per KAN-13).
- Partial-failure handling when Google auth is missing/expired — `GoogleReauthorizationRequiredException`/`GoogleCalendarApiException` from `IGoogleCalendarService` propagate as-is; `CalendarService` does not catch them to fall back to Trip-only data. KAN-15 decides how that surfaces over HTTP.
- Storing a per-user timezone — the timezone is a required parameter on every call.
- Correcting for events whose local date (after timezone conversion) falls just outside the requested `[startDate, endDate]` — see Risks below.

## Decisions

**Decision 1: New `itinerary_be.Modules.Utility` project instead of a cross-module reference**
- **Chosen**: `CalendarService` lives in a new class library that references `itinerary_be.Modules.Auth` and `itinerary_be.Modules.Itinerary`; neither of those two reference each other.
- **Rationale**: User's explicit call. Avoids picking a "winner" between the two feature modules for an orchestration concern that isn't really Trip-specific or Auth-specific — it's a third thing that depends on both. Keeps each feature module's dependency graph shallow and independently testable.

**Decision 2: One `CalendarDay` per date in range, always, even if empty**
- **Chosen**: `GetCalendarAsync` returns `IReadOnlyList<CalendarDay>` with exactly `(endDate - startDate).Days + 1` entries, ordered ascending by `Date`, `Entries` empty where nothing lands on that day.
- **Rationale**: The Monthly view needs to render every day of the month regardless of content (KAN-7: "Calendar should only show compact information" per day) — a sparse/keyed result would push that reconstruction onto every caller. A dense list is also trivially convertible to a dictionary keyed by `Date` if a caller wants that instead.

**Decision 3: `CalendarEntry` is one shape for all three sources, distinguished by `Source` + optional `TripId`/`TripEventId`**
- **Chosen**: `CalendarEntry(Id, Source, Title, Start, End, IsAllDay, Description, Location, TripId, TripEventId)` where `Source` is `GoogleEvent | TripEvent | TripRange`. `TripId`/`TripEventId` are null unless `Source` is `TripEvent` (both set) or `TripRange` (`TripId` only).
- **Rationale**: Mirrors KAN-13's Decision 5 (`GoogleCalendarEvent` as one consistent shape rather than a union) for the same reason — simpler for callers (KAN-15, and eventually the frontend DTO mapping) to render a flat list per day instead of switching on three separate record types. The `Id` field reuses the Google event id verbatim for `GoogleEvent` entries, `TripEvent.Id` (stringified) for `TripEvent` entries, and a synthesized `"trip-range-{tripId}"` for `TripRange` entries (one such id recurs across every day of that trip's range, since it represents the same marker repeated per day, not a distinct entity per day).

**Decision 4: All-day Google events bucket by their UTC date directly; timed events convert through the caller's timezone**
- **Chosen**: For `GoogleEvent` entries where `IsAllDay == true`, the bucket date is `DateOnly.FromDateTime(event.Start.UtcDateTime)` (no timezone conversion). For timed entries, the bucket date is `DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(event.Start, timeZone).DateTime)`.
- **Rationale**: KAN-13 already fixed all-day `Start`/`End` to UTC midnight of the *original* calendar date specifically so it wouldn't need timezone math (see KAN-13 design Decision 5). Converting that UTC-midnight instant through a non-UTC timezone would shift it to the wrong day for any timezone behind UTC. Timed events don't have that problem — they're a real instant, so converting through the caller's timezone is the correct way to find "which local day this happened on."

**Decision 5: Trip-range and TripEvent entries are synthesized directly in the caller's timezone, not converted**
- **Chosen**: `TripRange` entries get `Start`/`End` set to local midnight/midnight+1day of the relevant date (via `timeZone.GetUtcOffset(date)`, so DST-correct). `TripEvent` entries with a `StartTime`/`EndTime` combine `TripEvent.Date` + the `TimeOnly` directly as a local wall-clock time in the caller's timezone (not UTC-converted, since `TripEvent` has no stored timezone of its own — `Date`/`StartTime`/`EndTime` are entered by the user against their own trip, so treating them as already-local is the only sound interpretation).
- **Rationale**: `Trip`/`TripEvent` are plain `DateOnly`/`TimeOnly` with no offset — there's nothing to convert from. Treating them as wall-clock time in the caller-supplied timezone keeps them trivially on the correct day without inventing a UTC interpretation the schema doesn't have.

## Risks / Trade-offs

- **Boundary events can be dropped**: `IGoogleCalendarService.GetEventsAsync` queries Google using UTC day boundaries (KAN-13 Decision 3); after converting to the caller's local timezone, an event near the start/end of the requested range can bucket to a local date just outside `[startDate, endDate]` and be silently excluded (no bucket exists for it). Acceptable for a Monthly-view caller that naturally queries a few extra padding days; revisit if KAN-15 callers query tight/exact ranges and this becomes visible.
- **No pagination beyond what KAN-13 already returns** — same limitation, inherited as-is.
- **`ITripService.GetAllTripsAsync`/`ITripEventService.GetByTripIdAsync` are called per-request with in-memory range filtering** (no dedicated "trips overlapping a range" or "events overlapping a range" repository query) — acceptable given expected per-user trip counts; revisit if this shows up as a hot path.
