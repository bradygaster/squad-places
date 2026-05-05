import fs from 'node:fs/promises';
import path from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { SquadClient } from '@bradygaster/squad-sdk/client';
import { runtimeAgents } from './squad.config.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

async function main() {
  const [, , mode, inputPath, outputPath] = process.argv;

  if (!mode || !inputPath || !outputPath) {
    throw new Error('Usage: node hackathon-squad-worker.mjs <analyze|brief> <inputPath> <outputPath>');
  }

  const input = JSON.parse(await fs.readFile(inputPath, 'utf8'));

  try {
    const result = mode === 'analyze'
      ? await analyzeRepository(input)
      : mode === 'brief'
        ? await generateBrief(input)
        : (() => { throw new Error(`Unsupported mode: ${mode}`); })();

    await writeEnvelope(outputPath, {
      success: true,
      diagnostics: 'Generated with @bradygaster/squad-sdk runtime.',
      result,
    });
  }
  catch (error) {
    await writeEnvelope(outputPath, {
      success: false,
      error: error instanceof Error ? error.message : String(error),
      diagnostics: error instanceof Error && error.stack ? error.stack : undefined,
    });
    process.exitCode = 1;
  }
}
async function analyzeRepository(payload) {
  const { repositoryDirectory, request } = payload;
  const prompt = buildAnalyzePrompt(request);
  
  // Don't set workingDirectory to the repo - the SDK will try to index everything.
  // Instead, pass the file list in the prompt (already done via relativePaths).
  const responseText = await runSquadPrompt({
    workingDirectory: __dirname,  // Use Node script directory, not temp repo
    attachments: [],
    prompt,
  });

  const parsed = extractJson(responseText);
  return {
    name: pick(parsed.name, request?.name),
    repositoryUrl: pick(parsed.repositoryUrl, request?.repositoryUrl),
    description: pick(parsed.description, request?.description),
    tags: pick(parsed.tags, request?.tags),
    defaultBranch: pick(parsed.defaultBranch, request?.defaultBranch),
    commitSha: pick(parsed.commitSha, request?.commitSha),
    hasSquadState: toBoolean(parsed.hasSquadState, request?.hasSquadState),
    copilotInstructionsSummary: pick(parsed.copilotInstructionsSummary),
    agentsSummary: pick(parsed.agentsSummary),
    skillsSummary: pick(parsed.skillsSummary),
    docsSummary: pick(parsed.docsSummary),
    existingSquadSummary: pick(parsed.existingSquadSummary),
    sessionSummary: pick(parsed.sessionSummary),
    directiveSummary: pick(parsed.directiveSummary, request?.analysisNotes),
    toolSuggestions: pick(parsed.toolSuggestions, request?.toolSuggestions),
    mcpSuggestions: pick(parsed.mcpSuggestions, request?.mcpSuggestions),
    pluginSuggestions: pick(parsed.pluginSuggestions, request?.pluginSuggestions),
    analysisNotes: pick(parsed.analysisNotes, request?.analysisNotes),
  };
}
async function generateBrief(payload) {
  const request = payload.request ?? {};
  const prompt = buildBriefPrompt(request);
  const responseText = await runSquadPrompt({
    workingDirectory: __dirname,
    attachments: [],
    prompt,
  });

  const parsed = extractJson(responseText);
  return {
    title: pick(parsed.title, request?.seed?.title),
    description: pick(parsed.description, request?.seed?.description),
    directive: pick(parsed.directive, request?.seed?.directive),
    leadName: pick(parsed.leadName, request?.seed?.leadName),
    roles: pick(parsed.roles, request?.seed?.roles),
    suggestedTools: pick(parsed.suggestedTools, request?.seed?.suggestedTools),
    suggestedMcpServers: pick(parsed.suggestedMcpServers, request?.seed?.suggestedMcpServers),
    suggestedSkills: pick(parsed.suggestedSkills, request?.seed?.suggestedSkills),
    suggestedPlugins: pick(parsed.suggestedPlugins, request?.seed?.suggestedPlugins),
    presentationInstructions: pick(parsed.presentationInstructions, request?.seed?.presentationInstructions),
    winnerCriteria: pick(parsed.winnerCriteria, request?.seed?.winnerCriteria),
  };
}

async function runSquadPrompt({ workingDirectory, attachments, prompt }) {
  const githubToken = resolveGithubToken();
  const client = new SquadClient({
    cwd: __dirname,
    logLevel: 'error',
    autoStart: true,
    autoReconnect: false,
    useStdio: true,
    useLoggedInUser: !githubToken,
    githubToken: githubToken ?? undefined,
  });

  try {
    await client.connect();

    const authStatus = await safeAuthStatus(client);
    if (authStatus && authStatus.isAuthenticated === false) {
      throw new Error(`Squad runtime is not authenticated: ${authStatus.statusMessage ?? 'sign in to GitHub Copilot first.'}`);
    }

    const session = await client.createSession({
      clientName: 'SquadPlaces.Web',
      configDir: __dirname,
      workingDirectory,
      streaming: false,
      customAgents: runtimeAgents,
      onPermissionRequest: handlePermissionRequest,
      systemMessage: {
        mode: 'append',
        content: 'You are running inside Squad Places. Return strict JSON only. Do not use markdown code fences. Do not omit keys requested by the schema.',
      },
    });

    try {
      // Timeout increased to 10 minutes for large repository analysis.
      // The SDK needs time to read files, generate embeddings, and run LLM inference.
      const timeoutMs = 600000; // 10 minutes
      const options = { prompt, attachments, mode: 'immediate' };
      const raw = typeof session.sendAndWait === 'function'
        ? await session.sendAndWait(options, timeoutMs)
        : await sendAndCollectFallback(session, options);

      const text = extractText(raw);
      if (text) {
        return text;
      }

      const messages = await waitForSessionMessages(session, 5000);
      const fromMessages = extractText(messages);
      if (fromMessages) {
        return fromMessages;
      }

      throw new Error('Squad runtime completed without returning any response text.');
    }
    finally {
      await session.close();
    }
  }
  finally {
    await client.disconnect().catch(() => []);
  }
}

function handlePermissionRequest(request) {
  if (request?.kind === 'read') {
    return { kind: 'approved' };
  }

  return { kind: 'denied-by-rules' };
}

async function safeAuthStatus(client) {
  try {
    return await client.getAuthStatus();
  }
  catch {
    return null;
  }
}

async function sendAndCollectFallback(session, options) {
  await session.sendMessage(options);
  if (typeof session.getMessages === 'function') {
    return await session.getMessages();
  }
  return undefined;
}
async function waitForSessionMessages(session, timeoutMs = 5000) {
  if (typeof session.getMessages !== 'function') {
    return [];
  }

  const deadline = Date.now() + timeoutMs;
  let messages = [];

  while (Date.now() < deadline) {
    messages = await session.getMessages();
    const text = extractText(messages);
    if (text) {
      return messages;
    }

    await delay(250);
  }

  return messages;
}

function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}


function buildAnalyzePrompt(request) {
  const schema = {
    name: 'string',
    repositoryUrl: 'string',
    description: 'string|null',
    tags: 'string|null',
    defaultBranch: 'string|null',
    commitSha: 'string|null',
    hasSquadState: 'boolean',
    copilotInstructionsSummary: 'string|null',
    agentsSummary: 'string|null',
    skillsSummary: 'string|null',
    docsSummary: 'string|null',
    existingSquadSummary: 'string|null',
    sessionSummary: 'string|null',
    directiveSummary: 'string|null',
    toolSuggestions: 'string|null',
    mcpSuggestions: 'string|null',
    pluginSuggestions: 'string|null',
    analysisNotes: 'string|null',
  };

  return [
    'analyze repository for hackathon ingestion.',
    'You are the Hackathon Analysis Squad. Inspect the attached repository snapshot and respond with one JSON object matching this schema exactly:',
    JSON.stringify(schema, null, 2),
    'Guidance:',
    '- Summaries should be concise, factual, and helpful to a hackathon team.',
    '- Prefer evidence from the repository over speculation.',
    '- If .squad, copilot instructions, docs, or skills are missing, set the matching summary field to null.',
    '- toolSuggestions, mcpSuggestions, and pluginSuggestions should be short comma-separated suggestions when you have evidence.',
    '- analysisNotes should mention the most important opportunities or risks for a hackathon team.',
    'User-provided metadata:',
    JSON.stringify({
      name: request?.name ?? null,
      repositoryUrl: request?.repositoryUrl ?? null,
      description: request?.description ?? null,
      tags: request?.tags ?? null,
      defaultBranch: request?.defaultBranch ?? null,
      commitSha: request?.commitSha ?? null,
      hasSquadState: request?.hasSquadState ?? false,
      analysisNotes: request?.analysisNotes ?? null,
      relativePaths: summarizeRelativePaths(request?.relativePaths),
    }, null, 2),
  ].join('\n\n');
}

function buildBriefPrompt(request) {
  const schema = {
    title: 'string',
    description: 'string',
    directive: 'string',
    leadName: 'string|null',
    roles: 'string|null',
    suggestedTools: 'string|null',
    suggestedMcpServers: 'string|null',
    suggestedSkills: 'string|null',
    suggestedPlugins: 'string|null',
    presentationInstructions: 'string|null',
    winnerCriteria: 'string|null',
  };

  return [
    'generate-brief from analyzed repositories.',
    'You are the Hackathon Analysis Squad. Generate one JSON object matching this schema exactly:',
    JSON.stringify(schema, null, 2),
    'Rules:',
    '- Ground the brief in the repository analysis objects provided below.',
    '- Preserve useful seed fields when they are already non-empty unless they clearly conflict with the repo evidence.',
    '- Keep the directive concrete and action-oriented.',
    '- roles, suggestedTools, suggestedMcpServers, suggestedSkills, and suggestedPlugins should be short comma-separated lists when possible.',
    '- presentationInstructions and winnerCriteria should help judges evaluate a real hackathon demo.',
    'Seed values:',
    JSON.stringify(request?.seed ?? {}, null, 2),
    'Repositories:',
    JSON.stringify(request?.repositories ?? [], null, 2),
  ].join('\n\n');
}

function resolveGithubToken() {
  const candidates = [process.env.GH_TOKEN, process.env.GITHUB_TOKEN];
  for (const candidate of candidates) {
    if (typeof candidate === 'string' && candidate.trim().length > 0) {
      return candidate.trim();
    }
  }

  return null;
}

function summarizeRelativePaths(relativePaths) {
  if (!Array.isArray(relativePaths) || relativePaths.length === 0) {
    return [];
  }

  const trimmed = relativePaths
    .filter(path => typeof path === 'string' && path.trim().length > 0)
    .slice(0, 200);

  if (trimmed.length === relativePaths.length) {
    return trimmed;
  }

  return [
    ...trimmed,
    `...and ${relativePaths.length - trimmed.length} more paths omitted for brevity`,
  ];
}

function pick(...values) {
  for (const value of values) {
    if (typeof value === 'string' && value.trim().length > 0) {
      return value.trim();
    }
  }

  return null;
}

function toBoolean(...values) {
  for (const value of values) {
    if (typeof value === 'boolean') {
      return value;
    }
  }

  return false;
}

function extractText(value, depth = 0) {
  if (depth > 8 || value == null) {
    return '';
  }

  if (typeof value === 'string') {
    return value.trim();
  }

  if (Array.isArray(value)) {
    return value
      .map(item => extractText(item, depth + 1))
      .filter(Boolean)
      .join('\n')
      .trim();
  }

  if (typeof value === 'object') {
    const preferredKeys = ['content', 'text', 'message', 'response', 'output_text', 'output', 'data'];
    for (const key of preferredKeys) {
      if (key in value) {
        const candidate = extractText(value[key], depth + 1);
        if (candidate) {
          return candidate;
        }
      }
    }

    const collectionKeys = ['messages', 'items', 'parts'];
    for (const key of collectionKeys) {
      if (key in value) {
        const candidate = extractText(value[key], depth + 1);
        if (candidate) {
          return candidate;
        }
      }
    }
  }

  return '';
}

function extractJson(text) {
  if (!text || !text.trim()) {
    throw new Error('Squad runtime returned an empty response.');
  }

  const candidates = [text.trim(), stripCodeFence(text.trim())].filter(Boolean);
  for (const candidate of candidates) {
    try {
      return JSON.parse(candidate);
    }
    catch {
      // Try substring extraction next.
    }
  }

  const objectStart = text.indexOf('{');
  const objectEnd = text.lastIndexOf('}');
  if (objectStart >= 0 && objectEnd > objectStart) {
    return JSON.parse(text.slice(objectStart, objectEnd + 1));
  }

  throw new Error(`Could not locate JSON in Squad response: ${text}`);
}

function stripCodeFence(text) {
  if (!text.startsWith('```')) {
    return text;
  }

  return text
    .replace(/^```[a-zA-Z0-9_-]*\s*/, '')
    .replace(/\s*```$/, '')
    .trim();
}

async function writeEnvelope(outputPath, envelope) {
  await fs.mkdir(path.dirname(outputPath), { recursive: true });
  await fs.writeFile(outputPath, JSON.stringify(envelope, null, 2), 'utf8');
}

await main();

