# RTL (Right-to-Left) Testing and Review Guide

## Overview

This guide provides comprehensive instructions for reviewing, testing, and validating RTL (Right-to-Left) functionality in .NET MAUI. RTL support is critical for languages like Arabic, Hebrew, Persian, and Urdu.

## Understanding RTL in .NET MAUI

### Core Concepts

**FlowDirection Property:**
- `FlowDirection.LeftToRight` - Default for LTR languages (English, Spanish, etc.)
- `FlowDirection.RightToLeft` - For RTL languages (Arabic, Hebrew, etc.)
- `FlowDirection.MatchParent` - Inherit from parent element

**Platform-Specific Implementations:**

**Android:**
- Uses `LayoutDirection` and `TextDirection` enums
- Requires `android:supportsRtl="true"` in AndroidManifest.xml
- Can be checked via `Rtl.IsSupported` static property
- **Key Methods:**
  - `SetPaddingRelative()` - Automatically handles RTL padding (start/end instead of left/right)
  - `SetPadding()` - Absolute positioning, doesn't flip for RTL
  - `SetLayoutDirection()` - Sets layout direction on views

**iOS/MacCatalyst:**
- Uses `UIUserInterfaceLayoutDirection` enum
- Checks via `EffectiveUserInterfaceLayoutDirection` property
- **Key Patterns:**
  - Manually swap left/right insets when `UIUserInterfaceLayoutDirection.RightToLeft`
  - Use `UIEdgeInsets` for padding/margins
  - FlowDirection propagates through view hierarchy

**Windows:**
- Uses `FlowDirection` property natively
- Generally handles RTL automatically

### Affected Properties

Properties that should mirror in RTL mode:
- **Padding** - `Thickness(left, top, right, bottom)` should swap left/right
- **Margin** - Same as padding
- **HorizontalOptions** - Start/End should swap
- **HorizontalTextAlignment** - Start/End should swap
- **Shadow offsets** - X-axis should flip
- **Transforms** - TranslateX should flip

Properties that should NOT mirror:
- **Absolute positioning** - `AbsoluteLayout` positions
- **Rotation** - Unless specifically designed to flip

## Platform-Specific Implementation Patterns

### Android RTL Implementation

**Correct Pattern (Relative):**
```csharp
// ✅ CORRECT - Uses relative positioning, automatically handles RTL
textView.SetPaddingRelative(
    (int)textView.ToPixels(label.Padding.Left),   // Treated as "start"
    (int)textView.ToPixels(label.Padding.Top),
    (int)textView.ToPixels(label.Padding.Right),  // Treated as "end"
    (int)textView.ToPixels(label.Padding.Bottom)
);
```

**Incorrect Pattern (Absolute):**
```csharp
// ❌ WRONG - Absolute positioning, ignores RTL
textView.SetPadding(
    (int)textView.ToPixels(label.Padding.Left),   // Always left, never flips
    (int)textView.ToPixels(label.Padding.Top),
    (int)textView.ToPixels(label.Padding.Right),  // Always right, never flips
    (int)textView.ToPixels(label.Padding.Bottom)
);
```

**Other Android RTL APIs:**
```csharp
// Set layout direction
view.SetLayoutDirection(ALayoutDirection.Rtl);

// Get current layout direction
var direction = view.LayoutDirection; // Returns Ltr, Rtl, or Inherit

// Check if RTL is supported by the app
if (Rtl.IsSupported)
{
    // App supports RTL
}
```

### iOS/MacCatalyst RTL Implementation

**Correct Pattern:**
```csharp
// ✅ CORRECT - Manually swap insets for RTL
var insets = TextInsets;

if (EffectiveUserInterfaceLayoutDirection == UIUserInterfaceLayoutDirection.RightToLeft)
{
    insets = new UIEdgeInsets(
        insets.Top,
        insets.Right,   // Swap right to left position
        insets.Bottom,
        insets.Left     // Swap left to right position
    );
}

rect = insets.InsetRect(rect);
```

**Incorrect Pattern:**
```csharp
// ❌ WRONG - Doesn't check RTL direction
rect = TextInsets.InsetRect(rect);
// This will not flip for RTL, causing incorrect padding
```

**Other iOS RTL APIs:**
```csharp
// Check effective layout direction
if (view.EffectiveUserInterfaceLayoutDirection == UIUserInterfaceLayoutDirection.RightToLeft)
{
    // View is in RTL mode
}

// Set semantic content attribute (controls automatic flipping)
view.SemanticContentAttribute = UISemanticContentAttribute.ForceRightToLeft;
```

## Reviewing RTL Pull Requests

### Essential Review Checklist

When reviewing an RTL-related PR, verify:

- [ ] **Platform Coverage**: Changes apply to all affected platforms (Android, iOS, MacCatalyst, Windows if applicable)
- [ ] **Correct APIs Used**:
  - [ ] Android uses `SetPaddingRelative()` not `SetPadding()` for padding that should mirror
  - [ ] iOS/MacCatalyst manually swaps left/right when `EffectiveUserInterfaceLayoutDirection == RightToLeft`
- [ ] **Test Coverage**: Both UI test page and Appium test exist
- [ ] **Visual Verification**: Screenshots or test cases demonstrate the fix
- [ ] **No Regressions**: LTR mode still works correctly
- [ ] **Edge Cases**: Tests include varying padding values, nested RTL/LTR, etc.

### Code Review Focus Areas

**1. Android Platform Changes:**
```csharp
// Look for these patterns:
SetPadding()         → Should be SetPaddingRelative() for mirroring
SetMargin()          → Should use relative variants
LayoutDirection      → Should be checked/set appropriately
```

**2. iOS/MacCatalyst Platform Changes:**
```csharp
// Look for these patterns:
EffectiveUserInterfaceLayoutDirection  → Should be checked when flipping
UIEdgeInsets                           → Should swap left/right in RTL
SemanticContentAttribute               → May be relevant for certain controls
```

**3. Cross-Platform Logic:**
```csharp
// Look for these patterns:
FlowDirection == RightToLeft           → Conditional RTL logic
Thickness(left, top, right, bottom)    → Ensure left/right are properly used
HorizontalOptions.Start/End            → Should mirror in RTL
```

### Common RTL Bugs to Watch For

1. **Using Absolute Instead of Relative APIs (Android)**
   - Bug: `SetPadding()` instead of `SetPaddingRelative()`
   - Impact: Padding doesn't flip in RTL mode

2. **Not Checking Layout Direction (iOS)**
   - Bug: Direct use of `TextInsets` without checking `EffectiveUserInterfaceLayoutDirection`
   - Impact: Insets don't flip in RTL mode

3. **Incorrect Inset Swapping (iOS)**
   - Bug: Swapping top/bottom instead of left/right
   - Impact: Padding flips vertically instead of horizontally

4. **Forgetting Platform-Specific Implementations**
   - Bug: Only fixing Android or only fixing iOS
   - Impact: Inconsistent behavior across platforms

5. **Breaking LTR Mode**
   - Bug: RTL fix causes issues in LTR mode
   - Impact: Regression for majority of users

## Testing RTL Changes

### Manual Testing Procedure

**1. Visual Verification (Quick Test):**

```csharp
// Create a test page with RTL toggle
var content = new Label 
{ 
    Text = "Test Content",
    Padding = new Thickness(20, 0, 0, 0),  // Left padding only
    BackgroundColor = Colors.LightBlue
};

var button = new Button { Text = "Toggle RTL" };
button.Clicked += (s, e) => 
{
    content.FlowDirection = content.FlowDirection == FlowDirection.LeftToRight 
        ? FlowDirection.RightToLeft 
        : FlowDirection.LeftToRight;
};
```

**Expected Behavior:**
- LTR: Padding appears on the LEFT side
- RTL: Padding appears on the RIGHT side (mirrored)

**2. Automated UI Test (Required for PRs):**

Must include both files:

**TestCases.HostApp/Issues/IssueXXXXX.cs:**
```csharp
[Issue(IssueTracker.Github, XXXXX, "Description", PlatformAffected.iOS | PlatformAffected.Android)]
public class IssueXXXXX : ContentPage
{
    public IssueXXXXX()
    {
        var button = new Button 
        { 
            Text = "Toggle RTL",
            AutomationId = "ToggleButton"  // CRITICAL: AutomationId for test
        };
        
        var testContent = new Label
        {
            Padding = new Thickness(20, 0, 0, 0),
            AutomationId = "TestLabel"     // CRITICAL: AutomationId for test
        };
        
        button.Clicked += (s, e) => 
        {
            testContent.FlowDirection = FlowDirection.RightToLeft;
        };
    }
}
```

**TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs:**
```csharp
public class IssueXXXXX : _IssuesUITest
{
    public override string Issue => "Description";
    
    [Test]
    [Category(UITestCategories.Label)]  // Or appropriate category
    public void RTLModeShouldWork()
    {
        App.WaitForElement("ToggleButton");
        App.Tap("ToggleButton");
        
        // Visual verification via screenshot comparison
        VerifyScreenshot();
    }
}
```

### Running RTL Tests Locally

**Android:**
```bash
# 1. Deploy test app
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run

# 2. Run specific RTL test
dotnet test src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj --filter "FullyQualifiedName~IssueXXXXX"
```

**iOS:**
```bash
# 1. Find iPhone simulator with highest iOS version
UDID=$(xcrun simctl list devices available | grep "iPhone Xs" | tail -1 | sed -n 's/.*(\([A-F0-9-]*\)).*/\1/p')

# 2. Build app
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-ios

# 3. Boot simulator and install
xcrun simctl boot $UDID 2>/dev/null || true
xcrun simctl install $UDID artifacts/bin/Controls.TestCases.HostApp/Debug/net10.0-ios/iossimulator-arm64/Controls.TestCases.HostApp.app

# 4. Run test
dotnet test src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj --filter "FullyQualifiedName~IssueXXXXX"
```

### Validating RTL Fixes

**Critical: Revert Testing Process**

For bug fix PRs with RTL tests, you MUST validate the test correctly captures the bug.

**Quick Process:**
1. Save state: `git add -A && git commit -m "temp"`
2. Revert fix files: `git checkout HEAD~1 -- <RTL-fix-files>`
3. Run test - MUST FAIL
4. Restore: `git reset --hard HEAD`
5. Run test - MUST PASS

**For detailed step-by-step instructions, platform-specific commands, troubleshooting, and examples**, see:
- [PR Test Validation Guide](PR-Test-Validation-Guide.md) - Complete test validation procedures

## RTL Testing Best Practices

### For Contributors

**1. Test on Multiple Platforms:**
- Minimum: Test on the platforms listed in `PlatformAffected` attribute
- Ideal: Test on all platforms (Android, iOS, MacCatalyst, Windows)

**2. Test Both Directions:**
- Always verify LTR mode still works (no regression)
- Verify RTL mode works correctly (fix applied)
- Test switching between LTR and RTL at runtime

**3. Include Visual Tests:**
- Use `VerifyScreenshot()` for visual verification
- Screenshots automatically compared in CI
- Helps catch subtle visual issues

**4. Test Edge Cases:**
```csharp
// Various padding configurations
new Thickness(10, 0, 0, 0)    // Left only
new Thickness(0, 0, 10, 0)    // Right only
new Thickness(10, 5, 15, 5)   // Different left/right values
new Thickness(0)              // No padding (shouldn't break)
```

**5. Test Nested RTL/LTR:**
```csharp
// Parent RTL, child LTR
new VerticalStackLayout
{
    FlowDirection = FlowDirection.RightToLeft,
    Children = 
    {
        new Label 
        { 
            FlowDirection = FlowDirection.LeftToRight,  // Override
            Padding = new Thickness(10, 0, 0, 0)
        }
    }
};
```

### For Reviewers

**1. Check Platform-Specific Code:**
- Android: Look for `SetPaddingRelative()` usage
- iOS: Look for `EffectiveUserInterfaceLayoutDirection` checks
- Windows: Generally handled automatically, verify if custom code exists

**2. Review Test Quality:**
- Does test actually toggle RTL mode?
- Does test have proper `AutomationId` attributes?
- Does test use `VerifyScreenshot()` for visual verification?
- Can the test fail? (Run without fix to verify)

**3. Check for Related Issues:**
```bash
# Search for similar RTL issues
gh issue list --label "RTL" --label "right-to-left"

# Search for similar RTL PRs
gh pr list --label "RTL"

# Search codebase for similar patterns
grep -r "SetPadding\|SetPaddingRelative" src/Core/src/Platform/Android/
grep -r "EffectiveUserInterfaceLayoutDirection" src/Core/src/Platform/iOS/
```

**4. Request Comprehensive Testing:**
- Ask contributor to test on actual devices (not just emulators)
- Request screenshots showing before/after behavior
- Ask for testing in real RTL language context (Arabic, Hebrew)

## Common RTL Scenarios

### Scenario 1: Label Padding (Android)

**Issue:** Label padding doesn't mirror in RTL mode

**Root Cause:** Using `SetPadding()` instead of `SetPaddingRelative()`

**Fix:**
```csharp
// Before (WRONG)
textView.SetPadding(left, top, right, bottom);

// After (CORRECT)
textView.SetPaddingRelative(left, top, right, bottom);
```

**Files to Check:**
- `src/Core/src/Platform/Android/TextViewExtensions.cs`
- `src/Core/src/Platform/Android/*Extensions.cs`

### Scenario 2: Label Padding (iOS)

**Issue:** Label padding doesn't mirror in RTL mode

**Root Cause:** Not checking `EffectiveUserInterfaceLayoutDirection` and swapping insets

**Fix:**
```csharp
// Before (WRONG)
rect = TextInsets.InsetRect(rect);

// After (CORRECT)
var insets = TextInsets;
if (EffectiveUserInterfaceLayoutDirection == UIUserInterfaceLayoutDirection.RightToLeft)
{
    insets = new UIEdgeInsets(insets.Top, insets.Right, insets.Bottom, insets.Left);
}
rect = insets.InsetRect(rect);
```

**Files to Check:**
- `src/Core/src/Platform/iOS/MauiLabel.cs`
- `src/Core/src/Platform/iOS/Maui*.cs`

### Scenario 3: CollectionView Item Layout

**Issue:** CollectionView items don't reverse order in RTL mode

**Root Cause:** Layout manager doesn't respect `LayoutDirection`

**Fix:** Ensure layout manager checks and applies RTL layout direction

### Scenario 4: Navigation/Shell Direction

**Issue:** Navigation transitions don't reverse in RTL mode

**Root Cause:** Animation direction is hardcoded to LTR

**Fix:** Check `FlowDirection` and reverse animation direction for RTL

## Resources

### Key Files for RTL Implementation

**Android:**
- `src/Core/src/Platform/Android/Rtl.cs` - RTL support detection
- `src/Core/src/Platform/Android/FlowDirectionExtensions.cs` - FlowDirection conversions
- `src/Core/src/Platform/Android/*Extensions.cs` - Control-specific implementations

**iOS/MacCatalyst:**
- `src/Core/src/Platform/iOS/FlowDirectionExtensions.cs` - FlowDirection conversions
- `src/Core/src/Platform/iOS/Maui*.cs` - Custom control implementations

**Tests:**
- `src/Controls/tests/TestCases.HostApp/Issues/` - UI test pages
- `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/` - Appium tests

### Existing RTL Tests

Use these as reference examples:
- `Issue17057.cs` - Shell FlowDirection
- `Issue27711.cs` - CollectionView RTL
- `Issue29458.cs` - ScrollView RTL
- `Issue30367.cs` - SearchBar RTL
- `Issue30575.cs` - WebView RTL

### Related Documentation

- [UI Testing Guide](UITesting-Guide.md) - Comprehensive UI testing guide
- `.github/instructions/uitests.instructions.md` - UI test requirements
- `.github/copilot-instructions.md` - General coding guidelines

## AI Assistant Guidance

When reviewing RTL PRs, AI assistants should:

1. **Verify Platform Coverage**: Ensure fix applies to all affected platforms
2. **Check API Usage**: Verify correct platform-specific APIs are used
3. **Validate Tests**: Confirm revert testing process was followed
4. **Review Edge Cases**: Check for nested RTL/LTR, varying padding, etc.
5. **Request Improvements**: Suggest better patterns or additional test coverage

**Red Flags to Watch For:**
- ❌ Only Android OR iOS fixed (not both)
- ❌ Using `SetPadding()` instead of `SetPaddingRelative()` on Android
- ❌ Not checking `EffectiveUserInterfaceLayoutDirection` on iOS
- ❌ Test doesn't actually toggle RTL mode
- ❌ Test missing `AutomationId` attributes
- ❌ No visual verification (`VerifyScreenshot()`)
- ❌ Test wasn't validated with revert process

**Example Review Comment Template:**

```markdown
## RTL PR Review

**Platform Coverage**: ✅ Both Android and iOS fixed  
**API Usage**: ✅ Android uses `SetPaddingRelative()`, iOS checks `EffectiveUserInterfaceLayoutDirection`  
**Test Coverage**: ✅ UI test page and Appium test included  
**Test Validation**: ⚠️ Please confirm the revert testing process was followed

### Validation Request

Can you please confirm that you validated the test by:
1. Reverting the fix files (keeping the tests)
2. Running the test to confirm it fails
3. Restoring the fix
4. Running the test to confirm it passes

This ensures the test correctly captures the bug and will prevent regressions.

### Suggestion

Consider adding an edge case test with different left/right padding values:
```csharp
new Label { Padding = new Thickness(20, 0, 5, 0) }  // Different left vs right
```

This would make the visual difference more obvious in screenshots.
```

## Summary

Successfully reviewing RTL PRs requires:
1. Understanding platform-specific RTL APIs
2. Verifying correct API usage
3. Ensuring comprehensive test coverage
4. Validating tests with the revert process
5. Checking for edge cases and regressions

When in doubt, run the tests locally and visually verify the behavior on real devices with actual RTL languages.
