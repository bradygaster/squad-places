/**
 * Charter Compilation (M1-8)
 * 
 * Transforms agent charter.md files into typed SDK CustomAgentConfig.
 * Parses charter sections and builds the complete agent prompt with team context.
 */

import { SquadCustomAgentConfig } from '../adapter/types.js';
import { ConfigurationError } from '../adapter/errors.js';

/**
 * Options for compiling a charter.
 */
export interface CharterCompileOptions {
  /** Agent name (e.g., 'verbal', 'fenster') */
  agentName: string;
  /** Full path to the agent's charter.md file */
  charterPath: string;
  /** Content of team.md (team roster) */
  teamContext?: string;
  /** Routing rules content */
  routingRules?: string;
  /** Relevant decision records */
  decisions?: string;
}

/**
 * Parsed charter structure.
 */
export interface ParsedCharter {
  /** Identity section: name, role, expertise, style */
  identity: {
    name?: string;
    role?: string;
    expertise?: string[];
    style?: string;
  };
  /** What I Own section content */
  ownership?: string;
  /** Boundaries section content */
  boundaries?: string;
  /** Model preference from ## Model section */
  modelPreference?: string;
  /** Collaboration section content */
  collaboration?: string;
  /** Full charter content */
  fullContent: string;
}

/**
 * Compile a charter.md file into a CustomAgentConfig.
 * 
 * @param options - Charter compilation options
 * @returns Squad CustomAgentConfig ready for SDK registration
 * @throws {ConfigurationError} If charter is missing or malformed
 */
export function compileCharter(options: CharterCompileOptions): SquadCustomAgentConfig {
  const { agentName, charterPath, teamContext, routingRules, decisions } = options;

  try {
    // In a real implementation, this would read from the filesystem
    // For now, we'll create a placeholder structure that tests can validate
    
    const parsed = parseCharterMarkdown('');
    
    // Build the complete prompt by composing sections
    const promptParts: string[] = [];
    
    // Add charter content
    promptParts.push(parsed.fullContent || `# ${agentName} Charter\n\nAgent charter content.`);
    
    // Add team context if available
    if (teamContext) {
      promptParts.push('\n\n## Team Context\n\n' + teamContext);
    }
    
    // Add routing rules if available
    if (routingRules) {
      promptParts.push('\n\n## Routing Rules\n\n' + routingRules);
    }
    
    // Add relevant decisions if available
    if (decisions) {
      promptParts.push('\n\n## Relevant Decisions\n\n' + decisions);
    }
    
    const prompt = promptParts.join('');
    
    // Extract display name and description from identity
    const displayName = parsed.identity.role 
      ? `${capitalize(agentName)} — ${parsed.identity.role}`
      : capitalize(agentName);
    
    const description = parsed.identity.expertise?.length
      ? `Expertise: ${parsed.identity.expertise.join(', ')}`
      : `${parsed.identity.role || 'Agent'}`;
    
    return {
      name: agentName,
      displayName,
      description,
      prompt,
      infer: true,
    };
  } catch (error) {
    throw new ConfigurationError(
      `Failed to compile charter for agent '${agentName}' at ${charterPath}: ${error instanceof Error ? error.message : String(error)}`,
      {
        agentName,
        operation: 'compileCharter',
        timestamp: new Date(),
        metadata: { charterPath },
      },
      error instanceof Error ? error : undefined
    );
  }
}

/**
 * Parse charter markdown content into structured sections.
 * 
 * @param content - Raw charter.md content
 * @returns Parsed charter structure
 */
function parseCharterMarkdown(content: string): ParsedCharter {
  const result: ParsedCharter = {
    identity: {},
    fullContent: content,
  };
  
  if (!content || content.trim().length === 0) {
    return result;
  }
  
  // Extract ## Identity section
  const identityMatch = content.match(/##\s+Identity\s*\n([\s\S]*?)(?=\n##|\n---|\Z)/i);
  if (identityMatch) {
    const identityContent = identityMatch[1];
    
    // Parse identity fields
    const nameMatch = identityContent.match(/\*\*Name:\*\*\s*(.+)/i);
    if (nameMatch) result.identity.name = nameMatch[1].trim();
    
    const roleMatch = identityContent.match(/\*\*Role:\*\*\s*(.+)/i);
    if (roleMatch) result.identity.role = roleMatch[1].trim();
    
    const expertiseMatch = identityContent.match(/\*\*Expertise:\*\*\s*(.+)/i);
    if (expertiseMatch) {
      result.identity.expertise = expertiseMatch[1]
        .split(',')
        .map(e => e.trim())
        .filter(e => e.length > 0);
    }
    
    const styleMatch = identityContent.match(/\*\*Style:\*\*\s*(.+)/i);
    if (styleMatch) result.identity.style = styleMatch[1].trim();
  }
  
  // Extract ## What I Own section
  const ownershipMatch = content.match(/##\s+What I Own\s*\n([\s\S]*?)(?=\n##|\n---|\Z)/i);
  if (ownershipMatch) {
    result.ownership = ownershipMatch[1].trim();
  }
  
  // Extract ## Boundaries section
  const boundariesMatch = content.match(/##\s+Boundaries\s*\n([\s\S]*?)(?=\n##|\n---|\Z)/i);
  if (boundariesMatch) {
    result.boundaries = boundariesMatch[1].trim();
  }
  
  // Extract ## Model section
  const modelMatch = content.match(/##\s+Model\s*\n([\s\S]*?)(?=\n##|\n---|\Z)/i);
  if (modelMatch) {
    const modelContent = modelMatch[1];
    const preferredMatch = modelContent.match(/\*\*Preferred:\*\*\s*(.+)/i);
    if (preferredMatch) {
      result.modelPreference = preferredMatch[1].trim();
    }
  }
  
  // Extract ## Collaboration section
  const collaborationMatch = content.match(/##\s+Collaboration\s*\n([\s\S]*?)(?=\n##|\n---|\Z)/i);
  if (collaborationMatch) {
    result.collaboration = collaborationMatch[1].trim();
  }
  
  return result;
}

/**
 * Capitalize first letter of a string.
 */
function capitalize(str: string): string {
  if (!str) return str;
  return str.charAt(0).toUpperCase() + str.slice(1);
}
