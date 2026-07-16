FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY itinerary_be.Migration/itinerary_be.Migration.csproj itinerary_be.Migration/
RUN dotnet restore itinerary_be.Migration/itinerary_be.Migration.csproj

COPY itinerary_be.Migration/ itinerary_be.Migration/
RUN dotnet publish itinerary_be.Migration/itinerary_be.Migration.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set ConnectionStrings__DefaultConnectionString at runtime (see docker-compose.yml) to
# override the value baked into appsettings.json — Program.cs applies AddEnvironmentVariables()
# after AddJsonFile(), so the env var wins when present.
ENTRYPOINT ["dotnet", "itinerary_be.Migration.dll"]
