/**
 * Coordinator Orchestrator (PRD 5)
 *
 * The central orchestration engine. Replaces the 32KB squad.agent.md
 * prompt-based coordinator with a TypeScript program that manages
 * agent sessions, routes work, and observes progress via SDK events.
 */

// --- M3-1 Coordinator ---
export {
  SquadCoordinator,
  type SquadCoordinatorOptions,
  type CoordinatorResult,
  type SpawnStrategy,
  type CoordinatorContext,
} from './coordinator.js';

// --- M3-6 Direct Response ---
export {
  DirectResponseHandler,
  type DirectResponseResult,
  type DirectResponseCategory,
  type DirectResponsePattern,
} from './direct-response.js';

// --- M1-10 Fan-Out ---
export {
  spawnParallel,
  aggregateSessionEvents,
  type AgentSpawnConfig,
  type SpawnResult,
  type FanOutDependencies,
} from './fan-out.js';

// --- M3-4 Response Tiers ---
export {
  selectResponseTier,
  getTier,
  type TierName,
  type TierContext,
  type ModelTierSuggestion,
} from './response-tiers.js';

// --- Legacy types (kept for backwards compat) ---

import type { SquadClient, SquadSessionConfig } from '../client/index.js';
import type { EventBus } from '../runtime/event-bus.js';
import type { AgentSessionManager, AgentCharter } from '../agents/index.js';
import type { HookPipeline } from '../hooks/index.js';
import type { ToolRegistry } from '../tools/index.js';

// --- Routing Types ---

export type ResponseTier = 'direct' | 'lightweight' | 'standard' | 'full';

export interface RoutingDecision {
  /** Tier of response based on complexity */
  tier: ResponseTier;
  /** Target agent(s) for this request */
  agents: string[];
  /** Whether agents should run in parallel */
  parallel: boolean;
  /** Routing rationale (for observability) */
  rationale: string;
}

export interface CoordinatorConfig {
  /** Path to the team root (.squad/ directory) */
  teamRoot: string;
  /** Default model for routing decisions */
  model?: string;
  /** Enable parallel fan-out for multi-agent tasks */
  enableParallel?: boolean;
}

// --- Coordinator ---

export interface CoordinatorDeps {
  client?: SquadClient;
  eventBus?: EventBus;
  agentManager?: AgentSessionManager;
  hookPipeline?: HookPipeline;
  toolRegistry?: ToolRegistry;
}

export class Coordinator {
  private client: SquadClient | null = null;
  private eventBus: EventBus | null = null;
  private agentManager: AgentSessionManager | null = null;
  private hookPipeline: HookPipeline | null = null;
  private toolRegistry: ToolRegistry | null = null;
  private config: CoordinatorConfig;
  private initialized = false;
  private unsubscribers: (() => void)[] = [];

  constructor(config: CoordinatorConfig, deps?: CoordinatorDeps) {
    this.config = config;
    if (deps) {
      this.client = deps.client ?? null;
      this.eventBus = deps.eventBus ?? null;
      this.agentManager = deps.agentManager ?? null;
      this.hookPipeline = deps.hookPipeline ?? null;
      this.toolRegistry = deps.toolRegistry ?? null;
    }
  }

  /** Initialize the coordinator: wire up event subscriptions and mark ready */
  async initialize(): Promise<void> {
    if (this.eventBus) {
      this.unsubscribers.push(
        this.eventBus.subscribe('session:created', (event) => {
          console.log(`[Coordinator] Session created: ${event.sessionId ?? 'unknown'}`);
        }),
      );
      this.unsubscribers.push(
        this.eventBus.subscribe('session:error', (event) => {
          console.log(`[Coordinator] Session error: ${event.sessionId ?? 'unknown'}`);
        }),
      );
      this.unsubscribers.push(
        this.eventBus.subscribe('session:destroyed', (event) => {
          console.log(`[Coordinator] Session destroyed: ${event.sessionId ?? 'unknown'}`);
        }),
      );
    }
    this.initialized = true;
  }

  /** Route an incoming user message to the appropriate agent(s) */
  async route(message: string): Promise<RoutingDecision> {
    const lower = message.toLowerCase().trim();

    // Direct response: status queries, factual questions
    if (/^(status|help|what is|who is|how many|show|list)\b/.test(lower)) {
      return {
        tier: 'direct',
        agents: [],
        parallel: false,
        rationale: `Direct response for informational query: "${message}"`,
      };
    }

    // Agent name mention — route to that specific agent
    const agentMention = lower.match(/@(\w+)/);
    if (agentMention) {
      return {
        tier: 'standard',
        agents: [agentMention[1]!],
        parallel: false,
        rationale: `Routed to mentioned agent: ${agentMention[1]!}`,
      };
    }

    // Team-wide task — fan-out to all agents
    if (/\bteam\b/.test(lower)) {
      return {
        tier: 'full',
        agents: ['all'],
        parallel: true,
        rationale: 'Team-wide task detected — fan-out to all agents',
      };
    }

    // Default: route to lead agent (Keaton)
    return {
      tier: 'standard',
      agents: ['lead'],
      parallel: false,
      rationale: 'Default routing to lead agent (Keaton)',
    };
  }

  /** Execute a routing decision: emit routing event on EventBus */
  async execute(decision: RoutingDecision, message: string): Promise<void> {
    if (this.eventBus) {
      await this.eventBus.emit({
        type: 'coordinator:routing',
        payload: { decision, message },
        timestamp: new Date(),
      });
    }
  }

  /** Graceful shutdown: unsubscribe from events and release references */
  async shutdown(): Promise<void> {
    for (const unsub of this.unsubscribers) {
      unsub();
    }
    this.unsubscribers = [];
    this.initialized = false;
    this.client = null;
    this.eventBus = null;
    this.agentManager = null;
    this.hookPipeline = null;
    this.toolRegistry = null;
  }
}
