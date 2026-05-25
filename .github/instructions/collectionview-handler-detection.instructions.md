---
description: Critical guidance for CollectionView/CarouselView handler detection
applyTo: "src/Controls/src/Core/Handlers/Items/**,src/Controls/src/Core/Handlers/Items2/**"
---

# CollectionView Handler Detection

## Handler Implementation Status

There are **TWO separate handler implementations**, but they apply to **different platforms**:

1. **Items/** (`Handlers/Items/`) - Contains code for **ALL platforms** (Android, iOS, Windows, MacCatalyst, Tizen)
2. **Items2/** (`Handlers/Items2/`) - Contains code for **iOS/MacCatalyst ONLY**

### Platform-Specific Deprecation

The deprecation of Items/ **only applies to iOS/MacCatalyst**:

| Platform | Active Handler | Notes |
|----------|----------------|-------|
| **Android** | `Items/Android/` | **ONLY implementation** - Items2/ has no Android code |
| **Windows** | `Items/` | **ONLY implementation** - Items2/ has no Windows code |
| **iOS** | `Items2/iOS/` | Items/ iOS code is deprecated |
| **MacCatalyst** | `Items2/iOS/` | Items/ MacCatalyst code is deprecated |

**CRITICAL**: Items2/ is **iOS/MacCatalyst only**. There is NO Items2/ code for Android or Windows.

---

## Which Handler to Work On

### Decision Tree by Platform

```
Is the issue/PR for Android or Windows?
  YES → Work on Items/ (it's the ONLY implementation)
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
# - Contains "/Items/Android/" → Android (ONLY implementation, work here)
# - Contains "/Items/Windows/" or ".Windows.cs" → Windows (ONLY implementation, work here)
# - Contains "/Items2/iOS/" or "Items2/*.iOS.cs" → iOS/MacCatalyst (CURRENT)
# - Contains "/Items/*.iOS.cs" (not Items2) → iOS (DEPRECATED, prefer Items2/)
```

### Default Behavior by Platform

| Platform | Default Action |
|----------|----------------|
| **Android** | ✅ Work on `Items/Android/` - it's the only option |
| **Windows** | ✅ Work on `Items/` Windows files - it's the only option |
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
| `Handlers/Items/*.Windows.cs` | Windows | **ACTIVE** (only implementation) |
| `Handlers/Items2/iOS/` | iOS/MacCatalyst | **ACTIVE** (current) |
| `Handlers/Items/*.iOS.cs` | iOS/MacCatalyst | **DEPRECATED** (use Items2/) |

---

## Common Mistakes to Avoid

❌ **Wrong**: "Items/ is deprecated, so I should check if Items2/ needs the same Android fix"
- Items2/ has NO Android code - there's nothing to check

❌ **Wrong**: "This Android fix should also go in Items2/"
- Items2/ is iOS-only, Android code only exists in Items/

✅ **Correct**: "This is an Android-only issue, so I work in Items/Android/ which is the only Android implementation"
