---
applyTo:
  - "**/Platform/iOS/MauiView.cs"
  - "**/Platform/iOS/MauiScrollView.cs"
  - "**/Platform/iOS/*SafeArea*"
---

# Safe Area Guidelines (iOS/macCatalyst)

## Platform Differences

| | macOS 14/15 | macOS 26+ |
|-|-------------|-----------|
| Title bar inset | ~28px | ~0px |
| Used in CI | ✅ | ❌ |

Local macOS 26+ testing does NOT validate CI behavior. Fixes must pass CI on macOS 14/15.

| Platform | `UseSafeArea` default |
|----------|-----------------------|
| iOS | `false` |
| macCatalyst | `true` |

## Architecture (PR #34024)

**`IsParentHandlingSafeArea`** — before applying adjustments, `MauiView`/`MauiScrollView` walk ancestors to check if any ancestor handles the **same edges**. If so, descendant skips (avoids double-padding). Edge-aware: parent handling `Top` does not block child handling `Bottom`. Result cached in `bool? _parentHandlesSafeArea`; cleared on `SafeAreaInsetsDidChange`, `InvalidateSafeArea`, `MovedToWindow`. `AppliesSafeAreaAdjustments` is `internal` for cross-type ancestor checks.

**`EqualsAtPixelLevel`** — safe area compared at device-pixel resolution to absorb sub-pixel animation noise (`0.0000001pt` during `TranslateToAsync`), preventing oscillation loops (#32586, #33934).

## Anti-Patterns

**❌ Window Guard** — comparing `Window.SafeAreaInsets` to filter callbacks blocks legitimate updates. On macCatalyst + custom TitleBar, `WindowViewController` pushes content down, changing the **view's** `SafeAreaInsets` without changing the **window's**. Caused 28px CI shift (macOS 14/15 only). Never gate per-view callbacks on window-level insets.

**❌ Semantic mismatch** — `_safeArea` is filtered by `GetSafeAreaForEdge` (zeroes edges per `SafeAreaRegions`); raw `UIView.SafeAreaInsets` includes all edges. Never compare them — compare raw-to-raw or adjusted-to-adjusted.
