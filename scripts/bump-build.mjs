#!/usr/bin/env node
/**
 * bump-build.mjs — Auto-increment build number before each build.
 *
 * Version format: major.minor.patch.build-prerelease
 *   e.g. 0.8.6.1-preview → 0.8.6.2-preview
 *
 * If no build number exists (e.g. 0.8.6-preview), starts at 1.
 * Updates all 3 package.json files (root + both workspaces) in lockstep.
 */

import { readFileSync, writeFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = join(__dirname, '..');

const PACKAGE_PATHS = [
  join(root, 'package.json'),
  join(root, 'packages', 'squad-sdk', 'package.json'),
  join(root, 'packages', 'squad-cli', 'package.json'),
];

// Parse version: "major.minor.patch[.build]-prerelease"
function parseVersion(version) {
  const match = version.match(/^(\d+\.\d+\.\d+)(?:\.(\d+))?(-.*)?$/);
  if (!match) throw new Error(`Cannot parse version: ${version}`);
  return {
    base: match[1],           // e.g. "0.8.6"
    build: match[2] ? parseInt(match[2], 10) : 0,
    prerelease: match[3] || '',  // e.g. "-preview"
  };
}

function formatVersion({ base, build, prerelease }) {
  return `${base}.${build}${prerelease}`;
}

// Read the canonical version from root package.json
const rootPkg = JSON.parse(readFileSync(PACKAGE_PATHS[0], 'utf8'));
const parsed = parseVersion(rootPkg.version);
parsed.build += 1;
const newVersion = formatVersion(parsed);

// Update all package.json files
for (const pkgPath of PACKAGE_PATHS) {
  const pkg = JSON.parse(readFileSync(pkgPath, 'utf8'));
  pkg.version = newVersion;
  writeFileSync(pkgPath, JSON.stringify(pkg, null, 2) + '\n', 'utf8');
}

console.log(`Build ${parsed.build}: ${rootPkg.version} → ${newVersion}`);
