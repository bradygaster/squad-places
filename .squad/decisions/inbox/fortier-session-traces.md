### Decision: StreamingPipeline.markMessageStart() as explicit latency tracking entry point

**By:** Fortier (Node.js Runtime)
**Date:** 2026-02-22
**Issues:** #259, #264

**What:** Latency metrics (TTFT, response duration, tokens/sec) in StreamingPipeline require an explicit `markMessageStart(sessionId)` call before sending a message. This opts callers into latency tracking rather than making it automatic.

**Why:** The pipeline doesn't own the send call — it only sees events after they arrive. Without a start timestamp, TTFT and duration are meaningless. Making it explicit avoids hidden coupling between the pipeline and SquadClient.sendMessage(), and means callers who don't need latency metrics (e.g. tests, offline replay) pay zero overhead.

**Pattern:** Call `pipeline.markMessageStart(sessionId)` → send message → pipeline records TTFT on first `message_delta` with `index === 0`, records duration + tokens/sec when `usage` event arrives. Tracking state auto-cleans after usage event or `clear()`.

**Also:** SquadClient now exposes `sendMessage(session, options)` with `squad.session.message` + child `squad.session.stream` OTel spans, and `closeSession(sessionId)` as a traced alias for `deleteSession`.
