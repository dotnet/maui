---
name: try-fix
description: Attempts ONE fix for a provided problem, tests it, and reports results. Use when CI or an agent needs to try fixing a bug with full context provided upfront. Invoke with problem description, test command, target files, and optional hints.
metadata:
  author: dotnet-maui
  version: "2.0"
compatibility: Requires git, PowerShell, and .NET SDK for building and running tests.
---

# Try Fix Skill

Attempts ONE fix for a given problem. Receives all context upfront, tries a single approach, tests it, and reports what happened.

## Core Principles

1. **Single-shot**: Each invocation = ONE fix idea, tested, reported
2. **Context-driven**: All necessary information provided upfront by invoker
3. **Empirical**: Actually implement and test - don't just theorize
4. **Informative**: Report what was tried, what happened, and why

## When to Use

- ✅ CI automation needs to attempt a fix
- ✅ You have a clear problem description and test command
- ✅ You want ONE attempt with full reporting

## When NOT to Use

- ❌ For writing tests (use `write-tests` skill)
- ❌ For just running tests (use test commands directly)
- ❌ When problem context is unclear (gather context first)

---

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
| state_file | Optional | Path to PR agent state file (e.g., `.github/agent-pr-session/pr-12345.md`). If provided, try-fix will append its results to the Fix Candidates table. |

## Outputs

Results reported back to the invoker:

| Field | Description |
|-------|-------------|
| `approach` | What fix was attempted (brief description) |
| `files_changed` | Which files were modified |
| `result` | `PASS` or `FAIL` |
| `analysis` | Why it worked, or why it failed and what was learned |
| `diff` | The actual code changes made (for review) |

## Completion Criteria

The skill is complete when:
- [ ] Problem understood from provided context
- [ ] ONE fix approach designed and implemented
- [ ] Tests run and result captured
- [ ] Analysis provided (success explanation or failure reasoning)
- [ ] Results reported to invoker
- [ ] Working directory restored to original state

---

## Workflow

### Step 1: Understand the Problem

Review the provided context:
- What is the bug/issue?
- What test command verifies the fix?
- What files should be investigated?
- Are there hints about what to try or avoid?

**Do NOT search for additional context.** Work with what's provided.

### Step 2: Establish Baseline (if specified)

If a baseline is provided (e.g., "revert files X, Y, Z first"):

```bash
# Example: revert specific files to create broken state
git checkout HEAD~1 -- src/path/to/file.cs
```

If no baseline specified, work from current state.

### Step 3: Analyze Target Files

Read the target files to understand the code:

```bash
# Read the files specified in inputs
cat src/path/to/TargetFile.cs
```

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

Implement your fix. Track what you change:

```bash
# Before editing, note current state
git status --short

# Apply your fix
# [edit files]

# After editing, capture what changed
git diff
```

### Step 6: Run Tests

Run the provided test command:

```bash
# Use the exact test command provided in inputs
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform $PLATFORM -TestFilter "$TEST_FILTER"
```

**Capture the result:**
- ✅ **PASS** - Fix works (test passes)
- ❌ **FAIL** - Fix doesn't work (test fails or other issues)

### Step 7: Analyze Result

**If PASS:**
- Why did this fix work?
- Is it the minimal change needed?
- Any concerns or caveats?

**If FAIL:**
- What was your hypothesis?
- What actually happened?
- Why was the reasoning flawed?
- What insight does this provide for future attempts?

### Step 8: Capture Diff

Before reverting, capture the diff for reporting:

```bash
git diff > /tmp/fix-attempt.diff
# or just include inline in your report
```

### Step 9: Restore Working Directory

Revert all changes to restore original state:

```bash
git checkout -- .
```

### Step 10: Report Results

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
```

### Step 11: Update State File (if provided)

If `state_file` input was provided and file exists:

1. **Read current Fix Candidates table** from state file
2. **Determine next attempt number** (count existing try-fix rows + 1)
3. **Append new row** with this attempt's results:

| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| N | try-fix #N | [approach] | ✅ PASS / ❌ FAIL | [files] | [analysis] |

4. **Commit state file:**
```bash
git add "$STATE_FILE" && git commit -m "try-fix: attempt #N"
```

**If no state file provided:** Skip this step (results returned to invoker only).

**Ownership rule:** try-fix ONLY updates its own row. Never modify:
- Phase status fields
- "Exhausted" field
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

### Failure Analysis Quality

When a fix fails, analysis quality matters:

**Bad:** "Didn't work"

**Good:** "Fix attempted to reset state in OnPageSelected, but this fires after layout measurement. The cached MeasuredHeight value was already used. A fix needs to invalidate the cache BEFORE the layout pass, not after."

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

**Skill execution:**
1. Reads context - understands it's a disposal timing issue on Android
2. Analyzes target files - finds `DisconnectHandler` method
3. Designs fix - add `IsDisposed` check before accessing adapter
4. Applies fix - edits `CollectionViewHandler.Android.cs`
5. Runs test - ✅ PASS
6. Analyzes - "Adding IsDisposed check prevents access to disposed adapter during navigation"
7. Captures diff
8. Reverts changes
9. Reports results

**Output:**
```markdown
## Try-Fix Result

**Approach:** Add IsDisposed check in DisconnectHandler before accessing adapter

**Files Changed:**
- `src/Controls/src/Core/Handlers/Items/CollectionViewHandler.Android.cs` (+3/-0 lines)

**Result:** ✅ PASS

**Analysis:**
The ObjectDisposedException occurred because DisconnectHandler was called during
navigation after the handler was already disposed. Adding an early return when
IsDisposed is true prevents the null adapter access. This matches the pattern
used in ListView's fix (as noted in hints).

**Diff:**
```diff
 protected override void DisconnectHandler(RecyclerView platformView)
 {
+    if (IsDisposed)
+        return;
+
     base.DisconnectHandler(platformView);
```
```

---

## What the Invoker Controls

This skill does ONE attempt. The invoker (CI pipeline, agent, user) controls:

| Decision | Invoker's responsibility |
|----------|--------------------------|
| How many attempts | Invoke skill multiple times if needed |
| When to stop | Interpret results and decide |
| State file path | Optionally provide for automatic recording |
| Passing context between attempts | Provide updated hints on subsequent calls |
| Success criteria | Evaluate the reported result |
| Phase status | Update phase to COMPLETE when done |
| Final selection | Set "Exhausted" and "Selected Fix" fields |

The skill records its own results when a state file is provided, but does NOT decide when to stop or which fix to select.
