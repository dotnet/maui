# Release Notes Templates

## Index Page Template

File: `release-notes/maui-release-notes.md`

```markdown
# .NET MAUI Workload Release Notes

> **Last Updated:** {current_date}

This page lists all .NET MAUI workload release notes. Each entry links to detailed release notes with full dependency information and installation instructions.

---

## Releases

### {MMMM D, YYYY}

📄 [Full Release Notes](maui-release-notes-{YYYYMMDD}.md)

#### .NET {major}

**Workload Set:** `{workload_set_version}` (CLI: `{cli_version}`)

| Workload | Version | Requirements |
|----------|---------|--------------|
| MAUI | {maui_version} | |
| iOS | {ios_version} | Xcode ≥ {xcode_version} |
| Mac Catalyst | {maccatalyst_version} | Xcode ≥ {xcode_version} |
| tvOS | {tvos_version} | Xcode ≥ {xcode_version} |
| macOS | {macos_version} | Xcode ≥ {xcode_version} |
| Android | {android_version} | API {android_api}, JDK {jdk_version} |

| MAUI NuGet Packages | Implicit | Latest |
|---------------------|----------|--------|
| Microsoft.Maui.Controls | {implicit_version} | {latest_version} |

---
```

**Index Entry Rules:**
- Only include .NET version sections that had updates
- Link to full notes at top for quick access
- Requirements column: Xcode for Apple platforms, API + JDK for Android, empty for MAUI

---

## Dated Release Notes Template

File: `release-notes/maui-release-notes-{YYYYMMDD}.md`

### Header

```markdown
# .NET MAUI Workload Release Notes - {Month Day, Year}

> **Date:** {current_date}  
> **Source:** NuGet.org workload manifests (live data)

This document provides version information for .NET MAUI and related platform workloads across the two most recent major .NET versions.

⬅️ [Back to Release Notes Index](maui-release-notes.md)

---
```

### Per .NET Version Section

```markdown
# .NET {major}

## Workloads

**Workload Set:** [Microsoft.NET.Workloads.{major}.0.{band}](https://www.nuget.org/packages/Microsoft.NET.Workloads.{major}.0.{band}/{nuget_version})

## Installation

\`\`\`bash
dotnet workload update --version {cli_version}
# Or install specific workloads
dotnet workload install maui ios maccatalyst android --version {cli_version}
\`\`\`

## Workload Versions

| Workload | Full ID | Version | SDK Band | Links |
|----------|---------|---------|----------|-------|
| **MAUI** | Microsoft.NET.Sdk.Maui | {version} | {band} | [NuGet]({manifest_link}) · [Release]({dotnet_maui_release_link}) |
| **iOS** | Microsoft.NET.Sdk.iOS | {version} | {band} | [NuGet]({manifest_link}) · [Release]({dotnet_macios_release_link}) |
| **Mac Catalyst** | Microsoft.NET.Sdk.MacCatalyst | {version} | {band} | [NuGet]({manifest_link}) · [Release]({dotnet_macios_release_link}) |
| **Android** | Microsoft.NET.Sdk.Android | {version} | {band} | [NuGet]({manifest_link}) · [Release]({dotnet_android_release_link}) |
| **tvOS** | Microsoft.NET.Sdk.tvOS | {version} | {band} | [NuGet]({manifest_link}) · [Release]({dotnet_macios_release_link}) |
| **macOS** | Microsoft.NET.Sdk.macOS | {version} | {band} | [NuGet]({manifest_link}) · [Release]({dotnet_macios_release_link}) |



## MAUI NuGet Packages

The following NuGet packages are the core .NET MAUI libraries. The **Implicit Version** is bundled with the workload, while **Latest Version** may be newer due to out-of-band releases.

| Package | Implicit Version | Latest Version |
|---------|------------------|----------------|
| Microsoft.Maui.Controls | [{workload_version}]({nuget_link}) | [{latest_version}]({nuget_link}) |
| Microsoft.Maui.Controls.Compatibility | [{workload_version}]({nuget_link}) | [{latest_version}]({nuget_link}) |
| Microsoft.Maui.Essentials | [{workload_version}]({nuget_link}) | [{latest_version}]({nuget_link}) |
| Microsoft.Maui.Graphics | [{workload_version}]({nuget_link}) | [{latest_version}]({nuget_link}) |
| Microsoft.Maui.Maps | [{workload_version}]({nuget_link}) | [{latest_version}]({nuget_link}) |


## Apple Platform Dependencies (iOS, Mac Catalyst, tvOS, macOS)

| Workload | Xcode Requirement | Recommended Xcode |
|----------|-------------------|-------------------|
| iOS | ≥ {min_version} | {recommended} |
| Mac Catalyst | ≥ {min_version} | {recommended} |
| tvOS | ≥ {min_version} | {recommended} |
| macOS | ≥ {min_version} | {recommended} |

## Android Dependencies

| Dependency | Requirement | Recommended |
|------------|-------------|-------------|
| **JDK** | ≥ {min} and < {max} | {recommended} |

### Required Android SDK Packages

| Package | Description |
|---------|-------------|
| `build-tools;{version}` | Android SDK Build-Tools {major} |
| `cmdline-tools;{version}` | Android SDK Command-line Tools |
| `platforms;android-{api}` | Android SDK Platform {api} |
| `platform-tools` | Android SDK Platform-Tools |

### Optional Android SDK Packages

| Package | Description |
|---------|-------------|
| `emulator` | Android Emulator |
| `ndk-bundle` | NDK |
| `platforms;android-{preview_api}` | Android SDK Platform (Preview) |
| System Images | Google APIs ARM 64 v8a / x86_64 |

## MAUI Windows Dependencies

| Dependency | Minimum Version | Recommended |
|------------|-----------------|-------------|
| **Windows App SDK** | ≥ {version} | {version} |
| **Windows SDK Build Tools** | ≥ {version} | {version} |
| **Win2D** | ≥ {version} | {version} |
| **WebView2** | ≥ {version} | {version} |
```

### Reference Section (End of Document)

```markdown
---

# Reference

## Version Notation

| Notation | Meaning |
|----------|---------|
| `[17.0,22.0)` | ≥ 17.0 AND < 22.0 |
| `[26.2,)` | ≥ 26.2 (no upper bound) |

For more information, see the [.NET Workload Sets documentation](https://learn.microsoft.com/dotnet/core/tools/dotnet-workload-sets).

## Platform GitHub Repositories

| Platform | Repository | Releases |
|----------|------------|----------|
| MAUI | [dotnet/maui](https://github.com/dotnet/maui) | [Releases](https://github.com/dotnet/maui/releases) |
| iOS, Mac Catalyst, tvOS, macOS | [dotnet/macios](https://github.com/dotnet/macios) | [Releases](https://github.com/dotnet/macios/releases) |
| Android | [dotnet/android](https://github.com/dotnet/android) | [Releases](https://github.com/dotnet/android/releases) |

> NOTE: macios releases may not use exact versions in their name/tag... an example is: `dotnet-11.0.1xx-preview2-11425` - DO YOUR BEST to match the relevant release note link in this repo to the workload release version we are generating notes for.
```
