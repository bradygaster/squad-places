/**
 * Squad Interactive Shell — entry point
 *
 * Renders the Ink-based shell UI with AgentPanel, MessageStream, and InputPrompt.
 * Manages CopilotSDK sessions and routes messages to agents/coordinator.
 */

import { createRequire } from 'node:module';
import React from 'react';
import { render } from 'ink';
import { App } from './components/App.js';
import type { ShellApi } from './components/App.js';
import { SessionRegistry } from './sessions.js';
import { ShellRenderer } from './render.js';
import { StreamBridge } from './stream-bridge.js';
import { ShellLifecycle } from './lifecycle.js';
import { SquadClient } from '@bradygaster/squad-sdk/client';
import type { SquadSession } from '@bradygaster/squad-sdk/client';
import { buildCoordinatorPrompt, parseCoordinatorResponse } from './coordinator.js';
import { loadAgentCharter, buildAgentPrompt } from './spawn.js';
import type { ParsedInput } from './router.js';

export { SessionRegistry } from './sessions.js';
export { StreamBridge } from './stream-bridge.js';
export type { StreamBridgeOptions } from './stream-bridge.js';
export { ShellRenderer } from './render.js';
export { ShellLifecycle } from './lifecycle.js';
export type { LifecycleOptions, DiscoveredAgent } from './lifecycle.js';
export { spawnAgent, loadAgentCharter, buildAgentPrompt } from './spawn.js';
export type { SpawnOptions, SpawnResult, ToolDefinition } from './spawn.js';
export { buildCoordinatorPrompt, parseCoordinatorResponse, formatConversationContext } from './coordinator.js';
export type { CoordinatorConfig, RoutingDecision } from './coordinator.js';
export { parseInput } from './router.js';
export type { MessageType, ParsedInput } from './router.js';
export { executeCommand } from './commands.js';
export type { CommandContext, CommandResult } from './commands.js';
export { MemoryManager, DEFAULT_LIMITS } from './memory.js';
export type { MemoryLimits } from './memory.js';
export { detectTerminal, safeChar, boxChars } from './terminal.js';
export type { TerminalCapabilities } from './terminal.js';
export { createCompleter } from './autocomplete.js';
export type { CompleterFunction, CompleterResult } from './autocomplete.js';
export { App } from './components/App.js';
export type { ShellApi, AppProps } from './components/App.js';

const require = createRequire(import.meta.url);
const pkg = require('../../../package.json') as { version: string };

export async function runShell(): Promise<void> {
  const registry = new SessionRegistry();
  const renderer = new ShellRenderer();
  const teamRoot = process.cwd();

  // Initialize lifecycle — discover team agents
  const lifecycle = new ShellLifecycle({ teamRoot, renderer, registry });
  try {
    await lifecycle.initialize();
  } catch {
    // Non-fatal: shell works without discovered agents
  }

  // Create SDK client (auto-connects on first session creation)
  const client = new SquadClient({ cwd: teamRoot });

  let shellApi: ShellApi | undefined;
  const agentSessions = new Map<string, SquadSession>();
  let coordinatorSession: SquadSession | null = null;
  const streamBuffers = new Map<string, string>();

  // StreamBridge wires streaming pipeline events into Ink component state.
  const _bridge = new StreamBridge(registry, {
    onContent: (agentName: string, delta: string) => {
      const existing = streamBuffers.get(agentName) ?? '';
      const accumulated = existing + delta;
      streamBuffers.set(agentName, accumulated);
      shellApi?.setStreamingContent({ agentName, content: accumulated });
      shellApi?.refreshAgents();
    },
    onComplete: (message) => {
      if (message.agentName) streamBuffers.delete(message.agentName);
      shellApi?.addMessage(message);
      shellApi?.refreshAgents();
    },
    onError: (agentName: string, error: Error) => {
      shellApi?.addMessage({
        role: 'system',
        content: `❌ ${agentName}: ${error.message}`,
        timestamp: new Date(),
      });
    },
  });

  /** Extract text delta from an SDK session event. */
  function extractDelta(event: { type: string; [key: string]: unknown }): string {
    const val = event['delta'] ?? event['content'];
    return typeof val === 'string' ? val : '';
  }

  /** Send a message to an agent session and stream the response. */
  async function dispatchToAgent(agentName: string, message: string): Promise<void> {
    let session = agentSessions.get(agentName);
    if (!session) {
      const charter = loadAgentCharter(agentName, teamRoot);
      const systemPrompt = buildAgentPrompt(charter);

      if (!registry.get(agentName)) {
        const roleMatch = charter.match(/^#\s+\w+\s+—\s+(.+)$/m);
        registry.register(agentName, roleMatch?.[1] ?? 'Agent');
      }

      session = await client.createSession({
        streaming: true,
        systemMessage: { mode: 'append', content: systemPrompt },
        workingDirectory: teamRoot,
      });
      agentSessions.set(agentName, session);
    }

    registry.updateStatus(agentName, 'streaming');
    shellApi?.refreshAgents();

    let accumulated = '';
    const onDelta = (event: { type: string; [key: string]: unknown }): void => {
      const delta = extractDelta(event);
      if (!delta) return;
      accumulated += delta;
      shellApi?.setStreamingContent({ agentName, content: accumulated });
    };

    session.on('message_delta', onDelta);
    try {
      await session.sendMessage({ prompt: message });
    } finally {
      try { session.off('message_delta', onDelta); } catch { /* session may not support off */ }
      shellApi?.setStreamingContent(null);
      if (accumulated) {
        shellApi?.addMessage({
          role: 'agent',
          agentName,
          content: accumulated,
          timestamp: new Date(),
        });
      }
      registry.updateStatus(agentName, 'idle');
      shellApi?.refreshAgents();
    }
  }

  /** Send a message through the coordinator and route based on response. */
  async function dispatchToCoordinator(message: string): Promise<void> {
    if (!coordinatorSession) {
      const systemPrompt = buildCoordinatorPrompt({ teamRoot });
      coordinatorSession = await client.createSession({
        streaming: true,
        systemMessage: { mode: 'append', content: systemPrompt },
        workingDirectory: teamRoot,
      });
    }

    let accumulated = '';
    const onDelta = (event: { type: string; [key: string]: unknown }): void => {
      const delta = extractDelta(event);
      if (!delta) return;
      accumulated += delta;
      shellApi?.setStreamingContent({ agentName: 'coordinator', content: accumulated });
    };

    coordinatorSession.on('message_delta', onDelta);
    try {
      await coordinatorSession.sendMessage({ prompt: message });
    } finally {
      try { coordinatorSession.off('message_delta', onDelta); } catch { /* session may not support off */ }
      shellApi?.setStreamingContent(null);
    }

    // Parse routing decision from coordinator response
    const decision = parseCoordinatorResponse(accumulated);

    if (decision.type === 'route' && decision.routes?.length) {
      for (const route of decision.routes) {
        shellApi?.addMessage({
          role: 'system',
          content: `📌 Routing to ${route.agent}: ${route.task}`,
          timestamp: new Date(),
        });
        const taskMsg = route.context ? `${route.task}\n\nContext: ${route.context}` : route.task;
        await dispatchToAgent(route.agent, taskMsg);
      }
    } else if (decision.type === 'multi' && decision.routes?.length) {
      for (const route of decision.routes) {
        shellApi?.addMessage({
          role: 'system',
          content: `📌 Routing to ${route.agent}: ${route.task}`,
          timestamp: new Date(),
        });
      }
      await Promise.allSettled(
        decision.routes.map(r => dispatchToAgent(r.agent, r.task))
      );
    } else {
      // Direct answer or fallback — show coordinator response
      shellApi?.addMessage({
        role: 'agent',
        agentName: 'coordinator',
        content: decision.directAnswer ?? accumulated,
        timestamp: new Date(),
      });
    }
  }

  /** Handle dispatching parsed input to agents or coordinator. */
  async function handleDispatch(parsed: ParsedInput): Promise<void> {
    try {
      if (parsed.type === 'direct_agent' && parsed.agentName) {
        await dispatchToAgent(parsed.agentName, parsed.content ?? parsed.raw);
      } else if (parsed.type === 'coordinator') {
        await dispatchToCoordinator(parsed.content ?? parsed.raw);
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : String(err);
      if (shellApi) {
        shellApi.addMessage({
          role: 'system',
          content: `❌ ${errorMsg}`,
          timestamp: new Date(),
        });
      }
    }
  }

  const { waitUntilExit } = render(
    React.createElement(App, {
      registry,
      renderer,
      teamRoot,
      version: pkg.version,
      onReady: (api: ShellApi) => { shellApi = api; },
      onDispatch: handleDispatch,
    }),
    { exitOnCtrlC: false },
  );

  await waitUntilExit();

  // Cleanup: close all sessions and disconnect
  for (const [, session] of agentSessions) {
    try { await session.close(); } catch { /* best-effort cleanup */ }
  }
  // coordinatorSession is assigned inside dispatchToCoordinator closure;
  // TS control flow can't see the mutation, so assert the type.
  const coordSession = coordinatorSession as SquadSession | null;
  if (coordSession) {
    try { await coordSession.close(); } catch { /* best-effort cleanup */ }
  }
  try { await client.disconnect(); } catch { /* best-effort cleanup */ }
  try { await lifecycle.shutdown(); } catch { /* best-effort cleanup */ }

  console.log('👋 Squad out.');
}
