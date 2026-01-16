---
name: learn-from-pr
description: Analyzes completed PR to identify repository improvements. Use after PR finalized to extract lessons learned.
compatibility: Requires GitHub CLI (gh)
---

# Learn From PR

Extracts lessons learned from a completed PR to improve repository documentation and agent capabilities.

## Inputs

| Input | Required | Source |
|-------|----------|--------|
| PR number or Issue number | Yes | User provides (e.g., "PR #33352" or "issue 33352") |
| Session markdown | Optional | `.github/agent-pr-session/issue-XXXXX.md` or `pr-XXXXX.md` |

## Outputs

1. **Learning Analysis** - Structured markdown with:
   - What happened (problem, attempts, solution)
   - Fix location analysis (attempted vs actual)
   - Failure modes identified
   - Prioritized recommendations

2. **Actionable Recommendations** - Each with:
   - Category, Priority, Location, Specific Change, Why It Helps

## Completion Criteria

The skill is complete when you have:
- [ ] Gathered PR diff and metadata
- [ ] Analyzed fix location (attempted vs actual)
- [ ] Identified failure modes
- [ ] Generated at least one concrete recommendation
- [ ] Presented findings to user

## When to Use

- After complex PR with non-obvious solution
- After agent struggled to find the right fix
- When asked "what can we learn from PR #XXXXX?"

## When NOT to Use

- Before PR is finalized (use `pr-finalize` first)
- For trivial PRs (typo fixes, simple changes)
- When no agent was involved (no learning to extract)

---

## Workflow

### Step 1: Gather Data

```bash
# Required: Get PR info
gh pr view XXXXX --json title,body,files
gh pr diff XXXXX

# Check for session markdown
ls .github/agent-pr-session/issue-XXXXX.md .github/agent-pr-session/pr-XXXXX.md 2>/dev/null
```

**If no session markdown exists:** Skip to Step 3 (analyze PR diff only).

**Extract from session markdown (if exists):**
- Fix Candidates table (what was tried)
- Files each attempt targeted
- Why attempts failed

### Step 2: Fix Location Analysis

**Critical question:** Did agent attempts target the same files as the final fix?

```bash
# Where did final fix go?
gh pr view XXXXX --json files --jq '.files[].path' | grep -v test

# If session markdown exists, compare to attempted files
```

| Scenario | Implication |
|----------|-------------|
| Same files | Agent found right location |
| Different files | **Major learning opportunity** - document why |

**If different files:** Answer these questions:
- Why did agent think that was the right file?
- What search would have found the correct file?

### Step 3: Identify Failure Modes

Check for these common failure modes:

| Failure Mode | Indicator |
|--------------|-----------|
| **Wrong file entirely** | All attempts in File A, fix in File B |
| **Tunnel vision** | Only looked at file mentioned in error |
| **Trusted issue title** | Issue said "crash in X" so only looked at X |
| **Pattern not generalized** | Fixed one instance, missed others |
| **Didn't search codebase** | Never found similar code patterns |

### Step 4: Find Improvement Locations

```bash
# Discover where agent guidance lives
find .github/instructions -name "*.instructions.md" 2>/dev/null
find .github/skills -name "SKILL.md" 2>/dev/null
ls .github/architecture/ 2>/dev/null
```

| Location | When to Add Here |
|----------|------------------|
| `.github/instructions/*.instructions.md` | Domain-specific guidance |
| `.github/skills/*/SKILL.md` | Skill needs new step or checklist |
| `.github/architecture/*.md` | Architectural knowledge |
| Code comments | Non-obvious code behavior |

### Step 5: Generate Recommendations

For each recommendation, provide:

1. **Category:** Instruction file / Skill / Architecture doc / Inline comment / Linting issue
2. **Priority:** High (prevents class of bugs) / Medium (helps discovery) / Low (nice to have)
3. **Location:** Exact file path
4. **Specific Change:** Exact text to add
5. **Why It Helps:** Which failure mode it prevents

**Prioritization factors:**
- How common is this pattern?
- Would future agents definitely hit this again?
- How hard is it to implement?

### Step 6: Present Findings

Use the output template below to present your analysis.

---

## Error Handling

| Situation | Action |
|-----------|--------|
| PR not found | Ask user to verify PR number |
| No session markdown | Analyze PR diff only, note limited context |
| No agent involvement evident | Ask user if they still want analysis |
| Can't determine failure mode | State "insufficient data" and what's missing |

## Constraints

- **Analysis only** - Don't apply changes (use learn-from-pr agent for that)
- **Actionable recommendations** - Every recommendation must have specific file path and text
- **Don't duplicate** - Check existing docs before recommending new ones
- **Focus on high-value learning** - Skip trivial observations
- **Respect PR scope** - Don't recommend improvements unrelated to the PR's learnings

---

## Output Template

```markdown
# Learning Analysis: PR #XXXXX

## What Happened

**Problem:** [Brief description]
**Approaches Tried:** [From session markdown, or "N/A - no session file"]
**Final Solution:** [What was implemented]
**Key Challenge:** [What made this hard]

## Fix Location Analysis

| Aspect | Details |
|--------|---------|
| **Agent attempted** | [Files, or "N/A"] |
| **Actual fix location** | [Files changed] |
| **Location match?** | ✅ Same / ❌ Different |

## Failure Modes Identified

| Failure Mode | Evidence |
|--------------|----------|
| [Mode] | [Evidence] |

## Recommendations

### 🔴 High Priority

#### 1. [Title]
- **Category:** [Category]
- **Location:** [File path]
- **Change:** [Exact text]
- **Why:** [Failure mode prevented]

### 🟡 Medium Priority
[Same structure]

### 🟢 Low Priority
[Same structure]

## Summary

**Total:** X high, Y medium, Z low
**Highest Impact:** [Most important]
**Quick Wins:** [Easiest valuable changes]
```

---

## Examples

### Example: Wrong File Entirely

**PR #33352** - TraitCollectionDidChange crash on MacCatalyst

**What happened:**
- Issue title: "ObjectDisposedException in ShellSectionRootRenderer"
- Agent made 11 attempts, ALL in `ShellSectionRootRenderer.cs`
- Actual fix was in `PageViewController.cs`

**Failure Mode:** Trusted issue title instead of searching for pattern.

**Recommendation:**
- **Category:** Instruction File
- **Location:** `.github/instructions/ios-debugging.instructions.md`
- **Change:** "When fixing iOS crashes, search for the PATTERN across all files, not just the file named in the error"
- **Why:** Prevents tunnel vision on named file

---

## Integration

- **pr-finalize** → Use first to verify PR is ready
- **learn-from-pr skill** → Analysis only (this skill)
- **learn-from-pr agent** → Analysis + apply changes

For automated application of recommendations, use the `learn-from-pr` agent instead.
