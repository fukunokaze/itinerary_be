## Why

The `TripsController` currently violates the Clean Architecture principle by mixing presentation logic with business logic and direct database operations. The controller has tight coupling to the `ItineraryDbContext`, making it difficult to test, maintain, and extend. Moving business logic and data access to the `itinerary_be.Modules.Itinerary` application layer will enable better testability, reusability, and adherence to SOLID principles.

## What Changes

- Remove direct `ItineraryDbContext` dependency from `TripsController`
- Extract Trip CRUD operations and business logic into a dedicated service layer in `itinerary_be.Modules.Itinerary`
- Move entity-to-DTO mapping logic to a dedicated mapper or service method
- Introduce repository or service pattern to abstract data access
- Update `TripsController` to use the new service layer for all operations
- Ensure the controller focuses solely on HTTP request/response handling and delegates business logic to the application layer

## Capabilities

### New Capabilities
- `trip-management-service`: Service layer for Trip CRUD operations, including creation, retrieval, update, and deletion of trips with proper validation and error handling

### Modified Capabilities
<!-- Existing capabilities whose REQUIREMENTS are changing (not just implementation).
     Only list here if spec-level behavior changes. Each needs a delta spec file.
     Use existing spec names from openspec/specs/. Leave empty if no requirement changes. -->

## Impact

- **Affected Code**: `itinerary_be.WebAPI/Controllers/TripsController.cs`, `itinerary_be.Modules.Itinerary` (application layer)
- **Architecture**: Introduces service layer pattern with dependency injection
- **Dependencies**: `itinerary_be.Modules.Itinerary` project structure and dependencies
- **Testing**: Enables unit testing of business logic independent of controller
- **APIs**: HTTP endpoints remain unchanged; only internal implementation changes
