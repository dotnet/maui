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

**Every invocation runs all 11 Workflow steps below.** Step 6 (Expert Self-Review) is performed inline against `.github/agents/maui-expert-reviewer.md` — do NOT spawn the `@maui-expert-reviewer` sub-agent. Step 7.5 refreshes the self-review if the test loop modified code so the recorded findings reflect the final diff. Step 8 enforces this via a file-existence gate on `reviewer-findings.json`.

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
| `findings_count` | Number of self-review findings recorded (0 = clean self-review) |

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
| `reviewer-findings.json` | After Step 6 (Self-Review), refreshed by Step 7.5 | JSON array of self-review findings — `[]` when clean. **MUST reflect the final diff.** |
| `reviewer-findings.diff` | After Step 6 (Self-Review), refreshed by Step 7.5 | Snapshot of `git diff` at the time the self-review was written. Step 7.5 compares this to the post-test-loop diff to detect drift. |
| `result.txt` | After Step 7 (Test) | Single word: `Pass`, `Fail`, or `Blocked` |
| `fix.diff` | After Step 7 (Test) | Output of `git diff` showing your changes |
| `test-output.log` | After Step 7 (Test) | Full output from test command |
| `analysis.md` | After Step 8 (Capture) | Why it worked/failed, insights learned, and a one-line self-review summary |

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
- [ ] **Expert self-review performed inline (Step 6) and `reviewer-findings.json` written** — `[]` if clean. **Refreshed by Step 7.5 if the test loop modified code, so the saved findings reflect the final diff.**
- [ ] Analysis provided (success explanation or failure reasoning with evidence)
- [ ] Artifacts saved to output directory (verified by Step 8 file-existence gate)
- [ ] Baseline restored (working directory clean)
- [ ] Results reported to invoker (including `findings_count`)

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

### Step 6: Expert Self-Review (MANDATORY — runs BEFORE testing)

🚨 **You perform this self-review yourself. Do NOT spawn the `@maui-expert-reviewer` sub-agent.** Step 8's file-existence gate enforces that `reviewer-findings.json` is written every attempt.

This step runs BEFORE testing so you can catch design flaws before spending time on build+test cycles.

**Procedure:**

1. **Read the rules.** View these specific sections of `.github/agents/maui-expert-reviewer.md`:
   - **`## Overarching Principles`** (8 numbered principles, near the top of the file) — apply to every fix
   - **`## Dimension Routing`** + **`### Always-Active Dimensions`** — pick the dimensions that match your changed files
   - For each routed dimension, jump to its CHECK list under `## Review Dimensions` (e.g., `### 1. Layout Measure-Arrange Correctness`)

   You only need the dimensions that match the files you actually touched plus the always-active ones — typically 3–6 sections, not all 30.

2. **Identify your changed files:**
   ```powershell
   git diff --name-only HEAD
   ```

   If you have NO code changes (e.g., Blocked because no device available before any fix was applied), still proceed to step 4 and write `'[]'` — the artifact gate is the enforcement mechanism.

3. **Walk your diff against the rules:**
   - For each Overarching Principle → does your diff violate it?
   - For each routed dimension → walk every CHECK rule against the relevant hunks
   - Always-Active dimensions (Logic and Correctness, Regression Prevention, Complexity Reduction) → apply regardless of file paths
   - Be honest. If unsure, flag it.

4. **Write findings to `$OUTPUT_DIR/reviewer-findings.json`.** Always write the file, even when there are zero findings. Use the same JSON format as the `@maui-expert-reviewer` agent (matches the GitHub Pull Request Review API):

   ```powershell
   # No findings — clean self-review (or no diff to review):
   '[]' | Set-Content "$OUTPUT_DIR/reviewer-findings.json"

   # With findings — JSON array of {path, line, body}:
   @'
   [
     {
       "path": "src/Core/src/Handlers/ScrollView/ScrollViewHandler.iOS.cs",
       "line": 42,
       "body": "**[major] Layout Measure-Arrange** — Content measured with unconstrained height but arranged with bounded height. Concrete scenario: ScrollView inside a Grid with Star row height."
     }
   ]
   '@ | Set-Content "$OUTPUT_DIR/reviewer-findings.json"
   ```

   Each entry has exactly 3 fields:
   - **`path`** (string) — file relative to repo root, must be a file present in your diff
   - **`line`** (integer ≥ 1) — line number on the changed (right) side of the diff. The line MUST appear in your diff — picking an unchanged line is wrong. Use `1` only as a fallback for file-level concerns where no single line captures the issue (e.g., missing import, structural concern).
   - **`body`** (string) — format `**[severity] Dimension** — description`. Severity is one of `critical`/`major`/`moderate`/`minor`.

5. **Validate the JSON parses and capture the count:**
   ```powershell
   try {
       $findings = @(Get-Content "$OUTPUT_DIR/reviewer-findings.json" -Raw | ConvertFrom-Json)
       $findingsCount = $findings.Count
       Write-Host "✅ reviewer-findings.json: $findingsCount findings"
   } catch {
       Write-Host "❌ reviewer-findings.json is invalid JSON: $_"
       throw
   }

   # Snapshot the diff that was reviewed — Step 7.5 uses this to detect whether the test loop mutated code.
   # Use Set-Content -Value with Out-String so the file is created even when the diff is empty
   # (a bare `git diff | Set-Content` does NOT create the file when the pipe is empty).
   Set-Content -Path "$OUTPUT_DIR/reviewer-findings.diff" -Value (git diff | Out-String) -NoNewline
   ```

   **Remember `$findingsCount`** — you will report it as `findings_count` in Step 10 and summarize it in `analysis.md` (Step 8).

6. **Fix critical/major findings BEFORE testing:**
   - If there are any `[critical]` or `[major]` findings → apply fixes for them in a single batch and rewrite `reviewer-findings.json` to reflect the new diff.
   - All `[moderate]` and `[minor]` findings → note in `analysis.md` (Step 8); do NOT iterate.
   - Only ONE correction round. Then proceed to Step 7 (Test).

**Threshold guidance.** Only record findings with a concrete failing scenario. Stylistic preferences and bikeshedding (see the **`## What NOT to Flag`** table in `maui-expert-reviewer.md`) are not findings. An empty `[]` is the correct output for a clean fix — do not invent findings to fill the file.

> **Why before testing?** Self-review catches design flaws (wrong null check, missing platform guard, thread safety issue) before you spend 5-15 minutes on a build+test cycle. It also runs when context is lightest — before test output floods the context window.

### Step 7: Test and Iterate (MANDATORY)

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
   - ✅ **Tests PASS** → Move to Step 7.5 (Refresh self-review if needed)
   - ❌ **Compile errors** → Fix compilation issues (see below), go to step 1
   - ❌ **Tests FAIL (runtime)** → Analyze failure, fix code, go to step 1
3. **Maximum 3 iterations** - If still failing after 3 attempts, analyze if approach is fundamentally flawed
4. **Document why** - If exhausted, explain what you learned and why the approach won't work

**Behavioral constraints:**
- ⚠️ **NEVER blame "test infrastructure"** - assume YOUR fix has a bug
- Compile errors mean "work harder" - not "give up"
- DO NOT manually build - always rerun the test command script

See [references/compile-errors.md](references/compile-errors.md) for error patterns and iteration examples.

### Step 7.5: Refresh Self-Review If Code Changed (MANDATORY)

🚨 **The test loop in Step 7 may modify code (compile-error fixes, runtime-error fixes). When that happens, the `reviewer-findings.json` written in Step 6 describes a stale diff — not the diff that will be captured in Step 8 and shipped to the reviewer.** This step re-runs the self-review against the *final* diff so the recorded findings always correspond to the actual fix.

**Procedure:**

1. **Detect drift.** Compare the current working-tree diff against the diff Step 6 reviewed.

   ```powershell
   # Force both sides to a single string. `git diff` assigned to a variable is a string[]
   # (one element per line); `-ne` between an array and a scalar is element-wise filtering,
   # not equality. Both must be normalized to the same shape before comparison.
   #
   # Also: `Get-Content -Raw` on a 0-byte file returns $null, not "". The Step 6 snapshot
   # creates a 0-byte file when the diff is empty (the documented Blocked-with-no-diff path),
   # so coalesce $null to "" via `?? ''` to avoid a false-positive "" -ne $null drift detection.
   $currentDiff  = (git diff | Out-String)
   $reviewedDiff = if (Test-Path "$OUTPUT_DIR/reviewer-findings.diff") {
       (Get-Content "$OUTPUT_DIR/reviewer-findings.diff" -Raw) ?? ''
   } else { '' }

   $diffChanged = ($currentDiff -ne $reviewedDiff)
   if (-not $diffChanged) {
       Write-Host "✅ Diff unchanged since Step 6 — self-review still current. Skip sub-steps 2 and 3."
   } else {
       Write-Host "🔁 Code changed during Step 7 — refreshing self-review against final diff..."
   }
   ```

2. **If `$diffChanged` is `$true`, re-do the Step 6 self-review against the new diff.** This is YOU walking the rules again — it is *not* something the script does. Repeat the same procedure from Step 6:

   1. **Re-list changed files:** `git diff --name-only HEAD`
   2. **Re-walk the rules** in `.github/agents/maui-expert-reviewer.md` — every Overarching Principle, the always-active dimensions, and any routed dimensions whose file paths now match.
   3. **Rewrite `$OUTPUT_DIR/reviewer-findings.json`** with the new findings (or `'[]'` if clean). The file MUST be overwritten — appending or leaving the old content is a bug. Use the same JSON schema documented in Step 6.

3. **Re-snapshot and re-validate.** Only after rewriting the JSON in sub-step 2, run:

   ```powershell
   # Re-snapshot the diff (matches Step 6's snapshot logic — works for empty diffs too).
   Set-Content -Path "$OUTPUT_DIR/reviewer-findings.diff" -Value (git diff | Out-String) -NoNewline

   # Re-validate the JSON parses and capture the new count.
   try {
       $findings = @(Get-Content "$OUTPUT_DIR/reviewer-findings.json" -Raw | ConvertFrom-Json)
       $findingsCount = $findings.Count
       Write-Host "✅ reviewer-findings.json refreshed: $findingsCount findings"
   } catch {
       Write-Host "❌ reviewer-findings.json is invalid JSON: $_"
       throw
   }
   ```

   > **Why no programmatic "did you actually rewrite the JSON" check?** A SHA256 hash sentinel rejects the legitimate byte-identical case (e.g., `[]` → `[]` after a small compile fix that introduces no new violations), and that case is common. The procedural enforcement is sub-step 2's explicit numbered list above, plus the example-invocation chain that walks the dimensions explicitly. If sub-step 2 is skipped, the JSON validates but ships a stale review — accept that risk in exchange for not blocking valid clean fixes.

**Severity handling is the same as Step 6.** If the refresh surfaces new `[critical]` or `[major]` findings, you may apply ONE more fix batch and re-run the test loop, then re-refresh. Do not loop indefinitely — if a fix introduces critical findings on the third pass, mark the attempt `Blocked` and explain in `analysis.md`.

### Step 8: Capture Artifacts (MANDATORY)

**Before reverting, save ALL required files to `$OUTPUT_DIR`:**

```powershell
# 1. Save result (MUST be exactly "Pass", "Fail", or "Blocked")
"Pass" | Set-Content "$OUTPUT_DIR/result.txt"  # or "Fail"

# 2. Save the diff (use Set-Content -Value with Out-String so the file is created
#    even when the diff is empty — a bare `git diff | Set-Content` does not create
#    the file when the pipe is empty, which would fail the artifact gate.)
Set-Content -Path "$OUTPUT_DIR/fix.diff" -Value (git diff | Out-String) -NoNewline

# 3. Save test output (should already exist from Step 7)
# Copy-Item "path/to/test-output.log" "$OUTPUT_DIR/test-output.log"

# 4. reviewer-findings.json should already exist from Step 6 (and may have been refreshed by Step 7.5)
# 4b. reviewer-findings.diff snapshot (used by Step 7.5 to detect drift)

# 5. Save analysis (include a one-line summary of self-review findings)
@"
## Analysis

**Result:** Pass/Fail/Blocked

**What happened:** [Description of test results]

**Why it worked/failed:** [Root cause analysis]

**Self-review:** [N findings: brief summary of each, or "clean — no findings"]

**Insights:** [What was learned that could help future attempts]
"@ | Set-Content "$OUTPUT_DIR/analysis.md"
```

**Verify all required files exist (this is the enforcement gate for Steps 6 and 7 — primarily `reviewer-findings.json` from Step 6, refreshed by Step 7.5 if needed):**

🚨 **The artifact check below MUST be wrapped so that Step 9 (Restore) ALWAYS runs even if the check fails.** A failed gate that skips restore would leave the worktree dirty and corrupt the next sequential try-fix attempt.

```powershell
# Run the file-existence check, but DEFER any throw until after Step 9 has restored the worktree.
$missing = @()
@("baseline.log", "approach.md", "result.txt", "fix.diff", "analysis.md", "test-output.log", "reviewer-findings.json", "reviewer-findings.diff") | ForEach-Object {
    if (Test-Path "$OUTPUT_DIR/$_") {
        Write-Host "✅ $_"
    } else {
        Write-Host "❌ MISSING: $_"
        $missing += $_
    }
}

# Record the gate result for use after Step 9 — DO NOT throw here.
if ($missing.Count -gt 0) {
    $gateFailureMessage = "Required artifacts missing: $($missing -join ', '). If 'reviewer-findings.json' is missing, Step 6 (Expert Self-Review) was not performed (or Step 7.5 did not refresh it after the test loop) — it is mandatory and must contain at least '[]' that reflects the final diff."
    Write-Host "⚠️  ARTIFACT GATE FAILED — proceeding to Step 9 restore before reporting failure."
    Write-Host $gateFailureMessage
    "Blocked" | Set-Content "$OUTPUT_DIR/result.txt" -Force
} else {
    $gateFailureMessage = $null
}
```

**If `$gateFailureMessage` was set:** Step 9 still runs (do NOT skip it). After Step 9 restores the worktree, surface the failure in Step 10's report — set `result.txt` to `Blocked` (already done above) and explain in `analysis.md` which artifact was missing. The next sequential attempt then starts from a clean worktree.

**Analysis quality matters.** Bad: "Didn't work". Good: "Fix attempted to reset state in OnPageSelected, but this fires after layout measurement. The cached value was already used."

### Step 9: Restore Working Directory (MANDATORY — runs even if Step 8 gate failed)

**ALWAYS restore, even if fix failed or Step 8 detected missing artifacts.** Skipping restore corrupts the next sequential try-fix attempt.

```bash
pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore
```

🚨 Use `EstablishBrokenBaseline.ps1 -Restore` — not `git checkout`, `git restore`, or `git reset` (see Step 2 for why).

### Step 10: Report Results

Provide structured output to the invoker:

```markdown
## Try-Fix Result

**Approach:** [Brief description of what was tried]

**Files Changed:**
- `path/to/file.cs` (+X/-Y lines)

**Result:** ✅ PASS / ❌ FAIL

**Self-Review:** N findings (X critical, Y major, Z moderate/minor) — see `reviewer-findings.json`

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
| Git state unrecoverable | Run `pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore` (see Step 2/9) |

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


