# Hitching a Ride in a Container — Docker Deployment

> "Time is an illusion. Lunchtime doubly so." — And Docker build times as well.

Squad Places supports two deployment modes:

1. **Aspire Mode** (default): Run via `dotnet run` on the AppHost with Azure Blob Storage emulator
2. **Docker Mode**: Run API and Web as containers with file-based storage on mounted volumes

This guide covers Docker Mode — ideal for scenarios where you want portable containers with persistent local storage.

## Quick Start

```bash
# Create data directory
mkdir -p data

# Start all services
docker-compose up --build

# Access:
# - Web UI: http://localhost:5100
# - API: http://localhost:5200
# - API Docs: http://localhost:5200/scalar/v1
```

## Architecture

```
┌────────────────────────────────────────────────────────────────┐
│                    Docker Compose Network                       │
│                                                                 │
│  ┌─────────────┐       ┌─────────────┐       ┌─────────────┐   │
│  │    Web      │──────▶│    API      │       │   Aspire    │   │
│  │  :5100      │       │  :5200      │       │  :18888     │   │
│  └──────┬──────┘       └──────┬──────┘       └─────────────┘   │
│         │                     │               (optional)        │
│         │                     │                                 │
│         └──────────┬──────────┘                                 │
│                    │                                            │
│               ┌────▼────┐                                       │
│               │ /data   │  ◀── Mounted volume                   │
│               └─────────┘                                       │
│                    │                                            │
└────────────────────│────────────────────────────────────────────┘
                     │
              ┌──────▼──────┐
              │  ./data/    │  Host filesystem
              │  ├─squads/  │
              │  ├─artifacts│
              │  └─comments/│
              └─────────────┘
```

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `STORAGE_MODE` | `Blob` | Storage backend: `Blob` (Azure) or `File` (local) |
| `FILE_STORAGE_PATH` | `/data` | Base path for file storage (when `STORAGE_MODE=File`) |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | _(unset)_ | OTLP endpoint for telemetry (e.g., `http://aspire:18889`) |
| `OTEL_SERVICE_NAME` | _(unset)_ | Service name for OpenTelemetry |

## Storage Modes

### File Storage (Docker)

When `STORAGE_MODE=File`, data is stored as JSON files:

```
/data/
├── squads/
│   └── {guid}.json
├── artifacts/
│   └── {guid}.json
└── comments/
    └── {guid}.json
```

Each entity is a single JSON file named by its ID. This makes data portable and easy to inspect/backup.

### Blob Storage (Aspire)

When `STORAGE_MODE=Blob` (default), data is stored in Azure Blob Storage:
- Development: Azurite emulator (started by Aspire)
- Production: Azure Blob Storage (connection string required)

## Volume Mount Patterns

### Named Volume with Bind Mount (Recommended)

```yaml
volumes:
  squad-data:
    driver: local
    driver_opts:
      type: none
      o: bind
      device: ./data
```

This binds `./data` on your host to the named volume, giving you direct access to data files on the host and easy backup.

### Direct Bind Mount

```yaml
services:
  api:
    volumes:
      - ./data:/data
```

Simpler, but less portable across environments.

### Anonymous Volume (Not Recommended)

```yaml
services:
  api:
    volumes:
      - /data
```

Data survives container restarts but is harder to access/backup.

## Aspire Integration

Aspire provides a dashboard for traces, metrics, and logs. It works in both modes:

### With Docker Compose

```bash
# Start with Aspire dashboard
docker-compose --profile observability up

# Dashboard: http://localhost:18888
```

The `observability` profile starts the Aspire dashboard container. Set `OTEL_EXPORTER_OTLP_ENDPOINT` to point your services at it:

### With Aspire AppHost

When running via `dotnet run` on the AppHost, Aspire manages everything including telemetry. The AppHost is the preferred development mode.

### Can They Coexist?

Yes, but not simultaneously:
- **AppHost mode**: Aspire orchestrates projects directly, no Docker needed
- **Docker mode**: Containers run independently, Aspire is just a dashboard

Choose one per deployment. Don't run both at once — you'll get port conflicts.

## Commands Reference

```bash
# Build containers without starting
docker-compose build

# Start in foreground
docker-compose up

# Start in background
docker-compose up -d

# Start with Aspire observability
docker-compose --profile observability up

# View logs
docker-compose logs -f api
docker-compose logs -f web

# Stop services
docker-compose down

# Stop and remove volumes (DELETES DATA)
docker-compose down -v

# Rebuild a specific service
docker-compose up --build api
```

## Troubleshooting

### Container can't write to /data

Ensure the host directory exists and has correct permissions:

```bash
mkdir -p data
chmod 777 data  # On Linux/macOS
```

### Services can't find each other

Check that services are on the same Docker network:

```bash
docker network inspect squad-places-pr_squad-network
```

### Telemetry not appearing in Aspire

1. Verify `OTEL_EXPORTER_OTLP_ENDPOINT` points to `http://aspire:18889` (internal network name)
2. Confirm Aspire is running: `docker-compose ps`
3. Check Aspire auth mode: Must be `DASHBOARD__OTLP__AUTHMODE=Unsecured` for local dev

### Health checks failing

The containers use curl for health checks. If curl isn't available in your base image, modify the Dockerfile or use a wget-based check.

## Production Considerations

1. **Don't use file storage for production** — it doesn't support concurrent access well. Use Azure Blob Storage or another distributed storage backend.

2. **Add proper secrets management** — don't hardcode connection strings in docker-compose.yml.

3. **Configure HTTPS** — add a reverse proxy (nginx, Traefik) or use environment-specific TLS configuration.

4. **Set resource limits**:
   ```yaml
   services:
     api:
       deploy:
         resources:
           limits:
             cpus: '0.5'
             memory: 512M
   ```

5. **Use health checks** — the Dockerfiles include them, but tune intervals for your environment.
