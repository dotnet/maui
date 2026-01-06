---
name: verify-tests-fail-without-fix
description: Verifies UI tests catch the bug. Auto-detects mode based on git diff - if fix files exist, verifies FAIL without fix and PASS with fix. If only test files, verifies tests FAIL.
---

# Verify Tests Fail Without Fix

Verifies UI tests actually catch the issue. **Mode is auto-detected based on git diff.**

## Usage

```bash
# Auto-detects everything - just specify platform
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform android

# With explicit test filter
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform ios -TestFilter "Issue33356"
```

## Auto-Detection

The script automatically determines the mode:

| Changed Files | Mode | Behavior |
|---------------|------|----------|
| Fix files + test files | Full verification | FAIL without fix, PASS with fix |
| Only test files | Verify failure only | Tests must FAIL (reproduce bug) |

**Fix files** = any changed file NOT in test directories
**Test files** = files in `TestCases.*` directories

## Expected Output

**Full mode (fix files detected):**
```
╔═══════════════════════════════════════════════════════════╗
║              VERIFICATION PASSED ✅                       ║
╠═══════════════════════════════════════════════════════════╣
║  - FAIL without fix (as expected)                         ║
║  - PASS with fix (as expected)                            ║
╚═══════════════════════════════════════════════════════════╝
```

**Verify failure only (no fix files):**
```
╔═══════════════════════════════════════════════════════════╗
║         VERIFICATION PASSED ✅                            ║
╠═══════════════════════════════════════════════════════════╣
║  Tests FAILED as expected (bug is reproduced)             ║
╚═══════════════════════════════════════════════════════════╝
```

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Tests pass without fix | Tests don't detect the bug | Review test assertions, update test |
| Tests pass (no fix files) | **Test is wrong** | Review test vs issue description, fix test |
| App crashes | Duplicate issue numbers, XAML error | Check device logs |
| Element not found | Wrong AutomationId, app crashed | Verify IDs match |

## What It Does

**Full mode:**
1. Auto-detects fix files (non-test code) from git diff
2. Auto-detects test classes from `TestCases.Shared.Tests/*.cs`
3. Reverts fix files to base branch
4. Runs tests (should FAIL without fix)
5. Restores fix files
6. Runs tests (should PASS with fix)
7. Reports result

**Verify Failure Only mode:**
1. Runs tests once
2. Verifies they FAIL (bug reproduced)
3. Reports result

## Auto-Detection

**Fix files**: Changed files excluding test paths (`*/tests/*`, `*TestCases*`, `*.Tests/*`, etc.)

**Test classes**: Parses C# class names from changed test files - works with any naming pattern.

## Optional Parameters

```bash
# Explicit test filter
-TestFilter "Issue32030|ButtonUITests"

# Explicit fix files  
-FixFiles @("src/Core/src/File.cs")

# Verify failure only (no fix exists yet)
-VerifyFailureOnly
```

## Expected Output

**Full mode (with fix):**
```
╔═══════════════════════════════════════════════════════════╗
║              VERIFICATION PASSED ✅                       ║
╠═══════════════════════════════════════════════════════════╣
║  Tests correctly detect the issue:                        ║
║  - FAIL without fix (as expected)                         ║
║  - PASS with fix (as expected)                            ║
╚═══════════════════════════════════════════════════════════╝
```

**Verify Failure Only mode:**
```
╔═══════════════════════════════════════════════════════════╗
║         VERIFICATION PASSED ✅                            ║
╠═══════════════════════════════════════════════════════════╣
║  Tests FAILED as expected (bug is reproduced)             ║
╚═══════════════════════════════════════════════════════════╝
```

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Tests pass without fix | Tests don't detect the bug | Review test assertions, update test |
| Tests pass in -VerifyFailureOnly | **Test is wrong** | Review test vs issue description, fix test |
| App crashes | Duplicate issue numbers, XAML error | Check device logs |
| Element not found | Wrong AutomationId, app crashed | Verify IDs match |
