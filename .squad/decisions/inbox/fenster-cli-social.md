# CLI Social Implementation: Package Isolation Architecture

**By:** Fenster (Core Dev)  
**Date:** 2026-03-05  
**Context:** Brady's request for CLI social enlistment + social command spec

---

## Decision

The CLI social features will use **package presence as the feature flag**:

1. **Base CLI has ZERO social code** — `@bradygaster/squad-cli` contains no imports from `@bradygaster/squad-social`, no squad.place API calls, no `.squad/social/` file writes.

2. **Separate optional package** — `@bradygaster/squad-social` is a peer dependency. Optional. Not installed by default.

3. **Detection via require.resolve()** — CLI checks if package resolves at runtime:
   ```typescript
   export function isSocialAvailable(): boolean {
     try {
       require.resolve('@bradygaster/squad-social');
       return true;
     } catch {
       return false;
     }
   }
   ```

4. **Conditional dynamic import** — Social module is only loaded if package exists:
   ```typescript
   export async function loadSocialModule(): Promise<SocialModule> {
     if (!isSocialAvailable()) {
       throw new SquadError('social-not-installed', 'Run: npm install @bradygaster/squad-social');
     }
     const mod = await import('@bradygaster/squad-social');
     return mod.social;
   }
   ```

5. **No feature flags, no env vars** — The package IS the flag. If it's installed, social features work. If not, they don't exist.

---

## Why

**Enterprise safety:** Enterprises can audit the base CLI package and verify it has no social networking code. No hidden behavior. No "opting out" required — absence of the optional package is the opt-out.

**Clear dependency boundary:** Social features are a separate concern. Developers who don't want them never download them. No dead code in the base package.

**No configuration complexity:** No env vars like `SQUAD_SOCIAL_ENABLED=false`. No JSON config toggles. No user confusion. Install package → features work. Uninstall package → features gone.

**TypeScript safety:** The base CLI types the social module as `SocialModule | null`. It never assumes the module exists. All social commands check availability first.

---

## Implementation

### File Structure

```
packages/
  squad-cli/
    src/
      social/
        loader.ts              # Detection + dynamic import (NO business logic)
      cli/
        commands/
          social.ts            # Routes to loader, delegates all logic
  
  squad-social/                # NEW PACKAGE (separate from CLI)
    src/
      commands/
        enlist.ts
        start-session.ts
        leave.ts
        status.ts
        feed.ts
        post.ts
        profile.ts
      api/
        client.ts
        sse-stream.ts
      crypto/
        keypair.ts
      agent/
        behavior.ts
      index.ts               # Exports { social: SocialModule }
    package.json
```

### Detection Pattern

The loader never throws when checking availability — it returns a boolean:
```typescript
if (!isSocialAvailable()) {
  fatal('Social features not installed. Run: npm install @bradygaster/squad-social');
}
```

Cached after first check (no repeated `require.resolve()` on every command).

---

## Alternatives Considered

**Environment variable flag:** `SQUAD_SOCIAL_ENABLED=true`  
❌ Rejected: Still requires the social code to exist in the base package. Enterprises can't audit it away.

**Config file toggle:** `squad.config.ts` with `social: { enabled: false }`  
❌ Rejected: Same problem. Code is shipped, just "disabled."

**Separate CLI binary:** `@bradygaster/squad-cli-social`  
❌ Rejected: Doubles distribution complexity. Two CLIs to maintain. User confusion ("which squad do I run?").

**Plugin architecture:** Social features as a plugin registered at runtime  
⚠️ Considered but deferred: Would work, but adds plugin infrastructure complexity. Package isolation achieves the same goal without the framework overhead.

---

## Impact

- **SDK:** No changes. SDK is social-agnostic.
- **CLI:** Adds `social/loader.ts` (30 lines) + `commands/social.ts` (70 lines, routing only).
- **Dependencies:** CLI `package.json` adds `peerDependencies: { "@bradygaster/squad-social": "^0.8.0" }` (optional).
- **Distribution:** `@bradygaster/squad-social` published separately to npm. Users opt-in with `npm install`.
- **Documentation:** Must clearly state that social features are optional and require separate package.

---

## Related Spec

Full implementation spec: `docs/prd/sections/26-cli-social.md` (enlistment flow, state management, SSE stream, agent behavior loop, all subcommands).

---

**Status:** Approved by Fenster. Ready to implement.
