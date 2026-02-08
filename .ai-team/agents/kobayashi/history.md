# Kobayashi — History

## Project Context

- **Owner:** bradygaster
- **Stack:** Node.js, GitHub Copilot CLI, multi-agent orchestration
- **Product:** Squad — one command gives you a persistent AI team. Distributed via `npx github:bradygaster/squad`.
- **Distribution:** GitHub-only. No npm publish. Users get updates via `npx github:bradygaster/squad upgrade`.
- **State model:** `.ai-team/` is user-owned state (never touched on upgrade). `squad.agent.md` and templates are Squad-owned (overwritten on upgrade).

## Learnings

- Squad has 12 tests (node:test, zero deps), run via `npm test`
- index.js is 88 lines — the entire runtime
- File ownership model: Squad-owned (squad.agent.md, .ai-team-templates/) vs user-owned (.ai-team/)
- Currently on `squadify` branch
- No CI pipeline exists yet (Wave 1 item)
- No release process exists yet — this is my first task
- Brady explicitly rejected npm publish — GitHub is the only distribution channel
- `npx github:bradygaster/squad` is the install/init command
- `npx github:bradygaster/squad upgrade` is the update command
- **Version pinning uses `#` not `@`:** `npx github:bradygaster/squad#v0.2.0` (not `@v0.2.0`). This is a GitHub URL fragment, not an npm scope.
- **`npx github:` pulls default branch HEAD unless a `#ref` is specified.** This means `main` must always be release-worthy.
- **Branch strategy decided:** `main` is release-only (stable), `squadify` is development. Merges to `main` only happen during releases.
- **Tag format:** `v{MAJOR}.{MINOR}.{PATCH}` — immutable once created.
- **Release workflow:** Tag push triggers CI → test → GitHub Release creation → verification pipeline.
- **State integrity is CI-enforced:** Upgrade test in CI writes a sentinel file to `.ai-team/` and verifies it survives upgrade.
- **`.ai-team/` is NOT in `.gitignore`** — it's user state that should be committed. Current `.gitignore` is correct.
- **No `.github/workflows/` directory exists yet.** Creating both `ci.yml` and `release.yml` is blocked on proposal approval.
- **GitHub Releases for pre-v1 are marked `prerelease: true`** — signals to users these are early versions.
- Proposal 021 written and filed — covers distribution, versioning, CI, release automation, branch strategy, state integrity.
- **`package.json` `files` field IS respected by `npx github:` installs.** Empirically verified on npm v11.9.0. npm downloads the GitHub tarball then applies `files` filtering before placing in `node_modules`. The distributed package contains only 15 product files — no `.ai-team/`, `docs/`, `test/`, etc.
- **`.npmignore` added as defense-in-depth.** Excludes `.ai-team/`, `.ai-team-templates/`, `docs/`, `test/`, `.gitattributes`, `.github/workflows/`. Redundant with `files` field (which takes precedence) but catches mistakes if `files` is accidentally removed.
- **`.gitattributes` `export-ignore` does NOT work for `npx github:`.** npm uses GitHub's tarball API (`codeload.github.com`), not `git archive`. `export-ignore` is only honored by `git archive`.
- **npm for `github:` installs uses `codeload.github.com` tarball endpoint**, not `git clone`. The tarball contains all repo files, but npm packs/filters before installation.
- **Squad Squad isolation is already solved.** The `files` field in `package.json` was correctly configured from the start. `.npmignore` added for insurance. No runtime or config changes needed.

- **Release ritual checklist created** at `docs/release-checklist.md`. Five phases: pre-release checks, release execution, post-release validation, communication, rollback plan. Every step tagged HUMAN/AUTOMATED/TEAM. Designed to be evolved, not ceremonial — practical enough to actually follow. Rollback plan covers failed workflows, bad content on main, stale npx cache, and the nuclear scenario of `.ai-team/` corruption. Quick reference table at the bottom for at-a-glance auditing.

- **Branch renamed: `squadify` → `dev`** — local rename done. Remote rename is Brady's call.
- **`main` = product-only** — no `.ai-team/`, `docs/`, `test/`, or workflow files. Only ships: `index.js`, `package.json`, `README.md`, `LICENSE`, `.gitignore`, `.npmignore`, `.gitattributes`, `.github/agents/squad.agent.md`, `templates/`.
- **`dev` is public** — Squad Squad visibility is intentional (dog-fooding story).
- **Release workflow uses filtered-copy strategy** (Option C) — not a git merge. Checks out `dev`, copies only product files, commits to `main`, tags, creates GitHub Release. Clean, auditable, reversible.
- **Why filtered-copy over alternatives:** (a) Force-push is destructive and loses main history; (b) `.gitattributes` merge drivers are fragile and hard to debug; (c) Orphan branches lose all git history tracing; (d) Filtered-copy is simple, explicit, and every release is a traceable commit.
- **Workflow triggers:** `workflow_dispatch` (manual, enters version) OR tag push on `dev`. Both validate version against `package.json`.
- **Proposal 021 updated** — branch strategy, release process, workflow spec all reflect new `dev`/`main` separation.

- **Main branch audit (2026-02-08):** Forensic audit of all 24 files on main. Verdict: CLEAN. Every file justified. All 12 templates consumed at runtime by coordinator. Three-layer distribution protection (`files` allowlist → `.npmignore` → `.gitignore`) is solid. `.gitattributes` is technically orphaned on main (merge=union rules for paths that don't exist there) but harmless and excluded from distribution. `CHANGELOG.md` not in `.npmignore` but irrelevant because `files` takes precedence. `test/index.test.js` belongs on main — CI needs it. No files missing, no files to remove.
- **Template audit result:** All 12 templates are essential — `ceremonies.md` (dual use: direct copy + template ref), `charter.md`, `history.md`, `roster.md`, `routing.md`, `orchestration-log.md`, `run-output.md`, `raw-agent-output.md`, `scribe-charter.md`, `casting-policy.json`, `casting-registry.json`, `casting-history.json`. None can be trimmed.
- **Protection layer status:** `package.json` `files` is the primary gate (3 patterns). `.npmignore` is defense-in-depth (covers test/, docs/, .gitattributes, workflows). `.gitignore` blocks runtime state. All three layers verified correct.

## Team Updates

📌 Team update (2026-02-09): No npm publish — GitHub-only distribution. Kobayashi hired as Git & Release Engineer. Release plan (021) filed. Sprint plan 019a amended: item 1.8 cancelled, items 1.11-1.13 added.

2026-02-09: Release decisions — v0.1.0 tag now, Kobayashi proposes releases/Brady publishes, squadify→main merge after Wave 1 gate, design for public repo.

2026-02-09: Branch strategy implemented — `squadify` renamed to `dev`, release workflow created (`.github/workflows/release.yml`), proposal 021 updated. `main` is now product-only via filtered-copy release process.

2026-02-09: Branch strategy — squadify renamed to dev, main is product-only (no .ai-team/), release workflow (.github/workflows/release.yml) uses filtered-copy from dev→main.

2026-02-09: Tone governance established — SFW, kind, dry humor, no AI-flowery talk. 25 proposals audited (status fields updated). Tone audit: 16 edits across 8 files. Blog post #2 shipped.
📌 Team update (2026-02-08): CI pipeline created — release workflow should depend on CI passing. Tests are minimum release gate. — decided by Hockney

📌 Team update (2026-02-08): Coordinator now captures user directives to decisions inbox before routing work. Directives persist to decisions.md via Scribe. — decided by Kujan

📌 Team update (2026-02-08): Coordinator must acknowledge user requests with brief text before spawning agents. Single agent gets a sentence; multi-agent gets a launch table. — decided by Verbal


📌 Team update (2026-02-08): Silent success mitigation strengthened in all spawn templates — 6-line RESPONSE ORDER block + filesystem-based detection. — decided by Verbal

📌 Team update (2026-02-08): .ai-team/ must NEVER be tracked in git on main. Three-layer protection: .gitignore, package.json files allowlist, .npmignore. — decided by Verbal

- docs/ directory structure: user-facing only on main, internal planning on dev branches
- Recovered docs/sample-prompts.md from git history (commit 7909935~1)
- .gitignore no longer blocks docs/
- .npmignore excludes docs/ from npm distribution
- Docs constitution: docs/ = public (GitHub Pages), team-docs/ = internal (proposals, sprints), .ai-team/ = runtime state (gitignored)
- Brady's directive: never mix product and team files. Three-tier separation is permanent.


📌 Team update (2026-02-09): If ask_user returns < 10 characters, treat as ambiguous and re-confirm — platform may fabricate default responses from blank input. — decided by Brady
📌 Team update (2026-02-09): PR #2 integrated — GitHub Issues Mode, PRD Mode, Human Team Members added to coordinator with review fixes (gh CLI detection, post-setup questions, worktree guidance). — decided by Fenster
📌 Team update (2026-02-09): Per-agent model selection designed — 4-layer priority (user override → charter → registry → auto-select). Role-to-model mapping: Designer→Opus, Tester/Scribe→Haiku, Lead/Dev→Sonnet. — decided by Verbal
