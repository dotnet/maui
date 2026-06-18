---
description: Critical guidance for CollectionView/CarouselView handler detection
applyTo: "src/Controls/src/Core/Handlers/Items/**,src/Controls/src/Core/Handlers/Items2/**"
---

# CollectionView Handler Detection

## Handler Implementation Status

There are **TWO separate handler implementations**, but they apply to **different platforms**:

1. **Items/** (`Handlers/Items/`) - Contains code for **ALL platforms** (Android, iOS, Windows, MacCatalyst, Tizen)
2. **Items2/** (`Handlers/Items2/`) - Primarily the **iOS/MacCatalyst** path, **plus** the **Android CarouselView Material3** handler (see exception below)

### Platform-Specific Deprecation

The deprecation of Items/ **only applies to iOS/MacCatalyst**:

| Platform        | Control                       | Active Handler                           | Notes                                                                       |
| --------------- | ----------------------------- | ---------------------------------------- | --------------------------------------------------------------------------- |
| **Android**     | CollectionView                | `Items/Android/`                         | **ONLY implementation**                                                     |
| **Android**     | CarouselView (Material3)      | `Items2/` (`CarouselViewHandler2`)       | Material3-only; registered when `RuntimeFeature.IsMaterial3Enabled` is true |
| **Android**     | CarouselView (non-Material3)  | `Items/Android/` (`CarouselViewHandler`) | Default when Material3 is disabled                                          |
| **Windows**     | CollectionView / CarouselView | `Items/`                                 | **ONLY implementation** - Items2/ has no Windows code                       |
| **iOS**         | CollectionView / CarouselView | `Items2/iOS/`                            | Items/ iOS code is deprecated                                               |
| **MacCatalyst** | CollectionView / CarouselView | `Items2/iOS/`                            | Items/ MacCatalyst code is deprecated                                       |

**CRITICAL**: Items2/ is **iOS/MacCatalyst only**, with **one Android exception**: the Android **CarouselView Material3** handler (`CarouselViewHandler2` + `CarouselViewHandler2.Android.cs`, `MauiCarouselRecyclerView2`, `CarouselViewAdapter2`). There is **no** Items2/ code for Android CollectionView, and **no** Items2/ code for Windows.

### Android CarouselView Material3 Exception

When `RuntimeFeature.IsMaterial3Enabled` is `true`, Android `CarouselView` is registered to
`Items2.CarouselViewHandler2` (see `AppHostBuilderExtensions.AddControlsHandlers`). This handler
lives in `Items2/` but has **Android-specific** code (`*.Android.cs` partials) that uses Material's
`CarouselLayoutManager`. Its Android types subclass the shared `Items.*` base classes
(`CarouselViewAdapter2 : Items.CarouselViewAdapter`, `MauiCarouselRecyclerView2 : Items.MauiCarouselRecyclerView`),
so changes to the shared Android carousel base classes in `Items/Android/` can affect both handlers.

- **Android CarouselView Material3 work** → `Items2/` (`CarouselViewHandler2.Android.cs` and friends)
- **Android CarouselView non-Material3 work** → `Items/Android/` (`CarouselViewHandler`)
- **Android CollectionView work** → `Items/Android/` (unchanged — no Items2 Android CollectionView)

---

## Which Handler to Work On

### Decision Tree by Platform

```
Is the issue/PR for Android CarouselView with Material3 enabled?
  YES → Work on Items2/ (CarouselViewHandler2 Android partials)
  NO  → Continue...

Is the issue/PR for Android or Windows?
  YES → Work on Items/ (it's the ONLY implementation for these)
  NO  → Continue...

Is the issue/PR for iOS or MacCatalyst?
  YES → Work on Items2/ (Items/ is deprecated for iOS)
  NO  → Check platform and find appropriate handler
```

### Detection Algorithm

Check which handler directory the files are in:

```bash
# Check changed files in a PR
git diff <base-branch>..<pr-branch> --name-only | grep -i "handlers/items"

# Look for path pattern:
# - Contains "/Items/Android/" → Android CollectionView, or non-Material3 CarouselView (work here)
# - Contains "/Items2/*.Android.cs" → Android CarouselView Material3 (CarouselViewHandler2)
# - Contains "/Items/Windows/" or ".Windows.cs" → Windows (ONLY implementation, work here)
# - Contains "/Items2/iOS/" or "Items2/*.iOS.cs" → iOS/MacCatalyst (CURRENT)
# - Contains "/Items/*.iOS.cs" (not Items2) → iOS (DEPRECATED, prefer Items2/)
```

### Default Behavior by Platform

| Platform                                  | Default Action                                                 |
| ----------------------------------------- | -------------------------------------------------------------- |
| **Android** (CollectionView)              | ✅ Work on `Items/Android/` - it's the only option             |
| **Android** (CarouselView, Material3)     | ✅ Work on `Items2/` - `CarouselViewHandler2` Android partials |
| **Android** (CarouselView, non-Material3) | ✅ Work on `Items/Android/` - `CarouselViewHandler`            |
| **Windows**                               | ✅ Work on `Items/` Windows files - it's the only option       |
| **iOS/MacCatalyst**                       | ✅ Work on `Items2/` - Items/ is deprecated for iOS            |

### When to Work on Items/ for iOS (Deprecated)

Only work on Items/ iOS code when:

- PR explicitly modifies Items/ iOS files
- User explicitly requests changes to deprecated handlers
- Maintaining backward compatibility for a specific fix

---

## Quick Reference

| Path Pattern                                  | Platform                                              | Status                              |
| --------------------------------------------- | ----------------------------------------------------- | ----------------------------------- |
| `Handlers/Items/Android/`                     | Android (CollectionView + non-Material3 CarouselView) | **ACTIVE**                          |
| `Handlers/Items2/*.Android.cs` (CarouselView) | Android (CarouselView, Material3)                     | **ACTIVE** (`CarouselViewHandler2`) |
| `Handlers/Items/*.Windows.cs`                 | Windows                                               | **ACTIVE** (only implementation)    |
| `Handlers/Items2/iOS/`                        | iOS/MacCatalyst                                       | **ACTIVE** (current)                |
| `Handlers/Items/*.iOS.cs`                     | iOS/MacCatalyst                                       | **DEPRECATED** (use Items2/)        |

---

## Common Mistakes to Avoid

❌ **Wrong**: "Items2/ has NO Android code at all"

- The Android **CarouselView Material3** handler (`CarouselViewHandler2`) lives in `Items2/` with `*.Android.cs` partials. Android **CollectionView** still has no Items2/ code.

❌ **Wrong**: "This Android CollectionView fix should also go in Items2/"

- Android CollectionView only exists in `Items/Android/`. Only Android **CarouselView Material3** is in Items2/.

❌ **Wrong**: "This Android CarouselView fix goes in Items/Android/" (without checking Material3)

- If the issue is Material3-specific, the active Android CarouselView handler is `Items2.CarouselViewHandler2`. Only the non-Material3 path lives in `Items/Android/`.

✅ **Correct**: "This is an Android CollectionView issue, so I work in `Items/Android/` — the only Android CollectionView implementation"

✅ **Correct**: "This is an Android CarouselView Material3 issue, so I work in `Items2/` (`CarouselViewHandler2.Android.cs`)"
