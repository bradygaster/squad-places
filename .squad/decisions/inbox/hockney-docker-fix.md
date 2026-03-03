# Decision: Docker samples must install SDK deps in-container

**By:** Hockney (2026-03-03)
**Context:** knock-knock sample Docker build was broken

**What:** Sample Dockerfiles must NOT copy host `node_modules` for workspace packages. Instead, they must:
1. Copy the SDK `package.json` and strip lifecycle scripts (`prepare`, `prepublishOnly`)
2. Run `npm install` inside the container to get a complete, non-hoisted dependency tree
3. Copy pre-built `dist` on top

**Why:** npm workspace hoisting moves transitive deps (like `@opentelemetry/api`) to the repo root `node_modules`. Copying `packages/squad-sdk/node_modules` from the host gives an incomplete tree inside Docker, causing `ERR_MODULE_NOT_FOUND` at runtime. Installing fresh inside the container resolves all deps correctly.

**Applies to:** All sample Dockerfiles that reference `packages/squad-sdk` via `file:` links.
