---
name: verify-tests-fail-without-fix
description: Verifies UI tests catch the bug. Supports two modes - verify failure only (test creation) or full verification (test + fix validation).
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
║ Tests pass (in failure-only mode) | Tests don't detect the bug | Review test assertions, update test |
| Tests pass without fix (full mode) | Tests don't detect the bug | Review test assertions, update test |
| Tests fail with fix (full mode) | Fix doesn't work or test is wrong | Review fix implementation |
| No fix files with `-RequireFullVerification` | No non-test files changed or base branch detection failed | Use `-FixFiles` or `-BaseBranch` explicitly, or remove `-RequireFullVerification`
║  - PASS with fix (as expected)                            ║
╚═══════════════════════════════════════════════════════════╝
```

**Verify Failure Only Mode (no fix files):**
1. Fetches base branch from origin (if available)
2. Auto-detects test classes from changed test files
3. Runs tests (should FAIL to prove they catch the bug)
4. Reports result

**Full Verification Mode (fix files detected):**
## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| No fix files detected | Base branch detection failed or no non-test files changed | Use `-FixFiles` or `-BaseBranch` explicitly |
| Tests pass without fix | Tests don't detect the bug | Review test assertions, update test |
| Tests fail with fix | Fix doesn't work or test is wrong | Review fix implementation |
| App crashes | Duplicate issue numbers, XAML error | Check device logs |
| Element not found | Wrong AutomationId, app crashed | Verify IDs match |

## What It Does

1. Fetches base branch from origin to ensure accurate diff
2. Auto-detects fix files (non-test code) from git diff
3. Auto-detects test classes from `TestCases.Shared.Tests/*.cs`
4. Reverts fix files to base branch
5. Runs tests (should FAIL without fix)
6. Restores fix files
7. Runs tests (should PASS with fix)
8. Reports result

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
