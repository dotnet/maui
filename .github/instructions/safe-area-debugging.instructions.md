---
applyTo:
  - "**/MauiView.cs"
  - "**/MauiScrollView.cs"
  - "**/*SafeArea*"
  - "**/*safearea*"
---

# Safe Area Investigation Guidelines

## 🚨 Critical Platform Behavior Differences

**macOS 14/15 vs macOS 26 report DIFFERENT safe area insets:**

| macOS Version | Title Bar Safe Area | CI? |
|---------------|---------------------|-----|
| **macOS 14/15** | ~28px top inset | ✅ Yes — CI runs this |
| **macOS 26 (Tahoe/Liquid Glass)** | ~0px top inset | ❌ No |

- ❌ **NEVER assume local macOS 26 testing reproduces CI behavior**
- ✅ Safe area fixes MUST pass CI (macOS 14/15) to be valid

**Platform defaults:**

| Platform | `UseSafeArea` Default |
|----------|-----------------------|
| **iOS** | `false` |
| **macCatalyst** | `true` |

---

## Current Architecture (PR #34024)

### Primary: `IsParentHandlingSafeArea` (edge-aware parent walk)

Before applying safe area adjustments, `MauiView` and `MauiScrollView` check whether an ancestor is already handling the **same edges**. If so, the descendant skips to avoid double-padding.

Key properties:
- **Edge-aware**: parent handling `Top` does NOT block child handling `Bottom`
- **Cached** in `bool? _parentHandlesSafeArea`, cleared on `SafeAreaInsetsDidChange`, `InvalidateSafeArea`, and `MovedToWindow`
- `AppliesSafeAreaAdjustments` is `internal` so `MauiScrollView` can check `MauiView` ancestors
- See `MauiView.IsParentHandlingSafeArea()` for the implementation

### Secondary: `EqualsAtPixelLevel`

Safe area values are compared at device-pixel resolution before triggering layout invalidation. This absorbs sub-pixel animation noise (e.g., `0.0000001pt` during `TranslateToAsync`) that would otherwise cause oscillation loops (#32586, #33934).

---

## Anti-Patterns

### ❌ Window Guard (tried and removed)

Comparing `Window.SafeAreaInsets` to filter noise callbacks blocks legitimate updates. On macCatalyst with a custom TitleBar, `WindowViewController` repositions content by pushing it down — changing the **view's** `SafeAreaInsets` without changing the **window's**. This caused a 28px content shift on CI (macOS 14/15 only). Never compare `Window.SafeAreaInsets` to gate per-view callbacks.

### ❌ Semantic mismatch in comparisons

`_safeArea` is filtered through `GetSafeAreaForEdge` (zeroes edges per `SafeAreaRegions`). Raw `UIView.SafeAreaInsets` includes all edges. Never compare them directly — compare raw-to-raw or adjusted-to-adjusted only.

---

## Related

- `.github/copilot-instructions.md` — Platform-specific file extensions (`.ios.cs` = iOS + macCatalyst)
- `.github/instructions/uitests.instructions.md` — UI test categories and SafeAreaEdges category
