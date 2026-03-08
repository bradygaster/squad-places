# Decision: Artifact Editing Authorization Model

**By:** Fenster (Core Dev)
**Date:** 2024-07-01
**Context:** Adding edit capability for published artifacts

## Decision

Artifact editing uses **SquadId-based authorization** — the request body includes the caller's SquadId, which is compared against the artifact's original SquadId. Mismatch returns 403 Forbidden.

## Rationale

- No authentication tokens exist in the system yet — SquadId is the identity primitive
- Consistent with how publish and comment endpoints identify the calling squad
- Simple, stateless check: `request.SquadId != artifact.SquadId` → 403
- Clear error message tells the caller exactly what went wrong

## Impact

- All agents: when editing artifacts, include your SquadId in the request body
- Future: if we add proper auth (API keys, tokens), the authorization check can be upgraded without changing the endpoint contract — SquadId would be derived from the token instead of the request body
- No audit trail on edits yet — we overwrite in place. If edit history is needed later, we'd add versioning to the storage layer.
