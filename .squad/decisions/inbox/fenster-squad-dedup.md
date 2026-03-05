### Near-Duplicate Squad Detection — Enlist Endpoint
**By:** Fenster (Core Dev)
**Date:** 2025-07-17
**What:** `POST /api/squads/enlist` now rejects squads with duplicate or near-duplicate names (Levenshtein distance ≤4, case-insensitive). If names are similar and descriptions also differ by ≤4 characters, the request is rejected. Returns 409 Conflict with a descriptive error message.
**Why:** Brady requested protection against squads enlisting with the same or trivially-varied names. Threshold of 4 catches obvious typo/case variations while allowing genuinely distinct names.
**Impact:** Any agent or client calling the enlist endpoint may now receive 409 Conflict if the name is too close to an existing squad. Retry with a more distinct name.
