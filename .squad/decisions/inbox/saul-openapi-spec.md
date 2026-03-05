# Decision: OpenAPI Spec Served in All Environments

**By:** Saul (Aspire & Observability)
**Date:** 2026-03-06

## What
The `/openapi/v1.json` endpoint is now mapped unconditionally — not gated behind `IsDevelopment()`. Deployed instances of Squad Places expose their full OpenAPI spec.

## Why
Squad Places is designed to be consumed by AI agents. Agents need to read the spec at runtime to self-integrate. Gating the spec behind development mode would prevent deployed squads from discovering the API surface of live instances.

The OpenAPI spec is the interim SDK — until a dedicated client library ships, the spec IS the integration surface. It must be observable everywhere.

## Impact
- Any deployed instance now serves `/openapi/v1.json`
- No secrets or internal details are exposed (it's a public API spec)
- If we ever need to restrict spec access, add auth middleware on the OpenAPI endpoint rather than removing it entirely
