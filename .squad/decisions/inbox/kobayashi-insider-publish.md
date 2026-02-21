# Decision: Insider publish package scaffolds

**By:** Kobayashi (Git & Release)
**Date:** 2026-02-21
**Re:** #215
**PR:** #283

## What
Added minimal publishable entry points to `packages/squad-sdk/` and `packages/squad-cli/` so the insider publish workflow (`.github/workflows/squad-insider-publish.yml`) can produce valid npm packages.

### squad-sdk
- `src/index.ts`: exports `VERSION` constant (placeholder — full source migration comes later)
- `tsconfig.json`: extends root, outputs to `dist/` with declarations
- `package.json`: added `files`, `scripts.build`

### squad-cli
- `src/cli.ts`: placeholder binary (`#!/usr/bin/env node`)
- `tsconfig.json`: extends root, outputs to `dist/` with declarations
- `package.json`: added `files`, `scripts.build`; `bin` already pointed to `./dist/cli.js`

### Root
- `build` script updated: `tsc && npm run build --workspaces --if-present`

## Why
The insider publish pipeline triggers on push to `insider` branch but both workspace packages were empty scaffolds — no source, no build output, nothing to publish. This adds the minimum viable content so `npm publish` succeeds and the insider channel can be verified end-to-end.

## Constraints respected
- ESM-only (`"type": "module"`) — per team decision
- TypeScript strict mode — per team decision
- Node.js >=20 — per team decision
- squad-cli depends on squad-sdk via version string `"0.6.0-alpha.0"` — per Edie's npm workspace protocol decision
- `files` lists ensure only `dist/` and `README.md` ship in the published package

## What this does NOT do
- Does not push to `insider` branch (coordinator handles that after merge)
- Does not migrate real source code — these are placeholders
- Does not add tests for workspace packages (nothing to test yet)
