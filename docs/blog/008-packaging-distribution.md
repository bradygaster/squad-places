# How Squad Ships: From TypeScript to Your Terminal

> **⚠️ INTERNAL ONLY — DO NOT PUBLISH**

**Milestone:** M4 · **Issue:** #51 (M4-14)
**Date:** Sprint 4

---

## The Distribution Challenge

A multi-agent runtime is only useful if it reaches developers. M4 builds the pipeline that takes Squad from TypeScript source to installable package — covering bundling, npm packaging, GitHub releases, CI/CD generation, and self-update.

## Bundle Strategy

`BundleBuilder` wraps esbuild with Squad-aware defaults. The configuration externalizes `@github/copilot-sdk` (it's a peer dependency), tree-shakes unused modules, and produces a single ESM entry point. Bundle metadata — size, entry path, external list — is captured as a `BundleResult` so downstream steps can validate output before publish.

The format is deliberately simple: one entry, ESM only, optional source maps. Squad bundles don't need the complexity of multi-format builds because consumers always run inside a Copilot extension host that speaks ESM natively.

## npm Packaging

`NpmPackageBuilder` generates a publish-ready `package.json` from `SquadConfig`. Agent roles map to npm keywords for discoverability. The file manifest includes `dist/`, the config manifest, README, and icon. Required-field validation blocks the pipeline early — a missing description or invalid version fails at build time, not on `npm publish`.

## GitHub Distribution

Not every team uses npm. `GitHubDistBuilder` prepares release artifacts for GitHub Releases: auto-generated release notes from the config changelog, the `.squad-pkg` archive attached as a release asset, and semver tagging. Teams that prefer GitHub over npm get first-class support without extra tooling.

## CI/CD Pipeline Generation

`CiPipelineBuilder` generates GitHub Actions workflows from `SquadConfig`. The generated pipeline covers build → bundle → validate → package → publish as discrete steps. Each step is a composable function, so teams can customize the workflow or run steps locally. The zero-dependency scaffolding from `npx create-squad` is preserved — generated workflows use only actions available in public GitHub Actions.

## Upgrade CLI

The `squad upgrade` command checks the installed version against the registry, downloads the latest compatible release, and applies it in-place. Version pinning is respected: if a project locks to `0.6.x`, the CLI won't jump to `0.7.0`. The `ReleaseChannel` type supports `stable`, `preview`, and `insider` tracks.

Offline detection is built in. If the registry is unreachable, the check is queued for the next connection via `OfflineManager`. The `--dry-run` flag lets teams preview what would change without applying it.

## In-Copilot Installation

`detectCopilotEnvironment()` identifies the host — CLI, VS Code, or web — and provides platform-specific scaffolding instructions. `installInCopilot()` wires into `initSquad()` from M2 to create a full project without leaving the Copilot chat interface. The result includes the list of created files and next-step guidance tailored to the detected environment.

## Versioning

`VersionManager` handles semver bumping, tag creation, and changelog generation. It reads the current version from config, applies the requested bump type (major/minor/patch/prerelease), and writes back. Changelog entries are derived from commit history when available, or from manual descriptions.
