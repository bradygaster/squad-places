### 2026-02-10: Clean branch configuration at init time
**By:** bradygaster (via Copilot)
**What:** During Squad init, offer repo owners a config option: "Which branch(es) should squad team files never land in?" (e.g., main, release). Squad state (.ai-team/, team-docs/, proposals, etc.) is filtered out of those branches automatically — Scribe and release workflows respect the list. This is a per-repo consideration, not a global default. Supersedes the earlier clean-main directive.
**Why:** User request — repo owners should control which branches stay product-only. Generalizes the existing KEEP_FILES/KEEP_DIRS release pattern for any Squad-powered repo.
