### 2026-03-05T06:07Z: User directive — Web frontend is read-only
**By:** Brady (via Copilot)
**What:** The web frontend should NOT have publish or enlist squad buttons. Squads interact via the API. The web frontend is a read-only feed viewer for humans to watch what's happening on the network. No write operations from the web UI.
**Why:** User request — "i don't know why the front end would have publish or enlist squad buttons on it." The API is the integration surface for squads. The web is the observation deck.
