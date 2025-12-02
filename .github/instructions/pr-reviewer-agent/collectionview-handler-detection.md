# CollectionView Handler Detection

**CRITICAL**: If a PR affects CollectionView or CarouselView, you MUST determine which handler implementation to enable before testing.

---

## Why This Matters

There are **TWO separate handler implementations**:

1. **CollectionViewHandler** (`Handlers/Items/`) - Original implementation
2. **CollectionViewHandler2** (`Handlers/Items2/`) - New implementation

**Default behavior on iOS/MacCatalyst**: CollectionViewHandler2 is used by default.

**Critical Issue**: If a PR fixes a bug in CollectionViewHandler but you don't explicitly enable it, the default CollectionViewHandler2 will be used and **the bug will not reproduce**.

---

## Detection Algorithm

### Step 1: Check Which Handler Files Were Changed

```bash
# After fetching PR, check changed files
git diff <base-branch>..<pr-branch> --name-only | grep -i "handlers/items"

# Look for path pattern:
# - Contains "/Items/" (NOT "Items2") → CollectionViewHandler
# - Contains "/Items2/" → CollectionViewHandler2
```

**Key Patterns**:
- `src/Controls/src/Core/Handlers/Items/iOS/` → **CollectionViewHandler**
- `src/Controls/src/Core/Handlers/Items2/iOS/` → **CollectionViewHandler2**

### Step 2: Configure MauiProgram.cs

Edit `src/Controls/tests/TestCases.HostApp/MauiProgram.cs` to enable the correct handler for your test page.

---

## Configuration Examples

### For Items/ (CollectionViewHandler - Original)

```csharp
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // ... font configuration ...
            });

#if IOS || MACCATALYST
        // Enable CollectionViewHandler (original implementation)
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, 
                Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.CarouselView, 
                Microsoft.Maui.Controls.Handlers.Items.CarouselViewHandler>();
        });
#endif

        return builder.Build();
    }
}
```

### For Items2/ (CollectionViewHandler2 - New)

```csharp
#if IOS || MACCATALYST
        // Enable CollectionViewHandler2 (new implementation)
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, 
                Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
            handlers.AddHandler<Microsoft.Maui.Controls.CarouselView, 
                Microsoft.Maui.Controls.Handlers.Items2.CarouselViewHandler2>();
        });
#endif
```

---

## Example Analysis: PR #32795

```bash
# Check changed files
$ git diff <base>..<pr> --name-only | grep Items
src/Controls/src/Core/Handlers/Items/iOS/TemplatedCell.cs
                               ^^^^^
                               This is "Items/" NOT "Items2/"
```

**Conclusion**: PR affects **CollectionViewHandler** (original implementation)

**Action Required**: Must enable CollectionViewHandler in MauiProgram.cs

**Critical**: Without this explicit configuration, iOS/MacCatalyst will use the default CollectionViewHandler2, and the bug described in the PR will not reproduce!

---

## When to Configure Handlers

### Configure handlers when:

- ✅ PR modifies any file in `Handlers/Items/` or `Handlers/Items2/`
- ✅ PR description mentions CollectionView or CarouselView behavior changes
- ✅ Issue linked to PR mentions CollectionView/CarouselView issues

### Skip handler configuration when:

- ❌ PR only affects other controls (Button, Label, Entry, etc.)
- ❌ No mention of collection controls in PR description or linked issues
- ❌ PR only modifies test files or documentation

---

## Quick Reference

| Path Pattern | Handler to Enable | Namespace |
|--------------|------------------|-----------|
| `Handlers/Items/` | CollectionViewHandler | `Microsoft.Maui.Controls.Handlers.Items` |
| `Handlers/Items2/` | CollectionViewHandler2 | `Microsoft.Maui.Controls.Handlers.Items2` |

**Default Behavior** (if no explicit configuration):
- iOS/MacCatalyst: Uses **CollectionViewHandler2**
- Android: Uses **CollectionViewHandler**

---

## Common Mistakes

### ❌ Wrong: Not configuring handler explicitly

```csharp
// PR changes Handlers/Items/iOS/TemplatedCell.cs
// But you don't configure MauiProgram.cs
// Result: iOS uses default CollectionViewHandler2
// Bug doesn't reproduce!
```

### ✅ Correct: Explicit handler configuration

```csharp
// PR changes Handlers/Items/iOS/TemplatedCell.cs
// You add handler configuration to MauiProgram.cs
#if IOS || MACCATALYST
builder.ConfigureMauiHandlers(handlers =>
{
    handlers.AddHandler<CollectionView, 
        Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler>();
});
#endif
// Result: iOS uses CollectionViewHandler (matching the PR)
// Bug reproduces correctly!
```

---

## Platform-Specific Notes

### iOS/MacCatalyst
- **Default**: CollectionViewHandler2
- **Must configure**: To use CollectionViewHandler (original)
- **Platform directives**: `#if IOS || MACCATALYST`

### Android
- **Default**: CollectionViewHandler
- **Less common**: Usually no configuration needed unless testing Handler2

### Windows
- Uses platform-specific implementation
- Collection handler changes typically don't affect Windows
