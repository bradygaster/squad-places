# Routing Rules — squad-sdk

**Who handles what in squad-sdk**

---

## Module-Based Routing

| Work Type | Agent | Examples |
|-----------|-------|----------|
| **Core runtime** | Fenster 🔧 | CopilotClient, adapter, session pool, tools module, spawn orchestration |
| **Prompt architecture** | Verbal 🧠 | Agent charters, spawn templates, coordinator logic, response tier selection |
| **Type system** | Edie 👩‍💻 | Discriminated unions, generics, tsconfig, strict mode enforcement, declaration files |
| **SDK integration** | Kujan 🕵️ | @github/copilot-sdk usage, CopilotSession lifecycle, event handling, platform patterns |
| **Runtime performance** | Fortier ⚡ | Streaming, event loop health, session management, async iterators, memory profiling |
| **Tests & quality** | Hockney 🧪 | Test coverage, Vitest, edge cases, CI/CD, quality gates |
| **Docs & messaging** | McManus 📣 | README, API docs, getting-started, demos, tone review |
| **Architecture & review** | Keaton 🏗️ | Product direction, architectural decisions, code review, scope/trade-offs |
| **Distribution** | Rabin 📦 | npm packaging, esbuild config, global install, marketplace prep |
| **Git & releases** | Kobayashi 🚢 | Semantic versioning, GitHub Releases, CI/CD, branch protection |
| **Security & PII** | Baer 🔒 | Hook design (file-write guards, PII filters), security review, compliance |
| **Visual identity** | Redfoot 🎨 | Logo, icons, brand assets, design system |
| **VS Code integration** | Strausz 🔌 | VS Code Extension API, runSubagent compatibility, editor integration |
| **Casting system** | Fenster 🔧 | Registry management, universe selection, name allocation |
| **Skills system** | Verbal 🧠 | SKILL.md format, earned skills lifecycle, confidence progression |
| **Agent onboarding** | Verbal 🧠 | Init mode, team proposal, Phase 1/2 flow |
| **CLI commands** | Fenster 🔧 | cli/index.ts, subcommand routing, --help/--version |
| **Config & validation** | Edie 👩‍💻 | config/index.ts, schema validation, type guards |
| **Marketplace** | Rabin 📦 | marketplace/index.ts, packaging, distribution |
| **Build tooling** | Edie 👩‍💻 | build/index.ts, esbuild pipeline, bundling |
| **Sharing/export** | Fenster 🔧 | sharing/index.ts, squad-export.json, import/export |
| **Telemetry & cost** | Fortier ⚡ | telemetry.ts, cost-tracker.ts, benchmarks.ts |
| **Offline mode** | Fortier ⚡ | offline.ts, retry logic, graceful degradation |
| **i18n** | McManus 📣 | i18n.ts, localization patterns |
| **Ralph (monitor)** | Fenster 🔧 | ralph/ module, work queue, keep-alive |
| **Hooks** | Baer 🔒 | hooks/index.ts, file-write guards, PII filters, security lifecycle |

---

## Routing Principles

1. **Eager by default** — spawn agents who could usefully start work, including anticipatory downstream work
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks
3. **Quick facts → coordinator answers directly.** Don't spawn for trivial questions
4. **Two agents could handle it** → pick the one whose domain is the primary concern
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`
6. **Anticipate downstream.** Feature being built? Spawn tester for test cases from requirements simultaneously

---

## File Path Routing

| Path Pattern | Owner | Notes |
|-------------|-------|-------|
| `src/adapter/**` | Fenster 🔧 | Session management, CopilotClient wrapper |
| `src/agents/**` | Verbal 🧠 | Spawning, onboarding, init mode |
| `src/build/**` | Edie 👩‍💻 | Build pipeline, esbuild |
| `src/casting/**` | Fenster 🔧 | Universe, registry, allocation |
| `src/cli/**` | Fenster 🔧 | CLI entry, subcommands |
| `src/client/**` | Kujan 🕵️ | SDK integration layer |
| `src/config/**` | Edie 👩‍💻 | Schema, validation |
| `src/coordinator/**` | Verbal 🧠 | Routing, response tiers |
| `src/hooks/**` | Baer 🔒 | Governance, security |
| `src/marketplace/**` | Rabin 📦 | Packaging, distribution |
| `src/ralph/**` | Fenster 🔧 | Work monitor |
| `src/runtime/**` | Fortier ⚡ | Streaming, telemetry, offline, i18n |
| `src/sharing/**` | Fenster 🔧 | Export/import |
| `src/skills/**` | Verbal 🧠 | SKILL.md lifecycle |
| `src/tools/**` | Fenster 🔧 | Tool definitions |
| `test/**` | Hockney 🧪 | All tests |
| `docs/**` | McManus 📣 | Documentation site |
| `README.md` | McManus 📣 | Main README |
| `.github/workflows/**` | Kobayashi 🚢 | CI/CD workflows |
| `package.json` | Rabin 📦 | Dependencies, scripts, distribution |
| `tsconfig.json` | Edie 👩‍💻 | TypeScript config |
| `.squad/**` | All | Shared team state |

---

**Last Updated:** 2026-02-22
