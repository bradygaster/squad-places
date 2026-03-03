# Decision: GitHub npx dual-distribution — not recommended

**By:** Rabin (Distribution)
**Date:** 2026-07
**Status:** Recommendation (pending Brady's call)

## Question

Can we support `npx github:bradygaster/squad` alongside the existing npm channel (`npx @bradygaster/squad-cli`)?

## Answer: Technically yes, practically no

### What it would take (3 changes)

1. Add `"bin": { "squad": "./cli.js" }` to root `package.json`
2. Add `"prepare": "npm run build"` to root `package.json` scripts
3. Update root `cli.js` to forward to CLI without deprecation notice

### Why I recommend against it

| Factor | npm channel | GitHub npx channel |
|--------|------------|-------------------|
| First-run speed | ~3 seconds (pre-built tarball) | ~30+ seconds (clone + install + build) |
| Download size | CLI + production deps only | Entire repo + ALL devDeps (TS, esbuild, Vitest, Playwright) |
| Caching | npm cache works | Git clone every time |
| Version pinning | `@0.8.18` (semver) | `#v0.8.18` (git ref, not semver-aware) |
| Build failures | Never (pre-built) | User-facing if anything in build chain breaks |
| Maintenance | Zero (npm publish handles it) | Must ensure `prepare` script stays working |
| Windows | Fully tested | Works (tsc-only build), but less tested path |

### The core problem

`npx github:` installs from source. It clones the repo, installs ALL dependencies (including devDependencies), runs a build, then executes. This is fundamentally slower and more fragile than downloading a pre-built tarball from npm.

The old beta worked because it was a single-file CLI with zero build step. The new monorepo with TypeScript compilation makes the GitHub path a significantly worse experience.

### Alternative: GitHub Packages registry

If having a "GitHub-branded" install matters, publish to GitHub Packages (npm registry hosted on GitHub). Same pre-built tarball, different registry URL. But requires auth tokens for consumers, which is worse than public npm.

## Recommendation

**Keep npm-only.** Aligns with existing team decision (2026-02-21). The GitHub channel adds maintenance burden for a strictly worse user experience. The error-only shim in root `cli.js` (already implemented) is the right approach for users hitting the old path.

## Who this affects

- **All team members:** No change needed. Continue using npm references in docs/examples.
- **Brady:** If the "cool URL" matters enough to accept the UX trade-offs, the 3 changes above would work. But I don't recommend it.
