# Workload MSI Generation

This document describes how .NET MAUI generates MSI installers for Visual Studio workload insertion via the `src/Workload/workloads.csproj` project.

## Overview

MAUI uses the arcade `CreateVisualStudioWorkload` MSBuild task (from `Microsoft.DotNet.Build.Tasks.Workloads`) to:

1. Generate MSIs from NuGet workload packs
2. Wrap MSIs in NuGet packages (for signing and transport)
3. Generate SWIX projects for Visual Studio insertion (VSDrop)

The output flows into the VS insertion pipeline, which installs MAUI workload packs via the VS installer.

## WiX 6 Migration (completed in .NET 11)

As of .NET 11, MAUI uses **WiX 6** (wix.exe CLI) instead of WiX 3 (candle.exe/light.exe). This migration was inspired by [dotnet/android#11743](https://github.com/dotnet/android/pull/11743) and follows the pattern from the `Xamarin.yaml-templates` `convert-v5/convert.proj`.

### Key Changes from WiX 3 to WiX 6

| Aspect | WiX 3 (old) | WiX 6 (new) |
|--------|-------------|-------------|
| Tool | `candle.exe` / `light.exe` via `WixToolsetPath` | `wix.exe` via `WixExe` property |
| Package | `Microsoft.Signed.WiX` | `Microsoft.Wix` + extension packages |
| Extensions | Bundled in WiX install | Separate NuGet packages (`*.wixext`) |
| Heat | Part of WiX install | Separate `Microsoft.WixToolset.Heat` package |
| MSI table | `WixDependencyProvider` | `Wix4DependencyProvider` (renamed) |
| Signing | `CreateLightCommandPackageDrop` + separate sign step | Native `.wixpack` archives |
| Arcade parameter | `WixToolsetPath` | `WixExe`, `HeatExe`, `WixExtensions` |

### Required Arcade Version

The `CreateVisualStudioWorkload` task supports WiX 6 parameters (`WixExe`, `HeatExe`, `WixExtensions`) starting from arcade **11.0.0-beta.26330.112** or later. Earlier versions only support `WixToolsetPath` (WiX 3).

### NuGet Package Compatibility (NU1212 Fix)

`Microsoft.Wix` has `<PackageType>DotnetTool</PackageType>` in its nuspec. This means:
- It **cannot** be referenced from projects targeting `net8.0` or similar app TFMs (causes NU1212)
- **Fix**: Use `TargetFramework=netstandard2.0` and `ExcludeAssets="all"` on the Microsoft.Wix PackageReference
- `GenerateDependencyFile=false` is also needed since netstandard2.0 doesn't produce a deps file for this project type

### WiX Extension Path Pattern

Extensions are located at: `$(Pkg<PackageId_with_underscores>)\wixext6\WixToolset.<Name>.wixext.dll`

Example:
```xml
<WixExtensions Include="$(PkgMicrosoft_WixToolset_Dependency_wixext)\wixext6\WixToolset.Dependency.wixext.dll" />
```

## Architecture

### File: `src/Workload/workloads.csproj`

This is the central MSI generation project. It:

1. **Restores WiX packages** — `Microsoft.Wix`, `Microsoft.WixToolset.Heat`, and 3 extension packages
2. **Defines WiX tool paths** — `WixExe`, `HeatExe` properties pointing to package tools
3. **Lists WiX extensions** — `WixExtensions` item group with paths to `.wixext.dll` files
4. **Imports `vs-workload.props`** — Generated file listing workload packs to package
5. **Calls `CreateVisualStudioWorkload`** — Arcade task that orchestrates MSI creation
6. **Builds SWIX projects** — For VS insertion metadata (component definitions, manifests)
7. **Creates VSDrop zips** — Three zip files consumed by VS insertion automation

### Task: `CreateVisualStudioWorkload`

From `Microsoft.DotNet.Build.Tasks.Workloads` (arcade). Key parameters:

```xml
<CreateVisualStudioWorkload
    AllowMissingPacks="true"
    PackageSource="$(NuGetPackagePath)"
    WixExe="$(WixExe)"              <!-- Path to wix.exe -->
    HeatExe="$(HeatExe)"            <!-- Path to heat.exe -->
    WixExtensions="@(WixExtensions)" <!-- List of .wixext.dll paths -->
    WorkloadManifestPackageFiles="@(WorkloadPackages)" >
  <Output TaskParameter="Msis" ItemName="VSWorkloadMsis" />
  <Output TaskParameter="SwixProjects" ItemName="SwixProjects" />
</CreateVisualStudioWorkload>
```

### SWIX Build

SWIX (Software Installer for Xbox — repurposed for VS) projects generate the VS component definitions. We use `MicroBuild.Plugins.SwixBuild.Dotnet` to build these via MSBuild:

```xml
<MSBuild Projects="@(PartitionedSwixProjects)" Properties="SwixBuildTargets=$(SwixTargetsPath);ManifestOutputPath=%(ManifestOutputPath)"/>
```

> **Note**: The dotnet/dotnet (SDK) repository switched to using desktop MSBuild via `Exec` for SWIX builds because the `.Dotnet` variant of SwixBuild uses a `CodeTaskFactory` that may become incompatible with newer .NET. See [dotnet/dotnet#5265](https://github.com/dotnet/dotnet/pull/5265). If SWIX build failures occur in the future, this approach may need to be adopted.

### VSDrop Outputs

Three zip files are produced for VS insertion:

| Zip | Contents | SwixProjects filter |
|-----|----------|-------------------|
| `Workload.VSDrop.*.components.zip` | Stable components + manifests | `PackageType == 'component'` (non-preview) + manifests |
| `Workload.VSDrop.*-pre.components.zip` | Preview components + manifests | `PackageType == 'component'` (preview) + manifests |
| `Workload.VSDrop.*.packs.zip` | Workload pack MSIs | `PackageType == 'msi-pack'` |

## Validation Criteria

When making changes to MSI generation, validate:

1. **`Wix4DependencyProvider` table in MSI** — The MSI must contain this table (renamed from `WixDependencyProvider`)
2. **`ProviderKeyName` in `data/msi.json`** — Inside the MSI NuGet package, format: `PackageName,Version,Arch`
3. **SWIX vsman JSON** — Must have correct `providerKey`, `productCode`, and `installSizes`
4. **VSDrop zips** — All 3 zips must be produced with correct content
5. **`RelatedProducts`** — WiX 6 adds `WIX_UPGRADE_DETECTED` / `WIX_DOWNGRADE_DETECTED` entries (feature gain)

### How to Validate Locally

Download artifacts from the internal CI build and inspect:

```bash
# Check msi.json in an MSI nupkg
unzip -p Microsoft.Maui.Core.Msi.x64.*.nupkg data/msi.json | python -m json.tool

# Check VSDrop zip contents
unzip -l Workload.VSDrop.*.components.zip
```

## Package Version Management

WiX package versions are pinned in `eng/NuGetVersions.targets` using the `MicrosoftWixVersion` property defined in `eng/Versions.props`:

```xml
<!-- eng/Versions.props -->
<MicrosoftWixVersion>6.0.3-dotnet.4</MicrosoftWixVersion>

<!-- eng/NuGetVersions.targets -->
<PackageVersion Include="Microsoft.Wix" Version="$(MicrosoftWixVersion)" />
<PackageVersion Include="Microsoft.WixToolset.Heat" Version="$(MicrosoftWixVersion)" />
<PackageVersion Include="Microsoft.WixToolset.UI.wixext" Version="$(MicrosoftWixVersion)" />
<PackageVersion Include="Microsoft.WixToolset.Dependency.wixext" Version="$(MicrosoftWixVersion)" />
<PackageVersion Include="Microsoft.WixToolset.Util.wixext" Version="$(MicrosoftWixVersion)" />
```

## Testing Pipeline

The workload MSI generation runs in the internal signing pipeline:
- **Pipeline**: `dotnet-maui` (ID: 1095)
- **Org**: `https://dev.azure.com/dnceng`
- **Project**: `internal`
- **Trigger**: Push to any branch in the internal mirror (`dnceng/internal/_git/dotnet-maui`)

The pipeline only runs on internal builds because MSI signing requires MicroBuild infrastructure.

## Related PRs and References

- [dotnet/maui#36297](https://github.com/dotnet/maui/pull/36297) — MAUI WiX 6 migration
- [dotnet/android#11743](https://github.com/dotnet/android/pull/11743) — Android WiX 6 migration (reference)
- [dotnet/dotnet#5265](https://github.com/dotnet/dotnet/pull/5265) — Arcade-level WiX 5/6 rewrite (deeper refactor)
- Xamarin.yaml-templates `convert-v5/convert.proj` — Reference implementation pattern

## Future Considerations

1. **SwixBuild package**: We currently use `MicroBuild.Plugins.SwixBuild.Dotnet` (the .NET-compatible variant). The SDK repo has moved to `Microsoft.VisualStudioEng.MicroBuild.Plugins.SwixBuild` 1.1.922 with desktop MSBuild. If the `.Dotnet` variant is deprecated, MAUI will need to follow the same pattern.

2. **Arcade task evolution**: [dotnet/dotnet#5265](https://github.com/dotnet/dotnet/pull/5265) introduces `CreateWixPacks` (generating `.wixpack` archives for signing) and removes `CreateLightCommandPackageDrop`. When this arcade version ships, MAUI will consume these changes automatically through dependency flow.

3. **`UseWorkloadPackGroupsForVS`**: This property was removed in arcade 11.x. Do not re-add it.

4. **`IceSuppressions`**: WiX 6 doesn't run ICE validation by default (no `LGHT1105` NoWarn needed).
