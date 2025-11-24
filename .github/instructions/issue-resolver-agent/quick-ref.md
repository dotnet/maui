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

### Using BuildAndRunSandbox.ps1 (Recommended)

**One script handles everything**: Build, deploy, Appium testing, and log capture.

#### Step 1: Create Appium Test Script

```bash
# Copy template
cp .github/scripts/templates/RunWithAppiumTest.template.cs SandboxAppium/RunWithAppiumTest.cs

# Edit SandboxAppium/RunWithAppiumTest.cs:
# - Set ISSUE_NUMBER (replace 00000)
# - Set PLATFORM ("android" or "ios")
# - CUSTOMIZE the "Test Logic" section to match your Sandbox app:
#   * Find elements by AutomationId from your XAML
#   * Tap buttons, enter text, etc. to reproduce the issue
#   * Add assertions to verify expected behavior
#   * The template is just a starting point - modify it completely as needed!
```

#### Step 2: Modify Sandbox App (if needed)

```bash
# Edit Sandbox XAML
vim src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml

# Edit code-behind
vim src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs

# Add AutomationId attributes for Appium interaction
```

#### Step 3: Run Complete Test

```bash
# Android
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform android

# iOS (when supported)
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform ios
```

**What it does**:
- ✅ Builds Sandbox app from source
- ✅ Auto-detects devices
- ✅ Manages Appium (starts/stops automatically)
- ✅ Deploys and launches app
- ✅ Runs your Appium test script
- ✅ Captures all logs (Appium + device)
- ✅ Saves logs to `SandboxAppium/appium.log` and `SandboxAppium/logcat.log`

**Logs are in**: `SandboxAppium/` directory (persisted after script finishes)

📖 **Template reference**: `.github/scripts/templates/RunWithAppiumTest.template.cs`

### Manual Workflows (Advanced)

For manual testing without Appium, see [Instrumentation Guide](../instrumentation.md)

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

### For Issue Reproduction (Sandbox App)

**Use BuildAndRunSandbox.ps1** - See [Reproduction Workflows](#reproduction-workflows) above.

### For UI Tests (TestCases.HostApp)

**CRITICAL: When writing UI tests, use Appium - NOT adb/xcrun commands**

#### When Appium is REQUIRED:
- ✅ Writing UI tests in TestCases.HostApp
- ✅ Verifying UI interactions during testing
- ✅ ANY test that involves tapping, scrolling, or UI gestures

#### When ADB/xcrun ARE acceptable:
- ✅ `adb devices` - Check device connection (for troubleshooting)
- ✅ `xcrun simctl list` - List simulators (for troubleshooting)
- ❌ **NEVER manually run** `adb logcat` - The BuildAndRunSandbox.ps1 script captures all logs automatically

📖 **UI test guide**: [UI Tests Instructions](../uitests.instructions.md)
📖 **Appium scripting**: [Appium Control Scripts](../appium-control.instructions.md)

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

Every fix MUST include UI tests. Use this checklist:

### 1. Create HostApp Test Page

**File**: `src/Controls/tests/TestCases.HostApp/Issues/Issue{XXXXX}.xaml`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             Title="Issue XXXXX - [Brief Description]">

    <VerticalStackLayout Padding="20" Spacing="10">
        <!-- Every testable element needs AutomationId -->
        
        <Label Text="Test description or instructions"
               AutomationId="InstructionLabel"/>
        
        <Button Text="Trigger Bug"
                AutomationId="TriggerButton"
                Clicked="OnTriggerClicked"/>
        
        <Label Text="Result will appear here"
               AutomationId="ResultLabel"
               IsVisible="False"/>
    </VerticalStackLayout>
</ContentPage>
```

**Code-behind**: `src/Controls/tests/TestCases.HostApp/Issues/Issue{XXXXX}.xaml.cs`

```csharp
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, XXXXX, "Issue description", PlatformAffected.All)]
public partial class IssueXXXXX : ContentPage
{
    public IssueXXXXX()
    {
        InitializeComponent();
    }

    private void OnTriggerClicked(object sender, EventArgs e)
    {
        // Reproduce the bug scenario
        ResultLabel.IsVisible = true;
        ResultLabel.Text = "Expected behavior verified";
    }
}
```

### 2. Create NUnit Test

**File**: `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue{XXXXX}.cs`

```csharp
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class IssueXXXXX : _IssuesUITest
{
    public IssueXXXXX(TestDevice device) : base(device)
    {
    }

    public override string Issue => "Clear description of what this tests";
    
    [Test]
    [Category(UITestCategories.YourCategory)]  // Button, Label, Layout, etc.
    public void IssueXXXXX_TestDescription()
    {
        // Arrange - Wait for initial state
        App.WaitForElement("TriggerButton");
    
        // Act - Trigger the bug scenario
        App.Tap("TriggerButton");
    
        // Assert - Verify fix
        App.WaitForElement("ResultLabel");
        
        // Visual verification
        VerifyScreenshot();
    }
}
```

### 3. Run Test Locally

Use the BuildAndRunHostApp.ps1 script to build, deploy, and run your UI test:

**iOS**:
```bash
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "IssueXXXXX"
```

**Android**:
```bash
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "IssueXXXXX"
```

**All logs saved to**: `HostAppCustomAgentTmpLogs/`
- `appium.log` - Appium server logs
- `ios-device.log` / `android-device.log` - Device logs
- `test-results.log` - Test execution output

### 4. Verify Test Quality

- ✅ Test fails WITHOUT your fix
- ✅ Test passes WITH your fix
- ✅ AutomationIds are unique and descriptive
- ✅ Uses `VerifyScreenshot()` for visual validation
- ✅ Only ONE `[Category]` attribute

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

**"BuildAndRunSandbox.ps1 fails to find device"**:
```bash
# Check ADB connection (Android)
adb devices

# Restart ADB if needed
adb kill-server
adb start-server
adb devices

# Or manually specify device
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform android -DeviceUdid <your-device-id>
```

**"Need to manually install APK for troubleshooting"**:
```bash
# Only if BuildAndRunSandbox.ps1 fails and you need to debug
adb install -r artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-android/com.microsoft.maui.sandbox-Signed.apk
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
