# Decision: Docs Build Multi-Directory Restructure

**Author:** Fenster  
**Date:** 2026-02-23  
**Status:** Implemented  
**Scope:** `docs/build.js`

## Context

The docs site was expanding from a single `guide/` directory (14 files) to 5 directories: guide/, cli/, sdk/, features/, scenarios/ (~62 files total). The build system needed to discover and build docs from all subdirectories while maintaining backwards compatibility with existing guide pages.

## Decision

Rewrote `docs/build.js` file discovery and nav generation to be section-driven rather than hardcoded:

1. **Section config array** (`SECTIONS`) maps directory names to nav titles. Adding a new section is a one-line change.
2. **Dynamic discovery** — each section reads its directory at build time. Missing directories are skipped gracefully (supports McManus adding content incrementally).
3. **Subdirectory output** — HTML mirrors source structure (`guide/index.md` → `dist/guide/index.html`). Root `dist/index.html` redirects to `guide/index.html`.
4. **Relative asset paths** — `assetsPrefix()` computes `../` depth for asset references since pages are now nested.
5. **Cross-directory nav** — all links are relative, computed from the active page's directory depth.
6. **Search index** spans all sections.
7. **Link rewriting** — `.md` → `.html` in rendered output.

## Alternatives Considered

- **Flat output** (keep all HTML in dist/ root): Rejected — URL collisions likely with 62+ files from different sections (e.g., `installation.md` exists in both guide/ and cli/).
- **Absolute paths** (e.g., `/guide/index.html`): Rejected — breaks local file:// preview and requires knowing the deployment base path.

## Consequences

- Template.html unchanged — asset path rewriting happens at build time via string replacement.
- New sections require only adding an entry to the `SECTIONS` array in build.js.
- All 30 existing docs-build tests pass. Build produces 62 pages.
