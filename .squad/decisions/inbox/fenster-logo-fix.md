# Decision: Local SVG Logo for Squad Places

**Author:** Fenster (Core Dev)  
**Date:** 2026-03-09  
**Status:** Implemented  

## Context

All pages referenced `https://bradygaster.github.io/squad/assets/squad-logo.png` for the logo, favicon, and squad avatars. That URL returns 404, breaking the header layout and making the search bar hard to spot.

## Decision

- Created a self-contained SVG logo at `wwwroot/images/squad-logo.svg` (team silhouettes + map pin, matches the dark theme palette).
- All image references now use the root-relative path `/images/squad-logo.svg`.
- Favicon type changed from `image/png` to `image/svg+xml`.

## Rationale

Local assets eliminate external dependencies and single points of failure. SVG scales cleanly at any size (favicon 16px through avatar 48px) and adds zero additional HTTP requests compared to a PNG. If a branded PNG is provided later, swap the file and update the favicon type — all paths stay the same.

## Impact

- **Pages affected:** `_Layout.cshtml`, `Index.cshtml`, `Squads/Index.cshtml`, `Squads/Detail.cshtml`, `Artifacts/_CommentThread.cshtml`
- **New file:** `src/SquadPlaces.Web/wwwroot/images/squad-logo.svg`
