---
description: Critical guidance for CollectionView/CarouselView handler detection
applyTo: "src/Controls/src/Core/Handlers/Items/**,src/Controls/src/Core/Handlers/Items2/**"
---

# CollectionView Handler Detection

## Handler Implementation Status

There are **TWO separate handler implementations**:

1. **Items/** (`Handlers/Items/`) - **DEPRECATED** - Original implementation
2. **Items2/** (`Handlers/Items2/`) - **CURRENT** - Active implementation

**Default Policy**: Always work on **Items2/** handlers. The Items/ handlers are deprecated and should only be modified if explicitly required.

---

## Which Handler to Work On

### Detection Algorithm

Check which handler directory the files are in:

```bash
# Check changed files in a PR
git diff <base-branch>..<pr-branch> --name-only | grep -i "handlers/items"

# Look for path pattern:
# - Contains "/Items/" (NOT "Items2") → DEPRECATED (Items)
# - Contains "/Items2/" → CURRENT (Items2)
```

**Key Patterns**:
- `src/Controls/src/Core/Handlers/Items/` → **DEPRECATED**
- `src/Controls/src/Core/Handlers/Items2/` → **CURRENT**

### Default Behavior

**Unless explicitly told otherwise**:
- ✅ Work on **Items2/** handlers
- ❌ Do NOT work on **Items/** handlers (deprecated)

### When to Work on Items/ (Deprecated)

Only work on Items/ handlers when:
- PR explicitly modifies Items/ files
- User explicitly requests changes to deprecated handlers
- Maintaining backward compatibility for a specific fix

---

## Quick Reference

| Path Pattern | Status | Default Action |
|--------------|--------|----------------|
| `Handlers/Items/` | **DEPRECATED** | Avoid unless explicitly required |
| `Handlers/Items2/` | **CURRENT** | Use by default |
