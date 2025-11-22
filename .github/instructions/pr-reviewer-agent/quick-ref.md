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

## 📦 Platform Testing - Quick Commands

**Full workflows with error checking**: See [Shared Platform Workflows](../shared/platform-workflows.md)

### iOS Quick Start

```bash
# Get UDID, boot, build, install, launch - see shared doc for full sequence
UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')
xcrun simctl boot $UDID 2>/dev/null || true
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-ios
xcrun simctl install $UDID artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-ios/iossimulator-arm64/Maui.Controls.Sample.Sandbox.app
xcrun simctl launch --console-pty $UDID com.microsoft.maui.sandbox > /tmp/ios_test.log 2>&1 &
```

📖 **Complete workflow**: [Shared Platform Workflows - iOS](../shared/platform-workflows.md#complete-ios-reproduction-workflow)

### Android Quick Start

**⚠️ CRITICAL**: If no device found, START AN EMULATOR - don't skip testing!

```bash
# 1. Check for device
export DEVICE_UDID=$(adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1)

# 2. If no device found, start emulator (see below)
if [ -z "$DEVICE_UDID" ]; then
    echo "❌ No device found. Starting emulator..."
    # See "Android Emulator Startup" section below
fi

# 3. Build and deploy
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-android -t:Run

# 4. Monitor logs
adb logcat > /tmp/android_test.log 2>&1 &
LOGCAT_PID=$!

# ... do testing ...

# 5. Stop logcat and view output
kill $LOGCAT_PID
cat /tmp/android_test.log | grep "TEST OUTPUT"
```

📖 **Complete workflow**: [Shared Platform Workflows - Android](../shared/platform-workflows.md#complete-android-reproduction-workflow)

---

### Android Emulator Startup (REQUIRED when no device)

**When to use**: `adb devices` shows no devices connected

```bash
# CRITICAL: Use subshell with & to persist emulator
cd $ANDROID_HOME/emulator && (./emulator -avd Pixel_9 -no-snapshot-load -no-audio -no-boot-anim > /tmp/emulator.log 2>&1 &)

# Wait for boot
adb wait-for-device
until [ "$(adb shell getprop sys.boot_completed 2>/dev/null)" = "1" ]; do
    sleep 2
    echo -n "."
done

echo "✅ Emulator ready"

# Now get device UDID
export DEVICE_UDID=$(adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1)
```

📖 **Full startup sequence with error handling**: [Shared Platform Workflows - Android Emulator](../shared/platform-workflows.md#android-emulator-startup)

---

## 🧪 Test Code Template (Copy-Paste)

**Full instrumentation patterns**: See [Instrumentation Guide](../instrumentation.instructions.md)

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
- **Instrumentation**: [../instrumentation.instructions.md](../instrumentation.instructions.md)
- **SafeArea testing**: [../safearea-testing.instructions.md](../safearea-testing.instructions.md)
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
