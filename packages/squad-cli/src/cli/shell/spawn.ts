/**
 * Agent spawning — loads charters, builds prompts, and manages spawn lifecycle.
 *
 * Creates SDK sessions via SquadClient, sends the task, and streams the response.
 */

import { resolveSquad } from '@bradygaster/squad-sdk/resolution';
import { SquadClient } from '@bradygaster/squad-sdk/client';
import type { SquadSession } from '@bradygaster/squad-sdk/client';
import { SessionRegistry } from './sessions.js';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';

export interface SpawnOptions {
  /** Wait for completion (sync) or fire-and-track (background) */
  mode: 'sync' | 'background';
  /** Additional system prompt context */
  systemContext?: string;
  /** Tool definitions to register */
  tools?: ToolDefinition[];
  /** SquadClient instance for SDK session creation */
  client?: SquadClient;
  /** Working directory for the session */
  teamRoot?: string;
}

export interface ToolDefinition {
  name: string;
  description: string;
  parameters: Record<string, unknown>;
}

export interface SpawnResult {
  agentName: string;
  status: 'completed' | 'streaming' | 'error';
  response?: string;
  error?: string;
}

/**
 * Load agent charter from .squad/agents/{name}/charter.md
 */
export function loadAgentCharter(agentName: string, teamRoot?: string): string {
  const squadDir = teamRoot ? join(teamRoot, '.squad') : resolveSquad();
  if (!squadDir) {
    throw new Error('No .squad/ directory found. Run "squad init" first.');
  }
  const charterPath = join(squadDir, 'agents', agentName.toLowerCase(), 'charter.md');
  try {
    return readFileSync(charterPath, 'utf-8');
  } catch {
    throw new Error(`Charter not found for agent "${agentName}" at ${charterPath}`);
  }
}

/**
 * Build system prompt for an agent from their charter + optional context
 */
export function buildAgentPrompt(charter: string, options?: { systemContext?: string }): string {
  let prompt = `You are an AI agent on a software development team.\n\nYOUR CHARTER:\n${charter}`;
  if (options?.systemContext) {
    prompt += `\n\nADDITIONAL CONTEXT:\n${options.systemContext}`;
  }
  return prompt;
}

/**
 * Spawn an agent session.
 *
 * When a SquadClient is provided via options.client, creates a real SDK session,
 * sends the task, streams the response, and returns the accumulated result.
 * Without a client, returns a stub result for backward compatibility.
 */
export async function spawnAgent(
  name: string,
  task: string,
  registry: SessionRegistry,
  options: SpawnOptions = { mode: 'sync' }
): Promise<SpawnResult> {
  const teamRoot = options.teamRoot ?? process.cwd();
  const charter = loadAgentCharter(name, teamRoot);

  const roleMatch = charter.match(/^#\s+\w+\s+—\s+(.+)$/m);
  const role = roleMatch?.[1] ?? 'Agent';

  registry.register(name, role);
  registry.updateStatus(name, 'working');

  try {
    const systemPrompt = buildAgentPrompt(charter, { systemContext: options.systemContext });

    if (!options.client) {
      // No client provided — return stub for backward compatibility
      registry.updateStatus(name, 'idle');
      return {
        agentName: name,
        status: 'completed',
        response: `[Agent ${name} spawn ready — no client provided]`,
      };
    }

    const session: SquadSession = await options.client.createSession({
      streaming: true,
      systemMessage: { mode: 'append', content: systemPrompt },
      workingDirectory: teamRoot,
    });

    // Accumulate streamed response
    let accumulated = '';
    const onDelta = (event: { type: string; [key: string]: unknown }): void => {
      const val = event['delta'] ?? event['content'];
      if (typeof val === 'string') accumulated += val;
    };

    session.on('message_delta', onDelta);
    try {
      await session.sendMessage({ prompt: task });
    } finally {
      try { session.off('message_delta', onDelta); } catch { /* session may not support off */ }
    }

    try { await session.close(); } catch { /* best-effort cleanup */ }

    registry.updateStatus(name, 'idle');
    return {
      agentName: name,
      status: 'completed',
      response: accumulated || undefined,
    };
  } catch (error) {
    registry.updateStatus(name, 'error');
    return {
      agentName: name,
      status: 'error',
      error: error instanceof Error ? error.message : String(error),
    };
  }
}
