## Why

The Trip domain is a core entity in the itinerary application, representing a travel plan with associated details. Currently, there are no API endpoints to manage Trip resources. Implementing CRUD operations will enable clients to create, retrieve, update, and delete trips, establishing the foundation for a functional itinerary management system.

## What Changes

- Add HTTP endpoints for CRUD operations on Trip resources
- Create endpoints for creating new trips
- Create endpoints for retrieving trip details (single and list)
- Create endpoints for updating existing trips
- Create endpoints for deleting trips
- No authorization or authentication required for initial implementation

## Capabilities

### New Capabilities
- `trip-crud-api`: CRUD endpoints for managing Trip resources (create, read, update, delete operations)

### Modified Capabilities
<!-- No existing capabilities are being modified -->

## Impact

- **Code**: Adds new controller in itinerary_be.WebAPI project
- **APIs**: Exposes new HTTP endpoints for Trip management
- **Database**: Uses existing ItineraryDbContext and Trip entity from itinerary_be.Core and itinerary_be.Infrastructure
- **Dependencies**: Leverages existing Entity Framework Core setup and domain models
