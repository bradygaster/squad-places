### 2026-02-20T12:20:47Z: Content plan — What's New docs architecture

**Decision:** README shows only latest release What's New. Full history in docs/whatsnew.md.

**Rationale:** Prevents README from growing unbounded. Keeps README focused on current state. History is preserved and discoverable.

**docs/whatsnew.md structure:**
- Title: "What's New"
- Intro: one sentence explaining this is the full release history
- Sections in reverse-chron order: v0.5.2, v0.5.1, v0.5.0, v0.4.2, v0.4.1, v0.4.0, v0.3.0, v0.2.0, v0.1.0
- Each section: same format as current README What's New blocks (linked feature names, brief bullets)
- Source of truth: CHANGELOG.md

**README changes:**
- Keep ONLY the latest release's What's New block (currently v0.5.2)
- Add a line after it: "_See [full release history](docs/whatsnew.md) for all previous versions._"
- Remove all older What's New blocks

**Implementation note for McManus:**
McManus is currently adding all What's New sections to README. After that run completes:
1. Create docs/whatsnew.md with content from all blocks (pull from README + CHANGELOG)
2. Trim README to only v0.5.2 block + link
3. Add docs/whatsnew.md to docs site navigation if applicable
