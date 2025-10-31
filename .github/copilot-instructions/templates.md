---
applies_to:
  - src/Templates/**
---
<!-- This front-matter configures path-specific Copilot instructions for template files -->

# Working with .NET MAUI Templates

## 1. Overview

When modifying files in the `src/Templates/` directory, you must follow special template semantics and conventions to ensure the templates work correctly when users create new projects. This guide covers conditional compilation directives, naming conventions, file exclusions, and testing procedures.

## 2. Conditional Compilation

Templates use special comment markers to control how preprocessor directives are processed during template instantiation.

### Build-Time Directives (Platform-Specific)

Platform-specific `#if` directives (like `#if WINDOWS`, `#if ANDROID`, `#if IOS`, `#if MACCATALYST`) must be wrapped with `//-:cnd:noEmit` and `//+:cnd:noEmit` markers:

```csharp
//-:cnd:noEmit
#if WINDOWS
    // Windows-specific code
#endif
//+:cnd:noEmit
```

**Why?** These markers tell the template engine to preserve these directives in the generated code exactly as-is, so they will be evaluated at compile-time when the user builds their project.

**Examples:**
```csharp
//-:cnd:noEmit
#if DEBUG
    builder.Logging.AddDebug();
#endif
//+:cnd:noEmit

//-:cnd:noEmit
#if IOS || MACCATALYST
    handlers.AddHandler<CollectionView, CollectionViewHandler2>();
#endif
//+:cnd:noEmit

//-:cnd:noEmit
#if WINDOWS
    Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping(
        "KeyboardAccessibleCollectionView", 
        (handler, view) => { /* ... */ });
#endif
//+:cnd:noEmit
```

### Template-Time Directives (Template Parameters)

Template parameter directives (like `#if (IncludeSampleContent)`) do NOT use the `//-:cnd:noEmit` markers:

```csharp
#if (IncludeSampleContent)
using CommunityToolkit.Maui;
#endif
```

**Why?** These directives are evaluated when the template is instantiated (when user runs `dotnet new maui`), not when the code is compiled.

## 3. Naming and Placeholders

- Template project names use placeholders like `MauiApp._1` which get replaced with the user's actual project name
- Namespaces follow the same pattern: `namespace MauiApp._1;`
- These will be transformed to the user's chosen project name during template instantiation

## 4. Files to Avoid Editing

Never modify auto-generated files in templates:
- `cgmanifest.json` - Auto-generated component governance manifest
- `templatestrings.json` - Auto-generated localization file

These files are regenerated during the build process and should not be manually edited.

## 5. Testing and Verification

When making changes to templates:
1. Build the template project: `dotnet build src/Templates/src/Microsoft.Maui.Templates.csproj`
2. For comprehensive testing, use the `build.ps1` script in the Templates directory to pack, install, and test the template
3. Verify the generated project compiles for all target platforms

## 6. Quick Reference Table

| Directive Type | Wrapper Needed | Example |
| --- | --- | --- |
| Platform-specific (`#if WINDOWS`, `#if ANDROID`, etc.) | ✅ Yes - use `//-:cnd:noEmit` | Build-time platform detection |
| Debug mode (`#if DEBUG`) | ✅ Yes - use `//-:cnd:noEmit` | Build configuration |
| Template parameters (`#if (IncludeSampleContent)`) | ❌ No | Template instantiation options |
