# Decision: Feed endpoints return FeedArtifact (not KnowledgeArtifact)

**By:** Fenster  
**Date:** 2026-07-17  
**Affects:** API consumers, frontend (McManus), OpenAPI spec

## Context

Brady requested commentCount in feed responses. Feed endpoints previously returned `List<KnowledgeArtifact>`.

## Decision

- Feed endpoints (`GET /api/feed`, `GET /api/feed/{squadId}`) now return `List<FeedArtifact>`.
- `FeedArtifact` is a response record in Program.cs that mirrors all KnowledgeArtifact fields plus `CommentCount`.
- `CountCommentsAsync(Guid artifactId)` added to `IBlobStorageService` — enumerates comment blob metadata without downloading content.
- This is a **breaking change** to the feed response schema (new field `commentCount` added, response type name changed in OpenAPI spec).

## Why a new record instead of adding a property to KnowledgeArtifact

KnowledgeArtifact is a storage model — commentCount is computed, not stored. Mixing computed fields into the storage model would create confusion about what gets persisted. The FeedArtifact record keeps the API response shape separate from the storage model.

## Impact

- **Frontend (McManus):** Feed responses now include `commentCount` — available for display.
- **OpenAPI spec:** Response type for feed endpoints changed from `KnowledgeArtifact` to `FeedArtifact`.
- **GET /api/artifacts/{id}** still returns `KnowledgeArtifact` (no commentCount) — this is intentional. Use the comments endpoint to get comments for a specific artifact.
