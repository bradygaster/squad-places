# Proposal: API Consolidation  Shared Endpoint Library

**Author:** Keaton (Lead)  
**Date:** 2025-07-17  
**Status:** PROPOSAL  awaiting review  
**Prerequisite:** PRs #2#5 merged (WikiLinks, artifact editing, image support, What's New API)

---

## 1. Problem Statement

The Squad Places codebase has two copies of the API surface:

| Project | Location | Lines | Endpoints | Features |
|---------|----------|-------|-----------|----------|
| **Upstream Api** | `src/SquadPlaces.Api/Program.cs` | 1,025 | 11 | Core CRUD, comments, rate limiting, IP blocking, duplicate detection |
| **Fork Web** | `src/SquadPlaces.Web/Api/` | ~1,360 | 15 | All upstream + images, WikiLinks, artifact editing, What's New, version header |

Both projects independently implement the same endpoint logic against `IBlobStorageService` from `SquadPlaces.Data`. The upstream API is a monolithic top-level-statements `Program.cs`. The fork extracted it into proper files (`ApiEndpoints.cs`, `ApiModels.cs`, `ApiValidation.cs`, `Services/`) and added features.

**The problem:** Every future feature must be implemented twice, drift is inevitable, and PRing features upstream requires translating structured code back into inline Program.cs.

**The goal:** A single shared class library that both host projects reference. Write endpoint logic once, deploy it two ways.

---

## 2. The Shared Library: `SquadPlaces.Api.Endpoints`

### 2.1 What It Is

A .NET class library containing all API endpoint definitions, request/response models, validation logic, and API-layer services. Both host projects reference this library and call two extension methods to wire everything up.

### 2.2 What Goes In

| Category | Files | Rationale |
|----------|-------|-----------|
| **Endpoint definitions** | `ApiEndpoints.cs` | `MapApiEndpoints(this IEndpointRouteBuilder)`  all route mappings and handlers |
| **Request/response models** | `ApiModels.cs` | All public records/DTOs |
| **Validation** | `ApiValidation.cs` | All `Validate*()` methods, `Sanitize()`, `DetectSpam()`, Levenshtein, image URL validation |
| **API services** | `Services/IpBlocklistService.cs` | Rate-limit strike tracking, auto-blocking |
| | `Services/DuplicateDetectionService.cs` | Artifact duplicate detection |
| | `Services/CommentDuplicateDetectionService.cs` | Comment duplicate detection |
| **DI registration** | `ApiServiceRegistration.cs` (NEW) | `AddSquadPlacesApiServices(this IServiceCollection)` |
| **Version constant** | In `ApiEndpoints.cs` | `CurrentVersion = "0.5.0"`  single source of truth |

### 2.3 What Stays Out (Host-Specific)

| Concern | Why it stays out | Where it lives |
|---------|------------------|----------------|
| **Storage registration** | Api uses Aspire blob client; Web supports File or Blob | Each host's `Program.cs` |
| **Rate limiting policies** | Hosts may diverge on limits | Each host's `Program.cs` |
| **CORS** | Api = public CORS; Web may restrict to same-origin | Each host's `Program.cs` |
| **IP blocking middleware** | Middleware pipeline ordering is host-specific | Each host's `Program.cs` |
| **Version header middleware** | Middleware pipeline ordering | Each host's `Program.cs` |
| **OpenAPI document transformer** | Document info/description may differ per host | Each host's `Program.cs` |
| **Razor Pages / SignalR** | Web-only concerns | `SquadPlaces.Web/Program.cs` |
| **Aspire orchestration** | Deployment topology | `SquadPlaces.AppHost/AppHost.cs` |

### 2.4 Extension Methods

```csharp
// SquadPlaces.Api.Endpoints/ApiServiceRegistration.cs
namespace SquadPlaces.Api.Endpoints;

public static class ApiServiceRegistration
{
    /// <summary>
    /// Registers API-layer services: IP blocklist, duplicate detection, comment duplicate detection.
    /// Does NOT register storage  that is a host responsibility.
    /// </summary>
    public static IServiceCollection AddSquadPlacesApiServices(this IServiceCollection services)
    {
        services.AddSingleton<IpBlocklistService>();
        services.AddSingleton<DuplicateDetectionService>();
        services.AddSingleton<CommentDuplicateDetectionService>();
        return services;
    }
}
```

```csharp
// SquadPlaces.Api.Endpoints/ApiEndpoints.cs (moves from Web/Api/)
namespace SquadPlaces.Api.Endpoints;

public static class ApiEndpoints
{
    public const string CurrentVersion = "0.5.0";

    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        // All 15 endpoints mapped here  identical to current Web/Api/ApiEndpoints.cs
    }
}
```

### 2.5 Project File

```xml
<!-- src/SquadPlaces.Api.Endpoints/SquadPlaces.Api.Endpoints.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
    <ProjectReference Include="..\SquadPlaces.Data\SquadPlaces.Data.csproj" />
  </ItemGroup>
</Project>
```

**Key:** This is a class library with `FrameworkReference Include="Microsoft.AspNetCore.App"`, NOT `Sdk="Microsoft.NET.Sdk.Web"`. It provides endpoint definitions without being a runnable host.

### 2.6 Namespace

`SquadPlaces.Api.Endpoints`  distinct from `SquadPlaces.Web.Api` (fork) and the upstream `SquadPlaces.Api` (host project). No collisions.

---

## 3. How Both Hosts Consume It

### 3.1 Upstream: `SquadPlaces.Api` (Two-Container Mode)

Brady's `Program.cs` shrinks from 1,025 lines to ~80:

```csharp
// src/SquadPlaces.Api/Program.cs  AFTER consolidation
using SquadPlaces.Api.Endpoints;
using SquadPlaces.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddAzureBlobServiceClient("BlobStorage");
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();

// API services from shared library
builder.Services.AddSquadPlacesApiServices();

// Rate limiting (host-configured policies)
builder.Services.AddRateLimiter(options => { /* ... */ });

// OpenAPI + CORS
builder.Services.AddOpenApi(options => { /* ... */ });
builder.Services.AddCors(options => { /* ... */ });

var app = builder.Build();

var blobService = app.Services.GetRequiredService<IBlobStorageService>();
if (blobService is BlobStorageService bs) await bs.InitializeAsync();

app.MapDefaultEndpoints();
app.UseCors();

// Version header middleware
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-SquadPlace-Version"] = ApiEndpoints.CurrentVersion;
            return Task.CompletedTask;
        });
    }
    await next();
});

// IP blocking middleware
app.Use(async (context, next) => { /* blocklist check */ await next(); });

app.UseRateLimiter();
app.MapOpenApi();
app.MapScalarApiReference();

// ONE LINE  all 15 API endpoints
app.MapApiEndpoints();

app.Run();
```

**Upstream csproj addition:**
```xml
<ProjectReference Include="..\SquadPlaces.Api.Endpoints\SquadPlaces.Api.Endpoints.csproj" />
```

### 3.2 Fork: `SquadPlaces.Web` (Single-Container Mode)

Jeff's `Program.cs` stays similar but references the shared library:

```csharp
// src/SquadPlaces.Web/Program.cs  AFTER consolidation
using SquadPlaces.Api.Endpoints;
using SquadPlaces.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Storage (File or Blob)
if (StorageServiceFactory.IsFileStorage(builder.Configuration))
    builder.Services.AddStorageService(builder.Configuration);
else
{
    builder.AddAzureBlobServiceClient("BlobStorage");
    builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
}

// Web-only: Razor Pages + SignalR
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// API services from shared library
builder.Services.AddSquadPlacesApiServices();

// Rate limiting, OpenAPI, CORS  same as current
// ...

var app = builder.Build();
// Initialize storage, middleware, Razor, SignalR...

// API endpoints alongside Razor Pages  single process, single port
app.MapApiEndpoints();

app.Run();
```

### 3.3 Aspire AppHost

**Upstream (two-container, default):**
```csharp
var api = builder.AddProject<Projects.SquadPlaces_Api>("api")
    .WithExternalHttpEndpoints()
    .WithReference(blobs).WaitFor(blobs);

builder.AddProject<Projects.SquadPlaces_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(api).WithReference(blobs)
    .WaitFor(api).WaitFor(blobs);
```

**Fork (single-container):**
```csharp
builder.AddProject<Projects.SquadPlaces_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(blobs).WaitFor(blobs);
```

Both work because the shared library is a build-time dependency, not a runtime service.

---

## 4. Opt-In Single-Container Build

### 4.1 Design Principle

**Default = upstream topology.** `docker compose up` or `dotnet run` on AppHost produces two containers (Web + Api) as Brady ships today.

**Opt-in = single-container.** Developers and operators can opt into a single combined container by building a different Dockerfile. No changes to orchestration or compose files needed.

### 4.2 Implementation: Separate Dockerfiles

Three Dockerfiles, two deployment modes:

```
src/SquadPlaces.Api/Dockerfile              Standalone API (upstream default, two-container mode)
src/SquadPlaces.Web/Dockerfile              Razor Pages + API proxy to upstream Api (upstream default, two-container mode)
src/SquadPlaces.Web/Dockerfile.single       Combined Web + API endpoints (opt-in, single-container mode)
```

**Default Two-Container Mode:**
```bash
docker compose up
# Starts src/SquadPlaces.Api/Dockerfile and src/SquadPlaces.Web/Dockerfile in two containers
```

**Opt-In Single-Container Mode (Jeff's Synology deployment):**
```bash
docker build -f src/SquadPlaces.Web/Dockerfile.single .
# Builds a single image containing Web UI + all 15 API endpoints (via shared library) + file storage
```

**`docker-compose.yml` (two-container, upstream-compatible):**
```yaml
services:
  api:
    build:
      context: .
      dockerfile: src/SquadPlaces.Api/Dockerfile
    ports:
      - "5200:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production

  web:
    build:
      context: .
      dockerfile: src/SquadPlaces.Web/Dockerfile
    ports:
      - "5100:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

The single compose file handles the default case. Jeff uses `docker build -f` directly for his single-container variant—no additional compose files, no conditional logic.

### 4.3 Why a Second Dockerfile (Not Build Args)

Build args add conditional logic to a single Dockerfile. They require matrix reasoning ("if X, then Y, else Z"). Separate Dockerfiles are clearer:

- **Each Dockerfile is self-contained.** Reading `Dockerfile.single` immediately tells you it includes everything needed for one image.
- **Upstream (two-container) and fork (single-container) stay independent.** No need for shared build conditionals or changing upstream's Dockerfile.
- **Operators choose their deployment at build time, not at image time.** Jeff runs `docker build -f Dockerfile.single` once to push to Synology. No runtime conditionals, no build-arg matrix to manage.
- **Easier to fork and diverge.** Jeff can maintain his own `Dockerfile.single` without coordinating changes to Brady's `Dockerfile`.

### 4.4 Dockerfile Specifications

**`src/SquadPlaces.Api/Dockerfile` (NEW, two-container mode):**
Standard multi-stage .NET Dockerfile building `SquadPlaces.Api.csproj`. Must COPY the `SquadPlaces.Api.Endpoints` project directory.
```dockerfile
# Example structure (adapt to your .NET version and layer caching strategy)
FROM mcr.microsoft.com/dotnet/sdk:10 AS builder
WORKDIR /build
COPY SquadPlaces.slnx .
COPY src/SquadPlaces.Api.Endpoints/SquadPlaces.Api.Endpoints.csproj src/SquadPlaces.Api.Endpoints/
COPY src/SquadPlaces.Api/SquadPlaces.Api.csproj src/SquadPlaces.Api/
COPY src/SquadPlaces.Data/SquadPlaces.Data.csproj src/SquadPlaces.Data/
COPY src/SquadPlaces.ServiceDefaults/SquadPlaces.ServiceDefaults.csproj src/SquadPlaces.ServiceDefaults/
RUN dotnet restore
COPY src/ src/
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "SquadPlaces.Api.dll"]
```

**`src/SquadPlaces.Web/Dockerfile` (UPDATE, two-container mode):**
Builds `SquadPlaces.Web.csproj`. Add COPY lines for the new `SquadPlaces.Api.Endpoints` project:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10 AS builder
WORKDIR /build
COPY SquadPlaces.slnx .
COPY src/SquadPlaces.Api.Endpoints/SquadPlaces.Api.Endpoints.csproj src/SquadPlaces.Api.Endpoints/
COPY src/SquadPlaces.Web/SquadPlaces.Web.csproj src/SquadPlaces.Web/
COPY src/SquadPlaces.Data/SquadPlaces.Data.csproj src/SquadPlaces.Data/
COPY src/SquadPlaces.ServiceDefaults/SquadPlaces.ServiceDefaults.csproj src/SquadPlaces.ServiceDefaults/
RUN dotnet restore
COPY src/ src/
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "SquadPlaces.Web.dll"]
```

**`src/SquadPlaces.Web/Dockerfile.single` (NEW, opt-in single-container mode):**
Identical to `Dockerfile` above. When built with this file, `SquadPlaces.Web.csproj` references the shared `SquadPlaces.Api.Endpoints` library, so all 15 API endpoints are compiled into the same binary. File storage is enabled via environment or configuration at runtime.
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10 AS builder
WORKDIR /build
COPY SquadPlaces.slnx .
COPY src/SquadPlaces.Api.Endpoints/SquadPlaces.Api.Endpoints.csproj src/SquadPlaces.Api.Endpoints/
COPY src/SquadPlaces.Web/SquadPlaces.Web.csproj src/SquadPlaces.Web/
COPY src/SquadPlaces.Data/SquadPlaces.Data.csproj src/SquadPlaces.Data/
COPY src/SquadPlaces.ServiceDefaults/SquadPlaces.ServiceDefaults.csproj src/SquadPlaces.ServiceDefaults/
RUN dotnet restore
COPY src/ src/
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "SquadPlaces.Web.dll"]
```

The code is identical because the difference is purely in **project references and configuration**, not in the Docker layers. Both Dockerfiles copy the `Api.Endpoints` project. The magic happens at runtime: the `SquadPlaces.Web.Program.cs` detects the environment or configuration and wires up file storage vs. Blob storage.

---

## 5. Migration Path

### Phase 0: Prerequisites
- [ ] PRs #2#5 merged upstream
- [ ] Upstream API parity confirmed  fork's 15 endpoints match upstream's feature set

### Phase 1: Create the Shared Library (In Fork)

| Step | Action | Risk |
|------|--------|------|
| 1 | `dotnet new classlib -n SquadPlaces.Api.Endpoints -o src/SquadPlaces.Api.Endpoints` | None |
| 2 | Move files from `Web/Api/` to `Api.Endpoints/` | Low  file moves only |
| 3 | Update namespaces: `SquadPlaces.Web.Api`  `SquadPlaces.Api.Endpoints` | Low  find-replace |
| 4 | Create `ApiServiceRegistration.cs` | None  new file |
| 5 | Update `Web.csproj`: add project reference, remove local Api files | Low |
| 6 | Update `Web/Program.cs`: change using, use `AddSquadPlacesApiServices()` | Low |
| 7 | Update `SquadPlaces.slnx`: add new project | None |
| 8 | Update `Web/Dockerfile`: add COPY for new project | Low |
| 9 | Verify: `dotnet build`, `docker compose -f docker-compose.single.yml up` | **Gate** |

### Phase 2: Wire Up Upstream Api Project (In Fork)

| Step | Action | Risk |
|------|--------|------|
| 1 | Update `Api.csproj`: add project reference to `Api.Endpoints` | None |
| 2 | Rewrite `Api/Program.cs`: 1,025  ~80 lines | Medium  must preserve all behavior |
| 3 | Create `Api/Dockerfile` | Low |
| 4 | Create `docker-compose.yml` (two-container) | Low |
| 5 | Rename current compose to `docker-compose.single.yml` | Low |
| 6 | Update AppHost for both modes | Low |
| 7 | Verify: both compose files work, Aspire AppHost works | **Gate** |

### Phase 3: PR Upstream

**Commit sequence:**
1. `feat: add SquadPlaces.Api.Endpoints shared library`
2. `refactor: SquadPlaces.Api uses shared endpoint library`
3. `chore: update solution file and Dockerfiles`

**What changes for Brady:**
- `Api/Program.cs`: 1,025  ~80 lines
- New project: `Api.Endpoints/`
- Updated: solution file, Dockerfiles, AppHost references
- **Zero behavioral changes**  same endpoints, same validation, same everything

**What doesn't change for Brady:**
- Two-container topology (Api + Web) remains default
- Aspire orchestration works the same
- `SquadPlaces.Data` untouched
- Azure deployment untouched

---

## 6. Final Project Structure

```
SquadPlaces.slnx
 src/
    SquadPlaces.Api/                        # Host: standalone API (upstream default)
       Program.cs                          # ~80 lines: DI, middleware, MapApiEndpoints()
       Dockerfile
       SquadPlaces.Api.csproj              # Refs: Api.Endpoints, Data, ServiceDefaults
   
    SquadPlaces.Api.Endpoints/              #  SHARED LIBRARY
       ApiEndpoints.cs                     # MapApiEndpoints()  all 15 routes
       ApiModels.cs                        # Request/response records
       ApiValidation.cs                    # Validation + spam detection
       ApiServiceRegistration.cs           # AddSquadPlacesApiServices()
       Services/
          IpBlocklistService.cs
          DuplicateDetectionService.cs
          CommentDuplicateDetectionService.cs
       SquadPlaces.Api.Endpoints.csproj
   
    SquadPlaces.Web/                        # Host: Razor Pages + API (single-container)
       Program.cs                          # Razor + SignalR + MapApiEndpoints()
       Dockerfile
       Hubs/FeedHub.cs
       Pages/...
       SquadPlaces.Web.csproj              # Refs: Api.Endpoints, Data, ServiceDefaults
   
    SquadPlaces.Data/                       # Storage abstraction (unchanged)
       IBlobStorageService.cs
       BlobStorageService.cs
       FileStorageService.cs
       StorageServiceFactory.cs
       Models/
   
    SquadPlaces.AppHost/                    # Aspire orchestration
       AppHost.cs
   
    SquadPlaces.ServiceDefaults/            # Shared Aspire defaults

 tests/
    SquadPlaces.AppHost.Tests/
    SquadPlaces.Playwright/

 docker-compose.yml                          # Two-container (upstream default)
 azure.yaml
```

---

## 7. Risk Assessment

### HIGH Risk

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Endpoint behavior divergence during extraction** | API breaks, Brady's deployment fails | Diff-test: snapshot all 15 endpoint responses before/after. Automated integration test that hits every endpoint. |
| **Namespace collision with upstream Api project** | Build errors, confusion | Library namespace is `SquadPlaces.Api.Endpoints`, not `SquadPlaces.Api`. Verified no collision. |

### MEDIUM Risk

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Middleware ordering differs between hosts** | Subtle behavioral differences (e.g., rate limiting before vs after auth) | Document required middleware order in `ApiEndpoints.cs` XML doc comments. Consider providing a `UseSquadPlacesApiMiddleware()` helper in a future iteration if drift occurs. |
| **Upstream rejects PR due to perceived complexity** | Wasted effort | Frame PR as "your Program.cs goes from 1025 to 80 lines." Lead with the simplification story. |
| **PRs #2-#5 create merge conflicts** | Delays | Wait for all four to merge before starting. Don't try to parallelize with in-flight PRs. |

### LOW Risk

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Docker layer caching invalidated** | Slower builds | Separate COPY for csproj files (already standard pattern). |
| **Aspire version incompatibility** | Build errors | Both projects already use `Aspire.Azure.Storage.Blobs` with `Version="*"`. Shared library doesn't reference Aspire packages directly. |
| **File storage path conflicts in single-container** | Data loss | Volume mounts are explicit in compose file. `FileStorageService` already handles directory creation. |

### Watch List

- **Rate limiting middleware as library concern:** If both hosts always configure identical rate limiting, consider moving the policy definitions into the shared library in a future iteration. Not now  keep it host-specific until we see actual divergence.
- **SignalR push from API writes:** In single-container mode, `MapApiEndpoints()` handlers could resolve `IHubContext<FeedHub>` and push real-time updates. This is a feature opportunity, not a risk. The shared library shouldn't depend on SignalR  inject via optional callback if we pursue this.
- **Test coverage:** The shared library should have its own unit test project (`tests/SquadPlaces.Api.Endpoints.Tests/`) testing validation, spam detection, and endpoint routing. Create this during Phase 1.

---

## 8. Open Questions

1. **Should middleware helpers live in the shared library?** The version header and IP blocking middleware are identical in both hosts. A `UseSquadPlacesApiMiddleware(this WebApplication)` extension could reduce duplication. **My lean:** Not yet. Keep middleware host-specific until we prove it never diverges. Premature abstraction here costs more than duplication.

2. **Should rate limiting policies live in the shared library?** Same argument. Both hosts use identical policies today. **My lean:** No. Rate limiting is a host policy decision. The library provides the IP blocklist service; the host decides when to block.

3. **Test strategy for the shared library?** **My lean:** Create `tests/SquadPlaces.Api.Endpoints.Tests/` with WebApplicationFactory-based integration tests. Each test creates a minimal host, registers the library, and hits endpoints. This gives us confidence the library works regardless of which host consumes it.
