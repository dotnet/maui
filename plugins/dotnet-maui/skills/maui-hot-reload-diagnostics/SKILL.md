---
name: maui-hot-reload-diagnostics
description: >
  Diagnose and troubleshoot .NET MAUI Hot Reload issues (C# Hot Reload, XAML Hot Reload,
  Blazor Hybrid). Covers all UI approaches (XAML, MauiReactor, C# Markup, Blazor Hybrid),
  Visual Studio, VS Code, environment variables, encoding requirements, and MetadataUpdateHandler.
  USE FOR: "hot reload not working", "XAML hot reload", "C# hot reload", "UI not updating",
  "hot reload troubleshooting", "MetadataUpdateHandler", "hot reload Blazor Hybrid",
  "hot reload VS Code", "DOTNET_WATCH".
  DO NOT USE FOR: general build errors (not hot reload related), app lifecycle events
  (use maui-app-lifecycle), or performance profiling (use maui-performance).
---

# .NET MAUI Hot Reload Diagnostics

Systematically diagnose Hot Reload failures for .NET MAUI apps.

## Quick Diagnosis Checklist

1. **Identify what's failing**: XAML Hot Reload (`.xaml` changes) vs C# Hot Reload (`.cs` changes)
2. **Check run configuration**: Must be **Debug** config, started with F5/debugger attached
3. **Save file and re-execute code path**: C# changes require re-triggering the code
4. **Check Hot Reload output**: View > Output > "Hot Reload" (VS) or "C# Hot Reload" (VS Code)

## Environment Variables for Diagnostics

### Enable detailed logging

```bash
# Mac/Linux - Edit and Continue logs
export Microsoft_CodeAnalysis_EditAndContinue_LogDir=/tmp/HotReloadLog

# Windows
set Microsoft_CodeAnalysis_EditAndContinue_LogDir=%temp%\HotReloadLog

# XAML Hot Reload logging
export HOTRELOAD_XAML_LOG_MESSAGES=1

# Xamarin-style debug logging (legacy, may help)
export XAMARIN_HOT_RELOAD_SHOW_DEBUG_LOGGING=1
```

### Check if variables are set

```bash
# Mac/Linux
env | grep -i hotreload
env | grep -i EditAndContinue

# Windows PowerShell
Get-ChildItem Env: | Where-Object { $_.Name -match "hotreload|EditAndContinue" }
```

## File Encoding Requirement

**CRITICAL: All `.cs` files must be UTF-8 with BOM encoding.**

### Check encoding

```bash
# Check if file has BOM (should show "UTF-8 Unicode (with BOM)")
file -I *.cs

# Find files without BOM
find . -name "*.cs" -exec sh -c 'head -c 3 "$1" | od -An -tx1 | grep -q "ef bb bf" || echo "$1"' _ {} \;
```

### Fix encoding (convert to UTF-8 with BOM)

```bash
# Single file
sed -i '1s/^\(\xef\xbb\xbf\)\?/\xef\xbb\xbf/' file.cs

# Or use VS Code: Open file > Save with Encoding > UTF-8 with BOM
```

## VS Code Settings

Enable in VS Code settings (search "Hot Reload"):

```json
{
  "csharp.experimental.debug.hotReload": true,
  "csharp.debug.hotReloadOnSave": true,
  "csharp.debug.hotReloadVerbosity": "detailed"
}
```

## Visual Studio Settings

1. Tools > Options > Debugging > .NET/C++ Hot Reload
2. Enable: **Enable Hot Reload**, **Apply on file save**
3. Set **Logging verbosity** to **Detailed** or **Diagnostic**

## MetadataUpdateHandler

For custom hot reload handling (e.g., MauiReactor), implement `MetadataUpdateHandler`:

```csharp
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(HotReloadService))]

internal static class HotReloadService
{
    // Called when hot reload clears cached metadata
    public static void ClearCache(Type[]? updatedTypes) { }
    
    // Called after hot reload updates are applied
    public static void UpdateApplication(Type[]? updatedTypes)
    {
        // Trigger UI refresh here
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Refresh your UI framework
        });
    }
}
```

### Verify MetadataUpdateHandler is registered

```bash
# Search for handler in codebase
grep -rn "MetadataUpdateHandler" --include="*.cs"

# Check assembly attributes
grep -rn "assembly:.*MetadataUpdateHandler" --include="*.cs"
```

## MauiReactor-Specific Hot Reload

MauiReactor v3+ uses .NET's feature switch pattern for hot reload (no code call needed).

### Setup (MauiReactor v3+)

Add to your `.csproj` file:
```xml
<ItemGroup Condition="'$(Configuration)'=='Debug'">
  <RuntimeHostConfigurationOption Include="MauiReactor.HotReload" Value="true" Trim="false" />
</ItemGroup>

<!-- For Release builds (AOT compatibility) -->
<ItemGroup Condition="'$(Configuration)'=='Release'">
  <RuntimeHostConfigurationOption Include="MauiReactor.HotReload" Value="false" Trim="true" />
</ItemGroup>
```

**Note:** If migrating from MauiReactor v2, **remove** the `EnableMauiReactorHotReload()` call from `MauiProgram.cs`.

### Check MauiReactor hot reload setup

```bash
# Verify RuntimeHostConfigurationOption in csproj
grep -A2 "MauiReactor.HotReload" *.csproj

# Ensure EnableMauiReactorHotReload is NOT present (v3+)
grep -rn "EnableMauiReactorHotReload" --include="*.cs" && echo "WARNING: Remove this call for v3+"
```

### MauiReactor hot reload requirements

1. `RuntimeHostConfigurationOption` set in `.csproj` (not a code call)
2. Debug configuration
3. Debugger attached (F5)
4. Works on all platforms (iOS, Android, Mac Catalyst, Windows)
5. Works in VS Code and Visual Studio

## C# Markup (CommunityToolkit.Maui.Markup) Hot Reload

C# Markup apps need the `ICommunityToolkitHotReloadHandler` to refresh UI on hot reload.

### Setup

1. Add the NuGet package: `CommunityToolkit.Maui.Markup`

2. Enable in MauiProgram.cs:
```csharp
var builder = MauiApp.CreateBuilder();
builder
    .UseMauiApp<App>()
    .UseMauiCommunityToolkitMarkup(); // Enables hot reload support
```

3. Implement the handler interface on pages/views that need refresh:
```csharp
public partial class MainPage : ContentPage, ICommunityToolkitHotReloadHandler
{
    public MainPage()
    {
        Build();
    }

    void Build() => Content = new VerticalStackLayout
    {
        Children = 
        {
            new Label().Text("Hello, World!"),
            new Button().Text("Click Me")
        }
    };

    // Called by hot reload - rebuild UI
    void ICommunityToolkitHotReloadHandler.OnHotReload() => Build();
}
```

### Check C# Markup hot reload setup

```bash
# Verify package reference
grep -i "CommunityToolkit.Maui.Markup" *.csproj

# Check for UseMauiCommunityToolkitMarkup in MauiProgram.cs
grep -n "UseMauiCommunityToolkitMarkup" MauiProgram.cs

# Find classes implementing ICommunityToolkitHotReloadHandler
grep -rn "ICommunityToolkitHotReloadHandler" --include="*.cs"
```

### Key points for C# Markup hot reload

- Extract UI building into a separate `Build()` method
- Implement `ICommunityToolkitHotReloadHandler` on any page/view needing refresh
- The `OnHotReload()` method is called automatically after C# hot reload
- Works alongside standard C# Hot Reload (method body changes)

## Blazor Hybrid Hot Reload

Blazor Hybrid apps in MAUI have their own hot reload behavior for `.razor` and `.css` files.

### How Blazor Hybrid hot reload works

- **Razor components (`.razor`)**: Changes to markup and C# code blocks reload automatically
- **CSS files (`.css`)**: Style changes apply immediately
- **C# code-behind (`.razor.cs`)**: Uses standard C# Hot Reload rules
- **Shared C# code**: Standard C# Hot Reload applies

### Setup requirements

1. Debug configuration (not Release)
2. Debugger attached (F5, not Ctrl+F5)
3. For Visual Studio: Ensure "Hot Reload on File Save" is enabled

### Check Blazor Hybrid setup

```bash
# Verify BlazorWebView is configured
grep -rn "BlazorWebView" --include="*.xaml" --include="*.cs"

# Check for _Imports.razor
find . -name "_Imports.razor"

# Verify wwwroot exists
ls -la */wwwroot/ 2>/dev/null || ls -la wwwroot/ 2>/dev/null
```

### Blazor Hybrid hot reload limitations

- **Adding new components**: May require restart
- **Changing component parameters**: Usually works
- **Modifying `@inject` services**: May require restart
- **Static asset changes** (images, fonts): Require restart
- **Changes to `Program.cs` or `MauiProgram.cs`**: Always require restart

### Troubleshooting Blazor Hybrid

1. **Razor changes not applying**:
   - Ensure file is saved
   - Check browser dev tools console (if available) for errors
   - Verify the component is actually being rendered

2. **CSS changes not applying**:
   - Hard refresh may be needed (changes cached)
   - Check CSS isolation (`.razor.css`) is properly linked

3. **Partial updates / flickering**:
   - This is normal for Blazor's diff-based rendering
   - State may reset if component re-initializes

### Environment variable for Blazor debugging

```bash
# Enable detailed Blazor logging
export ASPNETCORE_ENVIRONMENT=Development
```

## Common Issues and Fixes

### Issue: "Nothing happens when I save"

1. Verify Debug configuration (not Release)
2. Check Hot Reload output for errors
3. Ensure file is saved (not just modified)
4. Re-execute the code path (navigate again, tap button again)

### Issue: "Unsupported edit" / "Rude edit"

Some changes require app restart:
- Adding/removing methods, fields, properties
- Changing method signatures
- Modifying static constructors
- Changes to generics

### Issue: XAML changes don't apply (iOS)

- Set Linker to **Don't Link** in iOS build settings
- Config must be named exactly **Debug**
- Don't use `XamlCompilationOptions.Skip`

### Issue: Changes apply but UI doesn't update

- For C#: Must re-trigger the code (re-navigate, re-tap)
- Check for cached binding values or state
- Verify you're editing the correct target framework file

## Diagnostic Commands

### Collect full diagnostic bundle

```bash
# 1. Environment info
dotnet --info > dotnet-info.txt
dotnet workload list > workloads.txt

# 2. Build with binary log
dotnet build -bl:build.binlog -c Debug

# 3. Check for encoding issues
find . -name "*.cs" -path "*/src/*" | head -20 | xargs file

# 4. Check hot reload env vars
env | grep -iE "(hotreload|editandcontinue|xamarin.*debug)" || echo "No hot reload env vars set"
```

### Enable all diagnostic logging then reproduce

```bash
export Microsoft_CodeAnalysis_EditAndContinue_LogDir=/tmp/HotReloadLog
export HOTRELOAD_XAML_LOG_MESSAGES=1
# Launch IDE from this terminal, reproduce issue, then check /tmp/HotReloadLog/
```

## References

- [Diagnosing Hot Reload (MAUI Wiki)](https://github.com/dotnet/maui/wiki/Diagnosing-Hot-Reload)
- [.NET Hot Reload in Visual Studio](https://learn.microsoft.com/visualstudio/debugger/hot-reload)
- [Supported code changes (C#)](https://learn.microsoft.com/visualstudio/debugger/supported-code-changes-csharp)
- [XAML Hot Reload for .NET MAUI](https://learn.microsoft.com/dotnet/maui/xaml/hot-reload)
- [CommunityToolkit.Maui.Markup Hot Reload](https://learn.microsoft.com/dotnet/communitytoolkit/maui/markup/dotnet-hot-reload)
- [Blazor Hybrid with .NET MAUI](https://learn.microsoft.com/aspnet/core/blazor/hybrid/tutorials/maui)
