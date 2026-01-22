---
name: try-fix
description: Attempts ONE alternative fix for a bug, tests it empirically, and reports results. ALWAYS explores a DIFFERENT approach from existing PR fixes. Use when CI or an agent needs to try independent fix alternatives. Invoke with problem description, test command, target files, and optional hints.
---

# Try Fix Skill

Attempts ONE fix for a given problem. Receives all context upfront, tries a single approach, tests it, and reports what happened.

## Core Principles

1. **Always run** - Never question whether to run. The invoker decides WHEN, you decide WHAT alternative to try
2. **Single-shot** - Each invocation = ONE fix idea, tested, reported
3. **Alternative-focused** - Always propose something DIFFERENT from existing fixes (review PR changes first)
4. **Empirical** - Actually implement and test, don't just theorize
5. **Context-driven** - All information provided upfront; don't search for additional context

**Every invocation:** Review existing fixes → Think of DIFFERENT approach → Implement and test → Report results

## Inputs

All inputs are provided by the invoker (CI, agent, or user).

| Input | Required | Description |
|-------|----------|-------------|
| Problem | Yes | Description of the bug/issue to fix |
| Test command | Yes | Command to verify if fix works (e.g., `pwsh BuildAndRunHostApp.ps1 -Platform android -TestFilter "Issue12345"`) |
| Target files | Yes | Files to investigate for the fix |
| Platform | Yes | Target platform (`android`, `ios`, `windows`, `maccatalyst`) |
| Hints | Optional | Suggested approaches, prior attempts, or areas to focus on |
| Baseline | Optional | Git ref or instructions for establishing broken state (default: current state) |
| state_file | Optional | Path to PR agent state file (e.g., `CustomAgentLogsTmp/PRState/pr-12345.md`). If provided, try-fix will append its results to the Fix Candidates table. |

## Outputs

Results reported back to the invoker:

| Field | Description |
|-------|-------------|
| `approach` | What fix was attempted (brief description) |
| `files_changed` | Which files were modified |
| `result` | `PASS` or `FAIL` |
| `analysis` | Why it worked, or why it failed and what was learned |
| `diff` | The actual code changes made (for review) |

## Output Structure

Save artifacts to `CustomAgentLogsTmp/PRState/<PRNumber>/try-fix/attempt-<N>/` with files: `approach.md`, `fix.diff`, `test-output.log`, `result.txt`, `analysis.md`.

See [references/output-structure.md](references/output-structure.md) for setup commands and directory structure details.

## Completion Criteria

The skill is complete when:
- [ ] Problem understood from provided context
- [ ] ONE fix approach designed and implemented
- [ ] Compile errors resolved (iterated up to 3 times if needed)
- [ ] Tests run and result captured
- [ ] Analysis provided (success explanation or failure reasoning)
- [ ] Results reported to invoker
- [ ] Working directory restored to original state

---

## Workflow

### Step 1: Understand the Problem and Review Existing Fixes

**MANDATORY:** Review what has already been tried:

1. **Check for existing PR changes:**
   ```bash
   git diff origin/main HEAD --name-only
   ```
   - Review what files were changed
   - Read the actual code changes to understand the current fix approach

2. **If state_file provided, review prior attempts:**
   - Read the Fix Candidates table
   - Note which approaches failed and WHY (the Notes column)
   - Note which approaches partially succeeded

3. **Identify what makes your approach DIFFERENT:**
   - Don't repeat the same logic/pattern as existing fixes
   - Think of alternative approaches: different algorithm, different location, different strategy
   - If existing fix modifies X, consider modifying Y instead
   - If existing fix adds logic, consider removing/simplifying instead

**Examples of alternatives:**
- Existing fix: Add caching → Alternative: Change when updates happen
- Existing fix: Fix in handler → Alternative: Fix in platform layer
- Existing fix: Add null check → Alternative: Ensure null never happens
- Existing fix: Change order of operations → Alternative: Change data structure

**Review the provided context:**
- What is the bug/issue?
- What test command verifies the fix?
- What files should be investigated?
- Are there hints about what to try or avoid?

**Do NOT search for additional context.** Work with what's provided.

### Step 2: Establish Baseline (MANDATORY)

🚨 **ALWAYS use EstablishBrokenBaseline.ps1 - NEVER manually revert files.**

```bash
pwsh .github/scripts/EstablishBrokenBaseline.ps1
```

The script reverts fix files to merge-base state while preserving test files and tracking changes for restoration. Optional flags: `-BaseBranch main`, `-FixFiles @("path/to/file.cs")`, `-DryRun`.

**If something fails mid-attempt:** `pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore`

### Step 3: Analyze Target Files

Read the target files to understand the code.

**Key questions:**
- What is the root cause of this bug?
- Where should the fix go?
- What's the minimal change needed?

### Step 4: Design ONE Fix

Based on your analysis and any provided hints, design a single fix approach:

- Which file(s) to change
- What the change is
- Why you think this will work

**If hints suggest specific approaches**, prioritize those.

### Step 5: Apply the Fix

Implement your fix. Use `git status --short` and `git diff` to track changes.

### Step 5.5: Work Through Compile Errors

**CRITICAL: Do NOT give up on first compile error!**

If your fix has compile errors, **iterate to fix them** before running tests:

1. **Attempt the build** (often your test command will do this automatically)
2. **If compile errors occur:** Read error messages (CS#### codes), diagnose, fix, rebuild
3. **Iterate up to 3 times** to resolve compile errors
4. **Only mark as FAIL** if you cannot get the code to compile after reasonable effort

See `references/compile-errors.md` for common error patterns and resolution strategies.

### Step 6: Run Tests

Run the provided test command, capturing output to `$OUTPUT_DIR/test-output.log`. Save result (PASS/FAIL) to `$OUTPUT_DIR/result.txt`.

### Step 7: Capture Artifacts

Before reverting, save all artifacts to `$OUTPUT_DIR/`:

| File | Content |
|------|---------|
| `approach.md` | What was tried, strategy used, why different from existing fixes |
| `fix.diff` | `git diff` output |
| `analysis.md` | Result, hypothesis, what happened, why it worked/failed, insights for future |

**Analysis quality matters.** Bad: "Didn't work". Good: "Fix attempted to reset state in OnPageSelected, but this fires after layout measurement. The cached value was already used."

### Step 8: Restore Working Directory (MANDATORY)

🚨 **ALWAYS restore, even if fix failed.**

```bash
pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore
git checkout -- .
```

### Step 9: Report Results

Provide structured output to the invoker:

```markdown
## Try-Fix Result

**Approach:** [Brief description of what was tried]

**Files Changed:**
- `path/to/file.cs` (+X/-Y lines)

**Result:** ✅ PASS / ❌ FAIL

**Analysis:**
[Why it worked, or why it failed and what was learned]

**Diff:**
```diff
[The actual changes made]
```

**Exhausted:** Yes/No
**Reasoning:** [Why you believe there are/aren't more viable approaches]
```

**Determining Exhaustion:** Set `exhausted=true` when you've tried the same fundamental approach multiple times, all hints have been explored, failure analysis reveals the problem is outside target files, or no new ideas remain. Set `exhausted=false` when this is the first attempt, failure analysis suggests a different approach, hints remain unexplored, or the approach was close but needs refinement.

### Step 10: Update State File (if provided)

If `state_file` input was provided and file exists:

1. **Read current Fix Candidates table** from state file
2. **Determine next attempt number** (count existing try-fix rows + 1)
3. **Append new row** with this attempt's results:

| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| N | try-fix #N | [approach] | ✅ PASS / ❌ FAIL | [files] | [analysis] |

4. **Set exhausted status** based on your determination above
5. **Commit state file:**
```bash
git add "$STATE_FILE" && git commit -m "try-fix: attempt #N (exhausted=$EXHAUSTED)"
```

**If no state file provided:** Skip this step (results returned to invoker only).

**Ownership rule:** try-fix updates its own row AND the exhausted field. Never modify:
- Phase status fields
- "Selected Fix" field
- Other try-fix rows

---

## Error Handling

| Situation | Action |
|-----------|--------|
| Problem unclear | Report "insufficient context" - specify what's missing |
| Test command fails to run | Report build/setup error with details |
| Test times out | Report timeout, include partial output |
| Can't determine fix approach | Report "no viable approach identified" with reasoning |
| Git state unrecoverable | Run `git checkout -- .` and `git clean -fd` if needed |

---

## Guidelines for Proposing Fixes

### Good Fix Approaches

✅ **Null/state checks** - Guard against unexpected null or state
✅ **Lifecycle timing** - Move code to correct lifecycle event
✅ **Platform-specific handling** - Add platform check if needed
✅ **Event ordering** - Fix race conditions or ordering issues
✅ **Cache invalidation** - Reset stale cached values

### Approaches to Avoid

❌ **Massive refactors** - Keep changes minimal
❌ **Suppressing symptoms** - Fix root cause, not symptoms
❌ **Ignoring provided hints** - Hints exist for a reason
❌ **Multiple unrelated changes** - ONE focused fix per invocation

---

## Example Invocation

**Inputs provided:**
```yaml
problem: |
  CollectionView throws ObjectDisposedException when navigating back
  from a page with a CollectionView on Android.

test_command: |
  pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "Issue54321"

target_files:
  - src/Controls/src/Core/Handlers/Items/ItemsViewHandler.Android.cs
  - src/Controls/src/Core/Handlers/Items/CollectionViewHandler.Android.cs

platform: android

hints: |
  - The issue seems related to disposal timing
  - Similar issue was fixed in ListView by checking IsDisposed before accessing adapter
  - Focus on the Disconnect/Cleanup methods
```

**Skill execution:** Reads context → Analyzes target files → Designs fix (add IsDisposed check) → Applies fix → Runs test (PASS) → Reports result → Reverts changes

---

## What the Invoker Controls

This skill does ONE attempt. The invoker (CI pipeline, agent, user) controls:

| Decision | Invoker's responsibility |
|----------|--------------------------|
| How many attempts | Invoke skill multiple times if needed |
| Max attempts | Configure loop limit (default: 5, can be set higher) |
| Early termination | Stop when try-fix reports `exhausted=true` |
| When to stop | Interpret results and decide (exhausted OR max reached) |
| State file path | Optionally provide for automatic recording |
| Passing context between attempts | Provide updated hints on subsequent calls |
| Success criteria | Evaluate the reported result |
| Phase status | Update phase to COMPLETE when done |
| Final selection | Set "Selected Fix" field when a fix passes |

The skill records its own results and exhausted status when a state file is provided. The loop should terminate when EITHER:
1. A fix passes (success)
2. `exhausted=true` is reported (no more viable approaches)
3. Max attempts reached (configurable cap)
