# Travel Planner App Backend App

## Bussiness Requirements
- Will serve the backend of Travel Planner app
- Should be able to manage record of Trip where it contains Flight, Lodging, and Activity/Trip Event


## Technical Spec & Best Practices
- **Framework**: Use the latest .NET (e.g., .NET 8 or newer).
- **Architecture**: Follow SOLID principles. The WebAPI layer should be thin, containing no business logic or direct database calls. Separate concerns using n-tier or Clean Architecture patterns. Consider CQRS if appropriate.
- **Dependency Injection**: Use built-in DI for loose coupling and testability.
- **Asynchronous Programming**: Use `async`/`await` all the way down for I/O bound operations.
- **Validation**: Use `FluentValidation` for comprehensive input validation, separated from domain models.
- **Exception Handling**: Implement global exception handling (e.g., using `IExceptionHandler` in .NET 8) to return standardized `ProblemDetails` responses.
- **Logging**: Use structured logging (e.g., `Serilog`) with correlation IDs to track cross-component requests.
- **Configuration**: Use the Options pattern (`IOptions<T>`) for strongly-typed configuration settings.
- **Security**: Secure endpoints with JWT or OAuth2. Avoid storing sensitive data in code; use User Secrets locally and a key vault in production.
- **API Design**: Implement API versioning from the start. Document all endpoints with Swagger/OpenAPI.
- **Observability**: Add Health Checks (`MapHealthChecks`) for the API and its dependencies (e.g., databases).
- **Libraries**: Use the latest and secure third-party libraries when necessary.
- **Testing**: 
  - Public methods in logic services must have unit tests.
  - Unit test naming should follow the `NameOfMethod_Scenario_ExpectedResult` format.
  - Use `WebApplicationFactory` for integration testing.