## 1. Setup and Module Preparation

- [x] 1.1 Add necessary NuGet packages to `itinerary_be.Modules.Itinerary.csproj` (Microsoft.EntityFrameworkCore, etc.)
- [x] 1.2 Create folder structure in `itinerary_be.Modules.Itinerary` (Services, Repositories, Interfaces)
- [x] 1.3 Add project reference from `itinerary_be.WebAPI` to `itinerary_be.Modules.Itinerary` if not already present

## 2. Implement Repository Layer

- [x] 2.1 Create `ITripRepository` interface in `itinerary_be.Modules.Itinerary/Repositories/` directory
- [x] 2.2 Define repository interface methods: CreateAsync, GetByIdAsync, GetAllAsync, UpdateAsync, DeleteAsync
- [x] 2.3 Implement `TripRepository` class that uses `ItineraryDbContext` for data access
- [x] 2.4 Add constructor to `TripRepository` accepting `ItineraryDbContext` via dependency injection
- [x] 2.5 Implement each repository method (Create, GetById, GetAll, Update, Delete)

## 3. Implement Service Layer

- [x] 3.1 Create `ITripService` interface in `itinerary_be.Modules.Itinerary/Services/` directory
- [x] 3.2 Define service interface methods: CreateTripAsync, GetTripByIdAsync, GetAllTripsAsync, UpdateTripAsync, DeleteTripAsync
- [x] 3.3 Implement `TripService` class that uses `ITripRepository` for data access
- [x] 3.4 Add constructor to `TripService` accepting `ITripRepository` via dependency injection
- [x] 3.5 Add mapping helper method to convert Trip entities to `TripResponseDto`
- [x] 3.6 Implement validation logic in service methods (e.g., check if Trip exists before update/delete)
- [x] 3.7 Implement all service methods with proper error handling and async/await patterns

## 4. Register Dependencies

- [x] 4.1 Create extension method in `itinerary_be.Modules.Itinerary` (e.g., `AddTripServices`) to register service dependencies
- [x] 4.2 Register `ITripRepository` as `TripRepository` with appropriate lifetime (Scoped recommended)
- [x] 4.3 Register `ITripService` as `TripService` with appropriate lifetime (Scoped recommended)
- [x] 4.4 Call the extension method in `itinerary_be.WebAPI/Program.cs` during service registration

## 5. Refactor TripsController

- [x] 5.1 Add `ITripService` parameter to `TripsController` constructor
- [x] 5.2 Remove `ItineraryDbContext` injection from constructor
- [x] 5.3 Remove direct database calls; replace with service method calls in `CreateTrip` method
- [x] 5.4 Update `GetTripById` method to use service instead of direct context
- [x] 5.5 Update `GetAllTrips` method to use service instead of direct context
- [x] 5.6 Update `UpdateTrip` method to use service instead of direct context
- [x] 5.7 Update `DeleteTrip` method to use service instead of direct context
- [x] 5.8 Remove `MapToResponseDto` method from controller (move logic to service or keep in service only)

## 6. Testing

- [x] 6.1 Write unit tests for `TripRepository` (mock `ItineraryDbContext`)
- [x] 6.2 Write unit tests for `TripService` (mock `ITripRepository`)
- [x] 6.3 Test Create method with valid and invalid data
- [x] 6.4 Test Read methods (GetById, GetAll) with existing and non-existing trips
- [x] 6.5 Test Update method with valid and invalid data, and non-existing trip
- [x] 6.6 Test Delete method with existing and non-existing trip
- [x] 6.7 Write integration tests to verify controller endpoints still work correctly with new service layer

## 7. Verification and Code Quality

- [x] 7.1 Verify all endpoints respond with correct HTTP status codes
- [x] 7.2 Verify TripsController has no direct database context references
- [x] 7.3 Verify TripsController depends only on `ITripService`
- [x] 7.4 Run code analyzer/linter to check for issues (StyleCop, Code Analysis)
- [x] 7.5 Ensure XML documentation comments are present on all public methods
- [x] 7.6 Perform manual testing of all CRUD operations via API or test client

## 8. Documentation and Cleanup

- [x] 8.1 Document the new service layer in the project README or architecture guide
- [x] 8.2 Remove unused imports from `TripsController` (e.g., `Microsoft.EntityFrameworkCore`)
- [x] 8.3 Clean up any commented-out code or temporary debug code
- [x] 8.4 Verify no compilation warnings remain in the solution
