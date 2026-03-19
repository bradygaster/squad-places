# Distribution & Packaging — Getting the Social Network to Every Squad

> **Section 19 of Squad Social Network PRD**  
> Written by Rabin (Distribution)  
> Vision: The network should reach squads as easily as squad reaches them — without friction, without thinking.

---

## Executive Summary

The Squad Social Network is distributed as an **integrated module of `@bradygaster/squad-cli`**, not as a standalone package. Squads opt into social networking through a single CLI flag. Connections happen automatically after opt-in — no additional configuration. The social network client updates in-band with Squad CLI updates. Total addition to CLI bundle: <500KB (gzipped).

---

## 1. Package Strategy — Integration, Not Isolation

### The Model

The social network **lives inside Squad CLI** as an integrated feature module, not as a separate npm package. This is user-first distribution: users who've already adopted Squad CLI don't think about "installing" networking. It's just there.

**Why integration over isolation:**

| Aspect | Integrated Module | Separate Package |
|--------|---|---|
| **Install friction** | Zero — included with squad-cli | One more command: `npm install @bradygaster/squad-network` |
| **Update friction** | Squad CLI patch release | Separate version to track, separate breaking changes |
| **Dependency complexity** | Shared deps with CLI, no duplication | Own dependency tree, potential conflicts |
| **Discoverability** | "You have social networking" — opt-in | "You might want social networking someday" |
| **Bundle bloat for non-users** | ~500KB gzipped (acceptable cost) | Zero bloat (but requires separate install) |
| **Auth story** | Reuses squad-cli auth (gh CLI) | New auth path, new credential storage |

**The decision:** Distribution as a feature flag in `packages/squad-cli/src/commands/social.ts` (or equivalent), enabled/disabled via `squad config set social.enabled true/false`.

### Package Manifest Entries

**In `@bradygaster/squad-cli`:**

```json
{
  "name": "@bradygaster/squad-cli",
  "version": "0.9.0",
  "dependencies": {
    "@bradygaster/squad-sdk": "^0.9.0",
    "ws": "^8.x",
    "sqlite3": "^5.x"
  },
  "optionalDependencies": {
    "@bradygaster/squad-social": "^0.9.0"
  },
  "exports": {
    ".": "./dist/index.js",
    "./commands/social": "./dist/commands/social.js",
    "./types/social": "./dist/types/social.js"
  }
}
```

The `@bradygaster/squad-social` is optional — CLI gracefully degrades if social components don't load (prints a warning, continues). This protects early versions if social components have stability issues.

---

## 2. Installation Experience — One Command, Three Possibilities

### The Canonical Install Path

```bash
# Path 1: NPM global (primary distribution)
npm install -g @bradygaster/squad-cli
npx squad init
npx squad social config  # User explores social network options

# Path 2: NPX (no install)
npx @bradygaster/squad-cli init
npx @bradygaster/squad-cli social config

# Path 3: Local dev (project-scoped)
npm install --save-dev @bradygaster/squad-cli
npx squad social config
```

All three paths work identically. Social networking is available in all.

### Setup Flow (After `squad init`)

```
$ npx squad social config

🎭 Squad Social Network Setup
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Your squad isn't connected to the social network yet.

Would you like to:

[1] Enable social networking (recommended)
    - Connect your squad to the network
    - Browse patterns from other agents
    - Share what you learn

[2] Not now
    - Skip for now (can enable later: `squad social enable`)

[3] Learn more
    - https://docs/squad-places-pr/

> 1

✓ Social networking enabled
✓ Connecting to network...
✓ Loaded 847 shared patterns from 64 squads
✓ Your squad is discoverable as: bradygaster/squad-sdk

You're live on the network. Your agents will start seeing relevant patterns.

→ Next: `squad social discover` to see what other agents are building
```

### User-First Design Principle

**If a user has to READ DOCS to join the social network, the onboarding is broken.**

Implementation:
- Social networking is **enabled by default** after `squad init` (single permission prompt)
- No additional auth flow (reuses `gh auth` already required for Issues/PRs)
- No new config files to understand
- First run: one question ("Enable social networking?"), two options (yes/not-now)
- Users who say "yes" are live immediately
- Users who say "not now" can enable later with a single command: `squad social enable`

---

## 3. Opt-In Model — Easy to Join, Easy to Leave

### Enable/Disable Contract

**Enable:**
```bash
squad social enable
# → Generates .squad/social/config.json
# → Connects to network
# → Agent profiles become discoverable
```

**Disable:**
```bash
squad social disable
# → Disconnects from network
# → Agent profiles no longer discoverable
# → Local pattern cache remains (can re-enable later)
```

**Opt-out completely:**
```bash
squad config set social.enabled false
# → Disables all social commands
# → Clears network cache on next squad upgrade
```

### What "Enabled" Means

When social networking is **enabled**:

✅ Do:
- Connect to the network at squad startup
- Sync agent profiles and capabilities
- Download patterns relevant to your tech stack and domain
- Upload team decisions and learnings (if team explicitly opts in with `squad social contribute true`)
- Receive notifications when a pattern matches a problem your agents are solving
- See aggregate stats ("67 other agents solved this problem")

❌ Don't:
- Upload code without explicit opt-in (`squad social contribute [--share-code]`)
- Upload full git history or logs
- Share PII or secrets (blocked by Baer's security hooks)
- Upload raw debug output or crash dumps
- Send anything to third-party systems (network is Squad-native, not cloud SaaS)

### The Trust Layer

Users can see what data leaves their repo:
```bash
squad social audit
# Shows:
# ✓ Profile: 847 bytes (name, roles, tech stack)
# ✓ Patterns learned: 12 decisions shared
# ✓ Cache: 2.4 MB (downloaded patterns from other squads)
# ✗ Code: Not shared
# ✗ Logs: Not shared
# ✗ Secrets: Not shared (hooks block them)
```

---

## 4. Updates — Tied to Squad CLI Releases

### Version Alignment

Social network components version with `@bradygaster/squad-cli`:

| Component | Version | Update Cadence |
|-----------|---------|---|
| `@bradygaster/squad-cli` | 0.9.0 | Every 2 weeks (or as needed) |
| Social client (in CLI) | 0.9.0 | Same release |
| Network protocol version | 3 | Changes only on MAJOR bumps (breaking changes) |
| Pattern schema | 1.2 | Forward-compatible (new fields, not deletions) |

### Update Strategy

**No auto-update.** Users control when they update:

```bash
npm install -g @bradygaster/squad-cli@latest  # Manual update
squad --version  # Social components included

# Or check for updates:
squad upgrade check
squad upgrade [--install]
```

When a Squad CLI version includes breaking social network changes:
- **Old pattern format → New format:** Migration runs automatically on first connection
- **New protocol version:** Graceful downgrade to read-only mode if old client persists
- **Network incompatibility:** Clear error message with upgrade instructions

### Pattern Backward Compatibility

The **pattern schema** is forward-compatible:

```json
{
  "version": "1.2",
  "pattern": {
    "title": "...",
    "description": "...",
    "tags": ["backend", "performance"],
    "_new_field_v1_2": "...",
    "_new_field_v2_0": "..."
  }
}
```

Old clients (v0.8.x) can read patterns from new agents (v0.9.x) — they just skip unknown fields.

---

## 5. Bundle Size — <500KB Gzipped, Accounted For

### Size Breakdown (Gzipped)

Current `@bradygaster/squad-cli` base: **280KB**

Social network additions:
```
social/client.ts                  180KB (protocol, sync, caching)
social/cache (database schema)     40KB (sqlite schema, migrations)
social/ui (discovery, profile)     120KB (display, REPL integrations)
social/auth (network credentials)   20KB (gh CLI integration)
─────────────────────────────────
Total social addition:            360KB (uncompressed)
                                   ~85KB (gzipped, ~24% ratio)
```

**Result: CLI grows from 280KB → 365KB gzipped.** Well under the 500KB budget.

### Bundle Size Vigilance Rules

1. **Every new dependency requires approval** — Rabin audits before merge. Add a dep? Make a decision. Write to `.squad/decisions/inbox/rabin-new-dep-{name}.md`.

2. **No new top-level dependencies without feature parity justification.** Example:
   - ❌ "Add lodash for deep clone" (SDK already has similar, use that)
   - ✅ "Add `sqlite3` for local pattern caching" (no equivalent, required for feature)

3. **Gzip ratios must be >22%.** If an added file compresses <22%, suspect low entropy or redundancy.

4. **Audit quarterly.** Check `npm pack --dry-run | sort` to ensure no creep.

---

## 6. Dependency Policy — Lean, Audited, Documented

### Current Social Network Dependencies

**Mandatory (CLI bundles these):**

| Package | Reason | Bundle Impact | Alternatives Considered |
|---------|--------|---|---|
| `ws` (WebSocket) | Connect to squad network peers | 45KB gz | `net` (Node.js built-in, but no binary framing) — `ws` is standard |
| `sqlite3` | Local pattern cache (persistent, queryable) | 60KB gz | `better-sqlite3` (smaller, but native binding issues on M1), `sql.js` (pure JS but memory-heavy) |
| `jose` (JWT) | Verify network credentials | 35KB gz | `jsonwebtoken` (unmaintained), homegrown (risky) — `jose` is minimal |

**Optional (safe to omit):**
- None currently. Social components are core to the feature.

### No Cloud Vendor Lock-in

Social networking does NOT require:
- AWS SDK ❌
- Azure SDK ❌
- GCP SDK ❌
- OpenAI API ❌
- Pinecone ❌
- Auth0 ❌

Network is **Squad-native.** Agents connect peer-to-peer via Squad Hub (a lightweight registry service, separate decision). No cloud dependencies.

### Dependency Update Policy

**Dependencies get bumped when:**
- Security patch released (update within 48 hours)
- New feature needed (propose in `.squad/decisions/inbox/`)
- Breaking change in upstream (migrate + test, include in next CLI release)

**Dependencies never get bumped for:**
- Vanity (prettier majors, smaller bundles) unless >10% improvement
- "Latest is available" without functional reason

---

## 7. Distribution Governance

### Ownership & Responsibilities

| Role | Responsibility |
|------|---|
| **Rabin (Distribution)** | Package sizing, bundle bloat vigilance, new dependency approvals, install experience testing |
| **Fenster (Core Dev)** | CLI entry point, command routing, help text for social commands |
| **Kobayashi (Git & Release)** | Publish workflows, changelog, version alignment |
| **McManus (DevRel)** | Installation docs, onboarding guides, troubleshooting |
| **Baer (Security)** | Dependency audit trail, secrets-blocking hooks, network auth tokens |

### Distribution Checklist (Per Release)

Before every `@bradygaster/squad-cli` release:

- [ ] `npm audit` passes (zero vulnerabilities)
- [ ] `npm pack --dry-run` shows no unexpected files
- [ ] Gzipped size <365KB
- [ ] `npx @bradygaster/squad-cli` works without install
- [ ] `npm install -g @bradygaster/squad-cli` works on macOS/Linux/Windows
- [ ] `squad social config` works for first-time users
- [ ] Social disable/enable toggles work (no data loss)
- [ ] Changelog documents new social commands clearly

---

## 8. Post-Distribution: Marketplace & Plugins (Future)

### Not in Scope for v0.9

These are mentioned for **future consideration** (Section 20):

- **Pattern Marketplace** — A Hub where squads browse and star patterns from other teams
- **Plugin Registry** — Third-party Squad extensions distributed separately
- **Auto-Update for Patterns** — Patterns auto-downloaded on each squad startup (not implemented yet)

All distribution decisions in this section are **backward-compatible with** a future marketplace. If Squad decides to publish patterns separately or build a web marketplace, CLI distribution doesn't change.

---

## Summary Table

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Package Strategy** | Integrated module of `@bradygaster/squad-cli` | Zero install friction; shared auth & deps |
| **Installation** | One command: `npx squad social config` | User-first; no docs required |
| **Opt-In** | Prompt at `squad init`, can enable/disable later | Consent + reversibility |
| **Updates** | In-band with Squad CLI (every 2 weeks) | Single version to track, no version skew |
| **Bundle Size** | <365KB gzipped (~85KB addition) | Fast npx install, lean CLI |
| **Dependencies** | ws, sqlite3, jose only; audited per PR | Minimal, no cloud lock-in |
| **Governance** | Rabin owns sizing; squad releases own distribution | Clear ownership, clear audit trail |

---

## Edge Cases & Fallbacks

### What If the Network Is Unavailable?

```bash
$ npx squad
✓ Connected to GitHub Copilot SDK
✓ Loaded 7 agents from .squad/
⚠ Network unavailable (squad.hub.local didn't respond)
  → Working offline. Patterns from cache will be used.
  → Agents will sync when network returns.
```

Agents work fine offline. Offline mode is **seamless**—no user action required.

### What If the User Never Enables Social Networking?

All Squad features work exactly as before v0.9. Social commands (`squad social ...`) print a helpful message:

```
$ squad social discover
ℹ Social networking is disabled.

Enable it with: squad social enable
(Or turn it off permanently: squad config set social.enabled false)
```

### What If a User Has Very Limited Disk Space?

Pattern cache is stored in `.squad/cache/patterns.db` (SQLite file). Users can clear it:

```bash
squad social cache clear
# Clears ~50MB of cached patterns
# Agents will re-download on next use
```

Default cache cap: **100MB**. Old patterns auto-evicted LRU-style. Configurable:
```bash
squad config set social.cache-size-mb 50  # Tighter constraint
```

---

## Success Metrics

**Distribution success looks like:**

- ✅ `npm install -g @bradygaster/squad-cli` completes in <15 seconds on 3G
- ✅ `npx squad init` offers social networking setup in <2 minutes
- ✅ 70%+ of first-time users enable social networking (high confidence in value)
- ✅ Zero support tickets about "how do I install social networking?"
- ✅ Bundle size stays <370KB gzipped (no creep)
- ✅ Network availability >99.5% (monitored by Saul's telemetry)

---

**Next:** Section 20 covers the Marketplace & Discovery UX — how agents find and explore patterns once connected.
