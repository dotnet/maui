---
name: write-ui-tests
description: Creates UI tests for a GitHub issue and verifies they reproduce the bug. Iterates until tests actually fail (proving they catch the issue). Use when PR lacks tests or tests need to be created for an issue.
metadata:
  author: dotnet-maui
  version: "1.1"
compatibility: Requires git, PowerShell, .NET SDK, and Appium for UI test execution.
---

# Write UI Tests Skill

Creates UI tests that reproduce a GitHub issue, following .NET MAUI conventions. **Verifies the tests actually fail before completing.**

## 🛑 BLOCKING REQUIREMENT

**YOU CANNOT COMPLETE THIS SKILL UNTIL TESTS FAIL.**

A test that passes does NOT prove it catches the bug. You MUST:
1. Run tests and observe them **FAIL**
2. If tests pass, **iterate on test code** until they fail
3. Never report "done" with passing tests

If tests keep passing after 3 iterations:
- STOP and ask user: "Tests are passing but they should fail to prove they catch the bug. The test scenario may not correctly reproduce the issue. Should I try a different approach?"

**Common mistakes that lead to passing tests:**
- Test scenario doesn't match issue reproduction steps
- Checking wrong element or property
- Bug only manifests on specific platform (try different platform)
- Bug requires specific timing or async behavior not captured
- Issue description is incomplete - may need to ask user for clarification

## When to Use

- ✅ PR has no tests and needs them
- ✅ Issue needs a reproduction test before fixing
- ✅ Existing tests don't adequately cover the bug

## Required Input

Before invoking, ensure you have:
- **Issue number** (e.g., 33331)
- **Issue description** or reproduction steps
- **Platforms affected** (iOS, Android, Windows, MacCatalyst)

**Platform selection guidance:**
- Start with the platform mentioned in the issue (often in title or labels)
- If issue says "iOS" or has `platform/iOS` label → test on iOS first
- If issue says "Android" or has `platform/Android` label → test on Android first
- If issue affects "All" platforms → start with Android (faster emulator boot)
- If test passes on one platform, try another before concluding test is wrong

## Workflow

### Step 1: Read the UI Test Guidelines

```bash
cat .github/instructions/uitests.instructions.md
```

This contains the authoritative conventions for:
- File naming (`IssueXXXXX.cs` for C#-only, or `IssueXXXXX.xaml`/`.xaml.cs` for XAML)
- File locations (`TestCases.HostApp/Issues/`, `TestCases.Shared.Tests/Tests/Issues/`)
- Required attributes (`[Issue()]`, `[Category()]`)
- Test patterns and assertions

### Step 2: Create HostApp Page

**Location:** `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.cs`

```csharp
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, XXXXX, "Brief description of issue", PlatformAffected.All)]
public partial class IssueXXXXX : ContentPage
{
    public IssueXXXXX()
    {
        // Create UI that reproduces the issue
        var button = new Button 
        { 
            Text = "Test Button",
            AutomationId = "TestButton"  // Required for Appium
        };
        
        var resultLabel = new Label
        {
            Text = "Waiting...",
            AutomationId = "ResultLabel"
        };
        
        button.Clicked += (s, e) => 
        {
            resultLabel.Text = "Success";
        };
        
        Content = new VerticalStackLayout
        {
            Children = { button, resultLabel }
        };
    }
}
```

**Key requirements:**
- Add `AutomationId` to all interactive elements
- Use `[Issue()]` attribute with tracker, number, description, platform
- Keep UI minimal - just enough to reproduce the bug

**Note:** XAML is optional. C#-only pages (as shown above) are simpler and preferred for most test scenarios. Use XAML only when the bug specifically relates to XAML parsing or markup behavior.

### Step 3: Create NUnit Test

**Location:** `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`

```csharp
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class IssueXXXXX : _IssuesUITest
{
    public override string Issue => "Brief description matching HostApp";

    public IssueXXXXX(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.Button)]  // Pick ONE appropriate category
    public void ButtonClickUpdatesLabel()
    {
        // Wait for element to be ready
        App.WaitForElement("TestButton");

        // Interact with the UI
        App.Tap("TestButton");

        // Verify expected behavior
        var labelText = App.FindElement("ResultLabel").GetText();
        Assert.That(labelText, Is.EqualTo("Success"));
    }
}
```

**Key requirements:**
- Inherit from `_IssuesUITest`
- Use same `AutomationId` values as HostApp
- Add ONE `[Category()]` attribute (check `UITestCategories.cs` for options)
- Use `App.WaitForElement()` before interactions

### Step 4: Verify Files Compile

```bash
# For Android
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -c Debug -f net10.0-android --no-restore -v q

# For iOS
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -c Debug -f net10.0-ios --no-restore -v q

# Test project (platform-independent)
dotnet build src/Controls/tests/TestCases.Shared.Tests/Controls.TestCases.Shared.Tests.csproj -c Debug --no-restore -v q
```

### Step 5: Verify Tests Reproduce the Bug ⚠️ CRITICAL

**Tests must FAIL to prove they catch the bug.** Run verification:

```bash
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform <platform> -TestFilter "IssueXXXXX"
```

Replace `<platform>` with `android`, `ios`, or `maccatalyst` based on the issue's affected platforms.

The script auto-detects that only test files exist (no fix files) and runs in "verify failure only" mode.

> **Why FAIL = success?** The test must fail NOW (before the fix) to prove it catches the bug. After the fix is applied, it should pass. A test that passes now proves nothing.

**If tests FAIL** → ✅ Success! Tests correctly reproduce the bug. Proceed to Output.

**If tests PASS** → ❌ **STOP.** Test doesn't catch the bug. Iterate:

1. **Re-read the issue reproduction steps** - Is your test doing exactly what the issue describes?
2. **Check if you're testing the right thing** - Are you asserting on the correct element/property?
3. **Try a different platform** - Bug may only manifest on iOS vs Android
4. **Add debug output** - Use `Console.WriteLine` in HostApp to trace execution
5. **Simplify** - Remove complexity until you isolate the bug behavior
6. **After 3 failed iterations, STOP and ask user:**
   > "Tests are passing after 3 iterations. This means either: (a) my test scenario doesn't correctly reproduce the bug, (b) the bug may already be fixed on this branch, or (c) I'm missing something from the issue description. How would you like me to proceed?"

**Common reasons tests pass when they shouldn't:**
| Symptom | Likely Cause | Fix |
|---------|--------------|-----|
| Test passes on all attempts | Test scenario doesn't match bug | Re-read issue reproduction steps carefully |
| Test asserts pass but bug exists | Asserting wrong property/element | Check what exactly the bug affects |
| Works on Android, fails on iOS | Bug is platform-specific | Try both platforms |
| Bug involves timing | Race condition not captured | Add delays or event handlers |
| Bug involves navigation | Page lifecycle not exercised | Ensure pages are actually pushed/popped |

**Do NOT mark this skill complete until tests FAIL.**

## Output

**⚠️ ONLY use this output format if tests FAIL.** If tests pass, you have not completed this skill.

After completion (tests verified to fail), report:
```markdown
✅ Tests created and verified for Issue #XXXXX

**Files:**
- `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.cs`
- `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`

**Test method:** `ButtonClickUpdatesLabel`
**Category:** `UITestCategories.Button`
**Verification:** Tests FAIL as expected (bug reproduced)
**Failure message:** `Expected "X" but got "Y"` (include actual assertion failure)
```

**If tests PASS after multiple iterations**, report instead:
```markdown
⚠️ Tests created but NOT verified for Issue #XXXXX

**Files:** [list files]
**Status:** Tests PASS when they should FAIL
**Iterations tried:** 3
**Problem:** [describe why test may not be catching the bug]
**Next steps:** Need guidance on reproduction steps
```

## Common Patterns

### Testing Property Changes
```csharp
// HostApp: Add a way to trigger and observe the property
var picker = new Picker { AutomationId = "TestPicker" };
var statusLabel = new Label { AutomationId = "StatusLabel" };
picker.PropertyChanged += (s, e) => {
    if (e.PropertyName == nameof(Picker.IsOpen))
        statusLabel.Text = $"IsOpen={picker.IsOpen}";
};

// Test: Verify the property changes correctly
App.Tap("TestPicker");
App.WaitForElement("StatusLabel");
var status = App.FindElement("StatusLabel").GetText();
Assert.That(status, Does.Contain("IsOpen=True"));
```

### Testing Layout/Positioning
```csharp
// Test: Use GetRect() for position/size assertions
var rect = App.WaitForElement("TestElement").GetRect();
Assert.That(rect.Height, Is.GreaterThan(0));
Assert.That(rect.Y, Is.GreaterThanOrEqualTo(safeAreaTop));
```

### Testing Visual State (Screenshots)
```csharp
// Use retryTimeout for animations - keeps retrying until success
App.Tap("AnimatedButton");
VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));

// retryTimeout handles timing variance, small tolerance for cross-machine rendering
VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
```

### Testing Platform-Specific Behavior
```csharp
// Only limit platforms when NECESSARY
[Test]
[Category(UITestCategories.Picker)]
public void PickerDismissResetsIsOpen()
{
    // This test should run on all platforms unless there's
    // a specific technical reason it can't
    App.WaitForElement("TestPicker");
    // ...
}
```

## iOS Device Selection

When running tests on iOS, you may need to target a specific device or iOS version:

```bash
# Default: iPhone Xs with iOS 18.5
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "Issue12345"

# Find iPhone Xs with iOS 18.5 and get its UDID
UDID=$(xcrun simctl list devices available --json | jq -r '
  .devices | to_entries 
  | map(select(.key | contains("iOS-18-5"))) 
  | map(.value) | flatten 
  | map(select(.name == "iPhone Xs")) | first | .udid')

# Run with specific device
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "Issue12345" -DeviceUdid "$UDID"
```

**Finding different device/version combinations:**
```bash
# iPhone 16 Pro with any iOS version
UDID=$(xcrun simctl list devices available --json | jq -r '
  .devices[][] | select(.name == "iPhone 16 Pro") | .udid' | head -1)

# Any device with iOS 18.0
UDID=$(xcrun simctl list devices available --json | jq -r '
  .devices | to_entries 
  | map(select(.key | contains("iOS-18-0"))) 
  | map(.value) | flatten | .[0].udid')
```

## Pre-Run Checklist

Before running `verify-tests-fail.ps1`, confirm:

- [ ] HostApp file exists: `TestCases.HostApp/Issues/IssueXXXXX.cs`
- [ ] NUnit test file exists: `TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`
- [ ] `[Issue()]` attribute present with all parameters
- [ ] All `AutomationId` values match between HostApp and test
- [ ] Test inherits from `_IssuesUITest`
- [ ] ONE `[Category()]` attribute from `UITestCategories.cs`

## References

- **Full conventions:** `.github/instructions/uitests.instructions.md`
- **Category list:** `src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs`
- **Example tests:** `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/`
