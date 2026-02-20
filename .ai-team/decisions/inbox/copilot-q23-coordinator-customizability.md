### 2026-02-20: Q23 — Coordinator customizability in TypeScript
**By:** Brady (via Copilot)
**What:** Config-driven architecture (option 1). Extract every customizable behavior into config files (JSON/YAML) that the TypeScript runtime reads. squad.agent.md becomes a reference doc only. Users customize via config, not code. Clean separation between compiled runtime and user-editable configuration.
**Why:** User decision — Kujan's #1 concern. Brady prefers full config extraction over hybrid prompt approach. Config files are more structured, validatable, and tooling-friendly than prompt files. Gives users a clear customization surface without needing to understand prompt engineering.
