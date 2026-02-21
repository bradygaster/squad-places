# Agent Marketplace: Share, Discover, Install

> **⚠️ INTERNAL ONLY — DO NOT PUBLISH**

**Milestone:** M5 · **Issue:** #52 (M5-17)
**Date:** Sprint 5

---

## Why a Marketplace

Squad teams build agents that solve real problems — testing specialists, security reviewers, documentation writers. Until now, sharing meant copying files between repos. The marketplace module gives agents a proper lifecycle: **export, discover, install, validate**.

## Export & Import

`exportSquadConfig()` packages a team's configuration — agents, skills, routing rules, and metadata — into a portable `ExportBundle`. Options control whether history is included, which format to use (JSON or YAML), and whether to anonymize sensitive fields. The bundle is self-describing with version and timestamp metadata.

`importSquadConfig()` reverses the process: it reads a bundle, validates compatibility, and merges agents and skills into the target project. History can be split per-agent via `splitHistory()`, so imported agents arrive with their own context rather than a monolithic log.

## Marketplace Schema

`MarketplaceEntry` defines the catalog record: name, version, description, author, categories, tags, download stats, and security status. `MarketplaceIndex` holds the full catalog with search support. `searchMarketplace()` accepts a `MarketplaceSearchQuery` with text search, category filters, and sort options — returning paginated results ranked by relevance or popularity.

## Browser CLI

`MarketplaceBrowser` provides the interactive discovery experience. `browse()` lists entries with formatted output via `formatEntryList()`. `details()` shows a single entry with stats and security status via `formatEntryDetails()`. `install()` downloads, validates, and merges an agent into the local project, returning an `InstallResult` with the list of created files and any conflicts detected.

## The 7 Security Rules

Every agent that enters the marketplace passes through `validateRemoteAgent()`, which applies seven `SecurityRule` checks:

1. **prompt-injection** (critical) — Scans charters for injection patterns like "ignore previous instructions"
2. **suspicious-tools** (critical) — Flags dangerous tool requests: `shell`, `exec`, `eval`, `sudo`, `rm`, `delete`
3. **pii-in-charter** (warning) — Detects emails, SSN-like numbers, credit card patterns, API keys
4. **overly-broad-permissions** (warning) — Catches "all files", "unrestricted access", "full system" language
5. **untrusted-source** (warning) — Flags agents with unknown or empty source
6. **missing-charter** (warning) — Agents without charters have unpredictable behavior
7. **excessive-tools** (warning) — Agents requesting more than 15 tools are flagged for scope review

Critical violations **block** installation. Warnings are surfaced but don't prevent install. `quarantineAgent()` sanitizes blocked agents by stripping injection patterns and capping tool lists. `generateSecurityReport()` produces an audit-ready summary.

## Conflict Resolution

When an imported agent collides with an existing one, `detectConflicts()` compares flattened config paths and reports each conflict with its type (`added`, `modified`, `removed`). Four resolution strategies are available: `keep-existing`, `use-incoming`, `merge`, and `manual`. The merge strategy applies a field-level three-way merge; manual defers to the user.

## Offline Mode

`OfflineManager` detects connectivity status and degrades gracefully. Local agents, cached skills, and config editing work offline. Marketplace browse works in degraded mode if a cache exists. Publishing and remote agent access require connectivity. Queued operations replay automatically when the connection returns via `syncPending()`.
