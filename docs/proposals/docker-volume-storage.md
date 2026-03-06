# Proposal: Docker Volume Storage Configuration

**Author:** Keaton (Lead)  
**Date:** 2025-03-06  
**Status:** Draft  
**Requested by:** Jeff Fritz

## Problem Statement

The current SquadPlaces configuration relies on Azure Blob Storage (via Azurite emulator in development). We need an alternate deployment configuration that:

1. Runs the Web and API services in Docker containers
2. Uses mounted Docker volumes for document storage instead of Azure Blob
3. Can coexist with the current Azure-based configuration

## Recommended Approach

### Configuration Pattern: Docker Compose + Storage Abstraction

**Why not a separate AppHost profile?**  
Aspire's AppHost is designed for orchestrating distributed apps with service discovery. For a standalone Docker deployment with volume mounts, `docker-compose.yml` is cleaner and more portable—no Aspire runtime needed.

**Why not environment variables alone?**  
We need a clean interface boundary. Environment flags lead to scattered conditionals. A proper abstraction keeps storage logic cohesive.

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      IBlobStorageService                     │
│  (unchanged interface - all existing code continues to work) │
└─────────────────────────────────────────────────────────────┘
              ▲                           ▲
              │                           │
┌─────────────────────────┐   ┌─────────────────────────┐
│   BlobStorageService    │   │  FileStorageService     │
│   (Azure Blob Storage)  │   │  (Local File System)    │
│   - Current impl        │   │  - NEW                  │
└─────────────────────────┘   └─────────────────────────┘
```

### Files to Create/Modify

| File | Action | Purpose |
|------|--------|---------|
| `src/SquadPlaces.Data/FileStorageService.cs` | Create | New `IBlobStorageService` implementation using local file system |
| `src/SquadPlaces.Api/Program.cs` | Modify | Add conditional DI registration based on `STORAGE_TYPE` env var |
| `src/SquadPlaces.Web/Program.cs` | Modify | Same conditional DI registration |
| `docker-compose.yml` | Create | Docker Compose orchestration with volume mounts |
| `docker-compose.override.yml` | Create | Local development overrides |
| `src/SquadPlaces.Api/Dockerfile` | Create or verify | API container definition |
| `src/SquadPlaces.Web/Dockerfile` | Create or verify | Web container definition |

### Storage Abstraction Strategy

The existing `IBlobStorageService` interface is already well-designed for abstraction:

```csharp
public interface IBlobStorageService
{
    Task SaveSquadAsync(Squad squad);
    Task<Squad?> GetSquadAsync(Guid id);
    Task<List<Squad>> ListSquadsAsync();
    // ... etc
}
```

**FileStorageService Implementation Notes:**

```csharp
// Mirrors BlobStorageService but writes to local filesystem
public class FileStorageService : IBlobStorageService
{
    private readonly string _basePath;  // From config, e.g., /data/storage
    
    // Directory structure mirrors blob containers:
    // /data/storage/squads/{id}.json
    // /data/storage/artifacts/{id}.json
    // /data/storage/comments/{id}.json
}
```

### DI Registration Pattern

In `Program.cs` for both API and Web:

```csharp
var storageType = builder.Configuration["STORAGE_TYPE"] ?? "azure";

if (storageType.Equals("file", StringComparison.OrdinalIgnoreCase))
{
    var storagePath = builder.Configuration["STORAGE_PATH"] ?? "/data/storage";
    builder.Services.AddSingleton<IBlobStorageService>(
        new FileStorageService(storagePath));
}
else
{
    builder.Services.AddAzureBlobClient(builder.Configuration);
    builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
}
```

### Docker Compose Configuration

```yaml
# docker-compose.yml
services:
  api:
    build:
      context: .
      dockerfile: src/SquadPlaces.Api/Dockerfile
    environment:
      - STORAGE_TYPE=file
      - STORAGE_PATH=/data/storage
      - ASPNETCORE_URLS=http://+:8080
    volumes:
      - squad-data:/data/storage
    ports:
      - "5001:8080"

  web:
    build:
      context: .
      dockerfile: src/SquadPlaces.Web/Dockerfile
    environment:
      - STORAGE_TYPE=file
      - STORAGE_PATH=/data/storage
      - ASPNETCORE_URLS=http://+:8080
      - services__api__http__0=http://api:8080
    volumes:
      - squad-data:/data/storage
    ports:
      - "5000:8080"
    depends_on:
      - api

volumes:
  squad-data:
```

### Dependency Considerations

1. **No new NuGet dependencies** - `System.IO` and `System.Text.Json` are already available
2. **Aspire references remain optional** - Docker mode won't use Aspire service discovery; explicit URLs via env vars
3. **Shared volume** - Both API and Web mount the same volume for data consistency
4. **InitializeAsync()** - `FileStorageService` creates directories on init (mirrors blob container creation)

### Trade-offs

| Consideration | Decision | Rationale |
|---------------|----------|-----------|
| Naming: `FileStorageService` vs `LocalStorageService` | `FileStorageService` | Explicit about what it does |
| Config: appsettings vs env vars | Env vars | More portable for containers, 12-factor |
| Volume: per-container vs shared | Shared | Consistency; API writes, Web reads |
| Interface: new `IStorageService` vs reuse `IBlobStorageService` | Reuse | Zero changes to existing consumers |

### Implementation Order

1. **FileStorageService** - Core abstraction implementation
2. **DI registration** - Wire up conditional logic in both projects
3. **Dockerfiles** - Ensure both projects can build as containers
4. **docker-compose.yml** - Orchestration with volumes
5. **Documentation** - README section on Docker deployment

### Open Questions

- [ ] Should we support hot-switching between storage backends, or is it startup-only config?
- [ ] Do we want a hybrid mode (read from file, fallback to blob)?
- [ ] Container registry for built images?

## Decision

Proposal-first, as always. Implementation follows approval.

---

*Architecture decisions compound. This abstraction makes future storage backends trivial to add.*
