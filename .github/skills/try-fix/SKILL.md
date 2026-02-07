---
name: try-fix
description: Attempts ONE alternative fix for a bug, tests it empirically, and reports results. ALWAYS explores a DIFFERENT approach from existing PR fixes. Use when CI or an agent needs to try independent fix alternatives. Invoke with problem description, test command, target files, and optional hints.
compatibility: Requires PowerShell, git, .NET MAUI build environment, Android/iOS device or emulator
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

## ⚠️ CRITICAL: Sequential Execution Only

🚨 **Try-fix runs MUST be executed ONE AT A TIME - NEVER in parallel.**

**Why:** Each try-fix run:
- Modifies the same source files (SafeAreaExtensions.cs, etc.)
- Uses the same device/emulator for testing
- Runs EstablishBrokenBaseline.ps1 which reverts files to a known state

**If run in parallel:**
- Multiple agents will overwrite each other's code changes
- Device tests will interfere with each other
- Baseline script will conflict, causing unpredictable file states
- Results will be corrupted and unreliable

**Correct pattern:** Run attempt-1, wait for completion, then run attempt-2, etc.

## Inputs

All inputs are provided by the invoker (CI, agent, or user).

| Input | Required | Description |
|-------|----------|-------------|
| Problem | Yes | Description of the bug/issue to fix |
| Test command | Yes | **Repository-specific script** to build, deploy, and test (e.g., `pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "Issue12345"`). **ALWAYS use this script - NEVER manually build/compile.** |
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
| `result` | `Pass`, `Fail`, or `Blocked` |
| `analysis` | Why it worked, or why it failed and what was learned |
| `diff` | The actual code changes made (for review) |

## Output Structure (MANDATORY)

**FIRST STEP: Create output directory before doing anything else.**

```powershell
# Set issue/PR number explicitly (from branch name, PR context, or manual input)
$IssueNumber = "<ISSUE_OR_PR_NUMBER>"  # Replace with actual number

# Find next attempt number
$tryFixDir = "CustomAgentLogsTmp/PRState/$IssueNumber/try-fix"
$existingAttempts = (Get-ChildItem "$tryFixDir/attempt-*" -Directory -ErrorAction SilentlyContinue).Count
$attemptNum = $existingAttempts + 1

# Create output directory
$OUTPUT_DIR = "$tryFixDir/attempt-$attemptNum"
New-Item -ItemType Directory -Path $OUTPUT_DIR -Force | Out-Null

Write-Host "Output directory: $OUTPUT_DIR"
```

**Required files to create in `$OUTPUT_DIR`:**

| File | When to Create | Content |
|------|----------------|---------|
| `baseline.log` | After Step 2 (Baseline) | Output from EstablishBrokenBaseline.ps1 proving baseline was established |
| `approach.md` | After Step 4 (Design) | What fix you're attempting and why it's different from existing fixes |
| `result.txt` | After Step 6 (Test) | Single word: `Pass`, `Fail`, or `Blocked` |
| `fix.diff` | After Step 6 (Test) | Output of `git diff` showing your changes |
| `test-output.log` | After Step 6 (Test) | Full output from test command |
| `analysis.md` | After Step 6 (Test) | Why it worked/failed, insights learned |

**Example approach.md:**
```markdown
## Approach: Geometric Off-Screen Check

Skip RequestApplyInsets for views completely off-screen using simple bounds check:
`viewLeft >= screenWidth || viewRight <= 0 || viewTop >= screenHeight || viewBottom <= 0`

**Different from existing fix:** Current fix uses HashSet tracking. This approach uses pure geometry with no state.
```

**Example result.txt:**
```
Pass
```



## Completion Criteria

The skill is complete when:
- [ ] Problem understood from provided context
- [ ] ONE fix approach designed and implemented
- [ ] Fix tested with provided test command (iterated up to 3 times if errors/failures)
- [ ] Either: Tests PASS ✅, or exhausted attempts and documented why approach won't work ❌
- [ ] Analysis provided (success explanation or failure reasoning with evidence)
- [ ] Artifacts saved to output directory
- [ ] Baseline restored (working directory clean)
- [ ] Results reported to invoker

🚨 **CRITICAL: What counts as "Pass" vs "Fail"**

| Scenario | Result | Explanation |
|----------|--------|-------------|
| Test command runs, tests pass | ✅ **Pass** | Actual validation |
| Test command runs, tests fail | ❌ **Fail** | Fix didn't work |
| Code compiles but no device available | ⚠️ **Blocked** | Device/emulator unavailable - report with explanation |
| Code compiles but test command errors | ❌ **Fail** | Infrastructure issue is still a failure |
| Code doesn't compile | ❌ **Fail** | Fix is broken |

**NEVER claim "Pass" based on:**
- ❌ "Code compiles successfully" alone
- ❌ "Code review validates the logic"
- ❌ "The approach is sound"
- ❌ "Device was unavailable but fix looks correct"

**Pass REQUIRES:** The test command executed AND reported test success.

**If device/emulator is unavailable:** Report `result.txt` = `Blocked` with explanation. Do NOT manufacture a Pass.

**Exhaustion criteria:** Stop after 3 iterations if:
1. Code compiles but tests consistently fail for same reason
2. Root cause analysis reveals fundamental flaw in approach
3. Alternative fixes would require completely different strategy

**Never stop due to:** Compile errors (fix them), infrastructure blame (debug your code), giving up too early.

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

**Review the provided context:**
- What is the bug/issue?
- What test command verifies the fix?
- What files should be investigated?
- Are there hints about what to try or avoid?

**Do NOT search for additional context.** Work with what's provided.

### Step 2: Establish Baseline (MANDATORY)

🚨 **ALWAYS use EstablishBrokenBaseline.ps1 - NEVER manually revert files.**

```powershell
# Capture baseline output as proof it was run
pwsh .github/scripts/EstablishBrokenBaseline.ps1 *>&1 | Tee-Object -FilePath "$OUTPUT_DIR/baseline.log"
```

The script auto-detects and reverts fix files to merge-base state while preserving test files. **Will fail fast if no fix files detected** - you must be on the actual PR branch. Optional flags: `-BaseBranch main`, `-DryRun`.

**Verify baseline was established:**
```powershell
# baseline.log should contain "Baseline established" and list of reverted files
Select-String -Path "$OUTPUT_DIR/baseline.log" -Pattern "Baseline established"
```

**If the script fails with "No fix files detected":** You're likely on the wrong branch. Checkout the actual PR branch with `gh pr checkout <PR#>` and try again.

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

**IMMEDIATELY create `approach.md`** in your output directory:

```powershell
@"
## Approach: [Brief Name]

[Description of what you're changing and why]

**Different from existing fix:** [How this differs from PR's current approach]
"@ | Set-Content "$OUTPUT_DIR/approach.md"
```

### Step 5: Apply the Fix

Implement your fix. Use `git status --short` and `git diff` to track changes.

### Step 6: Test and Iterate (MANDATORY)

🚨 **CRITICAL: ALWAYS use the provided test command script - NEVER manually build/compile.**

**For .NET MAUI repository:** Use `BuildAndRunHostApp.ps1` which handles:
- Building the project
- Deploying to device/simulator
- Running tests
- Capturing logs

```powershell
# Capture output to test-output.log while also displaying it
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform <platform> -TestFilter "<filter>" *>&1 | Tee-Object -FilePath "$OUTPUT_DIR/test-output.log"
```

**Testing Loop (Iterate until SUCCESS or exhausted):**

1. **Run the test command** - It will build, deploy, and test automatically
2. **Check the result:**
   - ✅ **Tests PASS** → Move to Step 7 (Capture Artifacts)
   - ❌ **Compile errors** → Fix compilation issues (see below), go to step 1
   - ❌ **Tests FAIL (runtime)** → Analyze failure, fix code, go to step 1
3. **Maximum 3 iterations** - If still failing after 3 attempts, analyze if approach is fundamentally flawed
4. **Document why** - If exhausted, explain what you learned and why the approach won't work

**Behavioral constraints:**
- ⚠️ **NEVER blame "test infrastructure"** - assume YOUR fix has a bug
- Compile errors mean "work harder" - not "give up"
- DO NOT manually build - always rerun the test command script

See [references/compile-errors.md](references/compile-errors.md) for error patterns and iteration examples.

### Step 7: Capture Artifacts (MANDATORY)

**Before reverting, save ALL required files to `$OUTPUT_DIR`:**

```powershell
# 1. Save result (MUST be exactly "Pass", "Fail", or "Blocked")
"Pass" | Set-Content "$OUTPUT_DIR/result.txt"  # or "Fail"

# 2. Save the diff
git diff | Set-Content "$OUTPUT_DIR/fix.diff"

# 3. Save test output (should already exist from Step 6)
# Copy-Item "path/to/test-output.log" "$OUTPUT_DIR/test-output.log"

# 4. Save analysis
@"
## Analysis

**Result:** Pass/Fail/Blocked

**What happened:** [Description of test results]

**Why it worked/failed:** [Root cause analysis]

**Insights:** [What was learned that could help future attempts]
"@ | Set-Content "$OUTPUT_DIR/analysis.md"
```

**Verify all required files exist:**
```powershell
@("baseline.log", "approach.md", "result.txt", "fix.diff", "analysis.md", "test-output.log") | ForEach-Object {
    if (Test-Path "$OUTPUT_DIR/$_") { Write-Host "✅ $_" } 
    else { Write-Host "❌ MISSING: $_" }
}
```

**Analysis quality matters.** Bad: "Didn't work". Good: "Fix attempted to reset state in OnPageSelected, but this fires after layout measurement. The cached value was already used."

### Step 8: Restore Working Directory (MANDATORY)

**ALWAYS restore, even if fix failed.**

```bash
pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore
git checkout HEAD -- .
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

**This Attempt's Status:** Done/NeedsRetry
**Reasoning:** [Why this specific approach succeeded or failed]
```

**Determining Status:** Set `Done` when you've completed testing this approach (whether it passed or failed). Set `NeedsRetry` only if you hit a transient error (network timeout, flaky test) and want to retry the same approach.

### Step 10: Update State File (if provided)

If `state_file` input was provided and file exists:

1. **Read current Fix Candidates table** from state file
2. **Determine next attempt number** (count existing try-fix rows + 1)
3. **Append new row** with this attempt's results:

| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| N | try-fix #N | [approach] | ✅ PASS / ❌ FAIL | [files] | [analysis] |

**If no state file provided:** Skip this step (results returned to invoker only).

**⚠️ Do NOT `git add` or `git commit` the state file.** It lives in `CustomAgentLogsTmp/` which is `.gitignore`d. Committing it with `git add -f` would cause `git checkout HEAD -- .` (used between phases) to revert it, losing data.

**⚠️ IMPORTANT: Do NOT set any "Exhausted" field.** Cross-pollination exhaustion is determined by the pr agent after invoking ALL 5 models and confirming none have new ideas. try-fix only reports its own attempt result.

**Ownership rule:** try-fix updates its own row ONLY. Never modify:
- Phase status fields
- "Selected Fix" field
- Other try-fix rows

## Error Handling

| Situation | Action |
|-----------|--------|
| Problem unclear | Report "insufficient context" - specify what's missing |
| Test command fails to run | Report build/setup error with details |
| Test times out | Report timeout, include partial output |
| Can't determine fix approach | Report "no viable approach identified" with reasoning |
| Git state unrecoverable | Run `git checkout HEAD -- .` and `git clean -fd` if needed |

---

## Guidelines for Proposing Fixes

### Good Fix Approaches

✅ **Null/state checks** - Guard against unexpected null or state
✅ **Lifecycle timing** - Move code to correct lifecycle event
✅ **Cache invalidation** - Reset stale cached values

### Approaches to Avoid

❌ **Massive refactors** - Keep changes minimal
❌ **Suppressing symptoms** - Fix root cause, not symptoms
❌ **Multiple unrelated changes** - ONE focused fix per invocation

---

See [references/example-invocation.md](references/example-invocation.md) for a complete example with sample inputs.


