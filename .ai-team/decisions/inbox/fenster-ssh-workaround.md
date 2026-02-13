# Decision: SSH workaround documentation pattern

**Date:** 2026-02-12
**Author:** Fenster
**Issue:** #30

## Context

Users reported `npx github:bradygaster/squad` appearing to hang. Root cause: npm resolves `github:` specifiers via `git+ssh://`, and when no SSH agent is running, git's passphrase prompt is hidden by npm's progress spinner. This is an npm TTY bug, not a Squad bug.

## Decision

1. Document workarounds inline in README (Install section + Known Limitations) rather than changing install mechanics.
2. Created `docs/scenarios/troubleshooting.md` as a central troubleshooting guide using problem → cause → fix format.
3. We do NOT work around this in code (e.g., no HTTPS fallback in index.js) — the install path via `github:` specifier is npm's responsibility.

## Impact

- Future community-reported "it doesn't work" issues should be added to the troubleshooting doc.
- The troubleshooting doc follows the same format as other scenario docs in `docs/scenarios/`.
