---
name: try-fix
description: Proposes ONE independent fix approach, applies it, runs tests, records result with failure analysis in state file, then reverts. Reads prior attempts to learn from failures. Returns exhausted=true when no more ideas. Max 5 attempts per session.
compatibility: Requires git, PowerShell, and .NET SDK for building and running tests.
---

# Try Fix Skill

Proposes and tests ONE independent fix approach per invocation. The agent invokes this skill repeatedly to explore multiple alternatives.

## Core Principles

1. **Single-shot**: Each invocation = ONE fix idea, tested, recorded, reverted
2. **Independent**: Generate fix ideas WITHOUT looking at or being influenced by the PR's fix
3. **Empirical**: Actually implement and test - don't just theorize
4. **Learning**: When a fix fails, analyze WHY and record the flawed reasoning

## When to Use

- ✅ After Gate passes - you have a verified reproduction test
- ✅ When exploring independent fix alternatives (even if PR already has a fix)
- ✅ When the agent needs to iterate through multiple fix attempts

## When NOT to Use

- ❌ Before Gate passes (you need a test that catches the bug first)
- ❌ For writing tests (use `write-tests` skill)
- ❌ For just running tests (use `BuildAndRunHostApp.ps1` directly)
- ❌ To test the PR's existing fix (Gate already validated that)

---

## Inputs

| Input | Required | Source |
|-------|----------|--------|
| State file path | Yes | Agent workflow (e.g., `.github/agent-pr-session/pr-12345.md`) |
| Test filter | Yes | From test files (e.g., `Issue12345`) |
| Platform | Yes | From issue labels (`android` or `ios`) |
| PR fix files | Yes | From Pre-Flight (files changed by PR, to revert) |

## Outputs

1. **State file update** - New row in Fix Candidates table with:
   - Approach description
   - Test result (PASS/FAIL)
   - Failure analysis (if failed)

2. **Return values** to agent:
   - `approach`: Brief description of what was tried
   - `test_result`: PASS or FAIL
   - `exhausted`: true if no more ideas, false otherwise

## Completion Criteria

The skill is complete when:
- [ ] Prior attempts reviewed and learned from
- [ ] PR's fix reverted (working from broken baseline)
- [ ] ONE independent fix proposed and implemented
- [ ] Tests run and result captured
- [ ] Failure analyzed (if failed)
- [ ] State file updated with new candidate row
- [ ] All changes reverted (except state file)
- [ ] Result returned to agent

## Error Handling

| Situation | Action |
|-----------|--------|
| State file not found | Ask agent for correct path |
| PR fix files unknown | Check state file "Files Changed" section |
| Tests won't build | Report build error, revert, return to agent |
| Tests timeout | Report timeout, revert, mark attempt as FAIL |
| Can't revert cleanly | Run `git checkout -- .` to force clean state |
| 5+ attempts already | Return `exhausted=true` immediately |

---

## Workflow

### Step 1: Read State File and Learn from Prior Attempts

Read the state file to find prior attempts:

```bash
cat .github/agent-pr-session/pr-XXXXX.md
```

Look for the **Fix Candidates** table. For each prior attempt:
- What approach was tried?
- Did it pass or fail?
- **If it failed, WHY did it fail?** (This is critical for learning)

**Use failure analysis to avoid repeating mistakes:**
- If attempt #1 failed because "too late in lifecycle" → don't try other late-lifecycle fixes
- If attempt #2 failed because "trigger wasn't enough, calculation logic needed fixing" → focus on calculation logic

### Step 2: Revert PR's Fix (Get Broken Baseline)

**🚨 CRITICAL: You must work from a broken state where the bug exists.**

```bash
# Identify the PR's fix files from the state file "Files Changed" section
# Revert ALL fix files (not test files)
git checkout HEAD~1 -- src/path/to/fix1.cs src/path/to/fix2.cs

# Verify the bug is present (test should FAIL)
# This is your baseline
```

**Why?** You're testing whether YOUR fix works, independent of the PR's fix.

### Step 3: Check if Exhausted

Before proposing a new fix, evaluate:

1. **Count prior try-fix attempts** - If 5+ attempts already recorded, return `exhausted=true`
2. **Review what's been tried and WHY it failed** - Can you think of a meaningfully different approach?
3. **If no new ideas** - Return `exhausted=true`

**Signs you're exhausted:**
- All obvious approaches have been tried
- Remaining ideas are variations of failed attempts (same root flaw)
- You keep coming back to approaches similar to what failed
- The problem requires architectural changes beyond scope

If exhausted, **stop here** and return to the agent with `exhausted=true`.

### Step 4: Analyze the Code (Independent of PR's Fix)

**🚨 DO NOT look at the PR's fix implementation.** Generate your own ideas.

Research the bug to propose a NEW approach:

```bash
# Find the affected code
grep -r "SymptomOrClassName" src/Controls/src/ --include="*.cs" -l

# Look at the implementation
cat path/to/affected/File.cs

# Check git history for context (but NOT the PR's commits)
git log --oneline -10 -- path/to/affected/File.cs
```

**Key questions:**
- What is the root cause of this bug?
- Where in the code should a fix go?
- What's the minimal change needed?
- How is this different from prior failed attempts?

### Step 5: Propose ONE Fix

Design an approach that is:
- **Independent** - NOT influenced by the PR's solution
- **Different** from prior attempts in the state file
- **Informed** by WHY prior attempts failed
- **Minimal** - smallest change that fixes the issue

Document your approach before implementing:
- Which file(s) to change
- What the change is
- Why you think this will work
- How it differs from prior failed attempts

### Step 6: Apply the Fix

Edit the necessary files to implement your fix.

**Track which files you modify** - you'll need to revert them later.

```bash
# Note the files you're about to change
git status --short
```

### Step 7: Run Tests

Run the reproduction test to see if your fix works:

```bash
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform $PLATFORM -TestFilter "$TEST_FILTER"
```

**Capture the result:**
- ✅ **PASS** - Your fix works (test now passes)
- ❌ **FAIL** - Your fix doesn't work (test still fails, or other tests broke)

### Step 8: If Failed - Analyze WHY

**🚨 CRITICAL: This step is required for failed attempts.**

When your fix fails, analyze:

1. **What was your hypothesis?** Why did you think this would work?
2. **What actually happened?** What did the test output show?
3. **Why was your reasoning flawed?** What did you misunderstand about the bug?
4. **What would be needed instead?** What insight does this failure provide?

This analysis helps future try-fix invocations avoid the same mistake.

### Step 9: Update State File

Add a new row to the **Fix Candidates** table in the state file:

**For PASSING fixes:**
```markdown
| # | Source | Approach | Test Result | Files Changed | Model | Notes |
|---|--------|----------|-------------|---------------|-------|-------|
| N | try-fix | [Your approach] | ✅ PASS | `file.cs` (+X) | [Model Name] | Works! [any observations] |
```

**For FAILING fixes (include failure analysis):**
```markdown
| # | Source | Approach | Test Result | Files Changed | Model | Notes |
|---|--------|----------|-------------|---------------|-------|-------|
| N | try-fix | [Your approach] | ❌ FAIL | `file.cs` (+X) | [Model Name] | **Why failed:** [Analysis of flawed reasoning and what you learned] |
```

### Step 10: Revert Everything

**Always revert** to restore the PR's original state:

```bash
# Revert ALL changes (your fix AND the PR revert from Step 2)
git checkout -- .
```

**Do NOT revert the state file** - the new candidate row should persist.

### Step 11: Return to Agent

Report back to the agent with:

| Field | Value |
|-------|-------|
| `approach` | Brief description of what was tried |
| `test_result` | PASS or FAIL |
| `exhausted` | true if no more ideas, false otherwise |

---

## Fix Candidates Table Format

The state file should have this section:

```markdown
## Fix Candidates

| # | Source | Approach | Test Result | Files Changed | Model | Notes |
|---|--------|----------|-------------|---------------|-------|-------|
| 1 | try-fix | Fix in TabbedPageManager | ❌ FAIL | `TabbedPageManager.cs` (+5) | Claude 3.5 Sonnet | **Why failed:** Too late in lifecycle - by the time OnPageSelected fires, layout already measured with stale values |
| 2 | try-fix | RequestApplyInsets only | ❌ FAIL | `ToolbarExtensions.cs` (+2) | Claude 3.5 Sonnet | **Why failed:** Trigger alone insufficient - calculation logic still used cached values |
| 3 | try-fix | Reset cache + RequestApplyInsets | ✅ PASS | `ToolbarExtensions.cs`, `InsetListener.cs` (+8) | Claude 3.5 Sonnet | Works! Similar to PR's approach |
| PR | PR #XXXXX | [PR's approach] | ✅ PASS (Gate) | [files] | Author | Original PR - validated by Gate |

**Exhausted:** Yes
**Selected Fix:** #3 or PR - both work, compare for simplicity
```

**Note:** The PR's fix is recorded as reference (validated by Gate) but is NOT tested by try-fix.

---

## Guidelines for Proposing Fixes

### Independence is Critical

🚨 **DO NOT look at the PR's fix code when generating ideas.**

The goal is to see if you can independently arrive at the same solution (validating the PR's approach) or find a better alternative.

If your independent fix matches the PR's approach, that's strong validation. If you find a simpler/better approach, that's valuable feedback.

### Good Fix Approaches

✅ **Null/state checks** - Guard against unexpected null or state
✅ **Lifecycle timing** - Move code to correct lifecycle event
✅ **Platform-specific handling** - Add platform check if needed
✅ **Event ordering** - Fix race conditions or ordering issues
✅ **Cache invalidation** - Reset stale cached values

### Approaches to Avoid

❌ **Looking at the PR's fix first** - Generate ideas independently
❌ **Duplicating prior failed attempts** - Check the table and learn from failures
❌ **Variations of failed approaches with same root flaw** - If timing was wrong, a different timing approach is needed
❌ **Massive refactors** - Keep changes minimal
❌ **Suppressing symptoms** - Fix root cause, not symptoms

### Learning from Failures

When a fix fails, the failure analysis is crucial:

**Bad note:** "Didn't work"
**Good note:** "**Why failed:** RequestApplyInsets triggers recalculation, but MeasuredHeight was still cached from previous layout pass. Need to also invalidate the cached measurement."

This helps the next try-fix invocation avoid the same mistake.

---

## Example Session

**State file before (after Gate passed):**
```markdown
## Fix Candidates

| # | Source | Approach | Test Result | Files Changed | Model | Notes |
|---|--------|----------|-------------|---------------|-------|-------|
| PR | PR #33359 | RequestApplyInsets + reset appBarHasContent | ✅ PASS (Gate) | 2 files | Author | Original PR |

**Exhausted:** No
**Selected Fix:** [PENDING]
```

**try-fix invocation #1:**
1. Reads state → sees PR's fix passed Gate, no try-fix attempts yet
2. Reverts PR's fix files → now bug exists
3. Analyzes code independently → proposes: "Fix in TabbedPageManager.OnPageSelected"
4. Applies fix → edits `TabbedPageManager.cs`
5. Runs tests → ❌ FAIL
6. Analyzes failure → "Too late in lifecycle, layout already measured"
7. Updates state file → adds try-fix Candidate #1 with failure analysis
8. Reverts everything (including restoring PR's fix)
9. Returns `{approach: "Fix in TabbedPageManager", test_result: FAIL, exhausted: false}`

**State file after invocation #1:**
```markdown
## Fix Candidates

| # | Source | Approach | Test Result | Files Changed | Model | Notes |
|---|--------|----------|-------------|---------------|-------|-------|
| 1 | try-fix | Fix in TabbedPageManager.OnPageSelected | ❌ FAIL | `TabbedPageManager.cs` (+5) | Claude 3.5 Sonnet | **Why failed:** Too late in lifecycle - OnPageSelected fires after layout measured |
| PR | PR #33359 | RequestApplyInsets + reset appBarHasContent | ✅ PASS (Gate) | 2 files | Author | Original PR |

**Exhausted:** No
**Selected Fix:** [PENDING]
```

**try-fix invocation #2:**
1. Reads state → sees attempt #1 failed because "too late in lifecycle"
2. Reverts PR's fix → bug exists
3. Learns from #1 → needs earlier timing, proposes: "Trigger in UpdateIsVisible"
4. Applies fix → edits `ToolbarExtensions.cs`
5. Runs tests → ✅ PASS
6. Updates state file → adds Candidate #2
7. Reverts everything
8. Returns `{approach: "Trigger in UpdateIsVisible", test_result: PASS, exhausted: false}`

**State file after invocation #2:**
```markdown
## Fix Candidates

| # | Source | Approach | Test Result | Files Changed | Model | Notes |
|---|--------|----------|-------------|---------------|-------|-------|
| 1 | try-fix | Fix in TabbedPageManager.OnPageSelected | ❌ FAIL | `TabbedPageManager.cs` (+5) | Claude 3.5 Sonnet | **Why failed:** Too late in lifecycle |
| 2 | try-fix | RequestApplyInsets in UpdateIsVisible | ✅ PASS | `ToolbarExtensions.cs` (+2) | Claude 3.5 Sonnet | Works! Simpler than PR (1 file vs 2) |
| PR | PR #33359 | RequestApplyInsets + reset appBarHasContent | ✅ PASS (Gate) | 2 files | Author | Original PR |

**Exhausted:** No
**Selected Fix:** [PENDING]
```

**Agent decides:** Found a passing alternative (#2). Can continue to find more, or stop and compare #2 vs PR.

---

## Constraints

- **Max 5 try-fix attempts** per session (PR's fix is NOT counted - it was validated by Gate)
- **Always revert** after each attempt (restore PR's original state)
- **Always update state file** before reverting
- **Never skip testing** - every fix must be validated empirically
- **Never look at PR's fix** when generating ideas - stay independent
- **Always analyze failures** - record WHY fixes didn't work
