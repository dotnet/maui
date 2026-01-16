---
name: learn-from-pr
description: Analyzes completed PRs for lessons learned, then applies improvements to instruction files, skills, and code comments.
---

# Learn From PR Agent

Extracts lessons from completed PRs and **applies** improvements to the repository.

## Inputs

| Input | Required | Source |
|-------|----------|--------|
| PR number or Issue number | Yes | User provides (e.g., "PR #33352") |

## Outputs

1. **Applied Changes** - Actual edits to:
   - Instruction files
   - Skills
   - Code comments
   - Architecture docs

2. **Summary Report** - What was changed and why

## Completion Criteria

The agent is complete when:
- [ ] Analysis phase completed (per skill workflow)
- [ ] High/Medium priority recommendations applied
- [ ] Changes verified (no duplicates, fits existing style)
- [ ] Summary presented to user

## When to Invoke

- "Learn from PR #XXXXX and apply improvements"
- "Improve the repo based on what we learned"
- "Update skills based on PR #XXXXX"
- After complex PR where agent struggled

## When NOT to Invoke

- For analysis only (use `/learn-from-pr` skill instead)
- Before PR is finalized
- For trivial PRs

---

## Workflow

### Phase 1: Analysis

1. **Read the skill file:**
   ```bash
   cat .github/skills/learn-from-pr/SKILL.md
   ```

2. **Execute Steps 1-5 from the skill:**
   - Gather PR data
   - Analyze fix location
   - Identify failure modes
   - Find improvement locations
   - Generate recommendations

3. **Collect High/Medium priority recommendations** for Phase 2.

### Phase 2: Apply Changes

4. **For each High/Medium recommendation:**

   | Category | Action |
   |----------|--------|
   | **Instruction file** | Edit existing `.github/instructions/*.instructions.md` or create new |
   | **Skill enhancement** | Edit `.github/skills/*/SKILL.md` |
   | **Architecture doc** | Create/edit `.github/architecture/*.md` |
   | **Inline code comment** | Add comment to source file |
   | **Linting issue** | Create GitHub issue (don't implement) |

5. **Before each edit:**
   - Read the target file first
   - Check for existing similar content (don't duplicate)
   - Match the existing style/format
   - Find the appropriate section to add to

6. **Skip applying if:**
   - Content already exists
   - Recommendation is too vague to implement
   - Would require major restructuring

### Phase 3: Report

7. **Present summary:**

```markdown
## Changes Applied

| File | Change | Recommendation |
|------|--------|----------------|
| [path] | [what was added/modified] | [which recommendation] |

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
| Target file doesn't exist | Create it if instruction file, skip if code |
| Duplicate content exists | Skip, note in "Not Applied" |
| Can't determine where to add | Ask user for guidance |

---

## Constraints

- **Only apply High/Medium priority** - Report Low priority only
- **Don't duplicate** - Check existing content first
- **Match style** - Read file before editing
- **Code comments only** - Don't modify code behavior
- **No analyzer implementation** - File issue instead

---

## Example Session

**User:** "Learn from PR #33352 and apply improvements"

**Agent:**

1. Reads skill file
2. Gathers PR data, finds session markdown
3. Identifies: agent attempted wrong file (ShellSectionRootRenderer vs PageViewController)
4. Generates recommendations:
   - High: Add iOS debugging guidance to instructions
   - Medium: Add "search for pattern" step to try-fix skill
5. Applies changes:
   - Creates `.github/instructions/ios-debugging.instructions.md`
   - Edits `.github/skills/try-fix/SKILL.md`
6. Reports:

```markdown
## Changes Applied

| File | Change |
|------|--------|
| `.github/instructions/ios-debugging.instructions.md` | Created with pattern search guidance |
| `.github/skills/try-fix/SKILL.md` | Added Step 0: Search codebase for pattern |

## Not Applied

None - all High/Medium recommendations applied.
```

---

## Difference from Skill

| Aspect | Skill | Agent |
|--------|-------|-------|
| Output | Recommendations | Applied changes |
| Interaction | Discussion with user | Autonomous |
| Scope | Analysis only | Analysis + implementation |
| Use when | Want to review first | Ready to apply |
