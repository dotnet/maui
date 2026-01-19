# Windows Helix Device Tests Migration - Session Summary

## PR #33328: Migrate Windows Device Tests to Helix

### Current State
- **Branch**: `feature/windows-helix-device-tests`
- **Latest Commit**: `94bd50ec32`
- **Build Status**: ✅ Windows Helix Tests PASSED (Build 2026-01-19)

### What's Working ✅
1. **All 5 Windows device test projects build and run on Helix**:
   - Controls.DeviceTests (807 tests, 46 categories)
   - Core.DeviceTests (2088 tests)
   - Graphics.DeviceTests (33 tests)
   - Essentials.DeviceTests
   - MauiBlazorWebView.DeviceTests

2. **Pipeline refactored** to use `${{ each }}` loop for Windows builds

3. **Fixed tests**:
   - `MinimizeAndThenMaximizingWorks` - changed to validate `OverlappedPresenter.State`
   - `SwitchingWindowsPostsToTheNewWindow` - **UNSKIPPED AND PASSES** ✅
   - `GraphicsViewCanDrawBackgroundImage` - **UNSKIPPED AND PASSES** ✅
   - `GraphicsViewCanDrawInlineImage` - **UNSKIPPED AND PASSES** ✅

### Tests Successfully Unskipped 🎉
| Test | File | Original Skip Reason | Status |
|------|------|---------------------|--------|
| `SwitchingWindowsPostsToTheNewWindow` | `ActiveWindowTracker_Tests.cs` | "Window message handling does not work reliably in headless/CI" | ✅ PASSES |
| `GraphicsViewCanDrawBackgroundImage` | `GraphicsViewHandlerTests.cs` | "Win2D image loading in background thread fails on Windows Helix CI" | ✅ PASSES |
| `GraphicsViewCanDrawInlineImage` | `GraphicsViewHandlerTests.cs` | "Win2D image loading in draw loop fails on Windows Helix CI" | ✅ PASSES |

**Conclusion**: The skip reasons were outdated - these tests work fine on Helix!

### Known Issues (Not Blocking)
- **Unpackaged builds disabled**: Apps crash with 0xC000027B (Windows App SDK Bootstrap failure)
- **iOS/MacCatalyst failures**: Pre-existing, unrelated to this PR

---

## User Guidelines (CRITICAL - Follow These Rules)

### 1. NEVER SKIP TESTS
> "NEVER SKIP A TEST UNLESS I TELL YOU TO, DO NOT GIVE UP TRYING TO FIX A TEST UNLESS I TELL YOU TOO"

- Do NOT add `Skip = "..."` to any test
- Do NOT assume a test can't be fixed
- Always diagnose through logging first
- Only skip if user explicitly approves

### 2. PROVE THROUGH LOGGING, DON'T ASSUME
> "please don't assume you know without proving through logging"

- Add `System.Diagnostics.Debug.WriteLine($"[TEST] ...")` statements
- Check actual values in Helix logs before proposing fixes
- Don't guess at root causes

### 3. FOCUS ON WINDOWS ONLY
> "ignore ios and catalyst we are just working on windows"

- Only work on Windows device test failures
- iOS/MacCatalyst issues are out of scope

### 4. TRIGGER BUILDS BY MODIFYING COMMENT
To trigger `maui-pr-devicetests` build:
```bash
gh api repos/dotnet/maui/issues/comments/3762034064 -X PATCH -f body="/azp run maui-pr-devicetests    "
```
(Add/remove spaces at end to modify the comment)

---

## Key Files

| File | Purpose |
|------|---------|
| `eng/pipelines/arcade/stage-device-tests.yml` | Pipeline with Windows build/test stages |
| `eng/helix_xharness.proj` | Helix work item definitions |
| `eng/devices/run-windows-devicetests.cmd` | Batch script that runs on Helix agents |
| `eng/devices/windows.cake` | Cake script for building Windows device tests |

## Test Files Being Investigated

| Test | File |
|------|------|
| `SwitchingWindowsPostsToTheNewWindow` | `src/Essentials/test/DeviceTests/Tests/Windows/ActiveWindowTracker_Tests.cs` |
| `GraphicsViewCanDrawBackgroundImage` | `src/Core/tests/DeviceTests/Handlers/GraphicsView/GraphicsViewHandlerTests.cs` |
| `GraphicsViewCanDrawInlineImage` | `src/Core/tests/DeviceTests/Handlers/GraphicsView/GraphicsViewHandlerTests.cs` |

---

## Build Monitoring

### Check Build Status
```bash
gh api repos/dotnet/maui/commits/$(git rev-parse HEAD)/check-runs --jq '.check_runs[] | select(.name | contains("Windows")) | {name: .name, status: .status, conclusion: .conclusion}'
```

### Get Helix Logs
After Windows tests complete, check Azure DevOps build logs for Helix console output.

---

## Session History

1. **Agent crashed** - User asked to read PR commits and understand state
2. **Investigated Lifecycle test** - Added logging, discovered WinUI doesn't fire `Activated(Deactivated)` for programmatic minimize
3. **Fixed MinimizeAndThenMaximizingWorks** - Changed to validate `OverlappedPresenter.State`
4. **Enabled all Windows device tests** - Uncommented all 5 projects
5. **Refactored pipeline** - Used `${{ each }}` loop for builds (saved ~25 lines)
6. **Unskipped tests for diagnosis** - `SwitchingWindowsPostsToTheNewWindow`, GraphicsView tests
7. **Waiting for build** - To analyze failures and fix

---

*Last updated: 2026-01-19 09:21 UTC*
