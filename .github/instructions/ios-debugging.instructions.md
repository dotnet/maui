# iOS/MacCatalyst Debugging and Problem-Solving Guidelines

This instruction file provides guidance for debugging iOS and MacCatalyst issues in .NET MAUI.

## When This Applies

This guidance applies when:
- Debugging crashes on iOS or MacCatalyst platforms
- Investigating issues in iOS-specific code paths
- Working with iOS lifecycle methods (ViewDidLoad, TraitCollectionDidChange, etc.)
- Fixing platform-specific bugs in handlers or renderers

## Critical Debugging Principles

### 1. Search for Patterns, Not Just Named Files

**❌ BAD APPROACH**: Trust the stack trace filename and only look there

**✅ GOOD APPROACH**: Search for the pattern across the entire codebase

When a crash occurs in a specific method (e.g., `TraitCollectionDidChange`, `ViewDidLoad`, `ViewWillAppear`):

1. **Search for ALL occurrences** of that method across the codebase:
   ```bash
   # Search for method overrides
   grep -r "override.*TraitCollectionDidChange" src/
   grep -r "override.*ViewDidLoad" src/
   
   # Search for interface implementations
   grep -r "void TraitCollectionDidChange" src/
   ```

2. **Check multiple layers** where the method might be implemented:
   - `src/Core/src/Platform/iOS/` - Core framework implementations
   - `src/Controls/src/Core/Platform/iOS/` - Controls layer
   - `src/Controls/src/Core/Compatibility/` - Legacy compatibility handlers
   - `src/Essentials/src/` - Essentials platform implementations

3. **Look for duplication** - If the same method exists in multiple files, the bug might be in a different file than where it crashed

### 2. Understand iOS Layer Architecture

iOS code in .NET MAUI is organized in layers:

| Layer | Location | Responsibility | Examples |
|-------|----------|----------------|----------|
| **Core** | `src/Core/src/Platform/iOS/` | Fundamental page/view lifecycle | PageViewController, ViewRenderer |
| **Controls** | `src/Controls/src/Core/Platform/iOS/` | Control-specific handlers | ButtonHandler, LabelHandler |
| **Compatibility** | `src/Controls/src/Core/Compatibility/` | Legacy renderer system | ShellRenderer, TabbedRenderer |

**Key principle**: Functionality should be implemented at the LOWEST appropriate layer.

- **Theme changes** → Core layer (affects ALL pages)
- **Button clicks** → Controls layer (specific to Button)
- **Shell navigation** → Compatibility layer (Shell-specific)

### 3. Check for Duplicate Implementations

**Common pattern that causes bugs**:
1. Feature was originally implemented in Controls or Compatibility layer
2. Later, feature was moved to Core layer for broader applicability
3. Original implementation was NOT removed, creating duplication
4. Duplication causes race conditions, double-firing, or crashes

**How to detect**:
```bash
# Example: Check if TraitCollectionDidChange exists in multiple layers
find src/ -name "*.cs" -exec grep -l "TraitCollectionDidChange" {} \; | grep -E "(Core|Controls|Compatibility)"

# Check for duplicate implementations
grep -r "public override void TraitCollectionDidChange" src/ | wc -l
# If count > expected number of unique classes, investigate duplication
```

### 4. Consider "Remove vs Patch" for Fixes

When you find a bug in a method override:

**Ask these questions BEFORE patching**:
1. Is this override still needed, or was it superseded by another implementation?
2. Is this functionality already provided by a parent class or different layer?
3. Was this code added for a specific PR that might have been replaced by a later PR?

**Check git history**:
```bash
# Find when the method was added
git log -p --all -S "TraitCollectionDidChange" -- "path/to/file.cs"

# Check if a later PR might have replaced this functionality
gh pr list --search "TraitCollectionDidChange" --state merged
```

**Decision tree**:
```
Is functionality provided elsewhere (different layer/class)?
  YES → Consider REMOVING this override entirely
  NO  → Patch the existing implementation
```

### 5. Race Conditions and Disposal

iOS lifecycle methods can be called AFTER disposal begins:

| Scenario | Timing Issue |
|----------|--------------|
| App exit | TraitCollectionDidChange called after ServiceProvider disposed |
| View dismissed | ViewDidDisappear called after handler disconnected |
| Rotation | TraitCollectionDidChange during orientation change mid-disposal |

**Safe patterns**:
```csharp
// Pattern 1: Check window handler (best for disposal detection)
var window = handler.MauiContext?.GetPlatformWindow()?.GetWindow();
if (window?.Handler == null)
{
    // Window is being destroyed, skip
    return;
}

// Pattern 2: Try-catch as safety net
try
{
    var service = handler.GetRequiredService<IService>();
    service.DoWork();
}
catch (ObjectDisposedException)
{
    // Services disposed, skip gracefully
}

// Pattern 3: Combine both for maximum safety
if (window?.Handler != null)
{
    try
    {
        // Safe to proceed
    }
    catch (ObjectDisposedException) { }
}
```

### 6. Common iOS Crash Patterns

| Pattern | Indicator | Solution |
|---------|-----------|----------|
| **ObjectDisposedException** | Accessing services after disposal | Check window.Handler before service access |
| **NullReferenceException in lifecycle** | Accessing properties after disconnect | Null-check all handler/context properties |
| **Double-firing events** | Same event triggered twice | Check for duplicate implementations in layers |
| **Race condition on exit** | Intermittent crash during app close | Add disposal checks to lifecycle methods |

## Workflow for iOS Bugs

When debugging an iOS issue:

1. **Read the stack trace** - Note the method and class where it crashed
2. **Search for ALL occurrences** of that method across the codebase (don't assume it's only in one place)
3. **Check layer architecture** - Is this in the right layer? (Core vs Controls vs Compatibility)
4. **Look for duplicates** - Does this method exist in multiple classes?
5. **Check git history** - When was this added? Was it superseded?
6. **Consider "remove vs patch"** - Should this code exist at all?
7. **If patching, add disposal checks** - Protect against race conditions

## Examples from Real Bugs

### Example 1: Issue #33352 - TraitCollectionDidChange Crash

**Stack trace pointed to**: `ShellSectionRootRenderer.TraitCollectionDidChange`

**Agent approach**: 11 attempts patching ShellSectionRootRenderer

**Actual fix**: 
- Searched for "TraitCollectionDidChange" across codebase
- Found DUPLICATE in PageViewController (Core layer)
- Recognized Core layer already handles theme changes
- REMOVED ShellSectionRootRenderer override (duplicate code)
- ENHANCED PageViewController with better disposal checks

**Key lesson**: Search for pattern, don't trust stack trace location alone

## Best Practices Checklist

When fixing iOS bugs, verify:
- [ ] Searched for method pattern across entire codebase (`grep -r "MethodName" src/`)
- [ ] Checked Core, Controls, and Compatibility layers
- [ ] Looked for duplicate implementations
- [ ] Checked git history to understand when code was added
- [ ] Considered whether code should be removed vs patched
- [ ] Added disposal checks if method can be called during teardown
- [ ] Verified fix doesn't break other platform implementations

## Related Documentation

- `.github/copilot-instructions.md` - General MAUI development guidelines
- `docs/design/HandlerResolution.md` - Handler architecture and resolution
- `.github/architecture/core-vs-controls.md` - Layer responsibility quick reference
