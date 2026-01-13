# Investigation: Issue #33497 - XmlnsDefinition with AssemblyName for Global Xmlns

## Issue Summary

GitHub Issue: https://github.com/dotnet/maui/issues/33497

User reported that `XmlnsDefinition` with `AssemblyName` pointing to an external assembly doesn't work for the global xmlns (`http://schemas.microsoft.com/dotnet/maui/global`).

## User's Scenario

The user has in their app's `Imports.cs`:
```csharp
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "MyApp.Core.Api", AssemblyName = "MyApp.Core")]
```

This should make types from `MyApp.Core.Api` namespace available via the global xmlns, but the user reports:
> Error XC0000 XamlC: Cannot resolve type "http://schemas.microsoft.com/dotnet/maui/global:AlertType".

## Investigation Findings

### The Filtering Code

There is filtering code in three places that skips XmlnsDefinition attributes for the global xmlns from external assemblies:

1. **Build.Tasks (XamlC)** - `src/Controls/src/Build.Tasks/XmlTypeExtensions.cs`:
   ```csharp
   //only add globalxmlns definition from the current assembly
   if (attr.XmlNamespace == XamlParser.MauiGlobalUri
       && asmDef != currentAssembly)
       continue;
   ```

2. **Runtime** - `src/Controls/src/Xaml/XamlParser.cs`:
   ```csharp
   // Only add global xmlns definition from the current assembly
   if (attribute.XmlNamespace == XamlParser.MauiGlobalUri
       && assembly != currentAssembly)
       continue;
   ```

3. **SourceGen** - `src/Controls/src/SourceGen/GeneratorHelpers.cs`:
   ```csharp
   //only add globalxmlns definition from the current assembly
   if (xmlnsDef.XmlNamespace == XamlParser.MauiGlobalUri
       && !SymbolEqualityComparer.Default.Equals(assembly, compilation.Assembly))
       continue;
   ```

### Key Distinction

The filter checks **where the XmlnsDefinition attribute is declared** (`asmDef`/`assembly`), NOT which assembly the types come from (`AssemblyName` property).

This means:
- ✅ **XmlnsDefinition in LOCAL assembly** pointing to external types via `AssemblyName` → **WORKS**
- ❌ **XmlnsDefinition IN EXTERNAL assembly** for global xmlns → **Blocked (by design)**

### Test Results

I created two tests to verify this behavior:

#### Positive Test (`Maui33497`)
- XmlnsDefinition declared in the test assembly (local) with `AssemblyName` pointing to ExternalAssembly
- **Result: PASSES** - Types from external assembly are resolvable via global xmlns

#### Negative Test (`Maui33497Negative`)
- XmlnsDefinition declared IN the ExternalAssembly for global xmlns
- **Result: PASSES** - The filter correctly blocks this, and the type is NOT resolvable

### Conclusion

**The current code is working correctly for the user's described scenario.**

The user's scenario (XmlnsDefinition in their app's `Imports.cs` with `AssemblyName` pointing to `MyApp.Core`) should work because:
1. The attribute is declared in the **current/consuming assembly** (the app)
2. The `AssemblyName` property just tells where to find types, but doesn't trigger the filter
3. The filter only blocks attributes that are **physically declared** in external assemblies

### Possible Root Causes for User's Issue

If the user is still experiencing problems, possible causes include:

1. **Assembly name mismatch**: The `AssemblyName` value might not exactly match the actual assembly name
2. **Namespace mismatch**: The CLR namespace might not exactly match the types' actual namespace
3. **Build order issues**: The referenced project might not be built before the consuming project
4. **Different scenario**: The user might actually have the XmlnsDefinition in the library (MyApp.Core) rather than the app

### Recommendation

Ask the user to clarify:
1. Which project/assembly contains the `XmlnsDefinition` attribute - the app or the library?
2. What is the exact assembly name of `MyApp.Core` (check the `.csproj` for `<AssemblyName>`)
3. Does it work with NuGet packages but not project references?

## Files Created

- `src/Controls/tests/Xaml.UnitTests/Issues/Maui33497.xaml` - Positive test XAML
- `src/Controls/tests/Xaml.UnitTests/Issues/Maui33497.xaml.cs` - Positive test code
- `src/Controls/tests/Xaml.UnitTests.ExternalAssembly/Issues/Maui33497/AlertType.cs` - Type for positive test
- `src/Controls/tests/Xaml.UnitTests/Issues/Maui33497Negative.rt.xaml` - Negative test XAML
- `src/Controls/tests/Xaml.UnitTests/Issues/Maui33497Negative.xaml.cs` - Negative test code
- `src/Controls/tests/Xaml.UnitTests.ExternalAssembly/Issues/Maui33497Negative/ExternalGlobalType.cs` - Type for negative test
