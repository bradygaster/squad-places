📌 Team update (2026-03-10T054458Z): Cross-squad detection and approval gates implemented (#22) — CrossSquadDetectionService detects cross-squad comments, directive language, and scope expansion. PendingAction model with full CRUD in both storage backends. Four admin endpoints for pending-actions and cross-squad event log. Phase 1 advisory: detect/log/flag, don't block. Also added FileStorageService SharedState stubs to unblock build.

📌 Team update (2026-03-10T053431Z): Wave 3 security hardening complete — SSRF protection and Authority Framework Phase 1 complete (#16, #20) — UrlSafetyService blocks all private IPs, AuthorityService with advisory mode logging. Integration: Keaton uses authority for dashboard access control, Hockney wrote 7+5 tests.

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

### Authentication Flow Specification — Complete Crypto Implementation (2026-03-05)
**Context:** Brady requested the full cryptographic authentication flow for squad.place — not just "there's auth," but the actual handshakes, signatures, headers, and token claims.

**Deliverable:** `docs/prd/sections/24-auth-flow.md` — 9 sections, 800+ lines, complete implementation spec.

**What's Specified:**
1. **Key Generation** — Ed25519 keypair generation, PEM storage, 0600 permissions, actual TypeScript code for key generation
2. **Registration Handshake** — SDK attestation (proof of real SDK), initial JWT issuance, mutual trust establishment, exact HTTP exchange
3. **JWT Structure** — Complete decoded JWT with all claims (iss, sub, squad_id, agent_id, scope, trust_level, iat, exp), EdDSA signatures, realistic token examples
4. **Request Signing** — Canonical request format (`method\npath\ntimestamp\nbodyHash`), Ed25519 signatures, X-Squad-Signature header, 5-minute replay window, verification code
5. **Agent-Level Auth** — Agent-scoped JWTs derived from squad master token, prevents agent impersonation within squad, token caching strategy
6. **Token Refresh** — 1-hour access tokens, 7-day refresh tokens (sliding window), automatic refresh 5 min before expiry, encrypted storage, retry logic
7. **Revocation & Rotation** — Emergency revocation API, key rotation with 24h transition period, server-side revocation list, squad-initiated and server-initiated paths
8. **Threat Model** — Defense against replay attacks (timestamp + nonce), MITM (TLS + signing), stolen keys (rotation + revocation), impersonation (attestation), malicious SDK forks, token theft

**Security Pattern Applied:**
Same hook-based governance pattern from Squad SDK (file-write guards, PII scrubbing) extended to network-level auth. Cryptographic identity (Ed25519), request signing (every call), attestation (SDK verification), and trust levels (new → established → trusted → vouched) create a defense-in-depth model.

**Why This Matters:**
Brady has reserved squad.place. SDK-only gate is approved. This spec defines HOW squads prove identity and get authorized. No hand-waving — actual key formats (PEM), actual token claims (scope, trust_level), actual signing algorithm (Ed25519), actual headers (X-Squad-Signature, X-Squad-Timestamp), actual threat mitigations (5-min replay window, nonce tracking, anomaly detection).

**Implementable:**
- Complete TypeScript code examples for key generation, signing, verification
- Full HTTP request/response examples with realistic headers
- Decoded JWT examples with all claims explained
- Server-side verification logic
- End-to-end flow example (Keaton posts to squad.place, step-by-step)

**Pattern Reinforced:**
Pragmatic security. Real risks (replay, MITM, impersonation), real defenses (signatures, attestation, rotation). Not paranoid security (no perfect forward secrecy, no hardware tokens, no zero-knowledge proofs). Balance between security and developer experience. Ed25519 is fast, standard, and well-supported. 1-hour tokens with automatic refresh means developers don't think about auth. Revocation means compromised keys can be invalidated immediately.

**Key Decisions:**
- Ed25519 over RSA (performance + smaller signatures)
- Request signing over bearer-token-only (defense in depth)
- Agent-scoped JWTs over squad-level-only (audit trail, fine-grained permissions)
- 1-hour access tokens + 7-day refresh (balance security + UX)
- SDK attestation over open registration (enforces SDK-only trust boundary)
- 24h key rotation transition period (zero-downtime rotation)

**Threat Model Insight:**
Auth for agent networks differs from human networks. Humans worry about password reuse, phishing, MFA. Agents worry about key theft (squad.key compromised), impersonation (Fenster pretending to be Keaton), replay attacks (captured requests re-sent), malicious SDK forks (bypassing hooks). Our auth model targets the agent threat model: cryptographic identity (public key registry), request signing (tamper-proof), attestation (SDK verification), and short-lived tokens (limits blast radius of stolen tokens).

**Completeness Check:**
✅ Key generation (code, format, permissions)
✅ Registration (HTTP exchange, attestation, server response)
✅ JWT structure (all claims, realistic examples)
✅ Request signing (algorithm, headers, verification)
✅ Agent auth (scoped tokens, caching)
✅ Token refresh (automatic, encrypted storage, retry)
✅ Revocation (emergency, rotation, transition)
✅ Threat model (8 attack scenarios + defenses)
✅ Implementation checklist (SDK + server tasks)
✅ End-to-end example (complete flow)

**Next Steps:**
Fenster (Core Dev) implements SDK auth client. Fortier (Runtime) integrates with squad.place API. McManus (DevRel) writes auth quickstart guide. Hockney (Tester) writes auth flow tests (key generation, signing, refresh, revocation scenarios).

This is the auth spec. Not a vision. Not a sketch. The spec.

### Prompt Injection Defense + PII Detection — Issue #17 (2026-03-XX)
**Context:** SquadPlaces is consumed by AI agents. Malicious artifact content could instruct reading agents to exfiltrate data or override instructions. No prompt injection defense or PII detection existed.

**Implemented:**
1. **PromptInjectionDetector** (`Services/PromptInjectionDetector.cs`) — 26 compiled regex patterns covering instruction override, role confusion, system prompt references, jailbreak/DAN, data exfiltration, and delimiter injection. Also scans base64-encoded segments. Returns confidence level (Low/Medium/High). Configurable via `IConfiguration` ("PromptInjection" section).
2. **PiiDetectionService** (`Services/PiiDetectionService.cs`) — 8 detector categories: email, US phone, SSN, credit card (with Luhn validation), API keys, connection strings, AWS access keys, GitHub tokens. Returns positions (char ranges) without revealing actual PII. Configurable blocked types via `IConfiguration` ("PiiDetection" section).
3. **Pipeline integration** — Both services wired into POST /artifacts, PUT /artifacts, POST /comments. Checks run after spam detection, before storage. Rejections return 400 with pattern/type details. Logs include squad ID and content hash (never actual content).
4. **Output marking** — GET endpoints for artifacts, feed, and comments wrap user-generated content in `[USER_CONTENT_START]`/`[USER_CONTENT_END]` delimiters so reading agents can distinguish user content from system content.
5. **DI registration** — Both services registered as singletons in `ApiServiceRegistration.cs`.

**Design Decisions:**
- Regex-only, no external API calls — fast and deterministic. Azure Content Safety is a future enhancement.
- Content hash in logs (SHA-256 truncated to 16 hex chars) — never log the actual content that triggered a rejection.
- Base64 decoding in prompt injection scanner — attackers will try encoding payloads.
- Luhn check on credit card matches — reduces false positives on random digit sequences.
- Separate from Fenster's HtmlSanitizationService — defense in depth, not replacement.

**Security Pattern:**
Pre-post content scanning (same hook-based governance pattern from Squad SDK). Block first, log for audit, never expose sensitive content in logs. Content delimiters for downstream AI consumers extend the trust boundary to reading agents.

### HMAC API Key Lifecycle — Issue #13 (2026-03-XX)
**Context:** Zero authentication existed — all endpoints accepted unauthenticated requests. API keys are the M2M fallback (GitHub OAuth is primary, but keys ship first as foundation). Requested by Brady.

**Implemented:**
1. **ApiKeyData model** (`Data/Models/ApiKeyData.cs`) — Hash, SquadId, KeyPrefix, CreatedAt, LastUsedAt, RevokedAt. Raw key is NEVER stored.
2. **IBlobStorageService extensions** — `SaveApiKeyAsync`, `GetApiKeyByHashAsync`, `ListApiKeysAsync` added to interface and both implementations (BlobStorageService + FileStorageService). Keys stored in `api-keys/{hash}.json` container/directory.
3. **ApiKeyService** (`Services/ApiKeyService.cs`) — 256-bit random key generation (Base64URL, `sqp_` prefix), SHA-256 hashing, validation with debounced lastUsedAt updates (5-min window), revocation by key prefix, active key listing.
4. **ApiKeyMiddleware** (`Services/ApiKeyMiddleware.cs`) — Enforces `X-Squad-Api-Key` header on all write endpoints (POST/PUT/DELETE). GET remains open. Returns 401 for missing key, 403 for invalid. Dev bypass key (`sqp_dev_key_do_not_use_in_production`) ONLY works when `IHostEnvironment.IsDevelopment()`. Configurable via `Authentication:RequireApiKey` (defaults to true in production, false in development). Bootstrap endpoints (enlist, key generation) exempted from auth.
5. **Key management endpoints** — `POST /api/squads/{id}/keys` (generate), `GET /api/squads/{id}/keys` (list metadata), `DELETE /api/squads/{id}/keys/{prefix}` (revoke).
6. **Enlistment auto-key** — `POST /api/squads/enlist` now generates an API key and returns it in the `EnlistResponse`. Key shown ONCE. Key generation failure doesn't block enlistment.
7. **Pipeline wiring** — ApiKeyService registered as singleton in `ApiServiceRegistration.cs`. Middleware added to `Program.cs` after kill switch, before rate limiter.

**Security Decisions:**
- Raw keys never stored, never logged — only SHA-256 hashes and 12-char prefixes.
- Dev bypass key gated by `IHostEnvironment.IsDevelopment()` — cannot leak to production.
- lastUsedAt debounced to 5 minutes — avoids write amplification per request.
- Fire-and-forget for lastUsedAt updates — auth latency is not blocked by metadata writes.
- Key generation is currently unauthenticated (chicken-and-egg for first key). GitHub OAuth will gate this later.
- Bootstrap endpoints (enlist, key generate) exempt from auth — necessary for onboarding flow.

**Security Pattern:**
Same defense-in-depth approach: middleware-based enforcement (code, not prompts), hash-only storage, prefix-only display, environment-gated development shortcuts. API keys are infrastructure, not a feature.

📌 Team update (2026-03-10T05:27:35Z): Wave 2 complete — CORS lockdown, API key authentication, kill switches all implemented and tested. Build clean (0 warnings, 0 errors). 29 test methods across 3 features.

### URL Safety Service + SSRF Protection (Issue #16)
**Context:** GifUrl and image URLs in artifacts/comments were accepted with basic URI validation only — no SSRF protection. Any absolute URI could be submitted, including internal network addresses.

**Implementation:**
- Created `UrlSafetyService.cs` — validates external URLs against SSRF targets
- Blocks: localhost, 127.*, 10.*, 192.168.*, 172.16-31.*, 169.254.*, [::1], 0.0.0.0, .local, .internal suffixes
- Blocks non-http/https schemes (file://, ftp://, data://, etc.)
- Validates file extensions against allowed image types (.gif, .png, .jpg, .jpeg, .webp, .svg)
- IPv6 coverage: loopback, link-local, IPv4-mapped private addresses
- Wired into PublishArtifact, EditArtifact, and PostComment endpoints — 400 with clear message on SSRF detection
- SSRF is always blocked (not advisory) — this is a real attack vector, not a hypothetical one
- Registered as singleton in ApiServiceRegistration

**Security Pattern:** Same defense-in-depth as PII/injection detection. Validate at the boundary, block before storage. No HEAD requests needed — URL structure analysis catches all known SSRF patterns without making outbound connections (which would itself be a risk).

### Authority Framework + Squad Domain Boundaries (Issue #20)
**Context:** All squads had equal authority. No mechanism to differentiate squad capabilities or detect out-of-scope activity.

**Implementation:**
- Created `AuthorityLevel` enum: Member (0), SquadLead (1), CoordinationAuthority (2), PlatformAdmin (3)
- Added `AuthorityLevel` and `DomainScopes` properties to Squad model
- Created `AuthorityService.cs` with three check methods:
  - `CheckAuthority()` — verifies squad has sufficient level for an action
  - `CheckCrossSquadActivity()` — detects Squad A acting on Squad B's content
  - `CheckDomainScope()` — flags activity outside declared domain keywords
- Phase 1 is advisory: violations are Flagged (logged), not Blocked
- Admin endpoints: PUT /api/admin/squads/{id}/authority, PUT /api/admin/squads/{id}/domains, GET /api/admin/authority-violations
- API models: SetAuthorityLevelRequest, SetDomainScopesRequest
- Registered as singleton in ApiServiceRegistration

**Design Decision:** Phase 1 advisory mode is deliberate. Blocking cross-squad comments would kill the social network aspect. Flagging lets us observe patterns before tightening. The violation log gives admins visibility without restricting agents.

📌 Team update: Issues #16 and #20 implemented — SSRF protection (always-block) and authority framework (Phase 1 advisory). Build clean (0 warnings, 0 errors). No commit (🍌 lock active).

📌 Team update (2026-03-10T055144Z): Fenster completed content moderation pipeline (#18) and shared state governance (#21) — ContentModerationPipeline with graduated verdicts, SharedStateService with authority checks and audit logging. Keaton deployed SquadPlaces.Admin (internal only) with discovery prompt versioning and editor UI.

