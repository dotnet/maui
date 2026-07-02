---
description: "Guidelines for working with workload MSI generation (WiX 6) in src/Workload/"
globs:
  - "src/Workload/**"
  - "eng/NuGetVersions.targets"
  - "eng/Versions.props"
---

# Workload MSI Generation (WiX 6)

## Architecture Overview

`src/Workload/workloads.csproj` generates MSIs for Visual Studio workload insertion using the arcade `CreateVisualStudioWorkload` task. As of .NET 11, this uses **WiX 6** (wix.exe) instead of WiX 3 (candle/light).

Full documentation: [docs/design/WorkloadMsi.md](/docs/design/WorkloadMsi.md)

## Critical Rules

### DO NOT

- **Do NOT change TargetFramework** from `netstandard2.0` — this avoids NU1212 errors with `Microsoft.Wix` (DotnetTool package type)
- **Do NOT remove `ExcludeAssets="all"`** from `Microsoft.Wix` PackageReference — required for DotnetTool compatibility
- **Do NOT remove `GenerateDependencyFile=false`** — needed for netstandard2.0 project that doesn't produce binaries
- **Do NOT add `UseWorkloadPackGroupsForVS`** — removed in arcade 11.x, will cause build errors
- **Do NOT use `WixToolsetPath`** — that's the WiX 3 parameter; use `WixExe`/`HeatExe`/`WixExtensions` instead
- **Do NOT reference `Microsoft.Signed.WiX`** — that's the old WiX 3 package

### MUST

- **Keep WiX package versions in sync** — All 5 WiX packages must use `$(MicrosoftWixVersion)` from `eng/Versions.props`
- **Use `GeneratePathProperty="true"`** on all WiX PackageReferences — needed for `$(Pkg*)` path resolution
- **Extension path pattern**: `$(Pkg<PackageId_dots_to_underscores>)\wixext6\WixToolset.<Name>.wixext.dll`
- **Test on internal pipeline** (ID 1095 at dnceng/internal) — MSI signing only works in internal builds

## Key Files

| File | Purpose |
|------|---------|
| `src/Workload/workloads.csproj` | Main MSI generation project — WiX 6 config + task invocation |
| `eng/Versions.props` | `MicrosoftWixVersion` property + arcade task versions |
| `eng/NuGetVersions.targets` | Central package version pins for WiX packages |
| `eng/pipelines/common/sdk-insertion.yml` | Pipeline referencing workload build |

## WiX 6 Package Dependencies

```xml
<!-- Required packages (all use MicrosoftWixVersion) -->
Microsoft.Wix                           <!-- wix.exe CLI tool -->
Microsoft.WixToolset.Heat               <!-- heat.exe harvesting tool -->
Microsoft.WixToolset.UI.wixext          <!-- UI extension -->
Microsoft.WixToolset.Dependency.wixext  <!-- Dependency provider extension -->
Microsoft.WixToolset.Util.wixext        <!-- Utility extension -->
```

## Arcade Task Parameters (WiX 6)

```xml
<CreateVisualStudioWorkload
    WixExe="$(WixExe)"              <!-- Path to wix.exe from Microsoft.Wix package -->
    HeatExe="$(HeatExe)"            <!-- Path to heat.exe from Microsoft.WixToolset.Heat -->
    WixExtensions="@(WixExtensions)" <!-- Item group of .wixext.dll paths -->
    ... />
```

The task requires arcade version **11.0.0-beta.26330.112** or later for WiX 6 support.

## SWIX Build

SWIX projects are built using `MicroBuild.Plugins.SwixBuild.Dotnet`. If this variant is deprecated in the future, the fallback is to use desktop MSBuild via `Exec` (as done in dotnet/dotnet#5265).

## Validation

After any MSI-related changes, verify on internal CI:

1. `Wix4DependencyProvider` table exists in generated MSIs
2. `ProviderKeyName` is populated in `data/msi.json` (format: `PackageName,Version,Arch`)
3. All 3 VSDrop zips are produced (components, components-pre, packs)
4. SWIX vsman JSON has correct providerKey and installSizes

## Troubleshooting

### NU1212: Microsoft.Wix package type incompatibility
**Cause**: Project TFM is not `netstandard2.0`
**Fix**: Ensure `<TargetFramework>netstandard2.0</TargetFramework>` and `ExcludeAssets="all"` on Microsoft.Wix

### "WixExe" or "HeatExe" parameter not recognized
**Cause**: Arcade version too old
**Fix**: Update `Microsoft.DotNet.Build.Tasks.Workloads` to 11.0.0-beta.26330.112+

### SWIX build failures / CodeTaskFactory errors
**Cause**: SwixBuild.Dotnet variant incompatible with current .NET
**Fix**: May need to switch to desktop MSBuild pattern (see dotnet/dotnet#5265)

### Missing WiX extension at expected path
**Cause**: `GeneratePathProperty="true"` missing from PackageReference
**Fix**: Add `GeneratePathProperty="true"` to the PackageReference; path follows pattern `$(Pkg<id_with_underscores>)\wixext6\...`
