---
name: learn-from-pr
description: Analyzes completed PRs for lessons learned, then applies improvements to instruction files, skills, and documentation.
---

# Learn From PR Agent

Extracts lessons from completed PRs and **applies** improvements to the repository.

## When to Invoke

- "Learn from PR #XXXXX and apply improvements"
- "Update the repo based on what we learned from PR #XXXXX"
- After any PR with agent involvement (failed, slow success, or quick success)

## When NOT to Invoke

- For analysis only without applying changes → use `/learn-from-pr` skill
- Before PR is finalized
- For trivial PRs with no learning value

---

## Workflow

### Phase 1: Analysis

Run the `/learn-from-pr` skill workflow (Steps 1-6) to generate recommendations.

The skill covers three outcome types:
- **Agent failed** - What was missing that caused wrong attempts
- **Agent succeeded slowly** - What would have gotten to solution faster
- **Agent succeeded quickly** - What patterns to reinforce

### Phase 2: Apply Changes

For each **High or Medium priority** recommendation:

| Category | Action |
|----------|--------|
| Instruction file | Edit existing or create new `.github/instructions/*.instructions.md` |
| Skill enhancement | Edit `.github/skills/*/SKILL.md` |
| Architecture doc | Edit `/docs/design/*.md` (detailed) or create quick-reference in `.github/architecture/` |
| General AI guidance | Edit `.github/copilot-instructions.md` |
| Code comment | Add comment to source file (don't modify behavior) |

**Before each edit:**
- Read the target file first
- Check for existing similar content (don't duplicate)
- Match the existing style/format
- Find the appropriate section

**Skip applying if:**
- Content already exists
- Recommendation is too vague
- Would require major restructuring

### Phase 2.5: Verify Changes

After applying changes:

1. Run `git diff` to review all edits
2. Verify no syntax errors in modified files (valid markdown)
3. Confirm style matches existing content
4. If issues found, fix or revert before reporting

### Phase 3: Report

Present a summary:

```markdown
## Changes Applied

| File | Change |
|------|--------|
| [path] | [what was added/modified] |

## Not Applied

| Recommendation | Reason |
|----------------|--------|
| [rec] | [why skipped] |
```

---

## Error Handling

| Situation | Action |
|-----------|--------|
| PR not found | Ask user to verify PR number |
| No session markdown | Proceed with PR diff analysis only |
| Target file doesn't exist | Create if instruction/architecture doc, skip if code |
| Duplicate content exists | Skip, note in report |
| Unclear where to add | Ask user for guidance |

## Constraints

- **Only apply High/Medium priority** - Report Low priority without applying
- **Don't duplicate** - Check existing content first
- **Match style** - Read file before editing
- **Code comments only** - Never modify code behavior
- **No linter implementation** - File issue instead of building analyzers

---

## Difference from Skill

| Aspect | `/learn-from-pr` Skill | This Agent |
|--------|------------------------|------------|
| Output | Recommendations to discuss | Applied changes |
| Mode | Analysis only | Autonomous |
| Use when | Want to review without applying | CI automation |
