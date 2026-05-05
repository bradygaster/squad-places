# Squad Places Web - Docker image
# Build: docker build -t squad-places-web -f src/SquadPlaces.Web/Dockerfile .
# Run from repo root

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

RUN apt-get update \
    && apt-get install -y curl ca-certificates gnupg \
    && curl -fsSL https://deb.nodesource.com/setup_24.x | bash - \
    && apt-get install -y nodejs \
    && rm -rf /var/lib/apt/lists/*

# Copy solution and project files
COPY SquadPlaces.slnx ./
COPY src/SquadPlaces.Web/SquadPlaces.Web.csproj src/SquadPlaces.Web/
COPY src/SquadPlaces.Api.Endpoints/SquadPlaces.Api.Endpoints.csproj src/SquadPlaces.Api.Endpoints/
COPY src/SquadPlaces.Data/SquadPlaces.Data.csproj src/SquadPlaces.Data/
COPY src/SquadPlaces.ServiceDefaults/SquadPlaces.ServiceDefaults.csproj src/SquadPlaces.ServiceDefaults/
COPY src/SquadPlaces.Web/Node/package.json src/SquadPlaces.Web/Node/
COPY src/SquadPlaces.Web/Node/package-lock.json src/SquadPlaces.Web/Node/

# Restore
RUN dotnet restore src/SquadPlaces.Web/SquadPlaces.Web.csproj

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

RUN apt-get update \
    && apt-get install -y curl ca-certificates gnupg \
    && curl -fsSL https://deb.nodesource.com/setup_24.x | bash - \
    && apt-get install -y nodejs \
    && rm -rf /var/lib/apt/lists/*

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