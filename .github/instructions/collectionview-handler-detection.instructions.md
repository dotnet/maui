---
description: Critical guidance for CollectionView/CarouselView handler detection
applyTo: "src/Controls/src/Core/Handlers/Items/**,src/Controls/src/Core/Handlers/Items2/**"
---

# CollectionView Handler Detection

## Handler Implementation Status

There are **TWO separate handler implementations**, but they apply to **different platforms**:

1. **Items/** (`Handlers/Items/`) - Contains the **ACTIVE** handler code for **Android, Windows, and Tizen**. Also contains legacy iOS/MacCatalyst code that has been superseded by Items2/.
2. **Items2/** (`Handlers/Items2/`) - Contains the **ACTIVE** handler code for **iOS/MacCatalyst ONLY**. There is NO Android, Windows, or Tizen code in Items2/.

### Active Handler Per Platform

| Platform | Active Handler | Status of Items/ code | Status of Items2/ code |
|----------|----------------|----------------------|----------------------|
| **Android** | `Items/Android/` | ✅ **ACTIVE** - this is the only implementation | ❌ Does not exist |
| **Windows** | `Items/` | ✅ **ACTIVE** - this is the only implementation | ❌ Does not exist |
| **iOS** | `Items2/iOS/` | Superseded by Items2/ for this platform only | ✅ **ACTIVE** |
| **MacCatalyst** | `Items2/iOS/` | Superseded by Items2/ for this platform only | ✅ **ACTIVE** |

**CRITICAL**: Items2/ is **iOS/MacCatalyst only**. There is NO Items2/ code for Android or Windows. The Items/ Android and Windows code is the **current, active, production implementation** — it is NOT deprecated.

---

## Code Review Guidance

**🚨 When reviewing PRs that modify Items/ Android or Windows code, NEVER call this code "deprecated" or suggest it needs to be migrated.**

Items/ for Android and Windows is the **only and active** implementation. Only the iOS/MacCatalyst portion of Items/ has been superseded by Items2/.

❌ **WRONG review comment**: "This PR fixes the DEPRECATED Items/ handlers"
- This is incorrect when the PR modifies Android or Windows code in Items/

❌ **WRONG review comment**: "Items/ handlers are deprecated and Items2/ is the active implementation"
- This is only true for iOS/MacCatalyst. For Android and Windows, Items/ IS the active implementation.

✅ **CORRECT review comment**: "This PR fixes the Android CollectionView handler in Items/Android/, which is the active implementation for Android"

✅ **CORRECT review comment**: "Items2/ only contains iOS/MacCatalyst code, so this Android fix in Items/ is in the correct location"

---

## Which Handler to Work On

### Decision Tree by Platform

```
Is the issue/PR for Android or Windows?
  YES → Work on Items/ (it's the ONLY and ACTIVE implementation for these platforms)
  NO  → Continue...

Is the issue/PR for iOS or MacCatalyst?
  YES → Work on Items2/ (Items/ iOS code has been superseded by Items2/)
  NO  → Check platform and find appropriate handler
```

### Detection Algorithm

Check which handler directory the files are in:

```bash
# Check changed files in a PR
git diff <base-branch>..<pr-branch> --name-only | grep -i "handlers/items"

# Look for path pattern:
# - Contains "/Items/Android/" → Android (ACTIVE, ONLY implementation - work here)
# - Contains "/Items/Windows/" or ".Windows.cs" → Windows (ACTIVE, ONLY implementation - work here)
# - Contains "/Items2/iOS/" or "Items2/*.iOS.cs" → iOS/MacCatalyst (ACTIVE current implementation)
# - Contains "/Items/*.iOS.cs" (not Items2) → iOS (superseded by Items2/, prefer Items2/ for new work)
```

### Default Behavior by Platform

| Platform | Default Action |
|----------|----------------|
| **Android** | ✅ Work on `Items/Android/` - it's the only and active implementation |
| **Windows** | ✅ Work on `Items/` Windows files - it's the only and active implementation |
| **iOS/MacCatalyst** | ✅ Work on `Items2/` - Items/ iOS code has been superseded by Items2/ |

### When to Work on Items/ for iOS

Only work on Items/ iOS/MacCatalyst code when:
- PR explicitly modifies Items/ iOS files
- User explicitly requests changes to the legacy iOS handlers
- Maintaining backward compatibility for a specific fix

---

## Quick Reference

| Path Pattern | Platform | Status |
|--------------|----------|--------|
| `Handlers/Items/Android/` | Android | ✅ **ACTIVE** (only implementation) |
| `Handlers/Items/*.Windows.cs` | Windows | ✅ **ACTIVE** (only implementation) |
| `Handlers/Items2/iOS/` | iOS/MacCatalyst | ✅ **ACTIVE** (current implementation) |
| `Handlers/Items/*.iOS.cs` | iOS/MacCatalyst | ⚠️ Superseded by Items2/ |

---

## Common Mistakes to Avoid

❌ **Wrong**: "Items/ is deprecated" (as a blanket statement)
- Items/ is ONLY superseded for iOS/MacCatalyst. For Android and Windows, Items/ is the active production code.

❌ **Wrong**: "Items/ is deprecated, so I should check if Items2/ needs the same Android fix"
- Items2/ has NO Android code - there's nothing to check

❌ **Wrong**: "This Android fix should also go in Items2/"
- Items2/ is iOS/MacCatalyst only, Android code only exists in Items/

❌ **Wrong**: "This PR fixes the DEPRECATED Items/ handlers" (when reviewing Android/Windows changes)
- Items/ Android and Windows code is NOT deprecated. Do not characterize it as such.

✅ **Correct**: "This is an Android-only issue, so I work in Items/Android/ which is the only Android implementation"

✅ **Correct**: "This PR correctly fixes the Android handler in Items/, which is the active implementation for Android"
