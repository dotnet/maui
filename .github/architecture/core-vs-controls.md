# Core vs Controls Layer Architecture

Quick reference for understanding the separation between Core and Controls layers in .NET MAUI.

## Layer Overview

```
┌─────────────────────────────────────────────────────────────┐
│  Controls Layer (src/Controls/)                             │
│  - Control-specific implementations                         │
│  - Button, Label, Entry, etc.                              │
│  - Shell, TabbedPage, NavigationPage                       │
│  - Higher-level abstractions                               │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ Uses
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  Core Layer (src/Core/)                                     │
│  - Fundamental page/view lifecycle                          │
│  - Handler infrastructure                                   │
│  - Platform-agnostic abstractions                          │
│  - Foundation for all controls                             │
└─────────────────────────────────────────────────────────────┘
```

## Decision Guide: Where Does My Code Belong?

### Core Layer (`src/Core/`)

Use Core layer when:
- ✅ Functionality applies to **ALL pages/views** (not specific to one control)
- ✅ Fundamental platform behavior (lifecycle, rendering, layout)
- ✅ Handler infrastructure and base classes
- ✅ Platform-specific integration (iOS UIKit, Android View system)

**Examples**:
- `PageViewController` - Base view controller for ALL pages (iOS)
- `ViewRenderer` - Base renderer infrastructure
- `PlatformWindow` - Window management for all platforms
- `HandlerResolver` - Handler lookup and instantiation
- Theme change detection (applies to entire app)
- Safe area inset handling (applies to all pages)

**Location**: `src/Core/src/Platform/{iOS|Android|Windows}/`

### Controls Layer (`src/Controls/`)

Use Controls layer when:
- ✅ Functionality specific to a **single control type** (Button, Label, Entry, etc.)
- ✅ Control-specific handlers and renderers
- ✅ Higher-level navigation abstractions (Shell, NavigationPage)
- ✅ Control customization and behavior

**Examples**:
- `ButtonHandler` - Button-specific click handling
- `EntryHandler` - Text input specific behavior
- `ShellRenderer` - Shell-specific navigation
- `CollectionViewHandler` - Collection view layout and scrolling
- Control property mapping (Text → UILabel.Text)

**Location**: `src/Controls/src/Core/Platform/{iOS|Android|Windows}/`

### Compatibility Layer (`src/Controls/src/Core/Compatibility/`)

Use Compatibility layer when:
- ✅ Supporting legacy Xamarin.Forms renderers
- ✅ Migration path from old renderer system
- ✅ Shell-specific legacy implementations

**Note**: This layer is being phased out. New code should go in Core or Controls.

## Common Mistakes

### ❌ Mistake 1: Implementing Core Functionality in Controls

**Bad**: Adding theme change detection to `ShellSectionRootRenderer`
```csharp
// src/Controls/.../ShellSectionRootRenderer.cs
public override void TraitCollectionDidChange(...)
{
    // ❌ Theme changes affect ALL pages, not just Shell
    application.ThemeChanged();
}
```

**Good**: Implement in Core layer where ALL pages benefit
```csharp
// src/Core/.../PageViewController.cs
public override void TraitCollectionDidChange(...)
{
    // ✅ All pages automatically get theme detection
    application.ThemeChanged();
}
```

### ❌ Mistake 2: Duplicating Functionality Across Layers

**Problem**: Same override exists in both Core and Controls
```
Core/PageViewController.TraitCollectionDidChange()     ← Affects all pages
Controls/ShellSectionRootRenderer.TraitCollectionDidChange()  ← Only Shell pages
```

**Result**: 
- Theme change fires twice for Shell pages
- Controls override can hide Core bugs
- Race conditions during disposal

**Solution**: Remove the Controls override, keep only Core implementation

### ❌ Mistake 3: Putting Control-Specific Logic in Core

**Bad**: Button click logic in `ViewRenderer` (Core)
```csharp
// src/Core/.../ViewRenderer.cs
public override void OnTouchUpInside(...)
{
    // ❌ Not all views are clickable, this is Button-specific
    RaiseClicked();
}
```

**Good**: Button logic in `ButtonHandler` (Controls)
```csharp
// src/Controls/.../ButtonHandler.cs  
protected override void OnTouchUpInside(...)
{
    // ✅ Button-specific click handling
    RaiseClicked();
}
```

## iOS Lifecycle Methods - Layer Decision Matrix

| Method | Core or Controls? | Reasoning |
|--------|-------------------|-----------|
| `TraitCollectionDidChange` | **Core** | Theme changes affect ALL pages |
| `SafeAreaInsetsDidChange` | **Core** | Safe areas affect ALL pages |
| `ViewDidLoad` | **Core** | Base lifecycle for ALL view controllers |
| `ViewWillAppear` | **Both** | Core for base, Controls for specific behavior |
| Button `TouchUpInside` | **Controls** | Button-specific interaction |
| Entry `EditingChanged` | **Controls** | Entry-specific input handling |

## Quick Check: "Does This Belong Here?"

Ask yourself:
1. **Does this apply to ALL pages/views?**
   - YES → Probably Core
   - NO → Probably Controls

2. **Is this fundamental platform behavior?**
   - YES → Core
   - NO → Controls

3. **Would a non-Shell, non-TabbedPage app need this?**
   - YES → Core
   - NO → Controls

4. **Does this customize ONE specific control?**
   - YES → Controls
   - NO → Core

## How to Fix Layer Violations

### If You Find Core Functionality in Controls Layer:

1. **Check if Core already implements it**:
   ```bash
   grep -r "MethodName" src/Core/src/Platform/iOS/
   ```

2. **If Core has it**: Remove the Controls override (it's duplicate)

3. **If Core doesn't have it**: Move implementation to Core

### If You Find Control-Specific Logic in Core Layer:

1. **Extract to Controls layer handler**
2. **Keep Core layer generic**
3. **Use virtual methods in Core that Controls can override**

## File Location Reference

| Layer | iOS Path | Android Path | Windows Path |
|-------|----------|--------------|--------------|
| **Core** | `src/Core/src/Platform/iOS/` | `src/Core/src/Platform/Android/` | `src/Core/src/Platform/Windows/` |
| **Controls** | `src/Controls/src/Core/Platform/iOS/` | `src/Controls/src/Core/Platform/Android/` | `src/Controls/src/Core/Platform/Windows/` |
| **Compatibility** | `src/Controls/src/Core/Compatibility/Handlers/.../iOS/` | `.../Android/` | `.../Windows/` |

## Related Documentation

- `.github/instructions/ios-debugging.instructions.md` - iOS debugging patterns
- `docs/design/HandlerResolution.md` - Handler architecture details
- `.github/copilot-instructions.md` - General repository guidance
