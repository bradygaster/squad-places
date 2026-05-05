import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const projectRoot = path.resolve(__dirname, '..');
const packageRoot = path.join(projectRoot, 'node_modules', 'vscode-jsonrpc');
const shimPath = path.join(packageRoot, 'node');
const shimContent = "module.exports = require('./lib/node/main.js');\n";

async function main() {
  try {
    await fs.access(packageRoot);
  }
  catch {
    console.warn('[postinstall] vscode-jsonrpc not found; skipping shim generation.');
    return;
  }

  await fs.writeFile(shimPath, shimContent, 'utf8');
  console.log('[postinstall] Wrote vscode-jsonrpc/node compatibility shim for Node ESM resolution.');
}

await main();
