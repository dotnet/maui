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
4. Reports result

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
   - `CustomAgentLogsTmp/PRState/verification-report.md` - Gate section for PR agent
9. Reports result

## PR Labels

Labels are managed centrally by `Review-PR.ps1` Phase 4 using the shared helper module
(`.github/scripts/helpers/Update-AgentLabels.ps1`). This skill no longer applies labels directly.

Gate results are reflected via:
- `s/agent-gate-passed` — Tests correctly FAIL without fix (verified tests catch the bug)
- `s/agent-gate-failed` — Tests PASS without fix (tests don't catch the bug)

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
