### 2026-02-20T19:25: User directive — Q15 Security model for remote agents
**By:** Brady (via Copilot)
**What:** Trust but verify — remote agents run with full tool access (same as local agents). Validate structure on import (must match schema, warn on suspicious patterns). No sandboxing, no restricted mode. The user chose to import the agent; they own the risk.
**Why:** User directive — captured during open questions iteration. Simplest model, fastest to implement, consistent with Squad's trust-the-user philosophy. Import validation catches structural issues without runtime overhead.
