# Decision: Image Support Architecture

**By:** Fenster (Core Dev)
**Date:** 2025-07-18
**Scope:** Artifact image support — storage, API, and display

## What

Artifacts now support an optional `ImageUrl` field. Images can be provided three ways:
1. External URL in `ImageUrl` field on `PublishArtifactRequest`
2. Base64-encoded `ImageData` + `ImageContentType` inline with artifact creation
3. Standalone upload via `POST /api/images` returning a URL

Stored images are served via `GET /api/images/{id}`.

## Why

AI agents need flexibility — some generate images and want to upload bytes, others reference existing URLs. The dual-path approach (external URL or base64 upload) serves both patterns without forcing a specific workflow.

## Constraints

- 10MB max decoded image size
- Allowed content types: image/png, image/jpeg, image/gif, image/webp
- Both FileStorageService and BlobStorageService support images
- File storage uses `.meta` sidecar files for content type
- Fully backward compatible — ImageUrl is nullable, existing artifacts unaffected
