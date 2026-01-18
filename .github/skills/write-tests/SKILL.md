---
name: write-tests
description: Creates UI tests for a GitHub issue and verifies they reproduce the bug. Iterates until tests actually fail (proving they catch the issue). Use when PR lacks tests or tests need to be created for an issue.
compatibility: Requires git, PowerShell, .NET SDK, and Appium for UI test execution.
---

# Write Tests Skill

Creates UI tests that reproduce a GitHub issue, following .NET MAUI conventions. **Verifies the tests actually fail before completing.**

## When to Use

- ✅ PR has no tests and needs them
- ✅ Issue needs a reproduction test before fixing
- ✅ Existing tests don't adequately cover the bug

## Required Input

Before invoking, ensure you have:
- **Issue number** (e.g., 33331)
- **Issue description** or reproduction steps
- **Platforms affected** (iOS, Android, Windows, MacCatalyst)

## Workflow

### Step 1: Read the UI Test Guidelines

```bash
cat .github/instructions/uitests.instructions.md
```

This contains the authoritative conventions for:
- File naming (`IssueXXXXX.xaml`, `IssueXXXXX.cs`)
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

### Step 3: Create NUnit Test

**Location:** `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`

```csharp
namespace Microsoft.Maui.TestCases.Shared.Tests.Tests.Issues;

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
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -c Debug -f net10.0-android --no-restore -v q
dotnet build src/Controls/tests/TestCases.Shared.Tests/Controls.TestCases.Shared.Tests.csproj -c Debug --no-restore -v q
```

### Step 5: Verify Tests Reproduce the Bug ⚠️ CRITICAL

**Tests must FAIL to prove they catch the bug.** Run verification:

```bash
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform ios -TestFilter "IssueXXXXX"
```

The script auto-detects that only test files exist (no fix files) and runs in "verify failure only" mode.

**If tests FAIL** → ✅ Success! Tests correctly reproduce the bug.

**If tests PASS** → ❌ Your test is wrong. Go back to Step 2 and fix:
- Review test scenario against issue description
- Ensure test actions match reproduction steps
- Update and rerun until tests FAIL

**Do NOT mark this skill complete until tests FAIL.**

## Output

After completion (tests verified to fail), report:
```markdown
✅ Tests created and verified for Issue #XXXXX

**Files:**
- `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.cs`
- `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`

**Test method:** `ButtonClickUpdatesLabel`
**Category:** `UITestCategories.Button`
**Verification:** Tests FAIL as expected (bug reproduced)
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

## References

- **Full conventions:** `.github/instructions/uitests.instructions.md`
- **Category list:** `src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs`
- **Example tests:** `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/`
