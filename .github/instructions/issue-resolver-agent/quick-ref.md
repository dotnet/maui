---
description: "Command cheat sheet and templates for issue resolution"
---

# Issue Resolver Quick Reference

Copy-paste commands and templates for common operations. Your go-to resource during active work.

**📚 For complete workflows**: See [Shared Platform Workflows](../shared/platform-workflows.md)

---

## Table of Contents

- [Reproduction Workflows](#reproduction-workflows)
- [Instrumentation Templates](#instrumentation-templates)
- [Common Fix Patterns](#common-fix-patterns)
- [UI Test Checklist](#ui-test-checklist)
- [PR Description Template](#pr-description-template)
- [Checkpoint Templates](#checkpoint-templates)
- [Common Errors & Solutions](#common-errors--solutions)

---

## Reproduction Workflows

### 🚨 CRITICAL: Use TestCases.HostApp ONLY

**All issue resolution work MUST be done in TestCases.HostApp**:
- ✅ Create test pages in HostApp/Issues/IssueXXXXX.xaml
- ✅ Write UI tests in TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs
- ✅ Run with BuildAndRunHostApp.ps1
- ❌ NEVER use Sandbox app for issue resolution

#### Step 1: Create Test Page in HostApp

```bash
# Create XAML test page
# File: src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml
# - Add XAML markup that reproduces the issue scenario
# - Include AutomationId attributes on all interactive elements
# - Follow the pattern of existing Issue*.xaml files

# Create code-behind
# File: src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml.cs
# - Add [Issue(IssueTracker.Github, XXXXX, "Description", PlatformAffected.All)] attribute
# - Implement the reproduction logic
# - Add instrumentation with Console.WriteLine for debugging
```

#### Step 2: Create UI Test

```bash
# Create NUnit test
# File: src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs
# - Inherit from _IssuesUITest
# - Add test method with [Test] and [Category] attributes
# - Use Appium to interact with the test page (WaitForElement, Tap, etc.)
# - Add assertions to verify the issue reproduces (test should fail before fix)
```

#### Step 3: Run Test to Verify Reproduction

```bash
# Android
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "IssueXXXXX"

# iOS
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "IssueXXXXX"
```

**What it does**:
- ✅ Builds TestCases.HostApp for target platform
- ✅ Auto-detects devices/simulators
- ✅ Manages Appium (starts/stops automatically)
- ✅ Runs dotnet test with your filter
- ✅ Captures all logs (Appium + device + test output)
- ✅ Saves logs to `CustomAgentLogsTmp/UITests/` directory

**Logs are in**: `CustomAgentLogsTmp/UITests/` directory
- `appium.log` - Appium server logs
- `android-device.log` / `ios-device.log` - Device logs (filtered to app PID)
- `test-output.log` - Test execution results

**Test behavior**: 
- ❌ Test FAILS without fix (proves you've reproduced the bug)
- ✅ Test PASSES with fix (proves the fix works)

---

## Instrumentation Templates

**📚 Complete instrumentation guide**: See [Instrumentation Instructions](../instrumentation.md)

### Quick Patterns

```csharp
// Basic instrumentation
void SomeMethod()
{
    Console.WriteLine($"[DEBUG] SomeMethod called - Parameter: {someValue}");
    // ... original code ...
    Console.WriteLine($"[DEBUG] SomeMethod completed");
}

// Lifecycle tracking
public MyControl()
{
    Console.WriteLine($"[LIFECYCLE] MyControl constructor - ID: {this.GetHashCode()}");
}

// Property mapper
public static void MapProperty(IMyHandler handler, IMyView view)
{
    Console.WriteLine($"[MAPPER] MapProperty: {view.Property}");
    // ... mapping code ...
}
```

📖 **Full patterns**: [Instrumentation Instructions](../instrumentation.md)

---

## UI Automation with Appium

### For ALL Issue Resolution Work (TestCases.HostApp)

**CRITICAL: When writing UI tests, use Appium - NOT adb/xcrun commands**

#### When Appium is REQUIRED:
- ✅ Writing UI tests in TestCases.HostApp
- ✅ Verifying UI interactions during testing
- ✅ ANY test that involves tapping, scrolling, or UI gestures

#### When ADB/xcrun ARE acceptable:
- ✅ `adb devices` - Check device connection (for troubleshooting)
- ✅ `xcrun simctl list` - List simulators (for troubleshooting)
- ❌ **NEVER manually run** `adb logcat` - The BuildAndRunHostApp.ps1 script captures all logs automatically

📖 **UI test guide**: [UI Tests Instructions](../uitests.instructions.md)
📖 **Appium scripting**: [Appium Control Scripts](../appium-control.md)

---

## Common Fix Patterns

**📚 Complete patterns library**: See [Shared Fix Patterns](../shared/fix-patterns.md)

### Quick Patterns

```csharp
// Null check
if (Handler is null) return;

// Platform-specific
#if IOS || MACCATALYST
    // iOS code
#elif ANDROID
    // Android code
#endif

// Property change
if (_myProperty == value) return;
_myProperty = value;
OnPropertyChanged();

// Lifecycle cleanup
protected override void DisconnectHandler(PlatformView platformView)
{
    platformView?.SomeEvent -= OnSomeEvent;
    base.DisconnectHandler(platformView);
}
```

📖 **Full patterns**: [Shared Fix Patterns](../shared/fix-patterns.md)

---

## UI Test Checklist

Every fix MUST include UI tests.

**📚 For complete UI test writing guidance, see**: [UI Tests Instructions](../uitests.instructions.md)

### Quick Checklist

**Before writing UI tests, read the uitests.instructions.md file.** It covers:
- Two-project requirement (HostApp page + Shared.Tests test)
- File naming conventions (IssueXXXXX pattern)
- AutomationId requirements
- Test structure and patterns
- Running tests locally
- Platform coverage rules

### Minimal Quick Reference

1. **Create HostApp Test Page**: `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml` + `.xaml.cs`
   - Add `[Issue()]` attribute in code-behind
   - Include `AutomationId` on all interactive elements

2. **Create NUnit Test**: `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`
   - Inherit from `_IssuesUITest`
   - Add `[Test]` and ONE `[Category]` attribute
   - Use Appium: `App.WaitForElement()`, `App.Tap()`, etc.

3. **Run Test**: 
   ```bash
   pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "IssueXXXXX"
   ```

4. **Verify**: Test fails without fix, passes with fix

📖 **See [uitests.instructions.md](../uitests.instructions.md) for complete details, examples, and patterns**

---

## PR Description Template

```markdown
<!-- ALWAYS keep this note for users -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

### Description of Change

[Brief description of what was changed and why]

### Issue Fixed

Fixes #XXXXX

### Root Cause

[Technical explanation of WHY the bug existed]

### Solution

[Explanation of HOW the fix addresses the root cause]

**Files Changed**:
- `src/Core/src/Platform/iOS/SomeHandler.cs` - Fixed null reference in UpdateProperty
- `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml` - Added UI test page
- `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs` - Added automated test

### Testing

**Manual Testing**:
- Platform: iOS 18.0, Android 14
- Verified: [Scenario description]
- Result: Issue no longer reproduces

**Automated Testing**:
- Added UI test `IssueXXXXX` that verifies the fix
- Test fails without fix, passes with fix
- Screenshot verification included

**Edge Cases Tested**:
1. [Edge case 1]
2. [Edge case 2]

### Before/After Evidence

**Before (bug)**:
```
[Console output or description showing the bug]
```

**After (fixed)**:
```
[Console output or description showing it works]
```

**Screenshots**:
[If visual issue, include before/after screenshots]

---

### PR Checklist

- [ ] Tests pass locally
- [ ] UI tests included
- [ ] No breaking changes
- [ ] PublicAPI.Unshipped.txt updated if needed
- [ ] Code formatted (`dotnet format`)
```

---

## Checkpoint Templates

**📚 Complete checkpoint guidance**: See [Shared Checkpoints](../shared/checkpoints.md)

### Checkpoint 1: After Reproduction (MANDATORY)

```markdown
## 🛑 Checkpoint 1: Issue Reproduced

**Platform**: [iOS/Android/Windows/Mac]
**Evidence**: [Console output showing bug]
**Root Cause Hypothesis**: [Initial analysis]
**Proposed Investigation**: [What you'll examine]

Should I proceed with investigating the fix?
```

📖 **Full template**: [Shared Checkpoints - Checkpoint 1](../shared/checkpoints.md#checkpoint-1-after-reproduction-mandatory)

### Checkpoint 2: Before Implementation (MANDATORY)

```markdown
## 🛑 Checkpoint 2: Fix Design

**Root Cause**: [WHY the bug exists]
**Proposed Solution**: [HOW to fix it]
**Implementation Plan**: [Steps]
**Files to Modify**: [List]
**Edge Cases**: [Priority list]

Should I proceed with implementation?
```

📖 **Full template**: [Shared Checkpoints - Checkpoint 2](../shared/checkpoints.md#checkpoint-2-before-implementation-mandatory)

---

## Common Errors & Solutions

**📚 Complete error handling**: See [Shared Error Handling](../shared/error-handling-common.md)

### Quick Fixes

| Error | Quick Fix | Full Details |
|-------|-----------|-------------|
| Build tasks not found | `dotnet build ./Microsoft.Maui.BuildTasks.slnf` | [Details](../shared/error-handling-common.md#error-build-tasks-not-found) |
| Dependency conflicts | `rm -rf bin/ obj/ && dotnet restore --force` | [Details](../shared/error-handling-common.md#error-dependency-version-conflicts) |
| PublicAPI errors | `dotnet format analyzers Microsoft.Maui.sln` | [Details](../shared/error-handling-common.md#error-publicapi-analyzer-failures) |
| App crashes on launch | Read logs for exception | [Details](../shared/error-handling-common.md#error-app-crashes-on-launch) |
| Zero measurements | Add delay: `Dispatcher.DispatchDelayed(500ms)` | [Details](../shared/error-handling-common.md#error-measurements-show-zero-or-null) |

📖 **See also**: [Issue-Specific Error Handling](error-handling.md) - Cannot reproduce, fix failures, etc.

**"BuildAndRunHostApp.ps1 fails to find device"**:
```bash
# Check ADB connection (Android)
adb devices

# Restart ADB if needed
adb kill-server
adb start-server
adb devices

# Or manually specify device
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -DeviceUdid <your-device-id> -TestFilter "IssueXXXXX"
```

**"Need to manually install APK for troubleshooting"**:
```bash
# Only if BuildAndRunHostApp.ps1 fails and you need to debug
adb install -r artifacts/bin/Controls.TestCases.HostApp/Debug/net10.0-android/com.microsoft.maui.uitests-Signed.apk
```

### Test Failures

**"Element not found"**:
```csharp
// Problem: Timing issue
App.Tap("ButtonId");  // Fails if element not ready

// Solution: Wait first
App.WaitForElement("ButtonId", timeout: TimeSpan.FromSeconds(10));
App.Tap("ButtonId");
```

**"Screenshot mismatch"**:
- First run: Download baseline from CI artifacts
- Add to: `src/Controls/tests/TestCases.{Platform}.Tests/snapshots/`
- Filename must match test method name EXACTLY

**"Test passes locally but fails in CI"**:
- Check platform differences (CI may use different OS versions)
- Verify timing (CI may be slower, increase waits)
- Check logs in CI artifacts (appium_*.log)

### Git Issues

**"Uncommitted changes blocking checkout"**:
```bash
# Stash changes temporarily
git stash push -m "WIP: Issue XXXXX reproduction"

# Apply later
git stash pop
```

**"Branch diverged from main"**:
```bash
# Rebase onto latest main
git fetch origin
git rebase origin/main

# Resolve conflicts if any
# Then: git rebase --continue
```

---

## Quick Links

**During reproduction**:
- [Instrumentation Guide](../instrumentation.md)
- [Common Testing Patterns](../common-testing-patterns.md)

**During investigation**:
- [Root cause analysis](solution-development.md#root-cause-analysis)
- [Error handling](error-handling.md)

**During implementation**:
- [Platform-specific code](solution-development.md#platform-specific-considerations)
- [Testing edge cases](solution-development.md#edge-case-testing)

**Before PR**:
- [UI Testing Guide](../../../docs/UITesting-Guide.md)
- [PR submission checklist](pr-submission.md#pr-checklist)

---

## Time-Saving Tips

1. **Use instrumentation liberally** - Console logs are free, debugging is expensive
2. **Test on actual devices when possible** - Simulators hide some bugs
3. **Read ALL issue comments** - Solutions often in discussions
4. **Check linked PRs/issues** - Someone may have tried fixing before
5. **Ask early if stuck** - Don't waste hours on wrong approach

---

## Remember

- ✅ Two checkpoints are MANDATORY (reproduction and fix design)
- ✅ Every fix needs UI tests
- ✅ Test on affected platforms
- ✅ Document your findings

**When in doubt**: Ask before implementing. Checkpoints exist to save time.
