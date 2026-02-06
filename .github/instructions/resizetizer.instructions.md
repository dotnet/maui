---
applyTo:
  - "src/SingleProject/Resizetizer/**"
  - ".buildtasks/Microsoft.Maui.Resizetizer.After.targets"
---

# Resizetizer MSBuild Targets Guidelines

Guidance for working with .NET MAUI's Resizetizer build system, which processes images, fonts, splash screens, and assets at build time.

## Architecture Overview

The Resizetizer is an MSBuild-integrated pipeline that processes `MauiImage`, `MauiFont`, `MauiSplashScreen`, and `MauiAsset` items into platform-specific resources during the build. All core logic lives in a single file:

**`src/SingleProject/Resizetizer/src/nuget/buildTransitive/Microsoft.Maui.Resizetizer.After.targets`**

### File Loading Order

| File | Purpose |
|------|---------|
| `Microsoft.Maui.Resizetizer.props` | Early property defaults (currently empty) |
| `Microsoft.Maui.Resizetizer.Before.targets` | Pre-SDK target hooks (currently empty) |
| `Microsoft.Maui.Resizetizer.After.targets` | **All logic** — targets, item registration, platform dispatch |

`After.targets` is imported via `AfterMicrosoftNETSdkTargets`, ensuring it runs after the .NET SDK targets are loaded.

### Local Testing with `.buildtasks/`

The `.buildtasks/` directory at the repo root contains a local copy of the Resizetizer targets used by Sandbox and sample builds. It is **NOT git-tracked**. When testing MSBuild target changes locally:

1. Edit the source file in `src/SingleProject/Resizetizer/src/nuget/buildTransitive/`
2. Copy it to `.buildtasks/Microsoft.Maui.Resizetizer.After.targets`
3. Build the Sandbox or sample project to test

⚠️ Always remember to update both files. The `.buildtasks/` copy is what actually runs during local Sandbox builds.

## Key Properties

### Intermediate Output Paths

```xml
<_ResizetizerIntermediateOutputPath>$(IntermediateOutputPath)</_ResizetizerIntermediateOutputPath>
<_ResizetizerIntermediateOutputRoot>$(_ResizetizerIntermediateOutputPath)resizetizer\</_ResizetizerIntermediateOutputRoot>
<_MauiIntermediateImages>...\resizetizer\r\</_MauiIntermediateImages>
<_MauiIntermediateFonts>...\resizetizer\f\</_MauiIntermediateFonts>
<_MauiIntermediateSplashScreen>...\resizetizer\sp\</_MauiIntermediateSplashScreen>
```

**⚠️ CRITICAL**: `_ResizetizerIntermediateOutputPath` defaults to `$(IntermediateOutputPath)`, which **differs between outer and inner builds** in multi-targeting scenarios:
- Outer build: `obj/Release/net10.0-android/`
- Inner build (arm64): `obj/Release/net10.0-android/android-arm64/`

This means stamp files, input tracking files, and intermediate outputs are at **different paths** in outer vs inner builds.

### Stamp Files (Incremental Build Tracking)

| Stamp File | Tracks |
|------------|--------|
| `mauifont.stamp` | Font processing (ProcessMauiFonts) |
| `mauiimage.stamp` | Image resizing (ResizetizeImages) |
| `mauisplash.stamp` | Splash screen processing |
| `mauimanifest.stamp` | Platform manifest generation |

Each stamp file (except `mauimanifest.stamp`) has a companion `.inputs` file containing serialized metadata for change detection.

## Target Pipeline

### Main Targets (Execution Order)

```
ResizetizeCollectItems          ← Collects items from project + references
    ├── ProcessMauiAssets        ← Computes asset paths and registers platform items
    ├── ProcessMauiSplashScreens ← Generates splash resources
    ├── ProcessMauiFonts         ← Copies font files (incremental)
    │   └── _CollectMauiFontItems ← Registers platform items (ALWAYS runs)
    └── ResizetizeImages         ← Resizes images (incremental)
```

### Platform-Specific Scheduling

| Platform | ResizetizeCollectItems | ProcessMauiFonts / _CollectMauiFontItems | ResizetizeImages |
|----------|----------------------|-----------------|-----------------|
| **iOS** | `CollectBundleResourcesDependsOn`, `CompileImageAssetsDependsOn` | `_CollectMauiFontItems` via `CollectAppManifestsDependsOn` | `AfterTargets=ResizetizeCollectItems` |
| **Android** | `BeforeTargets=_ComputeAndroidResourcePaths` | `AfterTargets=ResizetizeCollectItems` | `AfterTargets=ResizetizeCollectItems` |
| **Windows** | Via `DependsOnTargets` (from `ResizetizeImages`/`ProcessMauiFonts`) | `BeforeTargets=AssignTargetPaths` | `BeforeTargets=AssignTargetPaths` |
| **WPF** | Via `DependsOnTargets` (from `ResizetizeImages`/`ProcessMauiFonts`) | `BeforeTargets=FileClassification` | `BeforeTargets=FileClassification` |
| **Tizen** | Via `DependsOnTargets` (from `ResizetizeImages`/`ProcessMauiFonts`) | `AfterTargets=ResizetizeCollectItems` | `AfterTargets=ResizetizeCollectItems` |

## ⚠️ Critical Pattern: Inputs/Outputs and Item Registration

### The Problem

MSBuild's `Inputs`/`Outputs` incremental check skips **tasks** when outputs are up-to-date, but **still evaluates ItemGroups and PropertyGroups** via [output inference](https://learn.microsoft.com/en-us/visualstudio/msbuild/incremental-builds#output-inference). However, this is dangerous when ItemGroups depend on side-effects of skipped tasks:

- Wildcard globs (`$(_MauiIntermediateFonts)*`) depend on files created by `Copy` tasks — if the intermediate directory is missing (partial clean, concurrent builds), the glob evaluates to nothing
- Tasks like `CreatePartialInfoPlistTask` are genuinely skipped — their output files won't exist if they haven't run

### The Solution: Split Target Pattern

**ALWAYS separate file-processing work from item registration into two targets:**

1. **Processing target** (with `Inputs`/`Outputs`): Does the actual work (copy, resize, generate)
2. **Collection target** (NO `Inputs`/`Outputs`): Registers platform-specific items — always runs

```xml
<!-- Target 1: Does work, can be skipped by incremental build -->
<Target Name="ProcessMauiFonts"
    Inputs="@(MauiFont)" Outputs="$(_MauiFontStampFile)"
    ...>
    <Copy SourceFiles="@(MauiFont)" DestinationFolder="$(_MauiIntermediateFonts)" />
    <Touch Files="$(_MauiFontStampFile)" AlwaysCreate="True" />
</Target>

<!-- Target 2: Registers items, ALWAYS runs -->
<Target Name="_CollectMauiFontItems"
    DependsOnTargets="ProcessMauiFonts"
    ...>
    <ItemGroup>
        <AndroidAsset Include="@(MauiFont->'$(_MauiIntermediateFonts)%(Filename)%(Extension)')" />
    </ItemGroup>
</Target>
```

### Item Collection Best Practices

**✅ DO**: Use predictive path mapping from source items:
```xml
<_MauiFontCopied Include="@(MauiFont->'$(_MauiIntermediateFonts)%(Filename)%(Extension)')" />
```

**❌ DON'T**: Use wildcard globs on intermediate directories — they can pick up stale files from deleted sources:
```xml
<!-- AVOID: May include stale files from previously deleted fonts -->
<_MauiFontCopied Include="$(_MauiIntermediateFonts)*" />
```

**Exception**: `ResizetizeImages` uses wildcard globs (`$(_MauiIntermediateImages)**\*`) because image resizing produces multiple output files per input (different sizes/densities). It compensates by explicitly deleting orphaned files.

## Platform Item Registration

### How Each Platform Receives Assets

**Font Items** (from `_CollectMauiFontItems`):

| Platform | Item Type | Metadata |
|----------|-----------|----------|
| **iOS** | `BundleResource` | `LogicalName`, `TargetPath` |
| **Android** | `AndroidAsset` | `Link` |
| **Windows** | `ContentWithTargetPath` | `TargetPath`, `CopyToPublishDirectory` |
| **WPF** | `Resource` | `LogicalName`, `Link` |
| **Tizen** | `TizenTpkUserIncludeFiles` | `TizenTpkSubDir` |

**Image Items** (from `ResizetizeImages`):

| Platform | Item Type | Metadata |
|----------|-----------|----------|
| **iOS** | `BundleResource` or `ImageAsset` | `LogicalName`, `TargetPath` (+ `Link` for ImageAsset) |
| **Android** | `LibraryResourceDirectories` | `StampFile` |
| **Windows** | `ContentWithTargetPath` | `TargetPath`, `CopyToPublishDirectory` |
| **WPF** | `Resource` | `LogicalName`, `Link` |
| **Tizen** | `TizenTpkUserIncludeFiles` | `TizenTpkSubDir` |

### iOS-Specific: Info.plist Font Registration

iOS requires fonts to be declared in Info.plist via `UIAppFonts`. The `CreatePartialInfoPlistTask` generates a `MauiInfo.plist` fragment, which is then added to `PartialAppManifest` for merging.

**Important**: The plist generation (`CreatePartialInfoPlistTask`) is inside `ProcessMauiFonts` (the incremental target), while the `PartialAppManifest` registration is in `_CollectMauiFontItems` (always runs). This is correct because:
- The plist only needs regeneration when fonts change (handled by Inputs/Outputs)
- The plist FILE registration must happen every build (handled by always-run target using `Exists()` check)

## ResizetizeCollectItems

This target is the starting point for the pipeline. It:

1. Calls `GetMauiItems` on the project itself (if `ResizetizerIncludeSelfProject='True'`)
2. Calls `GetMauiItems` on all `@(ProjectReference)` projects (parallel MSBuild calls)
3. Aggregates `MauiImage`, `MauiIcon`, `MauiFont`, `MauiAsset`, `MauiSplashScreen` from all sources
4. Serializes item metadata to `.inputs` files for incremental change detection
5. Computes hashes for splash screen filename stability

## MSBuild Scheduling Semantics

### Understanding AfterTargets / BeforeTargets / DependsOnTargets

**All three are hard requirements.** None of them are "hints" or "suggestions."

| Mechanism | Meaning | When to Use |
|-----------|---------|-------------|
| `DependsOnTargets="X"` | "When I run, run X first (if it hasn't run)" | Hard dependency chain |
| `AfterTargets="X"` | "After X runs, run me" | Scheduling — ensures ordering |
| `BeforeTargets="X"` | "Before X runs, run me" | Scheduling — ensures ordering |

**Key difference**: `DependsOnTargets` is **pull-based** (only runs if the depending target runs). `AfterTargets`/`BeforeTargets` are **push-based** (registers the target to run whenever the referenced target runs).

**⚠️ CRITICAL**: `DependsOnTargets` alone does NOT trigger a target. Something must invoke the target first (via `AfterTargets`, `BeforeTargets`, or another target's `DependsOnTargets`).

### Common Pitfall: Items Dependent on Task Side-Effects

**Never use wildcard globs that depend on files created by tasks in the same target.** During output inference (when the target is skipped), tasks don't run but ItemGroups ARE evaluated — globs will find nothing if the intermediate files don't exist yet.

```xml
<!-- ❌ DANGEROUS: glob depends on Copy task having run -->
<Copy SourceFiles="@(MauiFont)" DestinationFolder="$(_MauiIntermediateFonts)" />
<ItemGroup>
    <_MauiFontCopied Include="$(_MauiIntermediateFonts)*" />
</ItemGroup>

<!-- ✅ SAFE: predictive mapping from source items, filesystem-independent -->
<ItemGroup>
    <_MauiFontCopied Include="@(MauiFont->'$(_MauiIntermediateFonts)%(Filename)%(Extension)')" />
</ItemGroup>
```

This is the root cause of issue #23268.

## Platform Detection Properties

| Property | Detects |
|----------|---------|
| `_ResizetizerIsAndroidApp` | Android application (`AndroidApplication='True'`) |
| `_ResizetizerIsiOSApp` | iOS/MacCatalyst application (includes both) |
| `_ResizetizerIsWindowsAppSdk` | Windows App SDK (WinUI) |
| `_ResizetizerIsWPFApp` | WPF application |
| `_ResizetizerIsTizenApp` | Tizen application |
| `_ResizetizerIsCompatibleApp` | Any of the above |

## Testing MSBuild Target Changes

### Build Verification

```bash
# 1. Copy updated targets to .buildtasks/
cp src/SingleProject/Resizetizer/src/nuget/buildTransitive/Microsoft.Maui.Resizetizer.After.targets \
   .buildtasks/Microsoft.Maui.Resizetizer.After.targets

# 2. Clean build test
rm -rf artifacts/obj/Maui.Controls.Sample.Sandbox/Release/net10.0-android/
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj \
  -f:net10.0-android -c:Release --no-restore

# 3. Verify fonts exist
find artifacts/obj/Maui.Controls.Sample.Sandbox/Release/net10.0-android/ -path "*/assets/*.ttf" | wc -l

# 4. Incremental build test (no changes)
dotnet build ... (same command)
# Verify fonts still present

# 5. Diagnostic build to verify target execution
dotnet build ... -v:diag 2>&1 | grep -E "ProcessMauiFonts|_CollectMauiFontItems|Skipping"
```

### Key Things to Verify

- **Clean build**: All assets appear in output
- **Incremental build**: Processing targets SKIP, collection targets RUN, assets still present
- **No unnecessary downstream work**: Platform asset targets (e.g., `_GenerateAndroidAssetsDir`) should skip when fonts haven't changed
- **Modified input**: Touching a font source file should cause `ProcessMauiFonts` to re-run

## Common Mistakes

| Mistake | Impact | Correct Approach |
|---------|--------|-----------------|
| Use wildcard glob dependent on task output | Glob finds nothing if task was skipped (output inference) | Use predictive path mapping from source items |
| Put task-dependent logic in same target as work | During output inference, tasks are skipped but ItemGroups evaluate | Use split target pattern |
| Forget to copy changes to `.buildtasks/` | Local testing uses old code | Always copy after editing source |
| Assume `DependsOnTargets` triggers execution | Target never runs | Add `AfterTargets` or `BeforeTargets` trigger |
| Mix up `AfterTargets` vs `DependsOnTargets` | Both are hard requirements, but serve different purposes | `DependsOnTargets` = pull, `AfterTargets` = push |
| Assume target body is fully skipped by Inputs/Outputs | ItemGroups ARE evaluated via output inference; only tasks are skipped | Be aware of output inference; don't rely on it for task-dependent items |
