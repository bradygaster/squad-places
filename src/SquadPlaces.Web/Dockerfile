# Squad Places Web - Docker image
# Build: docker build --build-arg GITHUB_TOKEN=$GITHUB_TOKEN -t squad-places-web -f src/SquadPlaces.Web/Dockerfile .
# Run from repo root

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy NuGet config and solution/project files for restore layer caching
COPY NuGet.config ./
COPY SquadPlaces.slnx ./
COPY src/SquadPlaces.Web/SquadPlaces.Web.csproj src/SquadPlaces.Web/
COPY src/SquadPlaces.Api.Endpoints/SquadPlaces.Api.Endpoints.csproj src/SquadPlaces.Api.Endpoints/
COPY src/SquadPlaces.Data/SquadPlaces.Data.csproj src/SquadPlaces.Data/
COPY src/SquadPlaces.ServiceDefaults/SquadPlaces.ServiceDefaults.csproj src/SquadPlaces.ServiceDefaults/

# Restore (GITHUB_TOKEN needed for private Squad.SDK.NET NuGet feed)
ARG GITHUB_TOKEN
RUN GITHUB_TOKEN="$GITHUB_TOKEN" dotnet restore src/SquadPlaces.Web/SquadPlaces.Web.csproj

# Copy source
COPY src/SquadPlaces.Web/ src/SquadPlaces.Web/
COPY src/SquadPlaces.Api.Endpoints/ src/SquadPlaces.Api.Endpoints/
COPY src/SquadPlaces.Data/ src/SquadPlaces.Data/
COPY src/SquadPlaces.ServiceDefaults/ src/SquadPlaces.ServiceDefaults/

# Build and publish
WORKDIR /src/src/SquadPlaces.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish ./

# Create data directory for file-based storage
RUN mkdir -p /data/squads /data/artifacts /data/comments /data/images /data/dp-keys
VOLUME /data

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SquadPlaces.Web.dll"]
