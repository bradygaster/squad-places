/**
 * Agent Source Registry
 * Pluggable agent discovery and loading
 */

export interface AgentSource {
  readonly name: string;
  readonly type: 'local' | 'github' | 'marketplace';
  listAgents(): Promise<AgentManifest[]>;
  getAgent(name: string): Promise<AgentDefinition | null>;
  getCharter(name: string): Promise<string | null>;
}

export interface AgentManifest {
  name: string;
  role: string;
  version?: string;
  source: string;
}

export interface AgentDefinition extends AgentManifest {
  charter: string;
  model?: string;
  tools?: string[];
  skills?: string[];
}

export class LocalAgentSource implements AgentSource {
  readonly name = 'local';
  readonly type = 'local' as const;

  constructor(private basePath: string) {}

  async listAgents(): Promise<AgentManifest[]> {
    return [];
  }

  async getAgent(name: string): Promise<AgentDefinition | null> {
    return null;
  }

  async getCharter(name: string): Promise<string | null> {
    return null;
  }
}

export class GitHubAgentSource implements AgentSource {
  readonly name = 'github';
  readonly type = 'github' as const;

  constructor(private repo: string, private ref?: string) {}

  async listAgents(): Promise<AgentManifest[]> {
    return [];
  }

  async getAgent(name: string): Promise<AgentDefinition | null> {
    return null;
  }

  async getCharter(name: string): Promise<string | null> {
    return null;
  }
}

export class MarketplaceAgentSource implements AgentSource {
  readonly name = 'marketplace';
  readonly type = 'marketplace' as const;

  constructor(private apiEndpoint: string) {}

  async listAgents(): Promise<AgentManifest[]> {
    return [];
  }

  async getAgent(name: string): Promise<AgentDefinition | null> {
    return null;
  }

  async getCharter(name: string): Promise<string | null> {
    return null;
  }
}

export class AgentRegistry {
  private sources: Map<string, AgentSource> = new Map();

  register(source: AgentSource): void {
    this.sources.set(source.name, source);
  }

  unregister(name: string): boolean {
    return this.sources.delete(name);
  }

  getSource(name: string): AgentSource | undefined {
    return this.sources.get(name);
  }

  async listAllAgents(): Promise<AgentManifest[]> {
    const results = await Promise.all(
      Array.from(this.sources.values()).map(s => s.listAgents())
    );
    return results.flat();
  }

  async findAgent(name: string): Promise<AgentDefinition | null> {
    for (const source of this.sources.values()) {
      const agent = await source.getAgent(name);
      if (agent) return agent;
    }
    return null;
  }

  async getCharter(name: string): Promise<string | null> {
    for (const source of this.sources.values()) {
      const charter = await source.getCharter(name);
      if (charter) return charter;
    }
    return null;
  }
}
