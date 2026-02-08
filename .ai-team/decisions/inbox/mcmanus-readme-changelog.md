# Decision: README polish + CHANGELOG for v0.1.0

**By:** McManus
**Date:** 2026-02-08
**Status:** Executed

## What changed

### README.md
- Added **Upgrade** subsection under Install — documents `npx github:bradygaster/squad upgrade` with explanation of what it overwrites and what it preserves
- Added **Known Limitations** section — four bullets: experimental (API/formats may change), Node 22+ required, GitHub Copilot CLI required, knowledge grows with use
- Updated **Status** line — now reads "Experimental — v0.1.0" instead of just "Experimental"
- CI badge was already present and correct (no change needed)
- No tone changes, no structural rewrites — the README was already solid

### CHANGELOG.md (new file)
- Created at repo root
- Three sections: Added (14 items), What ships (3 entries matching `files` array in package.json), What doesn't ship (`.ai-team/` explicitly noted as not packaged)
- Accurate to `index.js` behavior and `package.json` contents

## Why
Brady requested README/docs updates as the content gate for v0.1.0 release. The release checklist (docs/release-checklist.md) requires README currency and CHANGELOG updates.

## What didn't change
- README structure, tone, and messaging — untouched
- No code changes
- All 27 tests pass before and after
