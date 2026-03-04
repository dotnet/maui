---
applyTo:
  - "**/*SafeArea*.cs"
  - "**/Platform/iOS/**"
  - "**/Platform/MacCatalyst/**"
  - "**/*.ios.cs"
  - "**/*.maccatalyst.cs"
---

# Safe Area Investigation Guidelines

Rules for investigating safe area issues on iOS/macCatalyst to prevent common debugging mistakes.

## 🚨 Critical Platform Behavior Differences

### macOS Version Differences in Safe Area Reporting

**macOS 14/15 vs macOS 26 (Tahoe/Liquid Glass) report DIFFERENT safe area insets:**

| macOS Version | Title Bar Safe Area | Window Chrome Behavior |
|---------------|---------------------|------------------------|
| **macOS 14/15** | ~28px top inset | Traditional window chrome with visible title bar |
| **macOS 26 (Tahoe/Liquid Glass)** | ~0px top inset | Liquid Glass design with minimal chrome |

**CRITICAL RULES:**

1. ❌ **NEVER assume local macOS 26 testing reproduces CI behavior** - CI runs macOS 14/15
2. ✅ **ALWAYS check CI/CD macOS version when safe area tests fail** - Different OS = different insets
3. ✅ **ALWAYS test safe area changes on multiple macOS versions if possible**
4. ⚠️ **Local "works on my machine" does NOT validate safe area fixes** - Must pass CI on macOS 14/15

### Platform Default Differences

| Platform | `UseSafeArea` Default | Typical Safe Area Source |
|----------|----------------------|--------------------------|
| **iOS** | `false` | Status bar, home indicator, notch |
| **macCatalyst** | `true` | Title bar, window chrome |

**Rule**: When investigating macCatalyst safe area issues, remember `UseSafeArea = true` is the default behavior (opposite of iOS).

---

## 🔍 Investigation Rules

### Rule 1: When Tests Named After Feature Fail, Trace That Code Path FIRST

**Mistake**: TitleBar tests failed, but didn't immediately investigate WindowViewController TitleBar code.

**Rule**: Test names are direct clues to the code path that's broken.

**Pattern**:
```
Test name contains "TitleBar" → Investigate WindowViewController.TitleBar property
Test name contains "SafeArea" → Investigate SafeAreaInsetsDidChange callbacks
Test name contains "Padding" → Investigate ContentViewContainer padding logic
```

**Action**: Use test name keywords to grep for relevant code BEFORE forming hypotheses.

```bash
# Example: TitleBar test failing
grep -r "TitleBar" src/Controls/src/Core/Platform/iOS/
grep -r "WindowViewController" src/Controls/src/Core/Platform/iOS/ | grep -i titlebar
```

---

### Rule 2: Guard Conditions Are Prime Suspects When Callbacks Stop Firing

**Mistake**: Concluded "28px shift is a fix" when guard was blocking legitimate callbacks.

**Pattern**: When expected behavior mysteriously stops, CHECK GUARDS FIRST:

```csharp
// 🚨 DANGER: This guard can block legitimate updates
if (newValue == _lastValue)
    return;  // Blocks callback even when OTHER state changed
```

**Investigation checklist when callbacks don't fire**:

1. ✅ **Find all early-return guards** in the callback method
2. ✅ **Check if guard compares DIFFERENT state** than what actually changed
3. ✅ **Check if guard is too coarse-grained** (e.g., window-level when view-level changed)

**Example from PR #34024**:
```csharp
// ❌ BAD: Blocks view-level changes when only window-level is checked
if (windowInsets == _lastWindowSafeAreaInsets)
    return;  // View safe area could still change!

// ✅ GOOD: Check the actual thing that changed
if (view.SafeAreaInsets == _lastViewSafeAreaInsets)
    return;
```

**Red flags**:
- Guard compares X but callback should fire when Y changes
- Guard compares window-level but method updates view-level
- Guard compares raw data but method uses transformed data

---

### Rule 3: Ensure Semantic Compatibility in Comparisons

**Mistake**: First fix compared raw `SafeAreaInsets` (includes adjustments) against adjusted `_safeArea` (excludes adjustments).

**Rule**: NEVER compare values with different semantic meanings.

**Semantic mismatch example**:
```csharp
// ❌ WRONG: Comparing apples to oranges
var rawInsets = view.SafeAreaInsets;  // Includes all adjustments
if (rawInsets == _safeArea)  // _safeArea excludes title bar adjustment
    return;
```

**Correct patterns**:
```csharp
// ✅ Compare raw to raw
if (view.SafeAreaInsets == _lastRawInsets)
    return;

// ✅ Compare adjusted to adjusted
if (adjustedInsets == _lastAdjustedInsets)
    return;
```

**Before comparing, ask**:
- Do these values represent the same coordinate space?
- Have transformations/adjustments been applied to one but not the other?
- Are these values computed at the same point in the lifecycle?

---

### Rule 4: .ios.cs Files = iOS AND MacCatalyst

**Already documented in copilot-instructions.md, but bears repeating for safe area work:**

Files with `.ios.cs` extension compile for **BOTH iOS and MacCatalyst**.

**Common mistake**: Assuming `.ios.cs` code only affects iOS when debugging macCatalyst issues.

**Rule**: When investigating macCatalyst safe area bugs, CHECK `.ios.cs` FILES TOO.

**File extension summary**:
- `.ios.cs` → iOS + MacCatalyst (BOTH)
- `.maccatalyst.cs` → MacCatalyst ONLY
- Both can coexist and both will compile for MacCatalyst

---

## 🧪 Testing Strategy

### Local vs CI Testing

| Environment | macOS Version | Safe Area Behavior | Use For |
|-------------|---------------|-------------------|---------|
| **Local Dev Machine** | Often macOS 26 | ~0px title bar | Initial development |
| **CI (GitHub Actions)** | macOS 14/15 | ~28px title bar | Validation |

**CRITICAL**: Safe area fixes MUST pass CI tests on macOS 14/15 to be valid, even if they "work locally" on macOS 26.

### What "Working Locally" Actually Proves

✅ Code compiles  
✅ No crashes on your OS version  
❌ **DOES NOT prove** behavior is correct on CI macOS version  
❌ **DOES NOT prove** safe area insets are correct  

**Rule**: Never conclude "it works" based solely on local macOS 26 testing for safe area issues.

---

## 🔧 Debugging Checklist

Use this checklist when investigating safe area regressions:

```
□ Check CI macOS version vs local macOS version
□ Identify which callback method should handle the safe area change
□ Find ALL early-return guards in that callback
□ Verify guards compare semantically compatible values (raw vs raw, adjusted vs adjusted)
□ Verify guards aren't too coarse-grained (window-level blocking view-level changes)
□ Trace code path using test name as keyword (TitleBar test → TitleBar code)
□ Check both .ios.cs AND .maccatalyst.cs files for the feature
□ Test on CI environment (macOS 14/15) not just local
□ Verify UseSafeArea default behavior (true for macCatalyst, false for iOS)
```

---

## 📝 Code Review Focus Areas

When reviewing safe area PRs, pay extra attention to:

1. **Guards**: Are they too aggressive? Do they block legitimate updates?
2. **Comparisons**: Are they semantically compatible? (raw vs raw, adjusted vs adjusted)
3. **Platform assumptions**: Does code assume iOS behavior when it also runs on macCatalyst?
4. **Test coverage**: Does PR include tests that would fail on macOS 14/15 if regression occurs?

---

## Examples from PR #34024

### ❌ What Went Wrong

**Aggressive guard blocked legitimate callbacks**:
```csharp
// This blocked view-level safe area changes when window-level was unchanged
if (windowInsets == _lastWindowSafeAreaInsets)
    return;
```

**Semantic mismatch in comparison**:
```csharp
// Compared raw SafeAreaInsets (includes adjustments) to _safeArea (excludes adjustments)
if (vc.View!.SafeAreaInsets == _safeArea)
    return;
```

### ✅ Correct Approach

**Check actual changed value**:
```csharp
// Compare the value that actually changed (view-level insets)
if (vc.View!.SafeAreaInsets == _lastViewSafeAreaInsets)
    return;
```

**Maintain semantic compatibility**:
```csharp
// Both values are "view safe area insets" - semantically identical
_lastViewSafeAreaInsets = vc.View!.SafeAreaInsets;
```

---

## Related Documentation

- `.github/copilot-instructions.md` - Platform-specific file extensions
- `.github/instructions/uitests.instructions.md` - UI test categories and SafeAreaEdges category
