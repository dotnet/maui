---
name: learn-from-pr
description: Analyzes a completed PR to extract lessons learned from agent behavior. Use after any PR with agent involvement - whether the agent failed, succeeded slowly, or succeeded quickly. Identifies patterns to reinforce or fix, and generates actionable recommendations for instruction files, skills, and documentation.
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires GitHub CLI (gh)
---

# Learn From PR

Extracts lessons learned from a completed PR to improve repository documentation and agent capabilities.

## Inputs

| Input | Required | Source |
|-------|----------|--------|
| PR number or Issue number | Yes | User provides (e.g., "PR #33352" or "issue 33352") |
| Session markdown | Optional | `CustomAgentLogsTmp/PRState/issue-XXXXX.md` or `pr-XXXXX.md` |

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

- After agent failed to find the right fix
- After agent succeeded but took many attempts
- After agent succeeded quickly (to understand what worked)
- When asked "what can we learn from PR #XXXXX?"

## When NOT to Use

- Before PR is finalized (use `pr-finalize` first)
- For trivial PRs (typo fixes, simple changes)
- When no agent was involved (nothing to analyze)

---

## Workflow

### Step 1: Gather Data

```bash
# Required: Get PR info
gh pr view XXXXX --json title,body,files
gh pr diff XXXXX

# Check for session markdown
ls CustomAgentLogsTmp/PRState/issue-XXXXX.md CustomAgentLogsTmp/PRState/pr-XXXXX.md 2>/dev/null
```

**If session markdown exists, extract:**
- Fix Candidates table (what was tried)
- Files each attempt targeted
- Why attempts failed

**Analyzing without session markdown:**

When no session file exists, you can still learn from:
1. **PR discussion** - Comments reveal what was tried
2. **Commit history** - Multiple commits may show iteration
3. **Code complexity** - Non-obvious fixes suggest learning opportunities
4. **Similar past issues** - Search for related bugs

Focus on: "What would have helped an agent find this fix faster?"

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

### Step 3: Analyze Outcome

Determine which scenario applies and look for the relevant patterns:

#### Scenario A: Agent Failed

| Pattern | Indicator |
|---------|-----------|
| **Wrong file entirely** | All attempts in File A, fix in File B |
| **Tunnel vision** | Only looked at file mentioned in error |
| **Trusted issue title** | Issue said "crash in X" so only looked at X |
| **Pattern not generalized** | Fixed one instance, missed others |
| **Didn't search codebase** | Never found similar code patterns |
| **Missing platform knowledge** | Didn't know iOS/Android/Windows specifics |
| **Wrong abstraction layer** | Fixed handler when problem was in core |
| **Misread error message** | Error pointed to symptom, not cause |
| **Incomplete context** | Didn't read enough surrounding code |
| **Over-engineered** | Complex fix when simple one existed |

#### Scenario B: Agent Succeeded Slowly (many attempts)

| Pattern | Indicator |
|---------|-----------|
| **Correct file, wrong approach** | Found right file but tried wrong fixes first |
| **Needed multiple iterations** | Each attempt got closer but wasn't quite right |
| **Discovery was slow** | Eventually found it but search was inefficient |
| **Missing domain knowledge** | Had to learn something that could be documented |

**Key question:** What would have gotten agent to the solution faster?

#### Scenario C: Agent Succeeded Quickly

| Pattern | Indicator |
|---------|-----------|
| **Good search strategy** | Found right file immediately |
| **Understood the pattern** | Recognized similar issues from past |
| **Documentation helped** | Existing docs pointed to solution |
| **Simple, minimal fix** | Didn't over-engineer |

**Key question:** What made this work? Should we reinforce this pattern?

### Step 4: Find Improvement Locations

```bash
# Discover where agent guidance lives
find .github/instructions -name "*.instructions.md" 2>/dev/null
find .github/skills -name "SKILL.md" 2>/dev/null
ls docs/design/ 2>/dev/null
ls .github/copilot-instructions.md 2>/dev/null
```

| Location | When to Add Here |
|----------|------------------|
| `.github/instructions/*.instructions.md` | Domain-specific AI guidance (testing patterns, platform rules) |
| `.github/skills/*/SKILL.md` | Skill needs new step, checklist, or improved workflow |
| `/docs/design/*.md` | Detailed architectural documentation |
| `.github/copilot-instructions.md` | General AI workflow guidance |
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

**Pattern-to-Improvement Mapping (Failures/Slow Success):**

| Pattern | Likely Improvement |
|---------|-------------------|
| Wrong file entirely | Check `/docs/design/` for component relationships |
| Tunnel vision | Instruction file: "Always search for pattern across codebase" |
| Missing platform knowledge | Platform-specific instruction file |
| Wrong abstraction layer | Reference `/docs/design/HandlerResolution.md` |
| Misread error message | Code comment explaining the real cause |
| Over-engineered | Skill enhancement: "Try simplest fix first" |

**Pattern-to-Improvement Mapping (Quick Success - reinforce):**

| Pattern | Improvement |
|---------|-------------|
| Good search strategy | Document the search pattern that worked in skills |
| Documentation helped | Note which docs were valuable, ensure they stay updated |
| Recognized pattern | Add to instruction files as known pattern |

### Step 6: Present Findings

Present your analysis covering:
- What happened and what made it hard
- Where agent looked vs actual fix location
- Which patterns applied and evidence
- Prioritized recommendations with full details (category, priority, location, exact change, why it helps)

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

### Example: Slow Success

**PR #34567** - CollectionView scroll position not preserved

**What happened:**
- Agent took 5 attempts to find fix
- First 3 attempts were in wrong layer (handler vs core)
- Eventually found it after reading more context
- Final fix was simple once the right layer was identified

**Pattern:** Wrong abstraction layer - fixed handler when problem was in core.

**Recommendation:**
- **Category:** Architecture Doc
- **Location:** `.github/architecture/handler-vs-core.md`
- **Change:** Document layer responsibilities - handlers map properties, core handles behavior
- **Why:** Helps agent identify correct layer faster

### Example: Quick Success

**PR #35678** - Button disabled state not updating

**What happened:**
- Agent found fix in 1 attempt
- Searched for "IsEnabled" pattern across codebase immediately
- Found similar past fix in another control and applied same approach
- Simple, minimal change

**Pattern:** Good search strategy - recognized pattern from similar code.

**Recommendation:**
- **Category:** Skill Enhancement
- **Location:** `.github/skills/try-fix/SKILL.md`
- **Change:** Add to search strategy: "Search for same property pattern in other controls"
- **Why:** Reinforces successful discovery technique

---

## Integration

- **pr-finalize** → Use first to verify PR is ready
- **learn-from-pr skill** → Analysis only (this skill)
- **learn-from-pr agent** → Analysis + apply changes

For automated application of recommendations, use the `learn-from-pr` agent instead.
