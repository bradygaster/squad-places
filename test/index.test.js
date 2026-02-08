const { describe, it, beforeEach, afterEach } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('fs');
const path = require('path');
const os = require('os');
const { execSync } = require('child_process');

const ROOT = path.resolve(__dirname, '..');
const INDEX = path.join(ROOT, 'index.js');

function runInit(cwd) {
  return execSync(`node "${INDEX}"`, { cwd, encoding: 'utf8', env: { ...process.env } });
}

// --- copyRecursive unit tests ---

describe('copyRecursive', () => {
  let tmpDir;

  beforeEach(() => {
    tmpDir = fs.mkdtempSync(path.join(os.tmpdir(), 'squad-copy-'));
  });

  afterEach(() => {
    fs.rmSync(tmpDir, { recursive: true, force: true });
  });

  // We can't require copyRecursive directly (no module.exports), so we
  // replicate it here for isolated unit testing of the algorithm.
  function copyRecursive(src, target) {
    if (fs.statSync(src).isDirectory()) {
      fs.mkdirSync(target, { recursive: true });
      for (const entry of fs.readdirSync(src)) {
        copyRecursive(path.join(src, entry), path.join(target, entry));
      }
    } else {
      fs.mkdirSync(path.dirname(target), { recursive: true });
      fs.copyFileSync(src, target);
    }
  }

  it('copies a single file', () => {
    const src = path.join(tmpDir, 'src');
    const dest = path.join(tmpDir, 'dest');
    fs.mkdirSync(src);
    fs.writeFileSync(path.join(src, 'file.txt'), 'hello');

    copyRecursive(path.join(src, 'file.txt'), path.join(dest, 'file.txt'));
    assert.equal(fs.readFileSync(path.join(dest, 'file.txt'), 'utf8'), 'hello');
  });

  it('copies nested directories and preserves file contents', () => {
    const src = path.join(tmpDir, 'src');
    const dest = path.join(tmpDir, 'dest');

    // Create nested structure: src/a/b/deep.txt, src/root.md
    fs.mkdirSync(path.join(src, 'a', 'b'), { recursive: true });
    fs.writeFileSync(path.join(src, 'root.md'), '# Root');
    fs.writeFileSync(path.join(src, 'a', 'mid.json'), '{"key":"value"}');
    fs.writeFileSync(path.join(src, 'a', 'b', 'deep.txt'), 'deep content');

    copyRecursive(src, dest);

    assert.equal(fs.readFileSync(path.join(dest, 'root.md'), 'utf8'), '# Root');
    assert.equal(fs.readFileSync(path.join(dest, 'a', 'mid.json'), 'utf8'), '{"key":"value"}');
    assert.equal(fs.readFileSync(path.join(dest, 'a', 'b', 'deep.txt'), 'utf8'), 'deep content');
  });

  it('copies an empty directory', () => {
    const src = path.join(tmpDir, 'src');
    const dest = path.join(tmpDir, 'dest');
    fs.mkdirSync(src);

    copyRecursive(src, dest);
    assert.ok(fs.existsSync(dest));
    assert.equal(fs.readdirSync(dest).length, 0);
  });

  it('preserves binary file contents', () => {
    const src = path.join(tmpDir, 'src');
    const dest = path.join(tmpDir, 'dest');
    fs.mkdirSync(src);

    const buf = Buffer.from([0x00, 0x01, 0xFF, 0xFE, 0x89, 0x50]);
    fs.writeFileSync(path.join(src, 'bin.dat'), buf);

    copyRecursive(src, dest);
    const result = fs.readFileSync(path.join(dest, 'bin.dat'));
    assert.deepEqual(result, buf);
  });
});

// --- Init happy path ---

describe('init into empty directory', () => {
  let tmpDir;

  beforeEach(() => {
    tmpDir = fs.mkdtempSync(path.join(os.tmpdir(), 'squad-init-'));
  });

  afterEach(() => {
    fs.rmSync(tmpDir, { recursive: true, force: true });
  });

  it('creates .github/agents/squad.agent.md', () => {
    runInit(tmpDir);
    const agentFile = path.join(tmpDir, '.github', 'agents', 'squad.agent.md');
    assert.ok(fs.existsSync(agentFile), 'squad.agent.md should exist');

    // Content should match the source
    const expected = fs.readFileSync(path.join(ROOT, '.github', 'agents', 'squad.agent.md'), 'utf8');
    const actual = fs.readFileSync(agentFile, 'utf8');
    assert.equal(actual, expected);
  });

  it('creates .ai-team-templates/ with all template files', () => {
    runInit(tmpDir);
    const templatesDir = path.join(tmpDir, '.ai-team-templates');
    assert.ok(fs.existsSync(templatesDir), '.ai-team-templates/ should exist');

    // Every file in templates/ should be copied
    const sourceFiles = fs.readdirSync(path.join(ROOT, 'templates'));
    const destFiles = fs.readdirSync(templatesDir);
    assert.deepEqual(destFiles.sort(), sourceFiles.sort());

    // Spot-check: content matches for first template
    for (const file of sourceFiles) {
      const expected = fs.readFileSync(path.join(ROOT, 'templates', file), 'utf8');
      const actual = fs.readFileSync(path.join(templatesDir, file), 'utf8');
      assert.equal(actual, expected, `template ${file} content should match`);
    }
  });

  it('creates drop-box directories', () => {
    runInit(tmpDir);
    assert.ok(fs.existsSync(path.join(tmpDir, '.ai-team', 'decisions', 'inbox')),
      'decisions/inbox should exist');
    assert.ok(fs.existsSync(path.join(tmpDir, '.ai-team', 'orchestration-log')),
      'orchestration-log should exist');
    assert.ok(fs.existsSync(path.join(tmpDir, '.ai-team', 'casting')),
      'casting should exist');
  });

  it('outputs success messages', () => {
    const output = runInit(tmpDir);
    assert.ok(output.includes('squad.agent.md'), 'should mention squad.agent.md');
    assert.ok(output.includes('.ai-team-templates'), 'should mention templates');
    assert.ok(output.includes('Squad is ready'), 'should print ready message');
  });
});

// --- Re-init (idempotency) ---

describe('re-init into existing directory', () => {
  let tmpDir;

  beforeEach(() => {
    tmpDir = fs.mkdtempSync(path.join(os.tmpdir(), 'squad-reinit-'));
    // First init
    runInit(tmpDir);
  });

  afterEach(() => {
    fs.rmSync(tmpDir, { recursive: true, force: true });
  });

  it('skips squad.agent.md when it already exists', () => {
    // Modify the agent file so we can verify it's NOT overwritten
    const agentFile = path.join(tmpDir, '.github', 'agents', 'squad.agent.md');
    fs.writeFileSync(agentFile, 'user-customized content');

    const output = runInit(tmpDir);
    assert.ok(output.includes('already exists'), 'should report skipping');

    const content = fs.readFileSync(agentFile, 'utf8');
    assert.equal(content, 'user-customized content', 'should NOT overwrite user file');
  });

  it('skips .ai-team-templates/ when it already exists', () => {
    // Add a user file to templates dir
    const userFile = path.join(tmpDir, '.ai-team-templates', 'user-custom.md');
    fs.writeFileSync(userFile, 'custom content');

    const output = runInit(tmpDir);
    assert.ok(output.includes('already exists'), 'should report skipping templates');

    // User file should still be there
    assert.ok(fs.existsSync(userFile), 'user custom file should survive re-init');
  });

  it('drop-box directories still exist after re-init', () => {
    runInit(tmpDir);
    assert.ok(fs.existsSync(path.join(tmpDir, '.ai-team', 'decisions', 'inbox')));
    assert.ok(fs.existsSync(path.join(tmpDir, '.ai-team', 'orchestration-log')));
    assert.ok(fs.existsSync(path.join(tmpDir, '.ai-team', 'casting')));
  });

  it('does not corrupt existing drop-box contents', () => {
    // Put a file in inbox before re-init
    const inboxFile = path.join(tmpDir, '.ai-team', 'decisions', 'inbox', 'test-decision.md');
    fs.writeFileSync(inboxFile, '# Test Decision');

    runInit(tmpDir);

    assert.ok(fs.existsSync(inboxFile), 'inbox file should survive');
    assert.equal(fs.readFileSync(inboxFile, 'utf8'), '# Test Decision');
  });
});
