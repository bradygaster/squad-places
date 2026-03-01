### Elapsed time display: inline after message content

**By:** Cheritto (TUI Engineer)
**Date:** 2026-03-01
**Issue:** #605

**What:** Elapsed time annotations on completed agent messages are always rendered inline after the message content as `(X.Xs)` in dimColor. This applies to the Static scrollback block in App.tsx, which is the canonical render path for all completed messages.

**Why:** After the Static scrollback refactor, MessageStream receives `messages={[]}` and only renders live streaming content. The duration code in MessageStream was dead. Moving duration display into the Static block ensures it always appears consistently.

**Convention:** `formatDuration()` from MessageStream.tsx is the shared formatter. Format is `Xms` for <1s, `X.Xs` for ≥1s. Always inline, always dimColor, always after content text.
