---
name: verify-tests-fail-without-fix
description: Verifies UI tests catch the bug. Supports two modes - verify failure only (test creation) or full verification (test + fix validation).
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires git, PowerShell, and .NET SDK for building and running tests.
---

# Verify Tests Fail Without Fix

Verifies UI tests actually catch the issue. Supports two workflow modes:

## Activation Guard

🛑 **This skill ONLY verifies that existing tests reproduce a bug.** Do NOT activate for:
- Writing new tests → use write-tests-agent
- Running tests without verification context → use run-device-tests
- Code review → use code-review skill
- General test advice

Requires: a **platform** and either **test files in the PR** or an explicit **TestFilter**.

## ⚠️ CRITICAL: Inverted Pass/Fail Semantics

In this skill, test outcomes mean the OPPOSITE of normal:

| Test Result (without fix) | Verification Result | Why |
|--------------------------|--------------------|----|
| Tests FAIL | ✅ GOOD | Tests detect the bug |
| Tests PASS | ❌ BAD | Tests miss the bug |

NEVER say "verification passed" when tests PASS without the fix.

## Workflow

### Step 1: Determine Mode
- Check if fix files exist in the PR (non-test code changes detected by the script from the git diff)
- If **fix files present** → Full Verification mode (`-RequireFullVerification`)
- If **no fix files** → Verify Failure Only mode (omit the flag)

### Step 2: Construct Command
```powershell
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 `
  -Platform <platform> `
  -TestFilter "<filter>" `
  [-RequireFullVerification]  # Only if fix files exist
```

### Step 3: Interpret Results
⚠️ Remember: test outcomes are INVERTED from normal!
- Script outputs `VERIFICATION PASSED` → Tests catch the bug ✅
- Script outputs `VERIFICATION FAILED` → Tests don't catch the bug ❌
- Script outputs error/timeout → Report as Blocked

### Step 4: Report
- Report the result to the invoking orchestrator

## Mode 1: Verify Failure Only (Test Creation)

Use when **creating tests before writing a fix**:
- Runs tests to verify they **FAIL** (proving they catch the bug)
- No fix files required
- Perfect for test-first development

```bash
# Auto-detect test filter from changed test files
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform android

# With explicit test filter
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform ios -TestFilter "Issue33356"
```

## Mode 2: Full Verification (Fix Validation)

Use when **validating both tests and fix**:
1. **Without fix** - tests should FAIL (bug is present)
2. **With fix** - tests should PASS (bug is fixed)

```bash
# Auto-detect everything (recommended)
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform android -RequireFullVerification

# With explicit test filter
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform ios -TestFilter "Issue33356" -RequireFullVerification
```

**Note:** `-RequireFullVerification` ensures the script errors if no fix files are detected, preventing silent fallback to failure-only mode.

## Requirements

**Verify Failure Only Mode:**
- Test files in the PR (or working directory)

**Full Verification Mode:**
- Test files in the PR
- Fix files in the PR (non-test code changes)

The script auto-detects which mode to use based on whether fix files are present.

## Expected Output

**Verify Failure Only Mode:**
```
╔═══════════════════════════════════════════════════════════╗
║              VERIFICATION PASSED ✅                       ║
╠═══════════════════════════════════════════════════════════╣
║  Tests FAILED as expected!                                ║
║  This proves the tests correctly reproduce the bug.       ║
╚═══════════════════════════════════════════════════════════╝
```

**Full Verification Mode:**
```
╔═══════════════════════════════════════════════════════════╗
║              VERIFICATION PASSED ✅                       ║
╠═══════════════════════════════════════════════════════════╣
║  - FAIL without fix (as expected)                         ║
║  - PASS with fix (as expected)                            ║
╚═══════════════════════════════════════════════════════════╝
```

## What It Does

**Verify Failure Only Mode (no fix files):**
1. Fetches base branch from origin (if available)
2. Auto-detects test classes from changed test files
3. Runs tests (should FAIL to prove they catch the bug)
4. **Updates PR labels** based on result
5. Reports result

**Full Verification Mode (fix files detected):**
1. Fetches base branch from origin to ensure accurate diff
2. Auto-detects fix files (non-test code) from git diff
3. Auto-detects test classes from `TestCases.Shared.Tests/*.cs`
4. Reverts fix files to base branch
5. Runs tests (should FAIL without fix)
6. Restores fix files
7. Runs tests (should PASS with fix)
8. **Generates markdown reports**:
   - `CustomAgentLogsTmp/TestValidation/verification-report.md` - Full detailed report
   - `CustomAgentLogsTmp/PRState/verification-report.md` - Validate section for agent
9. **Updates PR labels** based on result
10. Reports result

## PR Labels

The skill automatically manages two labels on the PR to indicate verification status:

| Label | Color | When Applied |
|-------|-------|--------------|
| `s/ai-reproduction-confirmed` | 🟢 Green (#2E7D32) | Tests correctly FAIL without fix (AI verified tests catch the bug) |
| `s/ai-reproduction-failed` | 🟠 Orange (#E65100) | Tests PASS without fix (AI verified tests don't catch the bug) |

**Behavior:**
- When verification passes, adds `s/ai-reproduction-confirmed` and removes `s/ai-reproduction-failed` if present
- When verification fails, adds `s/ai-reproduction-failed` and removes `s/ai-reproduction-confirmed` if present
- If a PR is re-verified after fixing tests, labels are updated accordingly
- No label = AI hasn't verified tests yet

## Output Files

The skill generates output files under `CustomAgentLogsTmp/PRState/<PRNumber>/verify-tests-fail/`:

| File | Description |
|------|-------------|
| `verification-report.md` | Comprehensive markdown report with test results and full logs |
| `verification-log.txt` | Text log of the verification process |
| `test-without-fix.log` | Full test output from run without fix |
| `test-with-fix.log` | Full test output from run with fix |

**Plus UI test logs in** `CustomAgentLogsTmp/UITests/`:
- `android-device.log` or `ios-device.log` - Device logs
- `test-output.log` - NUnit test output

**Example structure:**
```
CustomAgentLogsTmp/
├── UITests/                           # Shared UI test logs
│   ├── android-device.log
│   └── test-output.log
└── PRState/
    └── 27847/
        └── verify-tests-fail/
            ├── verification-report.md  # Full detailed report
            ├── verification-log.txt
            ├── test-without-fix.log
            └── test-with-fix.log
```

**PR Number Detection:**
- Auto-detected from branch name (e.g., `pr-27847`)
- Falls back to `gh pr view` command
- Uses "unknown" if detection fails
- Can be manually specified with `-PRNumber` parameter

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| No fix files detected | Base branch detection failed or no non-test files changed | Use `-FixFiles` or `-BaseBranch` explicitly |
| Tests pass without fix | Tests don't detect the bug | Review test assertions, update test |
| Tests fail with fix | Fix doesn't work or test is wrong | Review fix implementation |
| App crashes | Duplicate issue numbers, XAML error | Check device logs |
| Element not found | Wrong AutomationId, app crashed | Verify IDs match |

## Optional Parameters

```bash
# Require full verification (fail if no fix files detected) - recommended
-RequireFullVerification

# Explicit test filter
-TestFilter "Issue32030|ButtonUITests"

# Explicit fix files  
-FixFiles @("src/Core/src/File.cs")

# Explicit base branch
-BaseBranch "main"
```
