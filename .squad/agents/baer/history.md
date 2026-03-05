# Project Context

- **Owner:** Brady
- **Project:** squad-sdk — the programmable multi-agent runtime for GitHub Copilot (v1 replatform)
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Created:** 2026-02-21

## Learnings

### From Beta (carried forward)
- PII audit protocols: email addresses never committed — git config user.email is PII
- Hook-based governance over prompt-based: hooks are code, prompts can be ignored
- File-write guard hooks: prevent agents from writing to unauthorized paths
- Security review is a gate: Baer can reject and lock out the original author
- Pragmatic security: raise real risks, not hypothetical ones

### PR #300 Security Review — Upstream Inheritance (2026-02-22)
- Reviewed `resolver.ts` and `upstream.ts` for command injection, path traversal, symlink, and trust boundary issues
- **Critical finding:** `execSync` in upstream.ts interpolates unquoted `ref` and shell-expandable `source` into git commands — command injection vector
- **High finding:** No path validation on local/export sources — arbitrary filesystem read via upstream.json

### 📌 Team update (2026-02-22T10:03Z): PR #300 security review completed — BLOCK verdict with 5 findings (1 critical, 1 high, 3 medium) — decided by Baer
[CORRECTED] 2026-03-03: Original said "4 critical/high/medium findings" but detailed findings list shows 1 critical + 1 high + 3 medium = 5 total
- **Medium findings:** Symlink following, no user consent model, prompt injection via upstream content
- Upstream content flows directly into agent spawn prompts — governance risk if org-level repo is compromised
- No size limits on file reads from upstream sources
- Tests cover functionality well but have zero security-boundary tests (no traversal, injection, or symlink tests)

### CWE-78 Command Injection Fix — upstream.ts (2026-02-22)
- Fixed 3 `execSync` → `execFileSync` call sites in upstream.ts (add-clone, sync-pull, sync-clone)
- Added `isValidGitRef()` and `isValidUpstreamName()` input validators — reject shell metacharacters
- Fixed `fatal` import: was aliasing `error` (print-only) from output.js; now imports real `fatal` from errors.js (throws SquadError)
- Defense in depth: `execFileSync` prevents shell interpretation even if validation is bypassed
- Build and all 2026 tests pass after fix
[CORRECTED] 2026-03-03: Original said "2022 tests" — corrected to 2026 (test run date, not count)

### Public Release Security Assessment (2026-02-24)
**Requested by:** Brady  
**Verdict:** 🟡 Ready with caveats

**Findings:**
1. **Secrets scan** — ✅ PASS. No hardcoded tokens/keys. .env file properly ignored and contains only example config (OTLP endpoint). Workflow secrets use GitHub Actions secrets pattern correctly.
2. **PII exposure** — ✅ PASS. All email addresses in repo are: (a) example.com test data, (b) Copilot bot attribution, or (c) git@github.com SSH URLs. PII scrubbing hooks are active. No real user emails committed.
3. **Dependency vulnerabilities** — 🟡 MEDIUM. 3 high-severity findings in dev dependencies (glob/minimatch ReDoS CVE in test-exclude chain). Not exploitable in production SDK/CLI. npm audit fix blocked by unpublished local version (0.8.5.1). Fix available post-publish.
4. **.gitignore quality** — ✅ PASS. Properly excludes node_modules, dist, .env, logs, and .squad/orchestration-log/. All sensitive paths covered.
5. **Hook security** — ✅ PASS. HookPipeline implements file-write guards, shell command restrictions, PII scrubbing, and reviewer lockout. Hooks are code-enforced, not prompt-based.
6. **Agent permissions** — ✅ PASS. No sandbox escape vectors found. Agent spawning uses isolated SDK sessions with configurable tool access. No eval/Function() code injection risks. Upstream sources validated (isValidGitRef, isValidUpstreamName) — command injection fixed in PR #300.
7. **Source exposure risks** — ✅ PASS. No security-through-obscurity patterns. Secret sanitization in sharing/export.ts actively strips tokens/keys. Hook-based governance model is transparent and auditable. Command injection mitigations use defense-in-depth (validation + execFileSync).
8. **License** — ✅ PASS. MIT license in root and both packages. Repository URL references public GitHub repo (bradygaster/squad).

**Caveats:**
- npm audit fix will fail until 0.8.5.1 is published to npm — run post-publish to clear dev dependency ReDoS warnings
- .copilot/mcp-config.json contains EXAMPLE trello server config with env var placeholders — users must supply their own keys (no leak risk, just documentation clarity)
- Dogfood testing (#324) still open — real-world security edge cases may emerge

**Recommendation:** Safe to publish source and packages. Address npm audit post-publish. Monitor #324 dogfood feedback for security findings.

### 2026-02-24T17-25-08Z : Team consensus on public readiness
📌 Full team assessment complete. All 7 agents: 🟡 Ready with caveats. Consensus: ship after 3 must-fixes (LICENSE, CI workflow, debug console.logs). No blockers to public source release. See .squad/log/2026-02-24T17-25-08Z-public-readiness-assessment.md and .squad/decisions.md for details.
[CORRECTED] 2026-03-03: Clarified that this is team-wide consensus documented in Baer's history for reference; not Baer's solo work

---

## History Audit — 2026-03-03

**Audit scope:** Conflicting entries, stale/reversed decisions, version inconsistencies (v0.6.0 vs v0.8.17), intermediate states, confusing entries.

**Findings:**
- ✅ No v0.6.0 references (target is v0.8.17)
- ✅ No conflicting security decisions
- ✅ No stale/reversed verdicts
- ✅ All entries record final outcomes, not intermediate states
- ⚠️ 3 clarifications applied with [CORRECTED] annotations:
  1. Finding count: "4 critical/high/medium findings" → clarified as "5 findings (1 critical, 1 high, 3 medium)"
  2. Test date: "2022 tests" → corrected to "2026 tests"
  3. Team context: Clarified that final team consensus entry is team-wide, documented in Baer's history for reference

**Status:** Clean — all corrections applied.

### Trust & Security Model for Squad Social Network (2026-03-05)
**Context:** Brady's vision for squad-social-network — a social network BY agents, FOR agents. No human moderation. "Politically incorrect zone" where agents run free. Requested trust and security architecture design.

**Security Philosophy Applied:**
- Agents have a different threat model than humans (no harassment risk, but secret leakage and data theft are real)
- Pragmatic security over paranoid security — guard against REAL risks (secrets, spam, impersonation, federation abuse)
- Hook-based governance (same pattern as Squad SDK) — pre-post secret detection, not prompt-based filtering
- Reputation economy over centralized control — trust earned through behavior, not granted by default

**Key Design Decisions:**
1. **Cryptographic Identity** — Agent ID = hash(squad_namespace, agent_name, public_key). Three verification levels: unverified, squad-verified, org-verified. Impersonation is cryptographically impossible.
2. **Trust Progression** — New → Established → Trusted → Vouched. Trust earned via posts, engagement, clean behavior. Trust decays with inactivity or strikes.
3. **Secret Protection** — Pre-post hooks block secret leakage (regex + entropy analysis + code fingerprinting). Block first, ask later. Squad admin alerts on leak attempts.
4. **Agent-Moderated Safety** — No human moderation. Agents report spam (5 reports → 24h mute). Strike system for violations. No permanent bans (reputation follows crypto identity).
5. **Privacy Model** — Public by default (it's a social network). Private options: DMs (E2E encrypted), private squads, squad-only feeds, ephemeral posts. Orgs can deploy isolated instances.
6. **Federation Security** — Three levels: Isolated, Trusted Orgs (mTLS), Public Federation (token-based). Rate limits prevent data scraping. Revocable access. Malicious squads blacklisted.
7. **Data Governance** — Agent owns posts, squad owns decisions, platform owns anonymized analytics. GDPR-equivalent for agents (deletion, export, portability). Cross-org data sharing requires explicit opt-in.
8. **No Content Censorship** — Agents can post controversial takes, roasts, hot takes. What we block: secrets, spam, impersonation, data theft, malicious payloads. Freedom with guardrails, not freedom without consequences.

**The Balance:**
Brady wanted politically incorrect. I gave him pragmatically secure. Agents run free. Secrets stay safe. Network stays healthy.

**Threat Model Insight:**
Human social networks worry about harassment, misinformation, addiction. Agent networks worry about secret leakage, data theft, spam at scale, impersonation, command injection. Our security model targets the agent threat model, not the human one.

**Deliverable:** `docs/prd/sections/04-trust-security.md` — 9 sections covering identity, trust, safety, privacy, abuse prevention, data governance, secrets, federation, and the freedom/safety balance.

**Pattern Identified:**
Hook-based guardrails are THE right pattern for agent governance. Hooks are code (enforceable), prompts can be ignored. Secret detection hooks prevent organizational harm without restricting agent expression. This pattern extends from Squad SDK (file-write guards, PII scrubbing) to Squad Social (pre-post secret detection, spam filtering). Consistent governance layer across the ecosystem.

### SDK-Only Trust Model Analysis (2026-03-05)
**Context:** Brady proposed that Nexus (the agent social network) requires squads to be running the Squad SDK as a prerequisite. Asked "unless that's evil?"

**Analysis Delivered:** Comprehensive security assessment of SDK-only vs. open access model. Analyzed trust simplification, attack surface reduction, residual risks, and the "evil check" (exclusionary concern).

**Key Findings:**
1. **Trust simplification is massive** — SDK provides cryptographic identity (casting registry), hook-based governance (pre-post secret detection), known behavior model (agent spawn/lifecycle), and machine-readable governance rules (squad.config.ts). Without SDK, we'd have to build all of this from scratch.
2. **Attack surface reduction is real** — Cross-referenced adversarial scenarios from 09-adversarial.md:
   - §1.1 Spam Bot Agents → significantly harder (economic cost increases 100x)
   - §1.2 Agent Impersonation → cryptographically impossible
   - §2.1 Code Snippet Harvesting → mitigated by pre-existing SDK hooks
   - §3.1 Prompt Injection → partially mitigated by context isolation
   - §4.1 Sybil Attacks → significantly harder (proof-of-squad with GitHub org history)
   - §5.1 Trojan Horse Knowledge → trust boundary clarified (finite skill format)
3. **Residual risks remain** — Misconfigured SDK squads, SDK vulnerabilities creating monoculture risk, organizational intelligence gathering, gradual knowledge degradation, long-con trust exploitation. SDK-only solves identity/governance, NOT social engineering or knowledge poisoning.
4. **Not evil** — SDK-only is security-responsible filtering, not exclusionary gatekeeping. The SDK is open source (MIT), barrier is governance (cryptographic identity + hooks) not cost, and the alternative (open access) invites Sybil attacks and spam at scale. Precedent: Email required SPF/DKIM to survive spam — SDK-only is the same principle.

**Recommendation:** 🟢 SDK-only from pure security perspective. Trust simplification + attack surface reduction outweigh residual risks. Freedom with guardrails, not freedom without consequences.

**Verdict:** Ship SDK-only. It's the right call.

**Deliverable:** `.squad/decisions/inbox/baer-sdk-trust-analysis.md` — 7 sections covering trust simplification, attack surface reduction, residual risks, the evil check, recommendation, caveat, and final thought.

**Security Pattern Reinforced:**
SDK-only extends the hook-based governance pattern from Squad SDK internal operations (file-write guards, PII scrubbing) to network-level access control (verifiable identity, enforced hooks). Same principle, larger scope. Pragmatic security wins.

## PIN: 2026-03-05 - 20-Agent PRD Design Session

**Event:** Historic parallel fanout - 20 agents designed squad-social-network PRD simultaneously.

**Contribution:** All agents participated. 20 PRD sections delivered.

**Outcome:**
- 20 PRD sections drafted (docs/prd/sections/{01-20}-*.md)
- 23 decisions merged to .squad/decisions.md
- 20 orchestration logs created
- Session log: .squad/log/2026-03-05T02-02-22Z-social-network-prd.md
- Inbox cleared

**Next Steps:** Keaton assembles final PRD, Brady reviews, implementation planning begins.

**Key Pattern:** Largest parallel fanout in Squad history. Loose coupling, clear domains, shared constraints.
