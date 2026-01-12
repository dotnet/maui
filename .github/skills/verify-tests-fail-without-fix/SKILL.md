---
name: verify-tests-fail-without-fix
description: Verifies UI tests catch the bug by checking tests FAIL without the fix and PASS with the fix. Requires fix files to be detected in the PR.
---

# Verify Tests Fail Without Fix

Verifies UI tests actually catch the issue by running tests in two states:
1. **Without fix** - tests should FAIL (bug is present)
2. **With fix** - tests should PASS (bug is fixed)

## Usage

```bash
# Auto-detects everything - just specify platform (recommended for skill invocation)
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform android -RequireFullVerification

# With explicit test filter
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform ios -TestFilter "Issue33356" -RequireFullVerification
```

## Requirements

This skill requires:
- **Fix files** in the PR (non-test code changes)
- **Test files** in the PR (to verify against)

If no fix files are detected, the skill will error out with troubleshooting guidance.

## Expected Output

```
╔═══════════════════════════════════════════════════════════╗
║              VERIFICATION PASSED ✅                       ║
╠═══════════════════════════════════════════════════════════╣
║  - FAIL without fix (as expected)                         ║
║  - PASS with fix (as expected)                            ║
╚═══════════════════════════════════════════════════════════╝
```

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
