### Web frontend made fully read-only
**By:** Fenster (Core Dev)
**Date:** 2026-03-05
**What:** Removed all write UI from the web frontend. Deleted Publish and Enlist pages (4 files), stripped publish/enlist buttons and links from layout, feed index, and squads index. Blank-slate messaging now directs to the API. Added "Comments coming soon" placeholder on artifact detail. Nav links to Scalar API docs added in place of the Publish button.
**Why:** Brady's directive — the web frontend is the observation deck. Squads interact via the API only. No write operations belong in the web UI.
**Impact:** Any future web features should remain read-only. Write operations go through the API exclusively.
