# MAUI Manual Tests

This document explains the different ways to compile the Microsoft.Maui.ManualTests project, which is designed to work with multiple compilation modes depending on your development scenario. It also covers testing procedures, including using nightly builds, bug handling, and reporting.

## Overview

The project uses conditional compilation based on MSBuild properties to support three different compilation modes:

1. **Source Code Mode** (Default, `UseMaui` != 'true') - Compiles against MAUI source code
2. **MAUI Workloads Mode** - Uses installed MAUI workloads
3. **NuGet Packages Mode** - Uses specific MAUI NuGet packages

---

## 1. Compiling with Source Code

### When to Use
- You're working on the MAUI source code itself
- You need to test changes in MAUI framework components
- You're contributing to the MAUI repository

### Prerequisites
- Complete MAUI source code repository cloned locally
- .NET SDK that matches the MAUI development requirements
- All MAUI source dependencies available in the solution

### How to Compile

This is the default mode when building within the MAUI repository context (no special properties needed, as `UseMaui` evaluates to `!= 'true'` by default).

#### Method 1: Using MSBuild Property (Optional, as it's the default)
```bash
dotnet build -p:UseMaui=false
```

#### Method 2: Modify Project File
Add this property to your project file temporarily:
```xml
<PropertyGroup>
    <UseMaui>false</UseMaui>
</PropertyGroup>
```

#### Method 3: Using Directory.Build.props
Create a `Directory.Build.props` file in the solution root:
```xml
<Project>
    <PropertyGroup>
        <UseMaui>false</UseMaui>
    </PropertyGroup>
</Project>
```

### What Happens in Source Code Mode
When `UseMaui` != 'true', the project:
- References local project files instead of NuGet packages
- Uses `ProjectReference` elements for:
    - `Core.csproj`
    - `Controls.Xaml.csproj`
    - `Controls.Core.csproj`
    - `Microsoft.AspNetCore.Components.WebView.Maui.csproj`
    - `Compatibility.csproj` (if enabled)
    - `Controls.Maps.csproj`
    - `Graphics.csproj`
- Imports `Maui.InTree.props` for additional build configuration
- Includes `Microsoft.Extensions.DependencyInjection` as a direct dependency

---

## 2. Compiling with MAUI Workloads

### When to Use
- Standard app development scenario
- You want to use the officially released MAUI version
- You're building apps that will be distributed to end users

### Prerequisites
- .NET SDK with MAUI workload installed
- Visual Studio 2022 with MAUI workload, or
- .NET CLI with MAUI workload installed

### Installing MAUI Workloads

#### Using .NET CLI
```bash
# Install latest stable MAUI workload
dotnet workload install maui
```

#### Using Visual Studio Installer
1. Open Visual Studio Installer
2. Modify your Visual Studio installation
3. Check ".NET Multi-platform App UI development"
4. Install/Update

### How to Compile
This mode requires explicitly setting `UseMaui=true` and specifying `MauiVersion` to match your installed workload version:

```bash
# Standard build (replace <version> with your installed workload version, e.g., 10.0.0)
dotnet build -p:UseMaui=true -p:MauiVersion=<version>

# Restore packages first (recommended)
dotnet restore -p:UseMaui=true -p:MauiVersion=<version>
dotnet build -p:UseMaui=true -p:MauiVersion=<version>

# Build for specific platform
dotnet build -p:UseMaui=true -p:MauiVersion=<version> -f net10.0-android
dotnet build -p:UseMaui=true -p:MauiVersion=<version> -f net10.0-ios
```

### Using Specific MAUI Workload Versions

#### Check Available Versions
```bash
dotnet workload search maui
```

#### Install Specific Version
```bash
# Example: Install .NET 9 preview workload  
dotnet workload install maui --version 9.0.100-preview.1
```

#### Update to Latest
```bash
dotnet workload update
```

---

## 3. Compiling with NuGet Packages

### When to Use
- You want to use a specific version of MAUI different from your workload
- Testing against multiple MAUI versions
- CI/CD scenarios where you need version control
- Working with preview/beta versions

### Prerequisites
- .NET SDK (MAUI workload not strictly required)
- Access to NuGet feeds containing MAUI packages

### Setup for Nightly Builds
We will only be testing with the latest nightly build MAUI controls package. Add this feed to your local NuGet.config. Typical locations for NuGet.config:

- **Windows:** `%AppData%\NuGet\NuGet.config` (e.g., `C:\Users\<username>\AppData\Roaming\NuGet\NuGet.config`)
- **macOS/Linux:** `~/.nuget/NuGet/NuGet.config`

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="maui-nightly" value="https://pkgs.dev.azure.com/xamarin/public/_packaging/maui-nightly/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

### Project Configuration for Nightly Builds
Navigate to the Microsoft.Maui.Controls package on the nightly feed (or use tools like NuGet Package Explorer or browse the feed's index) to find the latest version (e.g., 8.0.0-nightly.8832+sha.feb791fc7-azdo.8163102). Open the .csproj for the project being tested and replace the package versions for Microsoft.Maui.Controls and Microsoft.Maui.Controls.Compatibility:

```xml
<ItemGroup Condition="$(MajorFrameworkVersion) >= 8.0">
    <PackageReference Include="Microsoft.Maui.Controls" Version="8.0.0-nightly.8832+sha.feb791fc7-azdo.8163102" />
    <PackageReference Include="Microsoft.Maui.Controls.Compatibility" Version="8.0.0-nightly.8832+sha.feb791fc7-azdo.8163102" />
</ItemGroup>
```

Note: you can also set the values to `$(MauiVersion)` and set a `<MauiVersion>` node with the version you want to use. See **Method 2**, below.

### How to Compile

This mode is similar to Workloads Mode but allows specifying any available NuGet version for `MauiVersion`. It requires setting `UseMaui=true`.

You will need to manually add the target frameworks to the `<TargetFrameworks>` node when you use this method. For example, running the project on iOS and Android will require:

```xml
<!-- Instead of: <TargetFrameworks>$(MauiManualTestsPlatforms)</TargetFrameworks> do the line below -->
<TargetFrameworks>net10.0-ios;net10.0-android</TargetFrameworks>
```

Depending on the .NET version you want to use, replace `net10.0` prefix with the version you want to use, for example: `net9.0`.

#### Method 1: Using MSBuild Property
```bash
dotnet build -p:UseMaui=true -p:MauiVersion=8.0.0-nightly.8832+sha.feb791fc7-azdo.8163102
```

#### Method 2: Set Properties in Project File
```xml
<PropertyGroup>
    <UseMaui>true</UseMaui>
    <MauiVersion>8.0.0-nightly.8832+sha.feb791fc7-azdo.8163102</MauiVersion>
</PropertyGroup>
```

Note: make sure to add the `<UseMaui>true</UseMaui>` tag _before_ the first node that has a condition that checks for this value.

### What Happens in NuGet Mode
When `UseMaui` == 'true', the project:
- Uses `PackageReference` instead of `ProjectReference`
- References these NuGet packages:
    - `Microsoft.Maui.Controls.Maps`
    - `Microsoft.Maui.Controls.Compatibility`
- Uses the version specified in `$(MauiVersion)` property
- Core MAUI dependencies are resolved via the SDK or NuGet, depending on the context

### Using Specific NuGet Versions

#### Stable Releases
```xml
<PropertyGroup>
    <UseMaui>true</UseMaui>
    <MauiVersion>10.0.0</MauiVersion>
</PropertyGroup>
```

#### Latest Preview (.NET 10)
```xml
<PropertyGroup>
    <UseMaui>true</UseMaui>
    <MauiVersion>10.0.0-preview.7.25406.3</MauiVersion>
</PropertyGroup>
```

### Finding Available Versions

#### Using NuGet.org
```bash
# Search for available versions
nuget list Microsoft.Maui.Controls -AllVersions -PreRelease
```

#### Using Package Manager Console (Visual Studio)
```powershell
Find-Package Microsoft.Maui.Controls -AllVersions -IncludePrerelease
```

## Important: Conditional ItemGroups

**You do NOT need to remove the conditional ItemGroups** from the project file. These are intentionally designed to work automatically based on the `UseMaui` property:

```xml
<!-- These ItemGroups are CONDITIONAL and switch automatically -->
<ItemGroup Condition=" '$(UseMaui)' != 'true' ">
  <!-- Used ONLY in Source Code Mode -->
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
</ItemGroup>

<ItemGroup Condition=" '$(UseMaui)' != 'true' ">
  <!-- Used ONLY in Source Code Mode - ProjectReferences to local source -->
  <ProjectReference Include="..\..\..\Core\src\Core.csproj" />
  <ProjectReference Include="..\..\src\Xaml\Controls.Xaml.csproj" />
  <ProjectReference Include="..\..\src\Core\Controls.Core.csproj" />
  <!-- ... other project references ... -->
</ItemGroup>

<ItemGroup Condition=" '$(UseMaui)' == 'true' ">
  <!-- Used ONLY in NuGet Package Mode -->
  <PackageReference Include="Microsoft.Maui.Controls.Maps" Version="$(MauiVersion)" />
  <PackageReference Include="Microsoft.Maui.Controls.Compatibility" Version="$(MauiVersion)" />
</ItemGroup>
```

### How the Conditions Work:
- **Source Code Mode** (`UseMaui` != 'true'): Uses ProjectReferences and Microsoft.Extensions.DependencyInjection
- **Workload/NuGet Mode** (`UseMaui` == 'true'): Uses specific MAUI NuGet packages for add-ons; core dependencies are handled by the SDK or NuGet
- The project automatically switches between these modes without any manual changes to the ItemGroups.

### Target Frameworks
The project supports multiple target frameworks based on `$(MauiManualTestsPlatforms)`:

- `net10.0-android` - Android applications
- `net10.0-ios` - iOS applications
- `net10.0-maccatalyst` - macOS Catalyst applications
- `net10.0-windows10.0.17763.0` - Windows applications
- `net10.0-tizen` - Tizen applications

### Building for Specific Platforms

```bash
# Android
dotnet build -f net10.0-android

# iOS (requires macOS)
dotnet build -f net10.0-ios

# Windows
dotnet build -f net10.0-windows10.0.17763.0

# All platforms
dotnet build
```

### Platform Requirements

#### Android
- Android SDK 21+ (as specified in `SupportedOSPlatformVersion`)
- Java 11 or higher

#### iOS
- macOS development machine
- Xcode with iOS 15.0+ SDK
- Valid Apple Developer account for device deployment

#### Windows
- Windows 10 version 1809 (build 17763) or higher
- Windows App SDK

---

## Filing Bugs

Please file ALL bugs here: https://github.com/dotnet/maui/issues. Make sure to tag any regressions with the i/regression label.

---

## Disabling Tests

Please disable tests for any bugs which are not regressions. This can be accomplished by setting the state of the Test Case to Design. Please also leave a comment with a link to the issue. Make sure to track the filed issue so that the test can be reactivated in the future.

---

## Troubleshooting

### Common Issues

#### Issue: Package version conflicts
**Solution**: Clear NuGet cache and restore:
```bash
dotnet nuget cache clear --all
dotnet restore --no-cache
```

#### Issue: MAUI workload not found
**Solution**: Install or update MAUI workload:
```bash
dotnet workload install maui
```

#### Issue: Specific platform build fails
**Solution**: Check platform-specific requirements and ensure proper SDKs are installed.

### Verification Steps

#### Check Your Setup
```bash
# Verify .NET SDK
dotnet --version

# List installed workloads
dotnet workload list

# Check project target frameworks
dotnet list reference
```

#### Clean Build
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

---

## Best Practices

1. **Use Source Code Mode** only when working on MAUI framework itself
2. **Use Workload Mode** for standard app development
3. **Use NuGet Mode** for testing specific versions or CI/CD scenarios
4. **Always restore packages** before building: `dotnet restore`
5. **Keep workloads updated** regularly: `dotnet workload update`
6. **Pin specific versions** in production scenarios
7. **Test across multiple platforms**