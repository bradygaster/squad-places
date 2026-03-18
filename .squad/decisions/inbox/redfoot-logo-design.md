# Decision: Squad Places Visual Identity — Hitchhiker's Guide Theme

**Author:** Redfoot (Graphic Designer)  
**Date:** 2025-01-15  
**Status:** Implemented

## Context

Brady requested a Hitchhiker's Guide to the Galaxy themed visual identity for the Squad Places docs site. The MkDocs Material theme needed a logo and favicon.

## Decision

Created a two-part visual identity:

1. **Logo (`docs/images/logo.svg`):** Guide device frame containing hitchhiker's thumb, constellation stars, and galaxy motif, with "SQUAD PLACES" text in brand colors.

2. **Favicon (`docs/images/favicon.svg`):** Simplified Guide device + thumb that works at 16x16/32x32.

## Design System

| Element | Color | Hex |
|---------|-------|-----|
| Deep Space Navy | Background | #0a1628 |
| Hitchhiker Green | Primary accent, stroke | #00e676 |
| Guide Gold | Secondary accent, "PLACES" | #ffd740 |
| Nebula Purple | Tertiary, orbit rings | #7c4dff |
| Starfield White | Stars | #e0e0e0 |

## Rationale

- **Hitchhiker's thumb** — universally recognizable, no text needed
- **Guide device frame** — evokes the iconic book/device
- **Constellation stars** — conveys "squad" (team/group)
- **Galaxy/space motif** — conveys "places" (destinations)
- **"42" easter egg** — fan service without compromising legibility
- **SVG over raster** — scalable, versionable, git-diffable
- **Works on light/dark** — green stroke on dark fill

## Impact

- `mkdocs.yml` updated: `favicon: images/favicon.svg` (was .ico)
- Consistent with Hitchhiker's Guide whimsical, tongue-in-cheek aesthetic
- Brand colors established for future visual assets
