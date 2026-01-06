---
name: try-fix
description: Proposes a fix approach, applies it, runs tests, records result in state file, then reverts. Reads prior attempts to avoid repetition. Returns exhausted=true when no more ideas. Max 5 attempts per session.
---

# Try Fix Skill

Iteratively explores fix approaches for a bug. Each invocation proposes ONE new fix, tests it, and records the result.

## When to Use

- ✅ When you need to iteratively explore fixes for a bug
- ✅ When you have a reproduction test that catches the bug
- ✅ When you want to try multiple approaches and compare

## When NOT to Use

- ❌ Before you have a test that reproduces the bug
- ❌ For writing tests (use `write-tests` skill)
- ❌ For just running tests (use `BuildAndRunHostApp.ps1` directly)

---

## Inputs

Before invoking this skill, ensure you have:

| Input | Source | Example |
|-------|--------|---------|
| State file path | Agent workflow | `.github/agent-pr-session/pr-12345.md` |
| Test filter | From test files | `Issue12345` |
| Platform | From issue labels | `android` or `ios` |

---

## Workflow

### Step 1: Read State File

Read the state file to find prior attempts:

```bash
cat .github/agent-pr-session/pr-XXXXX.md
```

Look for the **Fix Candidates** table. Extract:
- Which approaches have been tried
- What the test results were
- Why they failed (if applicable)

### Step 2: Check if Exhausted

Before proposing a new fix, evaluate:

1. **Count prior attempts** - If 5+ attempts already recorded, return `exhausted=true`
2. **Review what's been tried** - Can you think of a meaningfully different approach?
3. **If no new ideas** - Return `exhausted=true`

**Signs you're exhausted:**
- All obvious approaches have been tried
- Remaining ideas are variations of failed attempts
- The problem requires architectural changes beyond the scope

If exhausted, **stop here** and return to the agent with `exhausted=true`.

### Step 3: Analyze the Code

Research the bug to propose a NEW approach:

```bash
# Find the affected code
grep -r "SymptomOrClassName" src/Controls/src/ --include="*.cs" -l

# Look at the implementation
cat path/to/affected/File.cs

# Check git history for context
git log --oneline -10 -- path/to/affected/File.cs
```

**Key questions:**
- What is the root cause?
- Where exactly should the fix go?
- What's the minimal change needed?

### Step 4: Propose a Fix

Design an approach that is:
- **Different** from prior attempts in the state file
- **Minimal** - smallest change that fixes the issue
- **Safe** - doesn't break other functionality

Document your approach before implementing:
- Which file(s) to change
- What the change is
- Why this should work

### Step 5: Apply the Fix

Edit the necessary files to implement your fix.

**Track which files you modify** - you'll need to revert them later.

```bash
# Note the files you're about to change
git status --short
```

### Step 6: Run Tests

Run the reproduction test to see if your fix works:

```bash
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform $PLATFORM -TestFilter "$TEST_FILTER"
```

**Capture the result:**
- ✅ **PASS** - Fix works (test now passes)
- ❌ **FAIL** - Fix doesn't work (test still fails, or other tests broke)

### Step 7: Update State File

Add a new row to the **Fix Candidates** table in the state file:

```markdown
| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| N | try-fix | [Your approach description] | ✅/❌ | `file.cs` (+X lines) | [Any notes] |
```

**Include:**
- Sequential number
- Source: `try-fix`
- Brief description of the approach
- Test result (PASS/FAIL)
- Files changed with line count
- Notes (why it failed, concerns, etc.)

### Step 8: Revert the Fix

**Always revert** to keep the working tree clean for the next attempt:

```bash
# Revert all changes to source files (keep state file changes)
git checkout -- src/
```

Or revert specific files:
```bash
git checkout -- path/to/file1.cs path/to/file2.cs
```

**Do NOT revert the state file** - the new candidate row should persist.

### Step 9: Return to Agent

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

| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| 1 | PR #XXXXX | Null check in Handler | ✅ PASS | `Handler.cs` (+3) | Original PR |
| 2 | try-fix | Move logic to Mapper | ❌ FAIL | `Mapper.cs` (+8) | Broke other test |
| 3 | try-fix | Guard in OnLoaded | ✅ PASS | `View.cs` (+2) | Simpler than #1 |

**Exhausted:** No
**Selected Fix:** (pending)
```

---

## Guidelines for Proposing Fixes

### Good Fix Approaches

✅ **Null/state checks** - Guard against unexpected null or state
✅ **Lifecycle timing** - Move code to correct lifecycle event
✅ **Platform-specific handling** - Add platform check if needed
✅ **Event ordering** - Fix race conditions or ordering issues

### Approaches to Avoid

❌ **Duplicating prior attempts** - Check the table first
❌ **Massive refactors** - Keep changes minimal
❌ **Unrelated "improvements"** - Focus on the bug only
❌ **Suppressing symptoms** - Fix root cause, not symptoms

### When Multiple Fixes Pass

If you find a passing fix but think there might be a better one:
- Continue proposing alternatives (up to max 5)
- The agent will compare all passing fixes and select the best one
- Simpler fixes are generally preferred

---

## Example Session

**State file before:**
```markdown
## Fix Candidates

| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| 1 | PR #12345 | Null check in MauiContext | ✅ PASS | `MauiContext.cs` (+3) | Original PR |
```

**Skill execution:**
1. Reads state → sees Candidate #1 (null check, passed)
2. Analyzes code → proposes simpler approach: guard in Mapper
3. Applies fix → edits `Mapper.cs`
4. Runs tests → PASS
5. Updates state file → adds Candidate #2
6. Reverts `Mapper.cs`
7. Returns `{approach: "Guard in Mapper", test_result: PASS, exhausted: false}`

**State file after:**
```markdown
## Fix Candidates

| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| 1 | PR #12345 | Null check in MauiContext | ✅ PASS | `MauiContext.cs` (+3) | Original PR |
| 2 | try-fix | Guard in Mapper callback | ✅ PASS | `Mapper.cs` (+2) | Simpler |
```

---

## Constraints

- **Max 5 attempts** per session (including PR's fix if exists)
- **Always revert** after each attempt
- **Always update state file** before reverting
- **Never skip testing** - every fix must be validated
