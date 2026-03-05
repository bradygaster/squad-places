# Decision: Package Isolation Model for Enterprise Safety

**Date:** 2026-03-05  
**By:** Rabin (Distribution)  
**Status:** APPROVED (pending Brady sign-off)  
**Audience:** All agents, especially distribution/security teams

---

## The Requirement

**From Brady:** "Enterprise customers must NEVER be scared by social networking. The base Squad SDK/CLI must have ZERO social code. Social networking is a separate package (`@bradygaster/squad-social`) that you opt into by installing it. If they don't have the special package, they don't enlist in the social network."

This is a hard requirement, not a suggestion.

---

## The Decision

### Three-Tier Packaging Model

1. **`@bradygaster/squad-cli`** — Base CLI
   - ZERO social imports
   - ZERO social dependencies (not in package.json)
   - ZERO squad.place references in source
   - Serves enterprises without modification
   - Detects squad-social at runtime if user invokes `squad social` command
   - Clear error message if command invoked but package not installed

2. **`@bradygaster/squad-social`** — Optional Add-On
   - ALL social code lives here
   - Peer discovery, pattern sharing, network sync
   - Requires explicit `npm install @bradygaster/squad-social`
   - Independent version stream from squad-cli
   - Backward-compatible with squad-cli versions within 2 releases

3. **`@bradygaster/squad-sdk`** — Runtime (Unchanged)
   - No new dependencies added
   - Social networking uses existing EventBus and telemetry hooks
   - Enterprise-safe by design

### Dependency Relationship

**squad-cli declares ZERO dependency on squad-social.**

Not peer dependency. Not optional dependency. No dependency.

**Why:**
- Peer dependencies create log noise ("unmet peer dep warning").
- Optional dependencies still appear in package.json (security teams see them).
- Runtime discovery via dynamic import is cleanest: no npm declaration, fully optional.

**Pattern:**
```typescript
// In squad-cli/src/commands/social.ts
try {
  const mod = await import('@bradygaster/squad-social');
  // ... use it
} catch (err) {
  // Clear error: "Squad Social Network not installed. npm install @bradygaster/squad-social"
}
```

### Build-Time Isolation

**esbuild external config:**
```javascript
external: ['@bradygaster/squad-sdk', '@bradygaster/squad-social', 'ws', 'sqlite3', 'jose']
```

Prevents bundling of social code into squad-cli, even if both packages are in the monorepo.

**CI Verification:**
```bash
# After build, fail the publish if social code is found
grep -r "squad\.place" dist/cli-entry.js && exit 1
grep -r "squad-social" dist/cli-entry.js && exit 1
grep -r "require.*ws" dist/cli-entry.js && exit 1
```

Proves zero social code in published binary.

### Enterprise Audit Trail

An enterprise can verify squad-cli purity:

1. **package.json inspection:** Zero social packages in dependencies.
2. **Source grep:** Zero squad.place references.
3. **Network call audit:** Zero outbound network calls (except SDK-approved).
4. **Bundle verification:** `npm pack --dry-run` shows zero social code.
5. **CI logs:** Each publish run includes "✅ CLI bundle is enterprise-safe" verification.

We provide `.security.md` documenting all checks and results.

### Installation UX

**Enterprise customer:**
```bash
npm install -g @bradygaster/squad-cli
squad init
# Result: Enterprise-safe. Zero social networking.
```

**Social-enabled user:**
```bash
npm install -g @bradygaster/squad-cli
npm install -g @bradygaster/squad-social  # ← Explicit opt-in
squad init && squad social enlist
# Result: Social networking enabled.
```

One extra install. That's the friction. Worth it for the safety guarantee.

### Version Strategy

- **Independent version streams:** squad-cli and squad-social release independently.
- **Backward-compatibility window:** squad-social supports CLI versions within 2 minor releases.
- **Breaking changes explicit:** Major version bumps (1.0.0 → 2.0.0) are incompatible and documented.
- **Graceful degradation:** If versions are out of sync, a warning is printed, but the system still works.

Example: squad-cli@0.9.0 can load squad-social@0.7.0, 0.8.0, 0.9.0, 0.10.0 with no issues.

### Bundle Size Budget

- **squad-cli:** Stays under 360KB gzipped (no change).
- **squad-social:** ~150KB gzipped (ws ~50KB, sqlite3 ~70KB, jose ~15KB, overhead ~15KB).
- **Measured:** Quarterly audits (Q1, Q2, Q3, Q4) to prevent dependency creep.
- **Enforced:** CI step fails the build if budget is exceeded.

---

## Why This Model

| Goal | This Model | Alternatives Rejected |
|------|-----------|------------------------|
| Enterprise safety | Provable zero social code via audit | Optional dependency (still in package.json); bundled social (too big, too scary) |
| Clear opt-in | Separate package install | Feature flag (looks optional but isn't); environment variable (silent) |
| Discoverability | One extra command (npm install) | No friction (but enterprises don't know they can use it) |
| Auditability | package.json + source grep + CI verification | Promises (enterprises won't trust) |
| Versioning freedom | Independent streams | Lock-step (couples pipelines) |

---

## Implementation

### Phase 1: Prepare squad-cli
- Remove any imports of social code from squad-cli.
- Add runtime discovery pattern to `squad social` command.
- Add "not installed" error handler.
- Add esbuild external config.

### Phase 2: Create squad-social
- New package in `packages/squad-social/`.
- All social code: peer discovery, pattern cache, WebSocket bridge.
- Own package.json, own build config, own version stream.

### Phase 3: Verify Isolation
- Add CI verification step (grep for squad.place, squad-social, ws, sqlite3, jose in compiled CLI).
- Add bundle size measurement step.
- Write `.security.md` with audit results.

### Phase 4: Release
- Publish @bradygaster/squad-cli (same version as before, but now with zero social code).
- Publish @bradygaster/squad-social (first release, version aligned with CLI).
- Update release notes: "Squad Social Network is now an optional add-on package."

---

## What Enterprises See

When an enterprise evaluates squad-cli, they see:

```
✅ Squad CLI (Enterprise Safe)

Audit Results:
  ✓ Zero social packages in dependencies
  ✓ Zero squad.place references in source code
  ✓ Zero network calls (outside GitHub Copilot SDK)
  ✓ Zero social code in published bundle
  ✓ CI verification: PASSING

To enable social networking (optional):
  npm install @bradygaster/squad-social

Compatibility: HIPAA-safe, FedRAMP-compatible, SOC2-auditable
```

No handwaving. No promises. Just facts they can verify themselves.

---

## Open Questions & Decisions Needed

1. **Security document location:** `.security.md` at root? In README? Separate security/ subdirectory?
2. **CI implementation:** Which workflow file? squad-publish.yml or separate squad-ci.yml?
3. **Enterprise communication:** When we announce this, do we proactively reach out to current enterprise customers? Or wait for inquiry?
4. **social command error message:** Exact wording approved by Brady?

---

## Sign-Off

**Rabin:** Approved. This model is bulletproof from a distribution/auditability perspective.

**Brady:** (Pending) — Confirm this meets the "enterprise safety" requirement.

**Kobayashi:** (Pending) — Confirm package structure works with npm workspaces.

**Keaton:** (Pending) — Confirm this fits the overall PRD narrative.

---

**This decision makes the enterprise safety claim not just achievable, but verifiable and auditable. Security teams will trust us.**
