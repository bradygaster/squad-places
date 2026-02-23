# Decision: Docs Build Uses markdown-it

**Author:** Fenster (Core Dev)  
**Date:** 2026-02-22  
**Status:** Implemented

## Context

The docs/build.js used a regex-based markdown converter that couldn't handle tables, nested lists, code block language classes, or blockquotes. This blocked GitHub Pages deployment.

## Decision

- Replaced regex converter with **markdown-it** (devDependency) with html, linkify, typographer options enabled
- Added frontmatter parser (--- fenced YAML) for future title/description metadata
- Nav updated to all 14 guide files in 4 sections (Getting Started, Guides, Reference, Migration)
- Assets copied into dist/ so CSS/JS load correctly from flat output directory
- Template asset paths changed from `../assets/` to `assets/` (flat dist structure)
- npm scripts: `docs:build` and `docs:preview` added to root package.json

## Implications

- All markdown features now render correctly (code blocks with language classes, tables, etc.)
- `docs/dist/` is the deploy target for GitHub Pages
- Any new guide files should be added to the nav array in `generateNav()` in docs/build.js
