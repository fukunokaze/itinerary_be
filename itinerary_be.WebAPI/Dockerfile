FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY itinerary_be.Core/itinerary_be.Core.csproj itinerary_be.Core/
COPY itinerary_be.Infrastructure/itinerary_be.Infrastructure.csproj itinerary_be.Infrastructure/
COPY itinerary_be.Modules.Auth/itinerary_be.Modules.Auth.csproj itinerary_be.Modules.Auth/
COPY itinerary_be.Modules.Itinerary/itinerary_be.Modules.Itinerary.csproj itinerary_be.Modules.Itinerary/
COPY itinerary_be.Modules.Logistics/itinerary_be.Modules.Logistics.csproj itinerary_be.Modules.Logistics/
COPY itinerary_be.Modules.Utility/itinerary_be.Modules.Utility.csproj itinerary_be.Modules.Utility/
COPY itinerary_be.WebAPI/itinerary_be.WebAPI.csproj itinerary_be.WebAPI/

RUN dotnet restore itinerary_be.WebAPI/itinerary_be.WebAPI.csproj

COPY itinerary_be.Core/ itinerary_be.Core/
COPY itinerary_be.Infrastructure/ itinerary_be.Infrastructure/
COPY itinerary_be.Modules.Auth/ itinerary_be.Modules.Auth/
COPY itinerary_be.Modules.Itinerary/ itinerary_be.Modules.Itinerary/
COPY itinerary_be.Modules.Logistics/ itinerary_be.Modules.Logistics/
COPY itinerary_be.Modules.Utility/ itinerary_be.Modules.Utility/
COPY itinerary_be.WebAPI/ itinerary_be.WebAPI/

RUN dotnet publish itinerary_be.WebAPI/itinerary_be.WebAPI.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Connection string, JWT, and Google OAuth settings are supplied at runtime via
# environment variables (e.g. ConnectionStrings__DefaultConnectionString, Jwt__Secret) —
# see docker-compose.yml. No appsettings.Development.json is baked into this image.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "itinerary_be.WebAPI.dll"]
