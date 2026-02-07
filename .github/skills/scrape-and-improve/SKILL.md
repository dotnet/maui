---
name: scrape-and-improve
description: Scrapes agent PR sessions, Copilot comments, CCA session data, and repository memories to identify patterns and generate instruction file updates for improved agent success rates.
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires GitHub CLI (gh) authenticated with access to dotnet/maui repository.
---

# Scrape and Improve

Collects data from multiple agent interaction sources, analyzes patterns of success and failure, and generates actionable instruction file updates to improve future agent performance.

## Data Sources

| Source | Location | What It Contains |
|--------|----------|------------------|
| Agent PR Sessions | `.github/agent-pr-session/*.md` | Structured PR review records with phases, fix candidates, root cause analysis |
| Copilot Comments | PR comments via `gh` CLI | AI Summary comments, try-fix attempts, test verification results |
| CCA Session Logs | `CustomAgentLogsTmp/PRState/` | Live session state files from Copilot Coding Agent runs |
| Copilot Review Comments | PR review comments via `gh` CLI | Code review feedback, inline suggestions |

## Inputs

| Input | Required | Source |
|-------|----------|--------|
| Date range | No | Default: last 30 days |
| PR numbers | No | Specific PRs to analyze (comma-separated) |
| Label filter | No | Filter PRs by label (e.g., `copilot`) |

## Outputs

1. **Data Collection Report** - Summary of gathered data:
   - Number of PR sessions analyzed
   - Number of Copilot comments scraped
   - Number of CCA sessions found
   - Key patterns identified

2. **Improvement Recommendations** - Prioritized list of instruction updates:
   - Category, Priority, Location, Specific Change, Evidence

3. **Generated Instruction Updates** - Ready-to-apply changes:
   - Diffs for existing instruction files
   - New instruction file proposals

## Completion Criteria

The skill is complete when you have:
- [ ] Collected data from all available sources
- [ ] Identified recurring patterns (failures, slow successes, quick successes)
- [ ] Generated at least one concrete instruction update
- [ ] Presented findings with evidence from specific PRs

## When to Use

- Periodically (weekly/monthly) to improve agent instructions
- After a batch of agent-assisted PRs
- When agent success rates seem low
- When asked "improve agent instructions" or "what patterns are agents struggling with?"

## When NOT to Use

- For a single PR analysis → Use `learn-from-pr` skill instead
- During active PR work → Use `pr` agent instead
- For writing tests → Use `write-tests-agent` instead

---

## Workflow

### Step 1: Collect Data

Run the data collection script to gather information from all sources:

```bash
# Collect from all sources (last 30 days)
pwsh .github/skills/scrape-and-improve/scripts/Collect-AgentData.ps1

# Collect for specific PRs
pwsh .github/skills/scrape-and-improve/scripts/Collect-AgentData.ps1 -PRNumbers "33380,33134,33392"

# Collect PRs with specific label
pwsh .github/skills/scrape-and-improve/scripts/Collect-AgentData.ps1 -Label "copilot"

# Collect from a specific date range
pwsh .github/skills/scrape-and-improve/scripts/Collect-AgentData.ps1 -Since "2026-01-01"
```

The script collects:

1. **Agent PR Session files** from `.github/agent-pr-session/`:
   - Parses fix candidates tables (approach, result, files changed)
   - Extracts root cause analysis sections
   - Identifies failure patterns and attempt counts

2. **Copilot comments on PRs**:
   - Finds PRs with `copilot` label or AI Summary comments
   - Extracts try-fix attempt results
   - Collects test verification outcomes
   - Gathers code review findings

3. **CCA session data** from `CustomAgentLogsTmp/PRState/`:
   - Reads session state files
   - Extracts fix attempt records
   - Identifies files targeted vs actual fix location

Output is saved to `CustomAgentLogsTmp/scrape-and-improve/collected-data.json`.

### Step 2: Analyze Patterns

Run the analysis script to identify patterns across collected data:

```bash
# Analyze collected data and generate recommendations
pwsh .github/skills/scrape-and-improve/scripts/Analyze-And-Recommend.ps1

# Analyze with specific input file
pwsh .github/skills/scrape-and-improve/scripts/Analyze-And-Recommend.ps1 -InputFile CustomAgentLogsTmp/scrape-and-improve/collected-data.json
```

The script analyzes:

| Pattern Category | What It Looks For | Example Finding |
|-----------------|-------------------|-----------------|
| **Wrong File** | Fix attempts targeting different files than final fix | "3/5 PRs had agents looking in handler when fix was in core" |
| **Slow Discovery** | PRs with >3 fix attempts before success | "Shell navigation fixes average 4.2 attempts" |
| **Quick Success** | PRs with 1 fix attempt | "Button fixes succeed first try when searching for property pattern" |
| **Platform Gaps** | Platform-specific failures not covered by instructions | "No iOS instruction for TraitCollection lifecycle" |
| **Test Gaps** | Missing test patterns for common scenarios | "CollectionView empty state tests missing" |
| **Common Root Causes** | Frequently appearing root causes | "int/double casting issues in Android layout code" |

Output is saved to `CustomAgentLogsTmp/scrape-and-improve/analysis-report.md`.

### Step 3: Generate Instruction Updates

Based on the analysis, generate concrete instruction file changes:

The analysis script produces:

1. **Updates to existing instruction files** - Specific additions to `.github/instructions/*.instructions.md`
2. **New instruction file proposals** - When a pattern doesn't fit existing files
3. **Skill enhancements** - Updates to `.github/skills/*/SKILL.md`
4. **copilot-instructions.md updates** - General guidance additions

Each recommendation includes:
- **Evidence**: Which PRs demonstrated this pattern
- **Priority**: High (prevents class of bugs) / Medium (helps discovery) / Low (nice to have)
- **Location**: Exact file path and section
- **Specific Change**: The text to add or modify
- **Impact**: Expected improvement in agent success rate

### Step 4: Review and Apply

Present the recommendations to the user for review. The user can:

1. **Apply all** - Accept all generated changes
2. **Apply selectively** - Choose which changes to apply
3. **Modify and apply** - Adjust recommendations before applying
4. **Save for later** - Keep the report without applying

When applying, use the learn-from-pr agent's Phase 2 approach:
- Read target file first
- Check for existing similar content
- Match existing style/format
- Find appropriate section

---

## Pattern-to-Improvement Mapping

| Pattern | Improvement Type | Target |
|---------|-----------------|--------|
| Wrong file entirely | Architecture doc | `.github/instructions/` - component relationships |
| Tunnel vision on error message | Instruction file | "Search for PATTERN across codebase" |
| Missing platform knowledge | Platform instruction | `.github/instructions/{platform}.instructions.md` |
| Wrong abstraction layer | Architecture doc | Handler vs core layer guidance |
| Repeated root cause type | Code comment | Source file with non-obvious behavior |
| Slow test discovery | Skill enhancement | try-fix SKILL.md search strategies |
| Quick success pattern | Skill reinforcement | Document what worked and why |

---

## Error Handling

| Situation | Action |
|-----------|--------|
| No agent PR sessions found | Report empty, suggest checking date range |
| `gh` CLI not authenticated | Report error with authentication instructions |
| No Copilot comments found | Proceed with session files only |
| No CCA session data | Proceed with PR sessions and comments only |
| No patterns identified | Report "insufficient data" with data source summary |

## Constraints

- **Read-only analysis** - Don't apply changes without user approval (unless triggered by CI action)
- **Evidence-based** - Every recommendation must cite specific PRs
- **Don't duplicate** - Check existing instruction files before recommending
- **Focus on actionable** - Skip vague observations, only recommend specific changes
- **Respect existing patterns** - Match style and structure of existing instruction files

---

## Integration

- **learn-from-pr** → Single PR analysis (this skill aggregates across many PRs)
- **pr agent** → Uses instruction files that this skill improves
- **GitHub Action** → Can trigger this skill automatically on schedule or manually
