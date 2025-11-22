# Error Handling and Troubleshooting

**For common build/deploy errors**: See [Shared Error Handling](../shared/error-handling-common.md)

This document covers issue resolver-specific errors: reproduction failures, fix development issues, and investigation challenges.

**Quick Links**:
- Common build errors: [Shared Error Handling](../shared/error-handling-common.md)
- Platform workflows: [Shared Platform Workflows](../shared/platform-workflows.md)
- Quick command reference: [quick-ref.md#common-errors--solutions](quick-ref.md#common-errors--solutions)

## When to Ask for Help

**Prioritize your time - don't get stuck indefinitely:**

🔴 **Ask immediately** (environment/infrastructure issues):
- SDK installation problems (wrong .NET version, missing workloads)
- IDE/tooling failures (VS Code crashes, extension errors)
- Repository configuration issues (git, permissions)
- Missing credentials or access

🟡 **Ask after 30 minutes** (stuck on technical issue):
- Cannot reproduce issue after trying multiple platforms/scenarios
- Build errors that common solutions don't fix
- Unclear how to instrument specific platform code
- Root cause analysis going in circles

🟢 **Ask after 2-3 retry attempts** (temporary failures):
- Intermittent test failures
- Emulator/simulator flakiness
- Network-related issues during build

**Remember**: Checkpoints exist to catch wrong approaches early. Use them.

## Reproduction Failures

### Issue Does Not Reproduce

**Common reasons and solutions:**

#### 1. Platform Differences

**Symptom**: Issue reproduces on reporter's device but not in simulator/emulator

**Diagnosis:**
```bash
# Check what device/OS reporter used
# Compare to your test environment
```

**Solutions:**

**iOS Version Mismatch:**
```bash
# Reporter used iOS 17.1, you're testing iOS 18.0
# Try older simulator if available

# List available iOS versions
xcrun simctl list runtimes | grep iOS

# Create simulator with specific iOS version if available
xcrun simctl create "iPhone 15 iOS 17.1" \
  "com.apple.CoreSimulator.SimDeviceType.iPhone-15" \
  "com.apple.CoreSimulator.SimRuntime.iOS-17-1"
```

**Android API Level Mismatch:**
```bash
# Reporter used Android 12 (API 31), you're testing Android 14 (API 34)
# Try emulator with matching API level

# List available system images
sdkmanager --list | grep system-images

# Create emulator with specific API level
avdmanager create avd -n Android31 -k "system-images;android-31;google_apis;x86_64"
```

**Real Device vs Simulator:**
- Some issues only reproduce on physical devices
- Request access to physical device if available
- Note in PR that physical device testing needed

#### 2. Timing/Race Conditions

**Symptom**: Issue is intermittent or timing-dependent

**Solutions:**

**Add delays to test different timing:**
```csharp
// Try different delays to expose race condition
private async void OnButtonClicked(object sender, EventArgs e)
{
    await Task.Delay(100);  // Try 0, 50, 100, 500ms
    
    // Rest of code
}
```

**Check for async/await issues:**
```csharp
// BAD: Fire and forget
UpdateSomethingAsync();  // Race condition

// GOOD: Proper await
await UpdateSomethingAsync();
```

**Add instrumentation to capture timing:**
```csharp
Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Event triggered");
```

#### 3. Data/State Dependencies

**Symptom**: Issue needs specific data or app state

**Solutions:**

**Vary the test data:**
```csharp
// Try different data scenarios
TestCollection.ItemsSource = new[] { "A" };  // Single item
TestCollection.ItemsSource = new[] { "A", "B", "C" };  // Few items
TestCollection.ItemsSource = Enumerable.Range(1, 100).Select(x => $"Item {x}");  // Many items
TestCollection.ItemsSource = new string[0];  // Empty
TestCollection.ItemsSource = null;  // Null
```

**Test different property combinations:**
```csharp
// Try different configurations
control.Property1 = "Value1";
control.Property2 = true;
control.Property3 = 42;

// vs
control.Property1 = null;
control.Property2 = false;
control.Property3 = 0;
```

**Test different page lifecycles:**
```csharp
// Issue might need navigation
await Navigation.PushAsync(new TestPage());
await Task.Delay(500);
await Navigation.PopAsync();
```

#### 4. Incomplete Reproduction Steps

**Symptom**: Issue report doesn't include all necessary steps

**Action**: Ask for clarification on the original issue:

```markdown
@[original-reporter] I'm attempting to reproduce this issue but need some additional information:

1. What exact device/OS version are you using?
2. Does this happen immediately or after specific actions?
3. Can you provide a minimal code sample that demonstrates the issue?
4. Are there any specific properties/settings required?
5. Does this happen in a new project or only in your specific app?

This will help me create an accurate fix. Thank you!
```

### When to Document "Cannot Reproduce"

If you genuinely cannot reproduce the issue after thorough attempts:

**Document your efforts:**
```markdown
## Investigation Results

**Attempted reproduction on:**
- iOS 18.0 Simulator (iPhone 15 Pro)
- Android 14.0 Emulator (Pixel 7)
- Windows 11

**Variations tested:**
- Different data sizes (empty, 1 item, 100 items)
- Different property combinations (FlowDirection, Padding, Margin)
- Different timing (immediate, delayed, rapid changes)
- Different page lifecycles (fresh load, navigation, modal)

**Result**: Unable to reproduce the reported behavior

**Next steps:**
- Requested additional information from reporter (see comment #123)
- May need physical device testing
- May be specific to reporter's environment

Waiting for more details before proceeding.
```

## Build Errors During Fix Development

**For common build errors**: See [Shared Error Handling - Build Errors](../shared/error-handling-common.md#build-errors)

**Note**: We assume your development environment is correctly set up with all required dependencies and platform SDKs. If you encounter environment setup issues, ask for guidance.

### Issue Resolver Specific Guidance

When a build error occurs during fix development:

**Assumption**: The baseline (code before your changes) compiled successfully. Focus on fixing the error your changes introduced.

#### Step 1: Try Common Fixes First

**Common build errors** (see [Shared Error Handling](../shared/error-handling-common.md#build-errors) for details):
- Build tasks not found → `dotnet build ./Microsoft.Maui.BuildTasks.slnf`
- Dependency conflicts → `rm -rf bin/ obj/ && dotnet restore --force`
- PublicAPI errors → `dotnet format analyzers Microsoft.Maui.sln` (NEVER disable the analyzer)

#### Step 2: Issue-Specific Build Errors

**Platform-Specific Compilation Errors**:

If you see `error CS0246: The type or namespace name 'UIKit' could not be found`:

```csharp
// Add proper conditional compilation
#if IOS || MACCATALYST
using UIKit;

internal static void UpdateLayoutDirection(this UICollectionViewCompositionalLayout layout)
{
    // iOS-specific code
}
#endif
```

**Verify targeting**:
```xml
<!-- In .csproj file -->
<TargetFrameworks>net10.0-android;net10.0-ios;net10.0-maccatalyst</TargetFrameworks>
```

**Null Reference Warnings** (`warning CS8602: Dereference of a possibly null reference`):

Add proper null checks:

```csharp
// ❌ BAD: Ignoring the warning
handler.PlatformView!.UpdateSomething();  // ! suppresses warning

// ✅ GOOD: Proper null handling
if (handler.PlatformView is not null)
    handler.PlatformView.UpdateSomething();

// OR
handler.PlatformView?.UpdateSomething();
```

#### Step 3: Build Error Decision Tree

```
Build error occurs during fix development
    │
    ├─ Check common patterns (Step 1):
    │  └─ PublicAPI → dotnet format analyzers
    │
    ├─ Check issue-specific patterns (Step 2):
    │  ├─ Platform compilation → Add conditional directives
    │  └─ Null warnings → Add proper null checks
    │
    └─ After 2-3 fix attempts fail:
           └─ Ask for guidance (see Step 4)
```

#### Step 4: When to Ask for Help

**Stop and ask for guidance if:**

1. **Error persists after 2-3 fix attempts**
2. **Error message is cryptic** ("Unknown error", unclear stack traces)
3. **Error affects files you didn't modify**
4. **Suspected build system or infrastructure issue**

**How to ask**:
```markdown
## Build Error During Fix for Issue #XXXXX

**What I changed**:
- `path/to/file1.cs` - [Description of changes]
- `path/to/file2.cs` - [Description of changes]

**Error message**:
```
[Full error output including stack trace]
```

**What I've tried**:
1. [First attempt] - [Result]
2. [Second attempt] - [Result]
3. [Third attempt] - [Result]

**Request**: [What you need help with]
```

## Unexpected Behavior After Fix

### Fix Doesn't Work as Expected

**Symptom**: Your fix compiles and runs, but doesn't actually resolve the issue

**Debugging steps:**

#### 1. Verify Fix Is Actually Running

**Add console logging:**
```csharp
public static void MapFlowDirection(ICollectionViewHandler handler, ICollectionView view)
{
    Console.WriteLine($"[DEBUG] MapFlowDirection called: {view.FlowDirection}");
    
    handler.PlatformView?.UpdateFlowDirection(view);
    
    Console.WriteLine("[DEBUG] UpdateFlowDirection completed");
}
```

**Check logs:**
```bash
# iOS
xcrun simctl spawn booted log stream --predicate 'eventMessage contains "[DEBUG]"'

# Android
adb logcat | grep "\[DEBUG\]"
```

**If logging doesn't appear:**
- Fix code isn't being called
- Check if mapper is registered correctly
- Verify handler is using your updated code

#### 2. Verify Property Values

**Check what values are actually being set:**
```csharp
internal static void UpdateFlowDirection(this UICollectionView view, ICollectionView collectionView)
{
    var attribute = collectionView.FlowDirection == FlowDirection.RightToLeft
        ? UISemanticContentAttribute.ForceRightToLeft
        : UISemanticContentAttribute.ForceLeftToRight;
    
    Console.WriteLine($"[DEBUG] Setting SemanticContentAttribute: {attribute}");
    Console.WriteLine($"[DEBUG] Current value: {view.SemanticContentAttribute}");
    
    view.SemanticContentAttribute = attribute;
    
    Console.WriteLine($"[DEBUG] New value: {view.SemanticContentAttribute}");
}
```

**If values are wrong:**
- Check enum mappings
- Verify platform API behavior
- Test with hardcoded values first

#### 3. Check Platform API Documentation

**Verify you're using the API correctly:**

**iOS:**
- Apple Developer Documentation: https://developer.apple.com/documentation/
- UIKit reference for control you're modifying
- Verify API is available in minimum supported iOS version

**Android:**
- Android Developer Documentation: https://developer.android.com/reference
- Check API level requirements
- Verify correct View class is being modified

**Windows:**
- WinUI 3 Documentation: https://docs.microsoft.com/windows/winui/
- Check if property/method exists in WinUI 3

### Fix Causes Side Effects

**Symptom**: Fix resolves the issue but breaks something else

**Diagnosis:**

**Test related scenarios:**
```csharp
// If you fixed RTL padding on CollectionView
// Test that LTR still works:
TestCollection.FlowDirection = FlowDirection.LeftToRight;
// Verify padding is correct

// Test that other collections still work:
var listView = new ListView { FlowDirection = FlowDirection.RightToLeft };
// Verify ListView not affected
```

**Run existing tests:**
```bash
# Run all CollectionView tests
dotnet test src/Controls/tests/TestCases.Shared.Tests/Controls.TestCases.Shared.Tests.csproj \
  --filter "Category=CollectionView"

# Check for failures
```

**Review your changes:**
- Did you modify shared code used by multiple controls?
- Did you change behavior for all cases, not just the bug case?
- Did you introduce a regression?

**Fix the side effect:**
- Make your fix more specific/targeted
- Add conditional logic to only apply fix to affected scenario
- Revert and try alternative approach

## When to Ask for Help

**Don't struggle alone - ask for guidance in these situations:**

### 1. Cannot Reproduce After Extensive Attempts

**When:**
- Tried all platform variations
- Tested with different data/timing/states
- Requested clarification from reporter
- Still cannot see the issue

**Action**: Comment on the issue:
```markdown
I've attempted to reproduce this issue with the following configurations:
- [List all attempts]

Unfortunately, I cannot reproduce the reported behavior. Would appreciate:
1. Additional reproduction steps
2. Sample project that demonstrates the issue
3. Confirmation issue still exists in latest version

@dotnet/maui-team - any suggestions for alternative approaches?
```

### 2. Root Cause Is Unclear

**When:**
- Can reproduce the issue
- Issue behavior is confirmed
- But don't understand WHY it's happening

**Action**: Document what you know and ask:
```markdown
**Reproduction**: ✅ Confirmed on iOS 18.0

**Observations:**
- [What you've observed]
- [What instrumentation showed]
- [What you've ruled out]

**Question**: The issue seems to be related to [X], but I'm not clear on why [Y] 
is causing this behavior. Could someone with more experience in [platform/control] 
provide guidance?
```

### 3. Multiple Possible Solutions

**When:**
- Have identified 2+ ways to fix the issue
- Unsure which approach is better for MAUI architecture
- Each has tradeoffs

**Action**: Present the options:
```markdown
**Root cause identified**: [Explanation]

**Possible solutions:**

**Option 1**: [Approach A]
- ✅ Pros: [Benefits]
- ❌ Cons: [Drawbacks]

**Option 2**: [Approach B]
- ✅ Pros: [Benefits]
- ❌ Cons: [Drawbacks]

**Question**: Which approach aligns better with MAUI architecture patterns? 
Or is there a third option I'm not considering?
```

### 4. Fix Works But Concerned About Side Effects

**When:**
- Fix resolves the issue
- But touches shared/core code
- Worried about unintended consequences

**Action**: Request review before PR:
```markdown
**Fix implemented**: [Brief description]

**Concerns:**
- Modified [shared file/method]
- Could potentially affect [other controls]
- Want to validate approach before creating PR

**Testing done:**
- [What you've tested]
- [What you haven't been able to test]

Could someone review the approach before I proceed with formal PR?

[Link to branch or code snippet]
```

### 5. Platform API Confusion

**When:**
- Platform API behavior doesn't match documentation
- Unexpected results from platform calls
- API seems broken or buggy

**Action**: Ask platform-specific experts:
```markdown
**Platform**: iOS (or Android/Windows/Mac)

**API**: UICollectionViewCompositionalLayout.Configuration

**Expected**: Setting ScrollDirection should [X]

**Actual**: [Y happens instead]

**Question**: Is this expected behavior? Is there a different API I should use? 
Or is this a platform SDK issue?

**Test code:**
```csharp
[Your minimal test case]
```
```

## Logging and Diagnostics

### Effective Debug Logging

**Structure your logs for easy filtering:**

```csharp
// Use consistent prefixes
Console.WriteLine($"[REPRO-12345] FlowDirection changed to {value}");
Console.WriteLine($"[REPRO-12345] Layout updated: {layout.Configuration}");
Console.WriteLine($"[REPRO-12345] Measurements: {width}x{height}");

// Grep for your specific issue
// adb logcat | grep "REPRO-12345"
```

**Log at key decision points:**
```csharp
public static void MapProperty(Handler handler, IView view)
{
    Console.WriteLine($"[DEBUG] MapProperty called");
    
    if (handler.PlatformView is null)
    {
        Console.WriteLine($"[DEBUG] PlatformView is null, returning");
        return;
    }
    
    Console.WriteLine($"[DEBUG] Updating PlatformView");
    handler.PlatformView.UpdateSomething();
    Console.WriteLine($"[DEBUG] Update completed");
}
```

### Common Diagnostic Patterns

**Measure timing:**
```csharp
var sw = System.Diagnostics.Stopwatch.StartNew();
// Operation
sw.Stop();
Console.WriteLine($"[PERF] Operation took {sw.ElapsedMilliseconds}ms");
```

**Capture call stacks:**
```csharp
Console.WriteLine($"[STACK] {Environment.StackTrace}");
```

**Dump object state:**
```csharp
Console.WriteLine($"[STATE] Control: {control.GetType().Name}");
Console.WriteLine($"[STATE] Width: {control.Width}, Height: {control.Height}");
Console.WriteLine($"[STATE] IsVisible: {control.IsVisible}");
// etc.
```

## Recovery Strategies

### Start Over vs. Pivot

**When to start over:**
- Approach is fundamentally flawed
- Too many side effects/complications
- Cleaner solution becomes apparent

**When to pivot:**
- Core approach is sound
- Just need different implementation details
- Can isolate problem to specific method/section

**When to seek help:**
- Stuck for > 2 hours on same problem
- Multiple approaches have failed
- Missing domain knowledge

**Remember:** It's okay to ask for help. The MAUI team wants issues fixed correctly, not quickly.
