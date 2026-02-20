### 2026-02-20T19:26: User directive — Q16 SkillSource parity
**By:** Brady (via Copilot)
**What:** Skill and agent sources should be mutually exclusive abstractions. SkillSource and AgentSource are separate interfaces — no shared base class, no inheritance, no common RemoteSource. Clean separation despite similar resolution patterns.
**Why:** User directive — captured during open questions iteration. Brady overrode the team's recommendation (option 2, specialization) in favor of option 1 (separate interfaces). Prioritizes clean conceptual boundaries over DRY. Some code duplication is acceptable.
