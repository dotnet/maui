---
description: "Quick reference card for PR reviewer commands and patterns"
---

# PR Reviewer Quick Reference Card

Quick reference for common commands, patterns, and decisions during PR reviews.

**📚 For complete workflows**: See [Shared Platform Workflows](../shared/platform-workflows.md)

---

## 🎯 App Selection Decision (ONE RULE)

```
User says: "Review PR" / "Test this fix" / "Validate changes"
→ Use Sandbox app ✅

User says: "Write UI tests" / "Validate UI tests"  
→ Use HostApp ✅

When in doubt → Sandbox app
```

---

## 📦 Platform Testing - Use BuildAndRunSandbox.ps1

**CRITICAL**: Use the `BuildAndRunSandbox.ps1` script for all Sandbox app testing. It handles all device detection, building, deployment, Appium management, and log capture automatically.

### Quick Start

**Script location**: `.github/scripts/BuildAndRunSandbox.ps1`

```powershell
# iOS
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform ios

# Android
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform android
```

**What the script handles**:
- ✅ Automatic device detection and boot (iPhone Xs for iOS, first available for Android)
- ✅ Building Sandbox app (always fresh build)
- ✅ App installation and deployment
- ✅ Appium server management (auto-start/stop)
- ✅ Running your Appium test script (`CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs`)
- ✅ Complete log capture to `CustomAgentLogsTmp/Sandbox/` directory:
  - `appium.log` - Appium server logs
  - `android-device.log` or `ios-device.log` - Device logs filtered to Sandbox app
  - Console output from your test script

**Prerequisites**:
1. Create `CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs` using template:
   ```bash
   cp .github/scripts/templates/RunWithAppiumTest.template.cs CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs
   ```
2. Modify the template for your test scenario

📖 **Full documentation**: See [Common Testing Patterns](../common-testing-patterns.md)

---

## 🤖 UI Automation with Appium (REQUIRED for UI interaction)

**CRITICAL: Use Appium for ANY user-visible interaction**

### When Appium is REQUIRED:
- ✅ Tapping buttons, controls, UI elements
- ✅ Opening menus, drawers, flyouts  
- ✅ Scrolling, swiping, gestures
- ✅ Entering text, rotating device
- ✅ ANY action a user would perform

### When ADB/xcrun ARE acceptable:
- ✅ `adb devices` - Check device connection
- ✅ `adb logcat` - Monitor logs (read-only)
- ✅ `adb shell getprop` - Read device properties (read-only)
- ✅ `xcrun simctl list` - List simulators
- ✅ `xcrun simctl boot` - Boot simulator
- ✅ Device setup/configuration (not UI interaction)

### Quick Appium Script Template:

**CRITICAL**: Use the template file provided in the repository:

```bash
# Copy template to CustomAgentLogsTmp/Sandbox directory
cp .github/scripts/templates/RunWithAppiumTest.template.cs CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs

# Edit the file to customize for your test scenario:
# 1. Set ISSUE_NUMBER constant
# 2. Set PLATFORM constant ("android" or "ios")
# 3. Implement test logic in the "Test Logic" section
```

**The template includes**:
- ✅ Proper .NET 10 scripting syntax (`#:package` directive)
- ✅ Platform-specific Appium configuration
- ✅ Automatic device UDID handling
- ✅ PID capture for Android logcat filtering
- ✅ Error handling and troubleshooting guidance
- ✅ Screenshot capture before/after test
- ✅ Complete example test logic to customize

**Run with**: `pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform <android|ios>`

📖 **Full Appium guide**: [../appium-control.instructions.md](../appium-control.instructions.md)

---

## 🧪 Test Code Template (Copy-Paste)

**Full instrumentation patterns**: See [Instrumentation Guide](../instrumentation.md)

### XAML

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.MainPage"
             Title="Issue #XXXXX Test">

    <!-- Red parent, yellow child for visual debugging -->
    <Grid x:Name="RootGrid" BackgroundColor="Red">
        <ContentView x:Name="TestElement"
                     BackgroundColor="Yellow"
                     Loaded="OnLoaded">
            <Label Text="Test Content" x:Name="ContentLabel"/>
        </ContentView>
    </Grid>
</ContentPage>
```

### Code-Behind

```csharp
using Microsoft.Maui.Controls;
using System;

namespace Maui.Controls.Sample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            // Wait for layout
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
            {
                CaptureState("OnLoaded");
            });
        }

        private void CaptureState(string context)
        {
            Console.WriteLine($"=== TEST OUTPUT: {context} ===");
            Console.WriteLine($"Element Bounds: {TestElement.Bounds}");
            Console.WriteLine($"Element Width: {TestElement.Width}, Height: {TestElement.Height}");
            
            if (ContentLabel != null)
            {
                Console.WriteLine($"Content Position: X={ContentLabel.X}, Y={ContentLabel.Y}");
            }
            
            var screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            Console.WriteLine($"Screen Size: {screenWidth}x{screenHeight}");
            Console.WriteLine("=== END TEST OUTPUT ===\n");
        }
    }
}
```

---

## 🔄 Test WITH/WITHOUT PR Pattern

```bash
# After fetching PR to test-pr-XXXXX branch

# 1. Test WITHOUT fix (baseline)
NUM_COMMITS=$(git log --oneline $ORIGINAL_BRANCH..HEAD | wc -l)
git checkout -b baseline-test HEAD~$NUM_COMMITS

# Build, deploy, capture baseline output
# ...

# 2. Test WITH fix
git checkout test-pr-XXXXX

# Build, deploy, capture improved output
# ...

# 3. Compare results in review
```

---

## 🛑 Checkpoint Templates

**Complete checkpoint guidance**: See [Shared Checkpoints](../shared/checkpoints.md)

### Before Building (MANDATORY)

```markdown
## 🛑 Validation Checkpoint - Before Building

**Test code created** (Sandbox app modified):

### XAML / Code
[Show test code]

### What I'm Measuring
[Explanation]

### Expected Results
- WITHOUT PR: [baseline]
- WITH PR: [improved]

### Build Time: ~10-15 minutes

Should I proceed with building?
```

📖 **Full template**: [Shared Checkpoints - Before Building](../shared/checkpoints.md#checkpoint-before-building-mandatory)

---

## 🚨 Common Errors - Quick Fixes

**📚 Complete error handling**: See [Shared Error Handling](../shared/error-handling-common.md)

| Error | Quick Fix | Full Details |
|-------|-----------|--------------|
| Build tasks not found | `dotnet build ./Microsoft.Maui.BuildTasks.slnf` | [Details](../shared/error-handling-common.md#error-build-tasks-not-found) |
| Dependency conflicts | `rm -rf bin/ obj/ && dotnet restore --force` | [Details](../shared/error-handling-common.md#error-dependency-version-conflicts) |
| PublicAPI errors | `dotnet format analyzers Microsoft.Maui.sln` | [Details](../shared/error-handling-common.md#error-publicapi-analyzer-failures) |
| App crashes on launch | Read logs for exception | [Details](../shared/error-handling-common.md#error-app-crashes-on-launch) |
| Zero measurements | Add delay: `Dispatcher.DispatchDelayed(500ms)` | [Details](../shared/error-handling-common.md#error-measurements-show-zero-or-null) |

---

## 🎯 CollectionView Handler Detection

**If PR affects CollectionView/CarouselView:**

```bash
# Check which files changed
git diff $ORIGINAL_BRANCH..HEAD --name-only | grep "Handlers/Items"

# Look for:
# "Items/" (NOT Items2) → Enable CollectionViewHandler
# "Items2/" → Enable CollectionViewHandler2
```

**Configuration** (add to `MauiProgram.cs`):

```csharp
#if IOS || MACCATALYST
builder.ConfigureMauiHandlers(handlers =>
{
    // For Items/ path:
    handlers.AddHandler<CollectionView, 
        Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler>();
    
    // For Items2/ path:
    handlers.AddHandler<CollectionView, 
        Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
});
#endif
```

---

## 📝 Review Structure Template

```markdown
## Summary
[2-3 sentences: what PR does, your assessment]

## Code Review
[WHY analysis, not just WHAT changed]

## Testing
**WITHOUT PR**: [Baseline behavior] ❌
**WITH PR**: [Fixed behavior] ✅
Verified on [platform].

## Issues Found
[None / List specific issues]

## Recommendation
✅ Approve / ⚠️ Request Changes / 💬 Comment / ⏸️ Paused
```

---

## 🧹 Cleanup

```bash
# Return to starting branch
git checkout $ORIGINAL_BRANCH

# Revert Sandbox changes
git checkout -- src/Controls/samples/Controls.Sample.Sandbox/

# Delete test branches
git branch -D test-pr-* baseline-test pr-*-temp 2>/dev/null || true
```

---

## ⏱️ Time Budgets

| PR Type | Expected Time |
|---------|---------------|
| Simple (property fix) | 30-45 min |
| Medium (bug fix) | 1-2 hours |
| Complex (architecture/SafeArea) | 2-4 hours |

**Exceeding budget?** Use checkpoint system, ask for help.

---

## 🔗 Quick Links

**Shared Resources**:
- 📦 [Platform Workflows](../shared/platform-workflows.md) - Complete iOS/Android workflows
- 🚨 [Error Handling](../shared/error-handling-common.md) - Build/deploy error solutions
- 🔧 [Fix Patterns](../shared/fix-patterns.md) - Common code patterns
- 🛑 [Checkpoints](../shared/checkpoints.md) - Validation templates

**PR Reviewer Specific**:
- **Full instructions**: [testing-guidelines.md](testing-guidelines.md)
- **Agent-specific errors**: [error-handling.md](error-handling.md)

**Specialized Guides**:
- **Instrumentation**: [../instrumentation.md](../instrumentation.md)
- **SafeArea testing**: [../safearea-testing.md](../safearea-testing.md)
- **Command patterns**: [../common-testing-patterns.md](../common-testing-patterns.md)

---

## ✅ Self-Check Before Building

- [ ] Am I using Sandbox app (not HostApp)?
- [ ] Did I show test code to user?
- [ ] Did I get approval to proceed?
- [ ] Will my test capture measurable data?
- [ ] Do I plan to test BOTH with and without PR?

**All YES?** Proceed with build.  
**Any NO?** Stop and correct.
