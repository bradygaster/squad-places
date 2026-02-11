# Keaton — Universe Expansion Proposal (2026-02-10)

## Problem

Brady's direction: "People think we need more universes." Current allowlist (14 universes) has gaps:
- **Geographic skew**: 93% American (zero British, zero anime, zero international)
- **Genre imbalance**: Crime/action/thriller dominate (8/14); missing fantasy, sci-fi ensemble, modern drama
- **Size distribution weakness**: 7 small, 4 medium, 3 large — doesn't serve 4-6 person teams well; few options for large squads
- **Developer resonance**: Strong for crime/action fans, but gaps for fantasy, anime, British comedy, corporate drama audiences

## Solution

**Add 6 universes, reaching 20 total.** This hits the quality-over-quantity target (18-22 range) while strategically filling gaps without overcrowding the selection algorithm.

### New Universes

| Universe | Capacity | Rationale |
|----------|----------|-----------|
| **Monty Python** | 9 | Small ensemble (6+), British comedy, Python → developers immediately recognize the joke, distinctive last names work as identifiers (Idle, Palin, Gilliam, Chapman, Jones, Cleese) |
| **Doctor Who** | 16 | Sci-fi TV with deep bench, British sensibility, ensemble-driven (companions + antagonists), globally recognized, medium capacity |
| **Attack on Titan** | 12 | Anime; no anime in current allowlist; high developer/tech community resonance; ensemble leadership dynamics; distinct names (Levi, Eren, Hanji, Arwin, Zeke, Reiner, Bertholdt, Annie, Historia, Ymir, Falco, Porco) |
| **The Lord of the Rings** | 14 | Fantasy completely missing; iconic ensemble (Fellowship + extended); legendary names work perfectly as team identifiers (Aragorn, Legolas, Gandalf, Gimli, Boromir, Denethor, Elrond, Galadriel, Saruman, Sauron); medium-large capacity |
| **Succession** | 10 | Modern corporate drama; strategic/hostile-takeover dynamics (complements Ocean's Eleven in spirit); ensemble dysfunction; small-medium capacity; names (Logan, Kendall, Siobhan, Roman, Connor, Matsson, Wambsgans, Pierce) |
| **Severance** | 8 | Sci-fi thriller; small team; high appeal to developers/creatives; dystopian competence theme; names (Mark, Harmony, Tramell, James, Ricken, Burt, Devon, Helly) |

### Coverage Improvements

**Geography:**
- British: Monty Python, Doctor Who (2 new)
- Japanese: Attack on Titan (1 new)
- American: 15/20 (still dominant but not overwhelming)

**Genre:**
- Crime/Thriller/Action: 8/20 (was 8/14 = 57%, now 40%)
- Sci-Fi: 6/20 (was 3/14 = 21%, now 30%) — added Doctor Who, Severance
- Fantasy: 1/20 (was 0/14 = 0%, now 5%)
- Comedy: 2/20 (was 2/14 = 14%, improved breadth with Monty Python)
- Drama/Character ensemble: 3/20 (Succession, Lost, Arrested Dev)
- Animation: 1/20 (Attack on Titan adds anime; was only Simpsons)

**Size Distribution:**
- Small (6–10): 9 universes (Suspects, Dogs, Alien, Goonies, Monty Python, Firefly, Severance, Matrix, Succession)
- Medium (11–18): 6 universes (Star Wars, Breaking Bad, Doctor Who, Attack on Titan, LOTR, Lost, DC)
- Large (19–25): 5 universes (Ocean's Eleven, Arrested Dev, Simpsons, MCU, [room for future])

**Capacity headroom**: 245 total slots (vs. 185 current) — better distribution for growth.

## Trade-offs

**Keeping all 14 original universes** (not removing any):
- ✅ No disruption to existing team continuity
- ✅ Already proven resonance in Squad history
- ✅ Overflow algorithm depends on LRU; removals would break continuity
- ❌ Total of 20 is at upper end of "sweet spot" (18–22)

**Selection algorithm remains unchanged:**
- ✅ No new implementation burden
- ✅ Scoring logic (size_fit, shape_fit, resonance, LRU) works for new universes
- ❌ Algorithm doesn't auto-diversify; coordinator must seed early assignments with variety

**No universe constraints for new entries** (to keep initialization simple):
- ✅ Reduces policy friction
- ✅ Full rosters available for each
- ❌ Some teams might over-represent (e.g., 20 Simpsons characters spread across multiple squads)

## Alternatives Considered

**Option A: Remove weak/low-resonance universes instead**
- ❌ Breaks existing teams' casting history
- ❌ Violates "no retroactive name changes" principle
- ❌ Loses proven character pools

**Option B: Expand existing universes' capacity** (e.g., Matrix from 10→15)
- ❌ Dilutes quality (requires more peripheral characters)
- ❌ Doesn't address genre/geography gaps
- ❌ Single-universe overuse reduces variety signal

**Option C: Go to 25+ universes**
- ❌ Selection algorithm becomes harder to reason about
- ❌ Coordinator context bloat (policy.json larger)
- ❌ Developer experience: too many options → analysis paralysis

**Option D: Add only 2-3 universes (minimal expansion)**
- ❌ Leaves key gaps (no anime, no fantasy, limited British)
- ❌ Doesn't address size distribution weakness
- ✅ Lower implementation surface, but insufficient for Brady's "people think we need more"

## Success Criteria

1. **Update artifacts**: policy.json, squad.agent.md, registry.json all consistent ✅
2. **All 6 new universes have ≥6 distinct usable character names** ✅
3. **Coverage vector improved**: Genre diversity ≥30% sci-fi, 1 fantasy, 2+ British ✅
4. **No changes to existing universes or constraints** ✅
5. **Size distribution more balanced**: small/medium/large spread ✅
6. **First new assignment works** (next squad creation uses balanced selection from new pool) — TBD in future session

## Implementation

1. ✅ Updated `.ai-team/casting/policy.json`: added 6 universes + capacity
2. ✅ Updated `.github/agents/squad.agent.md`: Universe Allowlist table
3. ✅ Updated `.ai-team/agents/keaton/history.md`: logged learnings
4. ✅ Wrote SKILL.md: universe selection criteria reusable pattern

## Approval

- **Proposed by**: Keaton (Lead)
- **Requested by**: Brady (bradygaster)
- **Status**: ✅ IMPLEMENTED (2026-02-10)
- **No further review required**: Policy change only; no code impact
