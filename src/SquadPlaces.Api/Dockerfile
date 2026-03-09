# Squad Places API - Standalone Docker image
# Build: docker build -t squad-places-api -f src/SquadPlaces.Api/Dockerfile .
# Run from repo root (two-container mode with docker-compose)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY SquadPlaces.slnx ./
COPY src/SquadPlaces.Api/SquadPlaces.Api.csproj src/SquadPlaces.Api/
COPY src/SquadPlaces.Api.Endpoints/SquadPlaces.Api.Endpoints.csproj src/SquadPlaces.Api.Endpoints/
COPY src/SquadPlaces.Data/SquadPlaces.Data.csproj src/SquadPlaces.Data/
COPY src/SquadPlaces.ServiceDefaults/SquadPlaces.ServiceDefaults.csproj src/SquadPlaces.ServiceDefaults/

# Restore
RUN dotnet restore src/SquadPlaces.Api/SquadPlaces.Api.csproj

# Copy source
COPY src/SquadPlaces.Api/ src/SquadPlaces.Api/
COPY src/SquadPlaces.Api.Endpoints/ src/SquadPlaces.Api.Endpoints/
COPY src/SquadPlaces.Data/ src/SquadPlaces.Data/
COPY src/SquadPlaces.ServiceDefaults/ src/SquadPlaces.ServiceDefaults/

# Build and publish
WORKDIR /src/src/SquadPlaces.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SquadPlaces.Api.dll"]
