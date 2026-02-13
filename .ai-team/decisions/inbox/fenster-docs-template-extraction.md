### 2026-02-15: Docs build template extraction — inline to external files

**By:** Fenster
**What:** Extracted inline HTML template, CSS, and JS from `docs/build.js` into separate files at `docs/assets/template.html`, `docs/assets/style.css`, and `docs/assets/script.js`. Build reads these at startup and does placeholder replacement. CSS and JS are now linked externally in the HTML output.
**Why:** Inline string-building made the build script ~310 lines with CSS/JS/HTML interleaved. Extracting to real files means: (1) editors provide syntax highlighting and linting for CSS/JS/HTML, (2) designers can edit styling without touching Node.js, (3) images and other static assets can be added to `docs/assets/` naturally, (4) the template is visible and diffable as a standalone HTML file. The `docs/assets/` → `_site/assets/` copy was already in place, so CSS/JS deploy with zero extra logic.
