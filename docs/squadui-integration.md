# SquadUI Integration Guide

This document describes the SDK changes in the P0 work for SquadUI integration. For SquadUI teams importing Squad parsers and CLI functions, here's what's changed and what's coming.

## What Changed (P0)

### 1. CRLF Normalization (#220, #221)

All 8 parsers now normalize line endings before parsing. If you're on Windows with `core.autocrlf=true`, line endings are cleaned automatically.

**What you get:** Values no longer contain trailing `\r` characters.

```typescript
// Before: value might be "member name\r"
// After: value is "member name"
const members = await parseTeamMarkdown(content);
```

No API changes — the fix is transparent.

### 2. CLI Entry Point Split (#187)

The CLI bootstrap has been separated from library exports.

**Old path:** `dist/index.js` (exported both parsers + CLI bootstrap)
**New paths:**
- **Library imports:** `dist/index.js` (unchanged — safe to import in extensions)
- **CLI execution:** `dist/cli-entry.js` (console output, process.exit calls)

If you're using Squad as a library (importing parsers), nothing changes. If you're directly invoking the binary, npm/npx handles the mapping automatically.

```typescript
// Safe in VS Code extensions (no process.exit risk):
import { parseTeamMarkdown, parseRoutingMarkdown } from '@bradygaster/squad-sdk';

const team = await parseTeamMarkdown(content);
```

### 3. `process.exit()` Removed from Library Functions (#189)

Library-consumable functions (`parseTeamMarkdown`, `resolveSquad`, etc.) no longer call `process.exit()`.

**What you get:** VS Code extensions and other host environments can safely import and use these functions without risking host termination.

**Error handling pattern:**

```typescript
// Before: parseTeamMarkdown could call process.exit(1) on error
// After: throws an exception instead
try {
  const team = await parseTeamMarkdown(content);
} catch (error) {
  // Handle gracefully in your extension
  logError(error);
}
```

## How to Import Parsers (Safe for Extensions)

```typescript
import {
  parseTeamMarkdown,
  parseRoutingMarkdown,
  // ... other parsers
} from '@bradygaster/squad-sdk';

// All safe — no process.exit() calls
const team = await parseTeamMarkdown(teamContent);
const routes = await parseRoutingMarkdown(routingContent);
```

## What's Coming (P1)

### Type Extensions
Richer TypeScript definitions for parsed entities (e.g., `Team`, `Agent`, `Route`).

### Subpath Exports
Named imports by category:
```typescript
import { parseTeamMarkdown } from '@bradygaster/squad-sdk/parsers';
import { resolveSquad } from '@bradygaster/squad-sdk/resolution';
```

### Error Types
Strongly typed error classes for parsing and resolution failures.

## Breaking Changes Summary

| Change | Impact | Migration Path |
|--------|--------|-----------------|
| Entry point split | CLI-only | No action if using npm/npx; update direct file refs to `cli-entry.js` |
| CRLF normalization | Automatic | No action — transparent fix |
| `process.exit()` removal | Error handling | Wrap calls in try/catch instead of relying on exit codes |

## Questions?

See the main [README](../README.md) for installation and the [CONTRIBUTING](../CONTRIBUTING.md) guide for development patterns.
