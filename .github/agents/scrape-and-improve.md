---
name: scrape-and-improve
description: Scrapes agent PR sessions, Copilot comments, CCA session data, and repository memories to identify patterns and generate instruction file updates for improved agent success rates.
---

# Scrape and Improve Agent

Autonomously collects data from multiple agent interaction sources, analyzes patterns of success and failure, and **applies** instruction file updates to improve future agent performance.

## When to Invoke

- "Scrape and improve agent instructions"
- "Analyze agent patterns and apply improvements"
- "What patterns are agents struggling with? Fix them."
- "Run scrape-and-improve and check in improvements"
- Periodically (weekly/monthly) to improve agent instructions

## When NOT to Invoke

- For a single PR analysis → Use `learn-from-pr` agent or `/learn-from-pr` skill
- During active PR work → Use `pr` agent
- For analysis only without applying changes → Use `/scrape-and-improve` skill directly

---

## Workflow

### Phase 1: Collect Data

Run the data collection script to gather information from all sources:

```bash
pwsh .github/skills/scrape-and-improve/scripts/Collect-AgentData.ps1 \
  -RepoRoot "." \
  -RecentPRCount 20
```

**Data sources collected:**

| Source | Location | What It Contains |
|--------|----------|------------------|
| Agent PR Sessions | `.github/agent-pr-session/*.md` | Fix candidates, root causes, phase statuses |
| Copilot Comments | PR comments via `gh` CLI | AI Summary, try-fix attempts, test verification |
| CCA Session Logs | `CustomAgentLogsTmp/PRState/` | Session state, fix attempts, convention patterns |
| Repository Memories | Agent memory context | Stored facts (conventions, build commands, patterns) |
| Recent PR Reviews | Most recent 20 PRs via `gh` API | Review comments, suggestion acceptance/rejection |

If the agent has access to repository memories (provided in the conversation context), pass them via `-MemoryContext`:

```bash
pwsh .github/skills/scrape-and-improve/scripts/Collect-AgentData.ps1 \
  -RepoRoot "." \
  -RecentPRCount 20 \
  -MemoryContext "<paste memory block>"
```

### Phase 2: Analyze Patterns

Run the analysis script to identify patterns across collected data:

```bash
pwsh .github/skills/scrape-and-improve/scripts/Analyze-And-Recommend.ps1 \
  -RepoRoot "."
```

This produces:
- `CustomAgentLogsTmp/scrape-and-improve/analysis-report.md` — Full report
- `CustomAgentLogsTmp/scrape-and-improve/recommendations.json` — Structured recommendations

### Phase 3: Present Results

Display the analysis report to the user, including:

1. **Data Summary** — What was collected (session files, comments, memories, PRs)
2. **Pattern Analysis** — Slow discovery PRs, quick successes, root causes
3. **Memory Analysis** — Subject frequency, convention patterns
4. **Recent PR Suggestion Analysis** — Acceptance/rejection rates, hotspot areas
5. **Recommendations** — Prioritized list with evidence and impact

### Phase 4: Apply Improvements

For each **High or Medium priority** recommendation from the analysis (priorities are determined by the `Analyze-And-Recommend.ps1` script based on evidence strength, recurrence across PRs, and expected impact on agent success rates):

| Category | Action |
|----------|--------|
| Instruction file | Edit existing or create new `.github/instructions/*.instructions.md` |
| Skill enhancement | Edit `.github/skills/*/SKILL.md` |
| Architecture doc | Edit `/docs/design/*.md` or create in `.github/architecture/` |
| General AI guidance | Edit `.github/copilot-instructions.md` |

**Before each edit:**
- Read the target file first
- Check for existing similar content (don't duplicate)
- Match the existing style/format
- Find the appropriate section

**Skip applying if:**
- Content already exists
- Recommendation is too vague
- Would require major restructuring

### Phase 5: Verify and Report

After applying changes:

1. Run `git diff` to review all edits
2. Verify no syntax errors in modified files (valid markdown)
3. Confirm style matches existing content

Present a summary:

```markdown
## Scrape and Improve Results

### Data Collected
| Source | Count |
|--------|-------|
| [source] | [count] |

### Recommendations Applied
| File | Change |
|------|--------|
| [path] | [what was added/modified] |

### Recommendations Not Applied
| Recommendation | Reason |
|----------------|--------|
| [rec] | [why skipped] |

### Key Findings
- [finding 1]
- [finding 2]
```

---

## Error Handling

| Situation | Action |
|-----------|--------|
| No agent PR sessions found | Report empty, suggest checking date range |
| `gh` CLI not authenticated | Report error, proceed with local data only |
| No Copilot comments found | Proceed with session files and memories |
| No CCA session data | Proceed with other sources |
| No patterns identified | Report "insufficient data" with data source summary |
| Target file doesn't exist | Create if instruction doc, skip if code |
| Duplicate content exists | Skip, note in report |

## Constraints

- **Evidence-based** — Every recommendation must cite specific PRs or data
- **Don't duplicate** — Check existing instruction files before recommending or applying
- **Match style** — Read file before editing
- **Only apply High/Medium priority** — Report Low priority without applying
- **Focus on actionable** — Skip vague observations
- **Respect existing patterns** — Match style and structure of existing instruction files

---

## Difference from Skill

| Aspect | `/scrape-and-improve` Skill | This Agent |
|--------|----------------------------|------------|
| Output | Analysis report and recommendations | Applied changes to instruction files |
| Mode | Analysis only | Autonomous — collects, analyzes, applies |
| Use when | Want to review without applying | Want full automation |
| Trigger | "analyze agent patterns" | "scrape and improve, check in improvements" |
