## Context

The `TripsController` currently violates Clean Architecture principles by:
1. Direct injection of `ItineraryDbContext`
2. Direct database operations in controller methods
3. Tightly coupled to Entity Framework Core
4. Mixed concerns: HTTP handling, business logic, and data access
5. Difficult to unit test business logic in isolation

The `itinerary_be.Modules.Itinerary` project exists but is empty, designed to house the Trip management domain logic.

## Goals / Non-Goals

**Goals:**
- Extract Trip CRUD logic into a dedicated service layer within `itinerary_be.Modules.Itinerary`
- Make `TripsController` depend on abstractions (interfaces) instead of concrete implementations
- Enable unit testing of business logic independent of HTTP layer
- Follow Repository and Service patterns for data access abstraction
- Maintain existing API contracts (no breaking changes to endpoints)
- Ensure the controller focuses solely on HTTP request/response handling

**Non-Goals:**
- Adding new endpoints or changing API behavior
- Database schema changes
- Adding authentication/authorization features
- Implementing complex business rules beyond CRUD validation
- Changing entity or DTO structures

## Decisions

### Decision 1: Implement Service Layer Pattern in Modules.Itinerary

**Choice**: Create a `TripService` class in the `itinerary_be.Modules.Itinerary` project that handles all Trip business logic.

**Rationale**: This provides a clear separation of concerns and allows the service to be reused by multiple consumers (Web API, other modules, background jobs). The service will be registered as an interface-based dependency.

**Alternatives Considered**:
- Direct repository injection into controller: Rejected because it still couples the controller to data access layer and doesn't provide a domain service abstraction.
- MediatR CQRS pattern: Rejected for now as it adds complexity; service pattern is sufficient for current CRUD-focused requirements.

### Decision 2: Abstract Data Access with Repository Pattern

**Choice**: Create a `TripRepository` interface and implementation that abstracts Entity Framework operations.

**Rationale**: This allows the service to request data without knowing how it's retrieved. In the future, this can be replaced with a different data source without modifying the service.

**Alternatives Considered**:
- Direct DbContext in service: Rejected because it maintains tight coupling and makes testing harder.
- Using DbSet extensions: Rejected because it's not as clean and doesn't provide centralized query logic.

### Decision 3: Dependency Injection via IServiceCollection

**Choice**: Register `ITripService` and `ITripRepository` in `Program.cs` (or extension method) with the DI container.

**Rationale**: Follows ASP.NET Core conventions and enables constructor-based dependency injection with full control over lifetimes.

**Alternatives Considered**:
- Factory pattern: Rejected as unnecessary when DI container is available.
- Service locator: Rejected as it's considered an anti-pattern.

### Decision 4: Keep Entity Mapping in Service Layer

**Choice**: Add a mapping helper method in the service or use AutoMapper for entity-to-DTO conversions.

**Rationale**: Keeps the mapping logic close to the domain objects and allows for consistent transformations. If using AutoMapper, it provides reusability across multiple consumers.

**Alternatives Considered**:
- Move mapping to controller: Rejected as it mixes concerns.
- Create separate mapper classes: Could be done, but for this simple case, service-level mapping is sufficient.

## Risks / Trade-offs

**Risk: Breaking Changes During Deployment**
├─ **Mitigation**: Maintain backward compatibility by keeping all endpoints unchanged. Only internal implementation changes occur. Deploy as a single atomic change.

**Risk: Repository Layer Adds Abstraction Overhead**
├─ **Mitigation**: The added abstraction is minimal (simple interface) and provides significant long-term testability and maintainability benefits. Trade-off is acceptable.

**Risk: Circular Dependencies Between Modules**
├─ **Mitigation**: `itinerary_be.Modules.Itinerary` must only depend on `itinerary_be.Core` (entities) and `itinerary_be.Infrastructure` (data context). The Web API depends on both Core and Modules. No reverse dependencies.

**Risk: Incomplete Migration During Rollout**
├─ **Mitigation**: Ensure all endpoints are updated simultaneously to use the new service. Do not leave any endpoints using direct context access.

## Migration Plan

1. **Phase 1 - Setup**: Create service and repository interfaces and implementations in `itinerary_be.Modules.Itinerary`
2. **Phase 2 - Dependency Injection**: Register services in `Program.cs`
3. **Phase 3 - Controller Refactoring**: Update `TripsController` to use the new service layer
4. **Phase 4 - Testing**: Write and run unit tests for the service layer
5. **Phase 5 - Validation**: Run integration tests to ensure endpoints still work correctly
6. **Phase 6 - Deployment**: Deploy as a single atomic change to avoid inconsistency

**Rollback Strategy**: If issues arise, revert the controller to use `ItineraryDbContext` directly (this is a code change only, no database migration needed).

## Open Questions

1. Should the `TripRepository` be created as a specific implementation or should the service use the `DbContext` directly through a generic repository interface?
2. Should validation of Trip properties (e.g., StartDate < EndDate) be in the service or in the entity?
3. Do we need to add auditing (created by, created date, modified by, modified date) to the Trip entity as part of this refactor?
