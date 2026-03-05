# Package Isolation — The Enterprise Safety Wall

> **Section 28 of Squad Social Network PRD**  
> Written by Rabin (Distribution)  
> Vision: Enterprise customers must NEVER be scared by social networking. The base SDK/CLI has ZERO social code.

---

## Executive Summary

Brady has a hard requirement: **Enterprise customers must NEVER be scared by social networking.** The base Squad SDK/CLI must have **ZERO social code**, **ZERO network calls to squad.place**, and **ZERO social dependencies**. Social networking is a separate, optional package (`@bradygaster/squad-social`) that enterprise teams explicitly opt into by installing it. If a customer doesn't install the social package, they get a guaranteed enterprise-safe experience. The decision is binary and auditable.

---

## 1. The Package Split — Three Distinct Products

### `@bradygaster/squad-cli` — Base CLI (Enterprise Safe)

**What it is:**
- The canonical Squad CLI, distributed globally and in enterprises.
- ZERO social imports. ZERO squad.place references. ZERO optional dependencies for social.
- Includes: init, commands, shell integration, all core CLI features.
- Ships with @bradygaster/squad-sdk as a required dependency.

**What it does NOT include:**
- `squad social` command (returns "not installed" error if invoked without the social package).
- `squad social enlist`, `squad social browse`, `squad social audit` commands.
- WebSocket client for peer connections (`ws`).
- SQLite3 database for pattern caching.
- jose/jwt for peer signing.
- Any registry lookup, network calls, or pattern sharing code.
- Any squad.place domain references.

**Package size target:** Stays under current budget (~360KB gzipped). Adding zero new dependencies.

### `@bradygaster/squad-social` — Social Add-On (Opt-In)

**What it is:**
- The complete social networking module: peer discovery, pattern sharing, network sync.
- Ships ONLY when explicitly installed.
- Contains: enrollment flow, peer discovery, pattern cache, network bridge, audit trail.
- Can be installed post-init or alongside CLI.

**What it includes:**
- `squad social` command and all subcommands.
- WebSocket client for peer connections (`ws`).
- SQLite3 for pattern cache and peer registry.
- jose for credential signing and peer verification.
- Telemetry for social activity (optional, user-controlled).

**Package size target:** ~150KB gzipped (ws, sqlite3, jose add ~120KB combined).

### `@bradygaster/squad-sdk` — The Runtime (Unchanged)

**What it is:**
- The runtime SDK. ZERO changes for social packaging.
- Social networking uses SDK's existing EventBus and telemetry hooks.
- No new SDK dependencies added.
- SDK remains enterprise-safe by design.

---

## 2. Dependency Relationship — The "No Dependency" Model

### Why NOT a Peer Dependency

Early drafts considered `squad-social` as a **peer dependency** in squad-cli (like otel in SDK). This was rejected because:

1. **npm install behavior:** Peer dependencies create expectations. Users see "warning: unmet peer dependency @bradygaster/squad-social" in install logs — noise and confusion.
2. **Discoverability:** Peer dependencies suggest "you should probably install this." We want the opposite — it's truly optional, truly hidden if not installed.
3. **Version coupling:** Peer dependency ranges force tight coupling. Social should be independently versioned.

### Why NOT a Direct Dependency

`squad-social` could be a direct dependency of `squad-cli`, bundled into every install. This was rejected because:

1. **Enterprise fear:** Security teams see `squad-social` in package.json, they Google "squad.place network," they get scared. We lose their trust.
2. **Auditable claim:** We want to claim "squad-cli has ZERO social code." That claim dies if `squad-social` is in package.json, even as optional.
3. **Bundle bloat:** Every enterprise CLI user pays 150KB even if they never enable it.

### The Approved Model: NO Dependency

**squad-cli declares ZERO dependency on squad-social** in package.json. Social is **discovered at runtime** if installed, **gracefully absent otherwise**.

**The result:**
- Enterprise audit of squad-cli sees NO social references in package.json.
- Enterprise audit of squad-cli sees NO squad.place references in source code.
- Enterprise audit of squad-cli sees NO network calls unless they explicitly install squad-social.

---

## 3. Detection Pattern — How Base CLI Discovers Social

### Runtime Discovery via Dynamic Import

When users invoke `squad social` subcommand, squad-cli attempts to load the social package. No import happens at load time.

```typescript
// In squad-cli/src/cli/commands/social.ts

export async function socialCommand(
  args: string[],
  config: SquadConfig
): Promise<void> {
  try {
    const socialMod = await import('@bradygaster/squad-social');
    const handler = socialMod.createSocialCommandHandler();
    return await handler(args, config);
  } catch (err) {
    if (err instanceof Error && err.code === 'MODULE_NOT_FOUND') {
      console.error(`
╭──────────────────────────────────────────╮
│ Squad Social Network not installed      │
╰──────────────────────────────────────────╯

The 'squad social' command requires @bradygaster/squad-social.

To enable social networking:
  npm install -g @bradygaster/squad-social

Or for project-scoped install:
  npm install --save-dev @bradygaster/squad-social

After install, run:
  squad social enlist

Your squad will join the peer network.
`);
      process.exit(1);
    }
    throw err;
  }
}
```

### Verification

- At **load time:** No `import` or `require` of squad-social anywhere in squad-cli source.
- At **CLI invoke time:** squad-cli checks if user typed `squad social`. If yes, attempts dynamic import. If import fails, returns clear error.
- **No silent degradation:** If user expects social to work, they see an actionable error message, not silent failures.

---

## 4. Build-Time Isolation — Proving Zero Social Code in Bundle

### esbuild External Config

In `packages/squad-cli/esbuild.config.mjs`:

```javascript
const config = {
  entryPoints: ['src/cli-entry.ts'],
  bundle: true,
  platform: 'node',
  target: 'node20',
  outfile: 'dist/cli-entry.js',
  
  // CRITICAL: External ensures squad-social is NOT bundled
  external: [
    '@bradygaster/squad-sdk',
    '@bradygaster/squad-social', // ← Never bundle this
    'ws',
    'sqlite3',
    'jose',
  ],
  
  // Minify, sourcemaps, etc.
  minify: true,
  sourcemap: true,
};
```

The `external` array tells esbuild to treat these as require() calls to runtime packages, never bundled. This is verified in CI.

### CI Verification — Zero Social References

**In `.github/workflows/squad-publish.yml` or `ci.yml`:**

```yaml
# After build, verify zero social code in bundle
- name: Verify No Social Code in CLI Bundle
  run: |
    # Check for squad.place domain strings
    if grep -r "squad\.place" dist/cli-entry.js; then
      echo "ERROR: squad.place found in CLI bundle"
      exit 1
    fi
    
    # Check for social module imports
    if grep -r "squad-social" dist/cli-entry.js; then
      echo "ERROR: squad-social import found in CLI bundle"
      exit 1
    fi
    
    # Check for ws, sqlite3, jose (only allowed in external imports)
    if grep -r "require.*ws" dist/cli-entry.js | grep -v "external"; then
      echo "ERROR: ws bundled into CLI"
      exit 1
    fi
    
    echo "✅ CLI bundle is enterprise-safe (zero social code)"
```

**Output:** Build passes only if squad-cli contains ZERO social, ZERO squad.place, ZERO optional network dependencies.

---

## 5. Enterprise Audit Trail — What Customers See

### When an Enterprise Evaluates squad-cli

An enterprise security team runs:

```bash
npm install @bradygaster/squad-cli
```

They audit by checking:

1. **package.json dependencies:**
   ```json
   {
     "dependencies": {
       "@bradygaster/squad-sdk": "^0.8.x",
       "ink": "^6.x",
       "react": "^19.x"
     }
   }
   ```
   ✅ **Zero social packages.** No @bradygaster/squad-social. No ws, sqlite3, jose.

2. **Source code for squad.place:**
   ```bash
   grep -r "squad\.place" packages/squad-cli/src/
   # (returns nothing)
   ```
   ✅ **Zero squad.place references.** CLI doesn't know the social network domain exists.

3. **Source code for network calls:**
   ```bash
   grep -r "https://" packages/squad-cli/src/
   grep -r "ws://" packages/squad-cli/src/
   # (returns only documentation/examples, no live calls)
   ```
   ✅ **Zero outbound network calls** (other than GitHub Copilot SDK, which is pre-approved).

4. **Bundled code:**
   ```bash
   npm pack --dry-run | grep -E "(ws|sqlite3|jose|squad-social)"
   # (returns nothing)
   ```
   ✅ **Zero social dependencies bundled.** Package tarball contains only CLI code.

5. **External dependencies:**
   ```bash
   npm ls --all
   # Shows: @bradygaster/squad-sdk, ink, react, react-dom
   ```
   ✅ **Transparent dependency tree.** Every node library is public.

### Security Certification Document

We provide `.security.md` at the root of the repo (or linked from squad-cli README):

```markdown
# Enterprise Security Certification — Squad CLI

## Social Network Isolation

`@bradygaster/squad-cli` (base CLI) contains **ZERO social networking code**.

### Audit Results

| Check | Result | Verification |
|-------|--------|--------------|
| Social packages in deps | ❌ NONE | `npm ls | grep squad-social` returns nothing |
| squad.place references | ❌ NONE | `grep -r "squad.place" src/` returns nothing |
| Network calls (ws/https outside SDK) | ❌ NONE | `grep -r "ws://" src/` + grep -r "https://" src/` show zero |
| Bundled optional deps | ❌ NONE | esbuild external config prevents bundling |
| CI verification | ✅ PASSING | Each publish run verifies zero social code |

### Social Networking (Optional)

To enable social networking, customers **explicitly install**:

```bash
npm install @bradygaster/squad-social
```

This is an opt-in add-on. Without this package, squad-cli has zero social features.

### Compliance

- ✅ HIPAA-safe (no patient data shared)
- ✅ FedRAMP-compatible (no external network required)
- ✅ SOC2-auditable (transparent dependencies, no hidden network)

**Last verified:** [build timestamp]  
**Published:** @bradygaster/squad-cli@[version]
```

---

## 6. Installation UX — Enterprise vs. Social User

### Enterprise Customer

```bash
# Step 1: Install squad CLI
npm install -g @bradygaster/squad-cli

# Step 2: Initialize
squad init

# Result
✅ Squad initialized
   No social networking enabled
   Your squad is enterprise-safe
   
# If they try social commands:
squad social enlist
# Error: Squad Social Network not installed.
# To enable social networking: npm install -g @bradygaster/squad-social
```

**Social features:** Completely absent. No temptation. No network calls.

### Social-Enabled User

```bash
# Step 1: Install CLI
npm install -g @bradygaster/squad-cli

# Step 2: Install social module
npm install -g @bradygaster/squad-social

# Step 3: Initialize
squad init

# Step 4: Enlist in social network
squad social enlist
✅ Your squad joined the network
   Peer ID: abc...xyz
   Enabled pattern sharing
   
# Now social commands work
squad social browse      # See patterns from peers
squad social publish     # Share your patterns
squad social audit       # See what data you're sharing
```

**Social features:** Fully enabled. Network operational. User explicitly opted in.

---

## 7. Version Coupling — Squad-CLI and Squad-Social Alignment

### Semantic Versioning Strategy

**squad-cli and squad-social use independent version streams** but maintain compatibility ranges.

#### In squad-cli package.json (when social support is added):

```json
{
  "name": "@bradygaster/squad-cli",
  "version": "0.9.0",
  "peerDependenciesMeta": {
    "@bradygaster/squad-social": {
      "optional": true
    }
  }
}
```

No entry in `dependencies` or `optionalDependencies`. Discovery is runtime-only.

#### Version Compatibility Matrix

| CLI Version | Social Version | Compatibility |
|---|---|---|
| 0.9.0 | 0.9.0 | ✅ Supported (first release) |
| 0.9.0 | 0.10.0 | ✅ Supported (backward-compat) |
| 0.10.0 | 0.9.0 | ⚠️ Degraded (old social, new CLI) |
| 1.0.0 | 0.x | ❌ Incompatible (major version bump) |

**Rule:** squad-social minor/patch updates are backward-compatible with CLI versions within 2 releases. Major versions are breaking.

### What If They're Out of Sync

**User installs squad-cli@0.9.0 + squad-social@0.7.0 (too old):**

```
squad social enlist
⚠️  Warning: squad-social 0.7.0 is out of date
    Current CLI: 0.9.0
    
This should still work, but some features may not be available.
To upgrade: npm install -g @bradygaster/squad-social@latest
```

Version mismatch is not fatal. Graceful degradation with warning.

**User installs squad-cli@0.8.0 + squad-social@1.0.0 (too new, breaking change):**

```
squad social enlist
❌  Error: squad-social 1.0.0 requires squad-cli >=1.0.0
    Your CLI: 0.8.0
    
To fix, upgrade both:
  npm install -g @bradygaster/squad-cli@latest
  npm install -g @bradygaster/squad-social@latest
```

Breaking changes are explicit. Users know they need to upgrade CLI.

---

## 8. Bundle Size Budget — Verification & Audits

### Baseline (squad-cli alone)

- Current size: ~360KB gzipped.
- Target: Stays under current budget.
- Yearly audit: Q1, Q2, Q3, Q4 to catch dependency creep.

### With Social Installed (optional)

- squad-social package: ~150KB gzipped.
  - ws: ~50KB gzipped.
  - sqlite3 (native binary): ~70KB gzipped.
  - jose: ~15KB gzipped.
  - Overhead: ~15KB gzipped.
- Total installed size: ~510KB gzipped (acceptable for opt-in).
- **Enterprise without social:** ~360KB (unchanged).
- **User with social:** ~510KB (acceptable trade for networking).

### Bundle Size CI Check

```yaml
# In publish workflow
- name: Measure Bundle Sizes
  run: |
    npm pack --dry-run squad-cli | tar -xzOf - package/dist/cli-entry.js | wc -c
    # Expected: ≤360KB gzipped
    
    npm pack --dry-run squad-social | tar -xzOf - package/dist/social.js | wc -c
    # Expected: ≤150KB gzipped
```

If budget is exceeded, build fails. Forces conscious decisions on new dependencies.

---

## 9. Security Guarantees

### What Enterprise Customers Get

1. **Zero Social Code:** squad-cli binary auditable, verifiable, safe.
2. **Zero Social Dependencies:** No ws, sqlite3, jose in core CLI.
3. **Zero Squad.place References:** No hardcoded domain, no network assumptions.
4. **Auditable Runtime:** If social is installed, it's explicit in `npm ls`.
5. **Clear Opt-In:** Users know they're enabling social. No hidden features.

### What Social Users Get

1. **Transparency:** Exactly which packages are loaded.
2. **Audit Trail:** `squad social audit` shows what data leaves the repo.
3. **Disconnect Option:** `squad social disable` turns off network calls (still installed, just off).
4. **No Surprises:** Social features never activate without user action.

---

## 10. Implementation Checklist

- [ ] squad-cli: Remove any `import` or `require` of squad-social (including test files).
- [ ] squad-cli: Add "not installed" error handler to `squad social` command.
- [ ] squad-social: Create as new npm package in `packages/squad-social/`.
- [ ] esbuild: Add `external: ['@bradygaster/squad-social', 'ws', 'sqlite3', 'jose']` to squad-cli config.
- [ ] CI: Add CI step to verify zero squad-social in bundled CLI.
- [ ] CI: Add bundle size checks for both packages.
- [ ] Docs: Create `.security.md` or security section in README.
- [ ] Tests: Add tests verifying dynamic import behavior (loads if present, errors gracefully if absent).
- [ ] Release: Publish both packages independently; note social as optional add-on in release notes.

---

## 11. Decision Summary

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Package Split** | squad-cli (base) + squad-social (add-on) | Enterprise safety; clear opt-in. |
| **Dependency** | Zero dependency (runtime discovery) | No npm warnings; fully optional; auditable. |
| **Detection** | Dynamic import with clear error | Lazy-load social only if user invokes it. |
| **Build Isolation** | esbuild external config + CI verification | Proven zero social code in bundle. |
| **Enterprise Audit** | package.json inspection + source grep | Transparent, no surprises. |
| **Versioning** | Independent version streams, backward-compat ranges | Social updates don't require CLI updates. |
| **Bundle Size** | CLI <360KB, social <150KB, total <510KB | Measurable, budgeted, audited quarterly. |

---

## Appendix: Enterprise Approval Email Template

Marketing teams can use this when presenting to enterprises:

```
Subject: Squad CLI — Enterprise Security Guarantee

Dear [Enterprise],

Squad CLI (@bradygaster/squad-cli) is HIPAA-safe and suitable for enterprise deployment.

VERIFIED: Zero social networking code in base CLI
  - No squad.place domain references
  - No peer-to-peer code
  - No WebSocket dependencies
  - No optional pattern sharing

Social networking is an OPT-IN add-on package that requires explicit installation:
  npm install @bradygaster/squad-social

Without this package, Squad CLI is guaranteed to be enterprise-safe.

Audit details: .security.md (attached)
Build verification: CI logs show zero social references in each published release

Ready to discuss compliance requirements?
```

---

**This architecture makes the enterprise safety claim bulletproof. An enterprise security team can audit the source, audit the published package, and verify the guarantee. No handwaving. No blind trust. Just facts.**
