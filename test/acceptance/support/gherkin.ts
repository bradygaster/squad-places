/**
 * Minimal Gherkin parser for .feature files.
 * Parses Feature, Scenario, Given/When/Then steps into structured data.
 */

export interface GherkinStep {
  keyword: 'Given' | 'When' | 'Then' | 'And' | 'But';
  text: string;
}

export interface GherkinScenario {
  name: string;
  steps: GherkinStep[];
}

export interface GherkinFeature {
  name: string;
  scenarios: GherkinScenario[];
}

export function parseFeature(content: string): GherkinFeature {
  const lines = content.split('\n').map((line) => line.trim());
  const feature: GherkinFeature = { name: '', scenarios: [] };
  let currentScenario: GherkinScenario | null = null;
  let lastKeyword: GherkinStep['keyword'] = 'Given';

  for (const line of lines) {
    // Skip empty lines and comments
    if (!line || line.startsWith('#')) continue;

    // Feature declaration
    if (line.startsWith('Feature:')) {
      feature.name = line.substring(8).trim();
      continue;
    }

    // Scenario declaration
    if (line.startsWith('Scenario:')) {
      if (currentScenario) {
        feature.scenarios.push(currentScenario);
      }
      currentScenario = {
        name: line.substring(9).trim(),
        steps: [],
      };
      continue;
    }

    // Step keywords
    if (
      line.startsWith('Given ') ||
      line.startsWith('When ') ||
      line.startsWith('Then ') ||
      line.startsWith('And ') ||
      line.startsWith('But ')
    ) {
      const spaceIdx = line.indexOf(' ');
      const keyword = line.substring(0, spaceIdx) as GherkinStep['keyword'];
      const text = line.substring(spaceIdx + 1).trim();

      // "And" and "But" inherit the previous keyword type
      const effectiveKeyword =
        keyword === 'And' || keyword === 'But' ? lastKeyword : keyword;

      if (keyword !== 'And' && keyword !== 'But') {
        lastKeyword = effectiveKeyword;
      }

      if (currentScenario) {
        currentScenario.steps.push({ keyword: effectiveKeyword, text });
      }
    }
  }

  // Push final scenario
  if (currentScenario) {
    feature.scenarios.push(currentScenario);
  }

  return feature;
}
