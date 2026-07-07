# itinerary_be
Repository for My Itinerary backend

## Architecture Overview

This project follows Clean Architecture principles with clear separation of concerns:

- **itinerary_be.Core**: Domain entities and core business logic
- **itinerary_be.Infrastructure**: Database context and data configurations using Entity Framework Core with PostgreSQL
- **itinerary_be.Modules.Itinerary**: Application layer services and repositories for itinerary features
- **itinerary_be.WebAPI**: ASP.NET Core Web API controllers and DTOs

## Service Layer Architecture

The application uses a Service-Repository pattern to ensure clean separation between the Web API layer and data access layer.

### Trip Management Service

The `itinerary_be.Modules.Itinerary` module provides Trip management capabilities:

#### Components

- **ITripService**: Service interface defining Trip business operations
  - `CreateTripAsync(title, startDate, endDate)`: Create a new Trip
  - `GetTripByIdAsync(id)`: Retrieve a Trip by ID
  - `GetAllTripsAsync()`: Retrieve all Trips
  - `UpdateTripAsync(id, title, startDate, endDate)`: Update an existing Trip
  - `DeleteTripAsync(id)`: Delete a Trip

- **TripService**: Service implementation handling Trip business logic
  - Validates Trip existence before update/delete operations
  - Logs all operations
  - Abstracts data access through the repository interface

- **ITripRepository**: Repository interface defining data access operations
  - `CreateAsync(trip)`: Persist a Trip
  - `GetByIdAsync(id)`: Retrieve a Trip by ID
  - `GetAllAsync()`: Retrieve all Trips
  - `UpdateAsync(trip)`: Persist Trip changes
  - `DeleteAsync(trip)`: Remove a Trip

- **TripRepository**: Repository implementation
  - Encapsulates Entity Framework Core operations
  - Handles database interactions for Trip entities

#### Dependency Injection

Services are registered in `Program.cs` using the `AddTripServices()` extension method:

```csharp
builder.Services.AddTripServices();
```

This automatically registers:
- `ITripRepository` → `TripRepository` (Scoped)
- `ITripService` → `TripService` (Scoped)

#### Controller Integration

The `TripsController` in `itinerary_be.WebAPI` depends only on `ITripService`:

```csharp
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;
    
    public TripsController(ITripService tripService, ILogger<TripsController> logger)
    {
        _tripService = tripService;
        _logger = logger;
    }
}
```

The controller:
- Converts inbound DTOs to domain object parameters
- Delegates all business logic to the service layer
- Maps domain entities to response DTOs
- Handles HTTP request/response concerns only

### Benefits

- **Testability**: Service and repository layers can be unit tested independently using mock implementations
- **Reusability**: Services can be consumed by multiple clients (Web API, background jobs, etc.)
- **Maintainability**: Clear separation of concerns makes the codebase easier to understand and modify
- **Scalability**: New features can be added to services without affecting the Web API layer
