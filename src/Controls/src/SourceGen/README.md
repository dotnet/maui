# XmlnsDefinition Source Generator

This document describes the XmlnsDefinition source generator that allows you to define XAML namespace mappings using MSBuild items instead of assembly attributes.

## Overview

The XmlnsDefinition source generator reads `XmlnsDefinition` items from your project file and generates the corresponding `XmlnsDefinitionAttribute` declarations automatically. This provides a convenient way to manage XAML namespace mappings declaratively.

## Usage

Add `XmlnsDefinition` items to your project file's `ItemGroup`:

```xml
<ItemGroup>
  <XmlnsDefinition Include="My.Local.Namespace.Here" />
  <XmlnsDefinition Include="My.External.Namespace.Here" AssemblyName="MyAssemblyName" />
</ItemGroup>
```

This will generate the following C# code:

```csharp
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "My.Local.Namespace.Here")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "My.External.Namespace.Here", AssemblyName="MyAssemblyName")]
```

## Item Attributes

- **Include** (required): The CLR namespace to map to the XML namespace
- **AssemblyName** (optional): The assembly name containing the types in the namespace

## Features

- Automatically uses the global MAUI XML namespace: `http://schemas.microsoft.com/dotnet/maui/global`
- Supports both local namespaces (within the current assembly) and external namespaces
- Generates clean, properly formatted assembly attributes
- Integrates seamlessly with the MAUI build process

## Generated Output

The source generator creates a file named `XmlnsDefinitions.g.cs` in your project's generated files directory with all the necessary assembly attributes.

## Comparison with Manual Approach

**Before (manual assembly attributes):**
```csharp
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "My.Local.Namespace.Here")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "My.External.Namespace.Here", AssemblyName="MyAssemblyName")]
```

**After (MSBuild items):**
```xml
<ItemGroup>
  <XmlnsDefinition Include="My.Local.Namespace.Here" />
  <XmlnsDefinition Include="My.External.Namespace.Here" AssemblyName="MyAssemblyName" />
</ItemGroup>
```

## Benefits

1. **Declarative**: Define namespace mappings in your project file alongside other build configuration
2. **Maintainable**: All XAML namespace mappings are centralized and easy to find
3. **Consistent**: Uses the same MAUI global XML namespace for all mappings
4. **Build-time**: Generates code only when needed, reducing manual maintenance

## Requirements

This feature is available in .NET MAUI projects that have the MAUI source generator enabled (which is the default).