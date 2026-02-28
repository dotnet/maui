# Test-Only PR Handling Guidelines

This document provides guidance for AI agents when working with test-only PRs.

## Detection Criteria

A PR can be treated as **test-only** **only when both** of the following are true:
- **Diff classification (required):** The PR only adds or modifies test files and contains **ZERO** changes to non-test source files (see patterns below).
- **Intent signals (optional hints):** One or more of the following may suggest the PR is test-focused, but they MUST be confirmed by diff classification and can never override it:
  - Title contains `[Testing]` tag
  - PR has `area-testing` label
  - Description explicitly states it's for test coverage only

**Handling Mixed PRs:**

If a PR modifies both functional code AND test files (i.e., contains at least one non-test file change):
- ✅ **Treat as issue-fix PR** - Run full workflow (all 4 phases)
- ❌ **Do NOT use test-only workflow**, even if the title, labels, or description suggest it is test-only

Only use test-only workflow when PR contains **ZERO** changes to non-test source files.

**File path patterns for "test files":**
- `*/tests/*` or `*/Tests/*`
- `*TestCases.HostApp/*` (UI test pages)
- `*TestCases.Shared.Tests/*` (UI test code)
- `*.UnitTests/*`
- Snapshot files (`snapshots/`)

**Non-test files** (even one file makes it issue-fix):
- `src/Controls/src/` - Production controls
- `src/Core/src/` - Core framework
- `src/Essentials/src/` - Platform APIs
- Platform handlers outside test projects

## Workflow Differences

| Phase | Issue-Fix PR | Test-Only PR |
|-------|--------------|--------------|
| Pre-Flight | Gather context, identify fix approach | Gather context, identify test coverage |
| Gate | `verify-tests-fail-without-fix` (full verification) | Run tests directly (pass verification) |
| Fix | Multi-model try-fix exploration | **Skip entirely** |
| Report | Compare fix alternatives | pr-finalize + code review only |

## Gate Phase: Test Verification

### Issue-Fix PRs
```powershell
# Verify tests fail without fix, pass with fix
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 `
    -Platform android -RequireFullVerification
```

### Test-Only PRs

**Choose test runner based on test type:**

| Test Type | How to Run | Example |
|-----------|------------|---------|
| **UI Tests** | `BuildAndRunHostApp.ps1` | `pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "Issue33632"` |
| **Unit Tests** | `dotnet test` | `dotnet test src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj` |
| **XAML Tests** | `dotnet test` | `dotnet test src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj` |
| **Integration Tests** | `run-integration-tests` skill | See `.github/skills/run-integration-tests/` |
| **Device Tests** | `run-device-tests` skill | See `.github/skills/run-device-tests/` |

**UI Tests are most common for test-only PRs.** If unclear, check file paths:
- `TestCases.HostApp/` + `TestCases.Shared.Tests/` → UI tests
- `*.UnitTests/` → Unit tests
- Integration test projects → Integration tests

### Gate Success Criteria for Test-Only PRs

**Gate success criteria for test-only PRs:**
- Tests build successfully
- Tests run without crashes
- All executed tests pass (the final test run is green). Platform guards such as `TEST_FAILS_ON_*` should result in tests being skipped or compiled out on affected platforms, **not** failing.

**NOT Gate failures (when the test run is green):**
- Test isolation issues (relies on Order(1) navigation) → capture as a code review finding, do not block Gate if tests still pass
- Tests guarded with `#if TEST_FAILS_ON_*` (expected behavior) → the guarded test should not run on affected platforms
- Snapshot differences that have been reviewed and accepted as intentional changes → capture as a code review finding

**Gate failures:**
- Tests crash with unhandled exceptions
- Build fails
- Any failing tests in the final run (including failures caused by test isolation issues or snapshot mismatches)

## Common Test-Only PR Patterns

### Test Isolation Pattern

Many test classes use `[Test, Order(1)]` for navigation setup:

```csharp
[Test, Order(1)]
public void FirstTest() 
{
    App.Tap("FeatureButton");  // Navigate to feature page
}

[Test]
public void SubsequentTest()
{
    App.WaitForElement("Option");  // Assumes navigation happened
}
```

**This is an established pattern, not a bug.** Document in code review:
- ✅ "Test relies on Order(1) navigation - consider adding navigation for isolation"
- ❌ Do NOT require fixes to approve PR

### Platform Guards

Tests for known issues use guards:
```csharp
#if TEST_FAILS_ON_ANDROID
[Test]
public void TestScrolledEvent() { ... }
#endif
```

**This is expected behavior.** The test documents the bug but doesn't run on affected platforms.

## PR Finalization

### Title

**Recommend `[Testing]` prefix if missing:**
- ❌ Bad: "Additional Feature Matrix Test Cases for CollectionView"
- ✅ Good: "[Testing] Additional Feature Matrix Test Cases for CollectionView"

### Description

**Skip Note block recommendation** for test-only PRs. Users don't need to test artifacts for test infrastructure changes.

### Code Review Focus Areas

1. **Test quality**: Are tests properly structured?
2. **Platform guards**: Are known issues properly documented?
3. **AutomationIds**: Are all interactive elements accessible?
4. **Snapshots**: Are snapshot baselines included?
5. **Test isolation**: Does test rely on Order(1) navigation? (document, don't block)

## State File Documentation

For test-only PRs, document:
```markdown
**PR Type:** Test-Only
**Phase 3 (Fix):** Skipped - no bug to fix
**Phase 4 (Report):** Simplified - pr-finalize + code review only
```
