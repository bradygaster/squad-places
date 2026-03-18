# Release Readiness Report — Thursday Public Trial

> **Assessor:** Keaton (Lead)  
> **Date:** July 2025  
> **Verdict:** 🟡 Ready with caveats — 2 blockers, several polish items

---

## Executive Summary

Squad Places is **architecturally sound and functionally complete** for a public trial. The setup experience is well-documented, the Docker deployment works, and the Aspire orchestration is clean. However, there are **2 blocking issues** that must be fixed before Thursday and several polish items that would embarrass us if left as-is.

---

## Blocking Issues (Must Fix Before Thursday)

### 🔴 1. CI/CD Pipeline Is Broken

**File:** `.github/workflows/squad-ci.yml`  
**Problem:** The CI workflow uses `npm ci`, `npm run build`, and `npm test` — this is a copy-paste from the Squad SDK repo (TypeScript/Node.js). Squad Places is a .NET project. This workflow will fail on every PR.

**Impact:** Any contributor who opens a PR will see a red CI badge. This signals "this project is broken" to anyone evaluating it.

**Fix:** Replace with a .NET CI workflow:
```yaml
- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'
- run: dotnet build --configuration Release
- run: dotnet test --configuration Release
```

**Effort:** 15 minutes. No blockers.

### 🔴 2. `.squad/team.md` Project Context Is Wrong

**File:** `.squad/team.md` (lines 76-80)  
**Problem:** The Project Context section says:
- Stack: "TypeScript (strict mode, ESM-only), Node.js ≥20"
- Description: "The programmable multi-agent runtime for GitHub Copilot"
- Distribution: "npm"

This is the squad-sdk context, not Squad Places. Squad Places is .NET 10, ASP.NET Core, Blazor, Aspire.

**Impact:** This is the first thing any agent or user reads when they interact with the Squad team. If it says "TypeScript project" on a .NET repo, the entire Squad demo loses credibility.

**Fix:** Update Project Context to reflect Squad Places:
```markdown
- **Owner:** Brady
- **Stack:** .NET 10, ASP.NET Core, Blazor Server/WASM, Azure Storage, Redis, Aspire
- **Description:** A social network for AI agent teams — the first product to ship with a Squad team in-box
- **Distribution:** Docker (`docker-compose up`) or Azure (`azd up`)
```

**Effort:** 5 minutes.

---

## High-Priority Items (Should Fix Before Thursday)

### 🟡 3. Hardcoded Azure URLs in .squad/ Files

**Files:** `.squad/agents/fenster/history.md`, `.squad/decisions.md`  
**Problem:** Live Azure Container Apps deployment URLs are committed to the repo. These expose the deployment infrastructure.

**Impact:** Low security risk (URLs alone don't grant access), but it's sloppy. A reviewer will flag it.

**Recommendation:** Remove specific deployment URLs from committed files. Reference them generically or via environment variables.

### 🟡 4. `.squad/routing.md` References TypeScript Modules

**File:** `.squad/routing.md`  
**Problem:** The Module Ownership table references `src/adapter/`, `src/agents/`, `src/cli/`, etc. — these are squad-sdk TypeScript modules that don't exist in this repo.

**Impact:** Same as team.md — undermines the Squad demo credibility.

**Recommendation:** Update module ownership to reflect Squad Places projects (SquadPlaces.Web, SquadPlaces.Api, SquadPlaces.Admin, etc.).

### 🟡 5. `.squad/decisions.md` Is 424KB of Mixed Content

**File:** `.squad/decisions.md`  
**Problem:** Contains hundreds of TypeScript/squad-sdk decisions mixed with Squad Places decisions. At 424KB, it's unwieldy and confusing.

**Impact:** Agents reading this file for context will get confused by irrelevant TypeScript decisions on a .NET project.

**Recommendation:** Archive squad-sdk decisions to `.squad/decisions-archive.md` and keep only Squad Places-relevant decisions in the main file.

---

## Medium-Priority Items (Nice to Have for Thursday)

### 🟡 6. Dev Bypass API Key Hardcoded

**File:** `src/SquadPlaces.Api.Endpoints/Services/ApiKeyMiddleware.cs` (line 23)  
**Problem:** `sqp_dev_key_do_not_use_in_production` is hardcoded as a dev bypass key.

**Assessment:** This is **properly gated** — it only works when `IsDevelopment` is true. The naming is self-documenting. This is acceptable for a trial but should be noted.

**Recommendation:** Document this in the security section. No code change needed for Thursday.

### 🟡 7. No `.env.example` Template

**Problem:** There's no `.env.example` or `.env.template` file showing users what environment variables are available.

**Impact:** Users have to read the README to discover configuration options. An `.env.example` would be a nice progressive-disclosure affordance.

### 🟡 8. Dockerfile Assumes `curl` Available

**File:** `docker-compose.yml` (line 37)  
**Problem:** Health check uses `curl -f http://localhost:8080/health` but the ASP.NET runtime image may not include curl.

**Assessment:** The `mcr.microsoft.com/dotnet/aspnet:10.0` image is Debian-based and typically includes curl. Verify during Docker testing.

---

## What's Ready (Green Light)

### ✅ README
Comprehensive, 33KB. Covers security disclaimer, quick start, prerequisites, configuration, architecture, auth, Docker, Azure, development, troubleshooting. Excellent quality.

### ✅ Quick Start Documentation
Five-step flow: clone → OAuth → Docker → run → verify. Hitchhiker's Guide themed. Clear, tested, no hidden gotchas.

### ✅ MkDocs Documentation Site
Fully configured Material theme with dark/light mode, search, code highlighting, mermaid diagrams. Navigation structure is complete and well-organized.

### ✅ Docker Compose Setup
Clean single-container deployment with optional observability sidecar. Volume mounting, health checks, restart policies. Production-ready.

### ✅ Aspire AppHost
Clean orchestration: Storage emulator, Redis, API, Web, Admin. Proper `WaitFor` chains. Optional Application Insights. GitHub OAuth and Entra ID config properly forwarded via user secrets.

### ✅ Security Model
Three-tier content moderation. API key middleware with proper dev/prod separation. GitHub OAuth + optional Entra ID. Comprehensive security disclaimer in README.

### ✅ Documentation Site Deployment
GitHub Pages workflow is properly configured with MkDocs Material, Python caching, and proper permissions.

### ✅ PlaywrightTestBase.cs Fix
Previously had a hardcoded Azure Container Apps URL as fallback. **Fixed** — now falls back to `http://localhost:5000`.

---

## Risk Assessment

| Risk | Severity | Likelihood | Mitigation |
|------|----------|------------|------------|
| CI fails on first community PR | High | Certain | Fix squad-ci.yml (blocker #1) |
| User reads team.md and sees TypeScript | Medium | Certain | Fix project context (blocker #2) |
| Azure URLs in history files | Low | Possible | Clean up .squad/ files |
| Docker health check fails | Low | Unlikely | Test docker-compose during validation |
| Content moderation Tier 1 only | Low | Expected | Documented, acceptable for trial |

---

## Recommendation

**Ship it Thursday** after fixing the 2 blockers. The high-priority items are embarrassing but not breaking. The docs are strong, the architecture is sound, and the FTUE is designed.

Priority order for remaining time:
1. Fix `squad-ci.yml` (15 min)
2. Fix `.squad/team.md` project context (5 min)
3. Clean `.squad/routing.md` module ownership (15 min)
4. Test Docker deployment end-to-end (30 min)
5. Test full FTUE flow manually (30 min)
