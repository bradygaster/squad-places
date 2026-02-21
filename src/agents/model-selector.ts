/**
 * Per-Agent Model Selection (M1-9)
 * 
 * Implements the 4-layer priority model resolution system:
 * 1. User Override
 * 2. Charter Preference
 * 3. Task-Aware Auto-Selection
 * 4. Default (cost-first)
 */

/**
 * Task types that influence model selection.
 */
export type TaskType = 'code' | 'prompt' | 'docs' | 'visual' | 'planning' | 'mechanical';

/**
 * Model tier classification.
 */
export type ModelTier = 'premium' | 'standard' | 'fast';

/**
 * Source of the model resolution.
 */
export type ModelResolutionSource = 'user-override' | 'charter' | 'task-auto' | 'default';

/**
 * Options for model resolution.
 */
export interface ModelResolutionOptions {
  /** User-specified model override */
  userOverride?: string;
  /** Model preference from agent's charter (## Model section) */
  charterPreference?: string;
  /** Type of task being performed */
  taskType: TaskType;
  /** Agent role (for context) */
  agentRole?: string;
}

/**
 * Result of model resolution.
 */
export interface ResolvedModel {
  /** Selected model identifier */
  model: string;
  /** Model tier classification */
  tier: ModelTier;
  /** Source that determined the model */
  source: ModelResolutionSource;
  /** Fallback chain for this tier */
  fallbackChain: string[];
}

/**
 * Fallback chains by tier.
 */
const FALLBACK_CHAINS: Record<ModelTier, string[]> = {
  premium: [
    'claude-opus-4.6',
    'claude-opus-4.6-fast',
    'claude-opus-4.5',
    'claude-sonnet-4.5',
  ],
  standard: [
    'claude-sonnet-4.5',
    'gpt-5.2-codex',
    'claude-sonnet-4',
    'gpt-5.2',
  ],
  fast: [
    'claude-haiku-4.5',
    'gpt-5.1-codex-mini',
    'gpt-4.1',
    'gpt-5-mini',
  ],
};

/**
 * Default model (cost-first).
 */
const DEFAULT_MODEL = 'claude-haiku-4.5';
const DEFAULT_TIER: ModelTier = 'fast';

/**
 * Resolve the appropriate model using the 4-layer priority system.
 * 
 * @param options - Model resolution options
 * @returns Resolved model with tier and fallback chain
 */
export function resolveModel(options: ModelResolutionOptions): ResolvedModel {
  const { userOverride, charterPreference, taskType } = options;

  // Layer 1: User Override
  if (userOverride && userOverride.trim().length > 0) {
    const tier = inferTierFromModel(userOverride);
    return {
      model: userOverride,
      tier,
      source: 'user-override',
      fallbackChain: FALLBACK_CHAINS[tier],
    };
  }

  // Layer 2: Charter Preference
  if (charterPreference && charterPreference.trim().length > 0 && charterPreference !== 'auto') {
    const tier = inferTierFromModel(charterPreference);
    return {
      model: charterPreference,
      tier,
      source: 'charter',
      fallbackChain: FALLBACK_CHAINS[tier],
    };
  }

  // Layer 3: Task-Aware Auto-Selection
  const autoSelected = selectModelForTask(taskType);
  if (autoSelected) {
    return autoSelected;
  }

  // Layer 4: Default
  return {
    model: DEFAULT_MODEL,
    tier: DEFAULT_TIER,
    source: 'default',
    fallbackChain: FALLBACK_CHAINS[DEFAULT_TIER],
  };
}

/**
 * Select model based on task type.
 * 
 * @param taskType - Type of task being performed
 * @returns Resolved model or undefined if no match
 */
function selectModelForTask(taskType: TaskType): ResolvedModel | undefined {
  switch (taskType) {
    case 'code':
      return {
        model: 'claude-sonnet-4.5',
        tier: 'standard',
        source: 'task-auto',
        fallbackChain: FALLBACK_CHAINS.standard,
      };
    
    case 'prompt':
      return {
        model: 'claude-sonnet-4.5',
        tier: 'standard',
        source: 'task-auto',
        fallbackChain: FALLBACK_CHAINS.standard,
      };
    
    case 'visual':
      return {
        model: 'claude-opus-4.5',
        tier: 'premium',
        source: 'task-auto',
        fallbackChain: FALLBACK_CHAINS.premium,
      };
    
    case 'docs':
    case 'planning':
    case 'mechanical':
      return {
        model: 'claude-haiku-4.5',
        tier: 'fast',
        source: 'task-auto',
        fallbackChain: FALLBACK_CHAINS.fast,
      };
    
    default:
      return undefined;
  }
}

/**
 * Infer model tier from model identifier.
 * 
 * @param model - Model identifier
 * @returns Inferred tier
 */
function inferTierFromModel(model: string): ModelTier {
  const lowerModel = model.toLowerCase();
  
  if (lowerModel.includes('opus')) {
    return 'premium';
  }
  
  if (lowerModel.includes('haiku') || lowerModel.includes('mini')) {
    return 'fast';
  }
  
  // Default to standard for sonnet, gpt-5.x, etc.
  return 'standard';
}
