---
description: Critical guidance for CollectionView/CarouselView handler detection
applyTo: "src/Controls/src/Core/Handlers/Items/**,src/Controls/src/Core/Handlers/Items2/**"
---

# CollectionView Handler Detection

## Handler Implementation Status

There are **TWO separate handler implementations**, with platform- and control-specific coverage:

1. **Items/** (`Handlers/Items/`) - Contains code for **ALL platforms** (Android, iOS, Windows, MacCatalyst, Tizen)
2. **Items2/** (`Handlers/Items2/`) - Contains code for **iOS/MacCatalyst** and **Windows CollectionView**

### Platform-Specific Deprecation

The deprecation of Items/ depends on both platform and control:

| Platform | Control | Active Handler | Notes |
|----------|---------|----------------|-------|
| **Android** | CollectionView / CarouselView | `Items/Android/` | **ONLY implementation** - Items2/ has no Android code |
| **Tizen** | CollectionView / CarouselView | `Items/Tizen/` | **ONLY implementation** - Items2/ has no Tizen code |
| **Windows** | CollectionView | `Items2/Windows/` | Default on .NET 11; Items/ is the obsolete feature-switch fallback |
| **Windows** | CarouselView | `Items/` | **ONLY implementation** - there is no Windows CarouselViewHandler2 |
| **iOS** | CollectionView / CarouselView | `Items2/iOS/` | Items/ iOS code is deprecated |
| **MacCatalyst** | CollectionView / CarouselView | `Items2/iOS/` | Items/ MacCatalyst code is deprecated |

**CRITICAL**: Items2/ does not replace Items/ wholesale. Android and Tizen use Items/ for both controls, and Windows CarouselView still uses Items/.

### Shared iOS/Mac Catalyst Infrastructure

Items2 intentionally reuses these public Items types:

- `IItemsViewSource`
- `IObservableItemsViewSource`
- `ILoopItemsViewSource`
- `MauiCollectionView`
- `IndexPathHelpers`
- `ScrollToPositionExtensions`

It also reuses internal item-source, template, index-path, snap, scroll-tracking, and reorder helpers from Items/. Do not classify or obsolete a type solely from its Items/ path; check Items2 references first.

---

## Which Handler to Work On

### Decision Tree by Platform

```
Is the issue/PR for Android or Tizen?
  YES → Work on Items/ (it's the ONLY implementation)
  NO  → Continue...

Is the issue/PR for iOS or MacCatalyst?
  YES → Work on Items2/ (Items/ is deprecated for iOS)
  NO  → Continue...

Is the issue/PR for Windows CollectionView?
  YES → Work on Items2/ (Items/ is the obsolete feature-switch fallback)
  NO  → Work on Items/ for Windows CarouselView
```

### Detection Algorithm

Check which handler directory the files are in:

```bash
# Check changed files in a PR
git diff <base-branch>..<pr-branch> --name-only | grep -i "handlers/items"

# Look for path pattern:
# - Contains "/Items/Android/" → Android (ONLY implementation, work here)
# - Contains "/Items/Tizen/" or ".Tizen.cs" → Tizen (ONLY implementation, work here)
# - Contains "/Items2/Windows/" or "Items2/*.Windows.cs" → Windows CollectionView (CURRENT)
# - Contains "/Items/*.Windows.cs" → Windows CarouselView or deprecated CollectionView fallback
# - Contains "/Items2/iOS/" or "Items2/*.iOS.cs" → iOS/MacCatalyst (CURRENT)
# - Contains "/Items/iOS/" or "Items/*.iOS.cs" (not Items2) → iOS/MacCatalyst legacy code;
#   prefer Items2/ unless the type is part of the shared infrastructure listed above
```

### Default Behavior by Platform

| Platform | Default Action |
|----------|----------------|
| **Android** | ✅ Work on `Items/Android/` - it's the only option |
| **Tizen** | ✅ Work on `Items/Tizen/` - it's the only option |
| **Windows CollectionView** | ✅ Work on `Items2/` Windows files - this is the .NET 11 default |
| **Windows CarouselView** | ✅ Work on `Items/` Windows files - it's the only option |
| **iOS/MacCatalyst** | ✅ Work on `Items2/` - Items/ is deprecated for iOS |

### When to Work on Items/ for iOS (Deprecated)

Only work on Items/ iOS code when:
- PR explicitly modifies Items/ iOS files
- User explicitly requests changes to deprecated handlers
- Maintaining backward compatibility for a specific fix

---

## Quick Reference

| Path Pattern | Platform | Status |
|--------------|----------|--------|
| `Handlers/Items/Android/` | Android | **ACTIVE** (only implementation) |
| `Handlers/Items/Tizen/` | Tizen | **ACTIVE** (only implementation) |
| `Handlers/Items2/Windows/`, `Handlers/Items2/*.Windows.cs` | Windows CollectionView | **ACTIVE** (default) |
| `Handlers/Items/*.Windows.cs` | Windows | **ACTIVE** for CarouselView; **DEPRECATED** CollectionView fallback |
| `Handlers/Items2/iOS/` | iOS/MacCatalyst | **ACTIVE** (current) |
| `Handlers/Items/iOS/`, `Handlers/Items/*.iOS.cs` | iOS/MacCatalyst | **DEPRECATED** handler implementation; retain Items types reused by Items2 |

---

## Common Mistakes to Avoid

❌ **Wrong**: "Items/ is deprecated everywhere"
- Items/ remains the only implementation for Android, Tizen, and Windows CarouselView

❌ **Wrong**: "Items2/ is iOS-only"
- Items2/ also contains the default .NET 11 Windows CollectionView implementation

❌ **Wrong**: "This Android fix should also go in Items2/"
- Items2/ has no Android code, so Android work stays in Items/

✅ **Correct**: "This is an Android-only issue, so I work in Items/Android/ which is the only Android implementation"
