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

## 🚨 Critical: When This Skill Runs

**ALWAYS RUN when invoked.** This skill's purpose is to explore alternative fixes, not to judge whether alternatives are needed.

**Every invocation MUST:**
1. ✅ Review existing fixes (from PR changes or state file)
2. ✅ Think of a DIFFERENT approach (don't repeat what already exists)
3. ✅ Implement and test the alternative
4. ✅ Report results

**NEVER:**
- ❌ Question whether a fix already exists
- ❌ Ask if alternatives should be explored
- ❌ Decide the skill "shouldn't run"

**The invoker decides WHEN to run try-fix. The skill decides WHAT alternative to try.**

## Core Principles

1. **Single-shot**: Each invocation = ONE fix idea, tested, reported
2. **Context-driven**: All necessary information provided upfront by invoker
3. **Alternative-focused**: Always propose something DIFFERENT from existing fixes
4. **Empirical**: Actually implement and test - don't just theorize
5. **Informative**: Report what was tried, what happened, and why

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

**All try-fix artifacts MUST be saved to:**
```
CustomAgentLogsTmp/PRState/<PRNumber>/try-fix/<attempt_number>/
```

**Required files per attempt:**

| File | Description |
|------|-------------|
| `approach.md` | Brief description of the fix approach |
| `fix.diff` | Git diff of the changes made |
| `test-output.log` | Full test command output |
| `result.txt` | PASS or FAIL |
| `analysis.md` | Detailed analysis of why it worked/failed |

**Example structure:**
```
CustomAgentLogsTmp/
└── PRState/
    └── 27847/
        └── try-fix/
            ├── attempt-1/
            │   ├── approach.md
            │   ├── fix.diff
            │   ├── test-output.log
            │   ├── result.txt
            │   └── analysis.md
            ├── attempt-2/
            │   ├── approach.md
            │   ├── fix.diff
            │   ├── test-output.log
            │   ├── result.txt
            │   └── analysis.md
            └── summary.md  # Overall summary of all attempts
```

**Auto-detecting PR number:**
```bash
# From branch name (e.g., pr-27847)
PR_NUMBER=$(git branch --show-current | sed -n 's/^pr-\([0-9]\+\).*/\1/p')

# Or use gh cli
PR_NUMBER=$(gh pr view --json number -q .number 2>/dev/null)
```

**Creating output directory:**
```bash
# Determine attempt number (count existing attempts + 1)
ATTEMPT_NUM=$(ls -d CustomAgentLogsTmp/PRState/$PR_NUMBER/try-fix/attempt-* 2>/dev/null | wc -l)
ATTEMPT_NUM=$((ATTEMPT_NUM + 1))

# Create output directory
OUTPUT_DIR="CustomAgentLogsTmp/PRState/$PR_NUMBER/try-fix/attempt-$ATTEMPT_NUM"
mkdir -p "$OUTPUT_DIR"
```

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

🚨 **ALWAYS use the EstablishBrokenBaseline.ps1 script - NEVER manually revert files.**

```bash
# REQUIRED: Establish baseline - reverts fix files to merge-base state
pwsh .github/scripts/EstablishBrokenBaseline.ps1
```

**Why this script is mandatory:**
- ✅ **Systematically reverts fix files** to merge-base (broken) state
- ✅ **Preserves test files** (doesn't touch them - tests stay on current branch)
- ✅ **Tracks what was reverted** for proper restoration later
- ✅ **Handles edge cases** (new files, deleted files, merge conflicts)
- ❌ Manual `git checkout` is error-prone and may revert tests accidentally

**Advanced usage (optional):**
```bash
# Specify base branch explicitly
pwsh .github/scripts/EstablishBrokenBaseline.ps1 -BaseBranch main

# Specify fix files manually (if auto-detection fails)
pwsh .github/scripts/EstablishBrokenBaseline.ps1 -FixFiles @("src/path/to/file.cs")

# Preview what would be reverted without making changes
pwsh .github/scripts/EstablishBrokenBaseline.ps1 -DryRun
```

**The script automatically:**
- Auto-detects merge-base from PR metadata or common branch patterns
- Identifies fix files (non-test files that changed since merge-base)
- Reverts only files that existed at merge-base (preserves new files)
- Saves state to `.git/baseline-state.json` for `-Restore` to undo later

**CRITICAL:** If something fails mid-attempt, always restore:
```bash
pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore
```

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

### Step 5.5: Work Through Compile Errors

**CRITICAL: Do NOT give up on first compile error!**

If your fix has compile errors, **iterate to fix them** before running tests:

1. **Attempt the build** (often your test command will do this automatically)
2. **If compile errors occur:** Read error messages (CS#### codes), diagnose, fix, rebuild
3. **Iterate up to 3 times** to resolve compile errors
4. **Only mark as FAIL** if you cannot get the code to compile after reasonable effort

See `references/compile-errors.md` for common error patterns and resolution strategies.

### Step 6: Run Tests

Run the provided test command and **save output to the attempt directory**:

```bash
# Use the exact test command provided in inputs, capturing output
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform $PLATFORM -TestFilter "$TEST_FILTER" 2>&1 | tee "$OUTPUT_DIR/test-output.log"

# Save test exit code
TEST_EXIT_CODE=$?
```

**Capture the result:**
- ✅ **PASS** - Fix works (test passes)
- ❌ **FAIL** - Fix doesn't work (test fails or other issues)

```bash
# Save result to file
if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo "PASS" > "$OUTPUT_DIR/result.txt"
else
    echo "FAIL" > "$OUTPUT_DIR/result.txt"
fi
```

### Step 7: Analyze Result and Save Analysis

**If PASS:**
- Why did this fix work?
- Is it the minimal change needed?
- Any concerns or caveats?

**If FAIL:**
- What was your hypothesis?
- What actually happened?
- Why was the reasoning flawed?
- What insight does this provide for future attempts?

**Save your analysis:**
```bash
cat > "$OUTPUT_DIR/analysis.md" << 'EOF'
# Fix Analysis

## Result
[PASS/FAIL]

## Hypothesis
[What you thought would happen]

## What Actually Happened
[Actual behavior observed]

## Why
[Explanation of success or failure]

## Insights for Future Attempts
[What was learned]
EOF
```

### Step 8: Capture Diff and Approach

Before reverting, save all artifacts:

```bash
# Save the approach description
cat > "$OUTPUT_DIR/approach.md" << 'EOF'
# Fix Approach

[Brief description of what was tried]

## Strategy
[What approach/pattern was used]

## Why Different from Existing Fixes
[How this differs from PR changes or prior attempts]
EOF

# Save the diff
git diff > "$OUTPUT_DIR/fix.diff"
```
```

### Step 9: Restore Working Directory (MANDATORY)

🚨 **ALWAYS use the EstablishBrokenBaseline.ps1 -Restore command - NEVER skip this step.**

```bash
# REQUIRED: Restore files reverted by EstablishBrokenBaseline.ps1 in Step 2
pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore
```

**What this does:**
- ✅ Reads the saved state from Step 2 (`.git/baseline-state.json`)
- ✅ Restores ONLY the files that were reverted to merge-base
- ✅ Preserves test files (they remain unchanged)
- ✅ Returns working directory to exact pre-attempt state

**After restoring baseline, revert your alternative fix changes:**
```bash
# Revert any changes made during the fix attempt
git checkout -- .
```

**CRITICAL:** Always run `-Restore` even if your fix attempt failed. This ensures proper cleanup for subsequent attempts.

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

**Exhausted:** Yes/No
**Reasoning:** [Why you believe there are/aren't more viable approaches]
```

### Determining Exhaustion

Before updating the state file, evaluate if you've exhausted viable approaches:

**Set `exhausted=true` when:**
- You've tried the same fundamental approach multiple times with variations
- All hints have been explored without success
- Failure analysis reveals the problem is outside the target files
- No new ideas remain based on prior failure analyses

**Set `exhausted=false` when:**
- This is the first attempt
- Failure analysis suggests a different approach within target files
- Hints remain unexplored
- The approach was close but needs refinement

### Step 11: Update State File (if provided)

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

**Skill execution:** Reads context → Analyzes target files → Designs fix (add IsDisposed check) → Applies fix → Runs test (PASS) → Reports result using Step 10 template → Reverts changes

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
