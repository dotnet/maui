---
name: try-fix
description: Attempts ONE alternative fix for a bug, tests it empirically, and reports results. ALWAYS explores a DIFFERENT approach from existing PR fixes. Use when CI or an agent needs to try independent fix alternatives. Invoke with problem description, test command, target files, and optional hints.
compatibility: Requires PowerShell, git, .NET MAUI build environment, Android/iOS device or emulator
---

# Try Fix Skill

Attempts ONE fix for a given problem. Receives all context upfront, tries a single approach, tests it, and reports what happened.

## Activation Guard

🚨 **This skill is ONLY for proposing and testing code fixes.** Do NOT activate for:
- Code review requests ("review this PR", "check code quality")
- PR summaries or descriptions ("what does this PR do?")
- Test-only requests ("run tests", "check CI status")
- General questions about code or architecture

If the prompt does not include a **problem to fix** and a **test command to verify**, this skill should not run.

## Core Principles

1. **Always run once activated** - Never question whether to run. The invoker decides WHEN, you decide WHAT alternative to try
2. **Single-shot** - Each invocation = ONE fix idea, tested, reported
3. **Alternative-focused** - Always propose something DIFFERENT from existing fixes (review PR changes first)
4. **Empirical** - Actually implement and test, don't just theorize
5. **Context-driven** - Work with what's provided and git history; don't search external sources

**Every invocation:** Review existing fixes → Think of DIFFERENT approach → Implement and test → Report results

## ⚠️ CRITICAL: Sequential Execution Only

🚨 **Try-fix runs MUST be executed ONE AT A TIME - NEVER in parallel.**

**Why:** Each try-fix run:
- Modifies the same target source files
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
| Test command | Yes | **Repository-specific script** to build and test. Use `BuildAndRunHostApp.ps1` for UI tests, `Run-DeviceTests.ps1` for device tests, or `dotnet test` for unit tests. The correct command is determined by the test type detected in the PR. **ALWAYS use the appropriate script - NEVER manually build/compile.** |
| Target files | Yes | Files to investigate for the fix |
| Platform | Yes | Target platform (`android`, `ios`, `windows`, `maccatalyst`) |
| Hints | Optional | Suggested approaches, prior attempts, or areas to focus on |
| Baseline | Optional | Git ref or instructions for establishing broken state (default: current state) |

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
$tryFixDir = "CustomAgentLogsTmp/PRState/$IssueNumber/PRAgent/try-fix"
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

> **Session limits:** Each try-fix *invocation* allows up to 3 compile/test iterations. The *calling orchestrator* controls how many invocations (attempts) to run per session (typically 4-5 as part of pr-review Phase 3).

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

2. **Review prior attempts if any are known:**
   - Note which approaches failed and WHY
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

**Do NOT search for external context.** Work with what's provided and the git history.

### Step 2: Establish Baseline (MANDATORY)

🚨 **ONLY use EstablishBrokenBaseline.ps1 — NEVER use `git checkout`, `git restore`, or `git reset` to revert fix files.**

The script auto-restores any previous baseline, tracks state, and prevents loops.
Manual git commands bypass all of this and WILL cause infinite loops in CI.

```powershell
pwsh .github/scripts/EstablishBrokenBaseline.ps1 *>&1 | Tee-Object -FilePath "$OUTPUT_DIR/baseline.log"
```

**Verify baseline was established:**
```powershell
Select-String -Path "$OUTPUT_DIR/baseline.log" -Pattern "Baseline established"
```

**If the script fails with "No fix files detected":** Report as `Blocked` — do NOT switch branches.

**If something fails mid-attempt:** `pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore`

### Step 3: Analyze Target Files

Read the target files to understand the code.

**Verify the platform code path before implementing.** Check which platform-specific file actually executes for the target scenario:
- Files named `.iOS.cs` compile for both iOS AND MacCatalyst
- Files named `.Android.cs` only compile for Android
- Some platforms use Legacy implementations (e.g., iOS NavigationPage uses `NavigationPage.Legacy.cs`, not `MauiNavigationImpl`)
If unsure which code path runs, check `AppHostBuilderExtensions` or handler registration to confirm.

**Key questions:**
- What is the root cause of this bug?
- Where should the fix go?
- What's the minimal change needed?

### Step 4: Design ONE Fix

Based on your analysis and any provided hints, design a single fix approach:

- Which file(s) to change
- What the change is
- Why you think this will work

**"Different" means different ROOT CAUSE hypothesis, not just different code location.**
- ❌ Bad: PR checks `adapter == null` in OnMeasure; you check `adapter == null` in OnLayout (same root cause assumption — just a different call site)
- ✅ Good: PR checks `adapter == null`; you prevent disposal from happening during measure (different root cause hypothesis)

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

**For .NET MAUI repository:** Use the test script matching the test type:

| Test Type | Command |
|-----------|---------|
| UITest | `pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform <platform> -TestFilter "<filter>"` |
| DeviceTest | `pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project <project> -Platform <platform> -TestFilter "<filter>"` |
| UnitTest | `dotnet test <project.csproj> --filter "<filter>"` |

```powershell
# Capture output to test-output.log while also displaying it
# Example for UI tests:
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform <platform> -TestFilter "<filter>" *>&1 | Tee-Object -FilePath "$OUTPUT_DIR/test-output.log"

# Example for device tests:
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project <project> -Platform <platform> -TestFilter "<filter>" *>&1 | Tee-Object -FilePath "$OUTPUT_DIR/test-output.log"
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
```

🚨 Use `EstablishBrokenBaseline.ps1 -Restore` — not `git checkout`, `git restore`, or `git reset` (see Step 2 for why).

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
(paste `git diff` output here)

**This Attempt's Status:** Done/NeedsRetry
**Reasoning:** [Why this specific approach succeeded or failed]
```

**Determining Status:** Set `Done` when you've completed testing this approach (whether it passed or failed). Set `NeedsRetry` only if you hit a transient error (network timeout, flaky test) and want to retry the same approach.

## Error Handling

| Situation | Action |
|-----------|--------|
| Problem unclear | Report "insufficient context" - specify what's missing |
| Test command fails to run | Report build/setup error with details |
| Test times out | Report timeout, include partial output |
| Can't determine fix approach | Report "no viable approach identified" with reasoning |
| Git state unrecoverable | Run `pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore` (see Step 2/8) |

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


