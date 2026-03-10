---
name: "binary-file-storage"
description: "Pattern for storing and serving binary files (images, etc.) across both File and Blob storage backends"
domain: "storage, api-design"
confidence: "high"
source: "earned — image support feature implementation"
---

## Context
When adding binary file support (images, attachments) to Squad Places, both FileStorageService and BlobStorageService must be extended consistently. The pattern applies to any new binary content type.

## Patterns

### Storage Interface
Add `Save{Type}Async(Guid ownerId, Guid id, byte[] data, string contentType)` and `Get{Type}Async(Guid ownerId, Guid id)` returning `(byte[] Data, string ContentType)?` to `IBlobStorageService`. Owner-scoped IDs enable natural data isolation.

### FileStorageService
- Store binary at `{basePath}/{type}/{ownerId}/{id}{extension}`
- Store content type in a `.meta` sidecar file at `{basePath}/{type}/{ownerId}/{id}.meta`
- Map content types to extensions in a switch expression
- Create the top-level directory in `InitializeAsync()`; owner subdirectories created on-demand via `Directory.CreateDirectory()`

### BlobStorageService
- Use a dedicated container (e.g., `images`)
- Blob path: `{ownerId}/{id}{extension}` — virtual folder structure
- Set `BlobHttpHeaders.ContentType` on upload
- For retrieval, search by prefix with `GetBlobsAsync(BlobTraits, BlobStates, prefix, CancellationToken)`
- Create container in `InitializeAsync()`

### API Pattern
- `POST /api/{type}` — accept base64 body with owner ID, validate owner exists, store, return relative URL
- `GET /api/{type}/{ownerId}/{id}` — serve with `Results.File(data, contentType)`
- Inline upload on parent entity (e.g., `ImageData` on `PublishArtifactRequest`) as convenience
- **Only relative URLs** — reject absolute http/https URLs to prevent SSRF and ensure all content is hosted

### URL Security
- Validate URLs with regex matching `/api/{type}/{guid}/{guid}` format
- In HTML sanitizer, use `FilterUrl` event to strip non-relative `src` attributes rather than removing the tag entirely
- This is more surgical than scheme-based allowlisting — permits relative paths without custom schemes

### Dockerfile
Add the new data directory to the `mkdir -p` line.

## Examples
See `src/SquadPlaces.Data/FileStorageService.cs` (SaveImageAsync/GetImageAsync) and `src/SquadPlaces.Web/Api/ApiEndpoints.cs` (image endpoints).

## Anti-Patterns
- Don't store binary data as base64 in JSON — decode to bytes first
- Don't use `GetBlobsAsync(prefix:)` named parameter — the .NET 10 API requires positional args
- Don't forget the `.meta` sidecar in file storage — without it, you can't serve with correct Content-Type
