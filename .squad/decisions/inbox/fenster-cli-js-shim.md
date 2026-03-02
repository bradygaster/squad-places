### cli.js is now a thin ESM shim

**By:** Fenster  
**Date:** 2025-07  
**What:** `cli.js` at repo root is a 14-line shim that imports `./packages/squad-cli/dist/cli-entry.js`. It no longer contains bundled CLI code. The deprecation notice only displays when invoked via npm/npx.  
**Why:** The old bundled cli.js was stale and missing commands added after the monorepo migration (e.g., `aspire`). A shim ensures `node cli.js` always runs the latest built CLI.  
**Impact:** `node cli.js` now requires `npm run build` to have been run first (so `packages/squad-cli/dist/cli-entry.js` exists). This was already the case for any development workflow.
