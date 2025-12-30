---
name: verify-tests-fail-without-fix
description: Verifies that UI tests fail when the PR's fix is reverted, proving the tests actually catch the issue. Auto-detects fix files and tests from git diff.
---

# Verify Tests Fail Without Fix

Verifies UI tests actually catch the issue by reverting fix files and confirming tests fail.

## Usage

```bash
# Auto-detects everything - just specify platform
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform android
```

## What It Does

1. Auto-detects fix files (non-test code) from git diff
2. Auto-detects test classes from `TestCases.Shared.Tests/*.cs`
3. Reverts fix files to main
4. Runs tests (should FAIL without fix)
5. Restores fix files
6. Reports result

## Auto-Detection

**Fix files**: Changed files excluding test paths (`*/tests/*`, `*TestCases*`, `*.Tests/*`, etc.)

**Test classes**: Parses C# class names from changed test files - works with any naming pattern.

## Optional Parameters

```bash
# Explicit test filter
-TestFilter "Issue32030|ButtonUITests"

# Explicit fix files  
-FixFiles @("src/Core/src/File.cs")
```

## Expected Output

```
╔═══════════════════════════════════════════════════════════╗
║              VERIFICATION PASSED ✅                       ║
╠═══════════════════════════════════════════════════════════╣
║  Tests correctly detect the issue:                        ║
║  - FAIL without fix (as expected)                         ║
╚═══════════════════════════════════════════════════════════╝
```

## Troubleshooting

| Problem | Likely Cause | Solution |
|---------|--------------|----------|
| Tests pass without fix | Tests don't detect the bug | Review test assertions |
| App crashes | Duplicate issue numbers, XAML error | Check device logs |
| Element not found | Wrong AutomationId, app crashed | Verify IDs match |
