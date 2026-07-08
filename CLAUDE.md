# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Backend for a Travel Planner app ("My Itinerary"). A Trip contains Flights, Lodgings, and Trip Events (activities). ASP.NET Core Web API on .NET 10, PostgreSQL via EF Core, FluentMigrator for schema migrations (not EF migrations).

## Commands

Build/run from the repo root (`itinerary_be.slnx`, the new XML solution format read by `dotnet` and VS2022+):

```
dotnet build itinerary_be.slnx
dotnet run --project itinerary_be.WebAPI
```

Run the DB migrator (creates the database if missing, then applies all pending FluentMigrator migrations found in `itinerary_be.Migration/Database/`):

```
dotnet run --project itinerary_be.Migration
```

Tests (xUnit + Moq, in `itinerary_be.UnitTests`):

```
dotnet test itinerary_be.UnitTests
dotnet test itinerary_be.UnitTests --filter "FullyQualifiedName~TripServiceTests"
dotnet test itinerary_be.UnitTests --filter "TripService_CreateTripAsync_ValidInput_ReturnsTrip"
```

WebAPI connects to Postgres via the `DefaultConnectionString` in `itinerary_be.WebAPI/appsettings.Development.json`; the migrator reads its own copy from `itinerary_be.Migration/appsettings.json`. Both must point at the same database.

In dev, OpenAPI/Scalar UI is mapped at app startup (`app.MapOpenApi()` / `app.MapScalarApiReference()`) — no separate Swagger UI package.

## Architecture

Clean Architecture across five projects, one-directional dependency flow:

- **itinerary_be.Core** — domain entities (`Domain/Entities`) and enums (`Domain/Enums`). No EF or ASP.NET references. Entities use `[Column("snake_case")]` attributes directly (no separate mapping layer for column names).
- **itinerary_be.Infrastructure** — `ItineraryDbContext` (EF Core + Npgsql) and `Data/Configurations/*` (`IEntityTypeConfiguration<T>` classes, auto-applied via `ApplyConfigurationsFromAssembly`). All tables live under the Postgres schema `itinerary` (`modelBuilder.HasDefaultSchema("itinerary")`); Postgres enums are registered explicitly (e.g. `HasPostgresEnum<EventTypes>("event_types")`).
- **itinerary_be.Modules.Itinerary** — application layer per feature area (Trip, TripEvent, etc.), each following: `Interfaces/I*Repository` + `Interfaces/I*Service` → `Repositories/*Repository` (EF Core data access) → `Services/*Service` (business rules, validation, logging). Registered for DI via one extension method per module in `ItineraryServiceRegistration.cs` (`AddTripServices()`), called once from `Program.cs`. **`itinerary_be.Modules.Logistics` is a sibling module scaffold for future features (currently unimplemented) — don't confuse the two.**
- **itinerary_be.WebAPI** — thin controllers only. Controllers depend solely on the `I*Service` interfaces (never repositories or the `DbContext` directly), translate DTOs (`DTOs/`) to/from domain entities, and map HTTP concerns (status codes, `ModelState`, `ProblemDetails`-style errors). FluentValidation validators live in `DTOs/Validators/`.
- **itinerary_be.Migration** — standalone console app using FluentMigrator. Each schema change is its own `Migration` class in `Database/` (e.g. `CreateTripTable.cs`, `AlterTripTableAddDescriptionAndDestination.cs`) ordered by migration version number. Running it also auto-creates the target database if it doesn't exist yet.

When adding a feature, follow the existing Trip/TripEvent pattern end to end: entity in Core → EF configuration in Infrastructure (+ a FluentMigrator migration in `itinerary_be.Migration/Database/`) → `I*Repository`/`I*Service` + implementations in Modules.Itinerary → registration in `ItineraryServiceRegistration.cs` → DTOs + validator + controller in WebAPI → unit tests for the service in `itinerary_be.UnitTests`.

Routes are nested by parent resource where applicable, e.g. `TripEventsController` is routed at `api/trips/{tripId}/events` rather than a flat `api/trip-events`.

## Spec-driven workflow (OpenSpec)

This repo tracks feature work under `openspec/`: `openspec/changes/<change-name>/` holds `proposal.md`, `design.md`, `tasks.md`, and `specs/` for an in-progress change; completed changes move to `openspec/changes/archive/<date>-<change-name>/`. When doing substantial feature work, check for a matching in-progress change folder and keep `tasks.md` checkboxes in sync with actual progress.

## Conventions from AGENTS.md / openspec/config.yaml

- WebAPI layer must stay thin: no business logic or direct DB calls in controllers — delegate to the service layer.
- Use FluentValidation for input validation (not just data-annotation attributes) — see `DTOs/Validators/`.
- All API routes must start with `/api`.
- Unit test naming: `NameOfMethod_Scenario_ExpectedResult`.
- Public methods on services must have unit tests.
- DTOs are kept fully separate from EF entities; controllers map between them explicitly (see `MapToResponseDto` helpers).
