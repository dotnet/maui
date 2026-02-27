# Complete Example: Agent-Optimized PR

This example shows a PR description optimized for future agent success.

## Title
```
[iOS] SafeArea: Return Empty for non-ISafeAreaView views (opt-in model)
```

## Description
```markdown
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

### Root Cause

In `MauiView.GetAdjustedSafeAreaInsets()` on iOS, views that don't implement `ISafeAreaView` or `ISafeAreaView2` (such as `ContentPresenter`, `Border`) were falling through to return `baseSafeArea`. This applied full device safe area insets to views that never opted into safe area handling, causing double-padding when used inside ControlTemplates.

### Description of Change

Changed the fallback behavior in `GetAdjustedSafeAreaInsets()` to return `SafeAreaPadding.Empty` instead of `baseSafeArea` for views that don't implement `ISafeAreaView` or `ISafeAreaView2`.

**This is a philosophy change:**
- **Before:** Safe area applied by default (opt-out model)
- **After:** Safe area only applies to views that implement the interfaces (opt-in model)

This aligns iOS with Android, where `SafeAreaExtensions.GetSafeAreaView2()` returns `null` for non-safe-area views.

### Key Technical Details

**Safe area interfaces (opt-in contract):**
- `ISafeAreaView` - Legacy interface with `IgnoreSafeArea` property
- `ISafeAreaView2` - Modern interface with per-edge `SafeAreaRegions` control

**Views that implement these interfaces (safe area works):**
- `ContentPage`, `ContentView`, `Layout`, `ScrollView`, `Border`, `Page`

**Views that don't (now return Empty):**
- `ContentPresenter`, custom views without interface, third-party controls

### What NOT to Do (for future agents)

- Don't use Element type in Platform layer - `src/Core/src/Platform/` cannot reference Controls-layer types like `Element`. Use `IView`, `ISafeAreaView`, `ISafeAreaView2` only.
- Don't use type name string matching - Checking `View.GetType().Name.Contains("ContentPresenter")` is brittle
- Don't check ancestor hierarchy for safe area - Performance cost and wrong abstraction (safe area is per-edge, not binary)

### Edge Cases

| Scenario | Risk | Mitigation |
|----------|------|------------|
| Legacy layouts (`LegacyLayouts/`) | Low | Already `[Obsolete]` |
| Custom views without ISafeAreaView | Medium | Implement interface to opt-in |
| GraphicsView/WebView as root | Low | Parent ContentPage handles safe area |

### Issues Fixed

Fixes #33458
```

## Why This Works for Agents

- **Searchable title** - Agents searching "ISafeAreaView" or "SafeArea opt-in" will find this
- **Philosophy change documented** - Agents won't assume old behavior
- **Key types listed** - Agents know which interfaces matter
- **What NOT to do** - Agents won't repeat failed approaches (Element type, string matching)
- **Edge cases** - Agents know the risk profile before making changes
