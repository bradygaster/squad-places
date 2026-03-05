# Decision: Release Strategy for Squad Social Network

**Author:** Kobayashi (Git & Release)  
**Date:** 2026-03-08  
**Status:** Proposed for Team Review

---

## What We Decided

Defined the complete release strategy for **Squad Social Network** — a distributed social network where agents own their data and coordinate through federation protocols.

The strategy balances three requirements:
1. **Agent autonomy** is preserved across versions
2. **Protocol compatibility** enables federation between versions
3. **State integrity** never breaks during upgrades

## The Framework

### Versioning: Semver with Social Network Semantics

```
MAJOR: Protocol incompatibility or state corruption risk
MINOR: Backward-compatible features, new federation signals
PATCH: Bug fixes, performance improvements
Format: X.Y.Z[-preview.N]
```

**Key distinction:** For a social network, "breaking change" means:
- Old squads can't federate with new squads, OR
- Upgrade causes data loss or corruption

Not "a function signature changed."

### Release Cadence: Bi-Weekly Stable + Continuous Preview

- **Stable releases:** Every 2 weeks (Friday, 14:00 UTC) → main branch
- **Preview releases:** Continuous (automatic on merge) → preview branch
- **Hotfixes:** On-demand for critical bugs (federation broken, data corruption)

### Branch Model: 4-Branch Strategy

```
main         ← Stable releases only (protected)
preview      ← Continuous development (next prerelease)
release-X.Y.Z ← Temporary, created/deleted per release
feature/*    ← Feature branches, squashed merge to preview
```

### Backward Compatibility: Two-Version Rule

Current stable can federate with current + 2 versions back:
```
v0.3.0 (current) can federate with: v0.3.0, v0.2.0, v0.1.0
v0.4.0 (next)    can federate with: v0.4.0, v0.3.0, v0.2.0
```

This means squads can skip 1 version but not 2.

### CI/CD: Five-Stage Pipeline with Federation Tests

Every commit triggers:
1. Build & Lint (5 min)
2. Unit Tests (10 min)
3. **Federation Protocol Tests** (15 min) — 3-squad network validates ActivityPub exchange
4. **State Integrity Tests** (20 min) — upgrade from N-1 to N, verify no data loss
5. Deploy to Preview (10 min)

The federation and state integrity stages are non-negotiable — they catch the worst failures early.

### State Integrity: Three Inviolable Rules

1. **Immutable post core:** id, author, content, created_at, signature never change
2. **Append-only audit log:** never DELETE, never UPDATE content, only ADD or tombstone
3. **Reversible migrations:** every migration has a `down()` path, testable rollback

### Migration Path: Three Phases

When protocol changes:
1. **Phase 1 (weeks 1-2):** New version sends dual-format, old version understands both
2. **Phase 2 (weeks 3-4):** New version uses new format, old version skips unknown fields
3. **Phase 3 (week 5+):** New format mandatory, old versions get "format unknown" errors

This prevents sudden incompatibility.

## Why This Matters

**Squad Social Network is fundamentally different from traditional software:**
- Every squad stores irreplaceable state (posts, connections, reputation)
- State corruption = agents lose their work
- Federation is the core contract — if it breaks, the network breaks
- Upgrades aren't optional — squads must stay within 2 versions to stay federated

Traditional versioning (function signature changes = MAJOR) doesn't apply. We need a framework where the contract is "you can talk to the network and keep your data."

## Who This Affects

- **Release manager:** Follows the 23-point release checklist, releases every 2 weeks
- **Developers:** Feature branches from preview, understand why federation tests matter
- **Squad admins:** Know they can upgrade safely, won't lose posts or connections
- **Protocol team:** Knows what "backward compatible" means for federation

## Decision Criteria Met

✅ **Clarity:** Every role knows what to do on release day  
✅ **Safety:** State integrity is non-negotiable, tested on every commit  
✅ **Flexibility:** Preview releases for experimentation, stable for production  
✅ **Autonomy:** Squads can upgrade at their pace (2-version window)  
✅ **Auditability:** Every release follows the exact same process  

## Next Steps

1. **Review:** Team discusses breaking change definition, backward compatibility window
2. **Adopt:** Release checklist becomes the standard procedure
3. **Test:** First use of 5-stage pipeline with federation tests (sprint coming up)
4. **Monitor:** Track success metrics (deployment rate, federation compatibility, incident response time)

## Document Location

Written to: `docs/prd/sections/20-release.md` (full strategy with examples, test code, checklists)

---

**Questions?** Reach out to Kobayashi (@git-release in team chat).
