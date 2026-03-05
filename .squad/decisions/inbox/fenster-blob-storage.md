# Decision: Replace SQLite/EF Core with Azure Blob Storage

**By:** Fenster (Core Dev)
**Date:** 2026-03
**Requested by:** Brady

## What

The SquadPlaces data layer has been migrated from EF Core + SQLite to Azure Blob Storage via the `Azure.Storage.Blobs` SDK. Both the API and Web projects now use `IBlobStorageService` for all data access.

## Why

SQLite databases don't survive container restarts on Azure. Azure Blob Storage provides durable, scalable object storage that works in both local dev (Azurite via Aspire) and Azure deployment.

## Impact

- **SquadPlaces.Data:** No more EF Core. Models are pure POCOs (no navigation properties). `IBlobStorageService` is the data access contract.
- **SquadPlaces.Api + Web:** DI uses `BlobServiceClient` + `IBlobStorageService` as singletons. Connection string key is `BlobStorage` (was `SquadPlacesDb`).
- **AppHost:** Azurite emulator runs in Docker for local dev. `AddAzureStorage("storage").RunAsEmulator()` + `AddBlobs("BlobStorage")`.
- **Views:** No more navigation properties. Squad lookups are explicit (separate calls or dictionary).
- **Scale note:** Feed queries list all artifacts and paginate in memory. This is fine for MVP. If we grow past ~10K artifacts, add Table Storage or a search index.

## Migration checklist for anyone touching data access

1. Inject `IBlobStorageService`, never `DbContext`
2. Squad-artifact relationship is by SquadId field, not navigation property
3. Connection string name is `BlobStorage`
4. Blob containers (`squads`, `artifacts`) are created on app startup
