# Proposal 021: Release Plan & Distribution Strategy

**Status:** Proposed  
**Authored by:** Kobayashi (Git & Release Engineer)  
**Date:** 2025-07-16  
**Requested by:** bradygaster  
**Relates to:** Proposal 019 §1.3 (CI), §1.4 (Version Stamping), Brady's no-npm directive

---

## What This Document Is

The definitive release and distribution plan for Squad. Covers how code gets from `squadify` branch to users' machines, how versions are tagged, how releases are cut, and how we guarantee `.ai-team/` state integrity through every upgrade.

**Non-negotiable constraint:** No npm publish. Ever. Squad is distributed exclusively via `npx github:bradygaster/squad`.

---

## 1. Distribution via GitHub (Not npm)

### How `npx github:bradygaster/squad` Works

When a user runs `npx github:bradygaster/squad`:

1. npm resolves `github:bradygaster/squad` as a GitHub repository reference
2. It clones/downloads the repo's **default branch** (`main`)
3. It reads `package.json`, finds the `bin` entry (`create-squad` → `./index.js`)
4. It executes `index.js` in a temporary directory, passing any CLI arguments

**Key implication:** Without tags, users always get `HEAD` of `main`. This is unstable — a half-merged PR on `main` ships to every user immediately.

### Version Pinning Syntax

The `@` syntax does NOT work for GitHub-hosted packages. The correct syntax uses `#`:

| Command | What It Pulls |
|---------|---------------|
| `npx github:bradygaster/squad` | HEAD of default branch (`main`) |
| `npx github:bradygaster/squad#v0.2.0` | Exact tag `v0.2.0` |
| `npx github:bradygaster/squad#main` | HEAD of `main` |
| `npx github:bradygaster/squad#squadify` | HEAD of `squadify` |
| `npx github:bradygaster/squad#abc1234` | Exact commit SHA |

**This means:**
- Users CAN pin versions: `npx github:bradygaster/squad#v0.2.0`
- The default (no `#`) always pulls `main` HEAD
- We MUST keep `main` in a release-worthy state at all times

### Tag Format

**Standard:** `v{MAJOR}.{MINOR}.{PATCH}` — e.g., `v0.1.0`, `v0.2.0`, `v0.2.1`

- Follows semver exactly
- The `v` prefix is a universal convention for git tags (matches GitHub Releases UI)
- Tags are immutable — once `v0.2.0` is tagged, it never moves

### Ensuring Users Get the Latest Release

Since `npx github:bradygaster/squad` pulls `main` HEAD (not the latest tag), we have two strategies:

**Strategy A: Main IS the latest release (Recommended)**
- `main` only receives code via tagged releases
- Merges to `main` happen ONLY as part of the release process
- `main` HEAD is always the latest tagged release
- Users who run the bare command get the latest stable release

**Strategy B: Latest-tag redirect in code**
- `upgrade` subcommand reads the GitHub API to find the latest release tag
- Adds complexity, requires network calls, fragile
- NOT recommended for v1

**Decision: Strategy A.** Keep `main` clean. Develop on `squadify` (or feature branches). Merge to `main` only when cutting a release. Tag the merge commit.

### The `upgrade` Subcommand

`npx github:bradygaster/squad upgrade` currently works by re-running `index.js` from whatever version npx resolves. With Strategy A:

1. User runs `npx github:bradygaster/squad upgrade`
2. npx fetches `main` HEAD (which IS the latest release)
3. `index.js` runs in upgrade mode — overwrites Squad-owned files, never touches `.ai-team/`
4. User gets the latest release automatically

For pinned users: `npx github:bradygaster/squad#v0.2.0 upgrade` — upgrades to that specific version.

---

## 2. Semantic Versioning Strategy

### Pre-v1 Rules

We're at `0.1.0`. Pre-v1 semver rules:

| Change Type | Version Bump | Example |
|-------------|-------------|---------|
| Breaking change to init/upgrade behavior | Minor bump | `0.1.0` → `0.2.0` |
| New subcommand (export, import) | Minor bump | `0.2.0` → `0.3.0` |
| Bug fix, test addition, prompt change | Patch bump | `0.1.0` → `0.1.1` |
| Template/agent content update only | Patch bump | `0.1.1` → `0.1.2` |

### Version Locations

The version lives in exactly ONE place: `package.json` `"version"` field. All other version references derive from it:

- `--version` flag reads `package.json`
- `squad.agent.md` version comment (when 1.4 ships) is stamped during release
- Git tag matches `package.json` version

### What Triggers a Release?

| Trigger | Version Bump | Example |
|---------|-------------|---------|
| Wave gate passes | Minor | Wave 1 complete → `v0.2.0` |
| Critical bug fix | Patch | Broken upgrade → `v0.1.1` |
| Brady's call | Either | "Ship this now" → whatever's appropriate |

**No pre-release/RC process for v0.x.** We're pre-v1, everything is effectively a pre-release. Ship when ready.

---

## 3. Release Process Checklist

This is the manual process for cutting a release. Later we automate parts of it with GitHub Actions.

### Pre-Release

- [ ] All tests pass on `squadify`: `npm test`
- [ ] Version in `package.json` is bumped to the new version
- [ ] CHANGELOG.md is updated with release notes (when it exists — McManus owns this, Wave 1.5)
- [ ] If version stamping exists (1.4): `squad.agent.md` has the correct version comment

### Cut the Release

```bash
# 1. Ensure squadify is clean and up to date
git checkout squadify
git pull origin squadify

# 2. Bump version in package.json (if not already done)
# Edit package.json "version" field

# 3. Commit the version bump
git add package.json
git commit -m "chore: bump version to 0.2.0"

# 4. Merge to main
git checkout main
git pull origin main
git merge squadify --no-ff -m "release: v0.2.0"

# 5. Tag the merge commit
git tag -a v0.2.0 -m "Release v0.2.0"

# 6. Push everything
git push origin main
git push origin squadify
git push origin v0.2.0
```

### Post-Release

- [ ] Create GitHub Release from the tag (manual or automated — see §4)
- [ ] Verify: `npx github:bradygaster/squad --version` outputs new version
- [ ] Verify: `npx github:bradygaster/squad#v0.2.0 --version` outputs new version
- [ ] Verify: upgrade path works — init with old version, upgrade with new version, `.ai-team/` intact
- [ ] Announce: blog post (McManus), README badge update if applicable

---

## 4. GitHub Actions Workflows

### 4a. CI Workflow (`.github/workflows/ci.yml`)

Runs on every push and pull request. This is Sprint Plan item 1.3.

```yaml
name: CI

on:
  push:
    branches: [main, squadify]
  pull_request:
    branches: [main, squadify]

jobs:
  test:
    name: Test
    runs-on: ubuntu-latest
    strategy:
      matrix:
        node-version: [22.x]

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup Node.js ${{ matrix.node-version }}
        uses: actions/setup-node@v4
        with:
          node-version: ${{ matrix.node-version }}

      - name: Run tests
        run: npm test

      - name: Verify init works
        run: |
          mkdir -p /tmp/squad-smoke-test
          node index.js
        working-directory: /tmp/squad-smoke-test
        env:
          SQUAD_ROOT: ${{ github.workspace }}

      - name: Verify upgrade preserves .ai-team/
        run: |
          mkdir -p /tmp/squad-upgrade-test
          cd /tmp/squad-upgrade-test
          node ${{ github.workspace }}/index.js
          echo "user state" > .ai-team/decisions/inbox/test-state.md
          node ${{ github.workspace }}/index.js upgrade
          test -f .ai-team/decisions/inbox/test-state.md || (echo "FAIL: .ai-team/ state was corrupted" && exit 1)
          echo "PASS: .ai-team/ state preserved after upgrade"
```

**Notes:**
- The smoke test runs `index.js` against a temp directory to verify init works
- The upgrade test verifies `.ai-team/` state preservation — this is our state integrity canary
- Node 22.x only (per `engines` field from Sprint Plan 1.4)
- Matrix can expand to Windows (`windows-latest`) later if needed

### 4b. Release Workflow (`.github/workflows/release.yml`)

Triggered when a version tag is pushed. Creates a GitHub Release with auto-generated notes.

```yaml
name: Release

on:
  push:
    tags:
      - 'v*'

permissions:
  contents: write

jobs:
  test:
    name: Test before release
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '22.x'

      - name: Run tests
        run: npm test

  release:
    name: Create GitHub Release
    needs: test
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Validate tag matches package.json version
        run: |
          TAG_VERSION="${GITHUB_REF_NAME#v}"
          PKG_VERSION=$(node -p "require('./package.json').version")
          if [ "$TAG_VERSION" != "$PKG_VERSION" ]; then
            echo "ERROR: Tag version ($TAG_VERSION) does not match package.json version ($PKG_VERSION)"
            exit 1
          fi
          echo "Version validated: $TAG_VERSION"

      - name: Generate release notes
        id: notes
        run: |
          # Get the previous tag
          PREV_TAG=$(git tag --sort=-creatordate | grep '^v' | sed -n '2p')
          if [ -z "$PREV_TAG" ]; then
            echo "First release — no previous tag"
            echo "PREV_TAG=" >> $GITHUB_OUTPUT
          else
            echo "Previous tag: $PREV_TAG"
            echo "PREV_TAG=$PREV_TAG" >> $GITHUB_OUTPUT
          fi

      - name: Create GitHub Release
        uses: actions/github-script@v7
        with:
          script: |
            const tag = context.ref.replace('refs/tags/', '');
            const version = tag.replace('v', '');

            const releaseBody = [
              `## Install`,
              '```bash',
              `npx github:bradygaster/squad`,
              '```',
              '',
              `## Upgrade`,
              '```bash',
              `npx github:bradygaster/squad upgrade`,
              '```',
              '',
              `## Pin this version`,
              '```bash',
              `npx github:bradygaster/squad#${tag}`,
              '```',
              '',
              `---`,
              '',
              `**Full Changelog**: https://github.com/bradygaster/squad/compare/${process.env.PREV_TAG || 'main'}...${tag}`,
            ].join('\n');

            await github.rest.repos.createRelease({
              owner: context.repo.owner,
              repo: context.repo.repo,
              tag_name: tag,
              name: `Squad ${tag}`,
              body: releaseBody,
              draft: false,
              prerelease: version.startsWith('0.'),
              generate_release_notes: true,
            });
          env:
            PREV_TAG: ${{ steps.notes.outputs.PREV_TAG }}

  verify:
    name: Verify release
    needs: release
    runs-on: ubuntu-latest
    steps:
      - name: Wait for GitHub to propagate
        run: sleep 10

      - name: Verify npx resolves the tag
        run: |
          TAG="${GITHUB_REF_NAME}"
          mkdir -p /tmp/verify-release
          cd /tmp/verify-release
          npx github:bradygaster/squad#${TAG} --version
          echo "Release verification passed"
```

**Key design decisions:**
- Tests run BEFORE the release is created — a failing test prevents a broken release
- Tag-to-`package.json` version validation prevents mismatched tags
- `generate_release_notes: true` uses GitHub's auto-generated changelog (commit-based)
- Custom release body includes install/upgrade/pin instructions
- Pre-v1 releases are marked as `prerelease: true`
- Post-release verification confirms `npx` resolution works

---

## 5. Branch Strategy

### Current State

- `main` branch exists (remote and local)
- `squadify` branch is the active development branch
- No branch protection rules

### Target State

```
main          ← release-only; always stable; this is what users get
  ↑
squadify      ← primary development branch
  ↑
feature/*     ← optional feature branches off squadify
```

### Branch Rules

| Branch | Who Pushes | What Gets Pushed | Protection |
|--------|-----------|------------------|------------|
| `main` | Kobayashi (release process only) | Merge commits from `squadify` + version tags | Protected: no direct push, no force push |
| `squadify` | All agents | Feature work, bug fixes, proposals | Default dev branch |
| `feature/*` | Individual agents | Scoped work for complex features | None — merge to `squadify` when done |

### Merge Strategy

- `squadify` → `main`: **Merge commit** (`--no-ff`), only during release
- `feature/*` → `squadify`: **Squash merge** preferred (clean history), or regular merge
- Direct push to `main` is prohibited

### Branch Protection Recommendations

When Brady enables branch protection on `main`:

```
Branch: main
├── Require pull request before merging: OFF
│   (Releases are merge-committed directly by the release process.
│    A PR is optional but not required — the release checklist is the gate.)
├── Require status checks to pass: ON
│   └── Required: "CI / Test"
├── Require branches to be up to date: ON
├── Do not allow force pushes: ON
├── Do not allow deletions: ON
└── Restrict who can push: ON
    └── Allowed: bradygaster (+ release automation if using PAT)
```

**Note:** Full branch protection requires a GitHub Pro/Team plan for private repos, or the repo must be public. For public repos, these rules are free.

### Moving from `squadify` to `main`

The first release (`v0.1.0` or `v0.2.0` — Brady's call) merges `squadify` into `main` and establishes the branch strategy going forward. Until then, all development stays on `squadify`.

---

## 6. State Integrity Guarantees

### The Contract

> **The `upgrade` subcommand MUST NEVER touch `.ai-team/`.** This is user-owned state. It contains team identity, decisions, history, casting — everything that makes a squad *their* squad.

### What `upgrade` Touches (Squad-Owned)

| Path | Action on Upgrade |
|------|-------------------|
| `.github/agents/squad.agent.md` | Overwritten |
| `.ai-team-templates/` | Overwritten (full directory) |

### What `upgrade` Never Touches (User-Owned)

| Path | Action on Upgrade |
|------|-------------------|
| `.ai-team/` | Never read, never written, never deleted |
| `.ai-team/agents/` | Untouched |
| `.ai-team/decisions/` | Untouched |
| `.ai-team/casting/` | Untouched |
| `.ai-team/orchestration-log/` | Untouched |
| `.ai-team/decisions/inbox/` | Untouched |

**Exception:** `init` (not upgrade) creates `.ai-team/decisions/inbox/`, `.ai-team/orchestration-log/`, and `.ai-team/casting/` directories if they don't exist. This is additive-only — it never overwrites existing files.

### CI Enforcement

The CI workflow (§4a) includes an upgrade state integrity test:

1. Run `init` to create a project
2. Write a sentinel file into `.ai-team/decisions/inbox/`
3. Run `upgrade`
4. Assert the sentinel file still exists and is unmodified

**Hockney should expand this** into a proper test in `test/index.test.js`:
- Test that `.ai-team/` contents survive upgrade (files, subdirectories, nested content)
- Test that Squad-owned files ARE updated on upgrade
- Test that the version output changes after upgrade (when version stamping ships)

### `.gitignore` Policy

`.ai-team/` is **NOT** in `.gitignore`. It is user state that SHOULD be committed to their repo:

- Team identity (casting, charters) is project-specific
- Decision history is valuable project documentation
- Agent histories contain project context and learnings
- Committing `.ai-team/` enables collaboration — teammates see the same squad

Current `.gitignore` is correct. No changes needed.

---

## 7. Version Check for Users

### Problem

Users don't know when a new version of Squad is available. They might run an outdated version indefinitely.

### Proposed Solution (Wave 2, item 2.2 scope)

Add a lightweight version check to the `upgrade` subcommand:

```javascript
// In upgrade mode, after completing the upgrade:
const https = require('https');
const RELEASES_URL = 'https://api.github.com/repos/bradygaster/squad/releases/latest';

function checkForUpdate(currentVersion) {
  return new Promise((resolve) => {
    https.get(RELEASES_URL, { headers: { 'User-Agent': 'squad-cli' } }, (res) => {
      let data = '';
      res.on('data', chunk => data += chunk);
      res.on('end', () => {
        try {
          const latest = JSON.parse(data).tag_name.replace('v', '');
          if (latest !== currentVersion) {
            console.log(`\n${DIM}New version available: v${latest}`);
            console.log(`Run: npx github:bradygaster/squad upgrade${RESET}\n`);
          }
        } catch { /* silent — don't break init for a version check */ }
        resolve();
      });
    }).on('error', () => resolve()); // Network failure is not a user problem
  });
}
```

**Characteristics:**
- Non-blocking — failure is silent, never breaks init/upgrade
- Uses GitHub's public API (no auth required for public repos)
- Only runs on `upgrade` subcommand (not init — don't slow down first impressions)
- Respects rate limits (60 requests/hour for unauthenticated, more than sufficient)

**Timeline:** This is a Wave 2 item. For now, users check manually or follow the repo.

---

## 8. Release Cadence & Naming

### Version-to-Wave Mapping (Planned)

| Version | Wave | What Ships |
|---------|------|------------|
| `v0.1.0` | Current | Init, upgrade, 12 tests, templates |
| `v0.2.0` | Wave 1 complete | Error handling, 20+ tests, CI, version stamping, feels heard |
| `v0.3.0` | Wave 2 complete | Tiered modes, smart upgrade, skills Phase 1, export |
| `v0.4.0` | Wave 3 complete | Import, skills Phase 2, history summarization |
| `v1.0.0` | All waves + polish | The "holy cow" release |

Patch releases (`v0.2.1`, etc.) happen between waves for critical fixes only.

### GitHub Release Naming

Format: `Squad v{version}` — e.g., "Squad v0.2.0"

Release notes include:
- What shipped (from wave items)
- Install/upgrade/pin commands
- Link to full changelog (commit diff)
- Breaking changes (if any)

---

## 9. Implementation Plan

### Phase 1 — Immediate (This Wave)

| Task | Owner | Status |
|------|-------|--------|
| Write this proposal | Kobayashi | ✅ Done |
| Create `.github/workflows/ci.yml` | Kobayashi (drafted in §4a) → Hockney implements | Blocked on approval |
| Create `.github/workflows/release.yml` | Kobayashi (drafted in §4b) → Kobayashi implements | Blocked on approval |
| First release: tag `v0.1.0` on `main` | Kobayashi | Blocked on Brady's go-ahead |

### Phase 2 — Wave 1 Gate

| Task | Owner |
|------|-------|
| CI passing on all pushes | Hockney |
| State integrity test in CI | Hockney |
| Version stamping in `squad.agent.md` | Fenster |
| Cut `v0.2.0` release | Kobayashi |

### Phase 3 — Wave 2

| Task | Owner |
|------|-------|
| Version check in upgrade subcommand | Fenster |
| Automated release notes refinement | Kobayashi |
| CHANGELOG.md established | McManus |

---

## 10. Open Questions for Brady

1. **First release timing:** Should we tag `v0.1.0` now (current state) or wait for Wave 1 to complete and tag `v0.2.0` as the first release?

2. **Repo visibility:** Is `bradygaster/squad` currently public or private? Branch protection rules and GitHub API access (for version checks) depend on this.

3. **Release authority:** Should releases require Brady's explicit approval, or can I cut releases when wave gates pass?

4. **`squadify` → `main` merge:** When do we merge `squadify` into `main` for the first time? This establishes the branch strategy.

---

## Summary

| Area | Decision |
|------|----------|
| Distribution | `npx github:bradygaster/squad` — pulls from `main` HEAD |
| Version pinning | `npx github:bradygaster/squad#v0.2.0` — uses `#` not `@` |
| Tag format | `v{MAJOR}.{MINOR}.{PATCH}` |
| Branch strategy | `main` (releases only) ← `squadify` (development) |
| Release trigger | Wave gate pass or critical fix |
| CI | GitHub Actions on push/PR, Node 22.x, ubuntu-latest |
| Release automation | Tag push triggers test → release → verify pipeline |
| State integrity | `.ai-team/` never touched by upgrade; enforced in CI |
| npm publish | **Never.** |

---

**Review requested from:** bradygaster  
**Implements:** Sprint Plan items 1.3 (CI), release process (new), distribution strategy (new)  
**Authored by:** Kobayashi — if it ships, it ships correctly.
