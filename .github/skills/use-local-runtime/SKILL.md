---
name: use-local-runtime
description: "Build and test the MAUI repo against a locally-built .NET runtime using dev shipping packages."
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires a local dotnet/runtime build. macOS, Linux, or Windows.
---

# Use Local Runtime Skill

Build and test .NET MAUI against a locally-built .NET runtime from [dotnet/runtime](https://github.com/dotnet/runtime). Useful when investigating runtime bugs that affect MAUI, verifying runtime fixes, or co-developing across both repos.

This uses the runtime's dev shipping packages — the same NuGet packages that flow through darc — so it exercises the full package resolution pipeline.

## Step 1: Build the runtime shipping packages

```bash
cd /path/to/runtime
./build.sh -s clr+libs+packs+host     # macOS/Linux
# build.cmd -s clr+libs+packs+host    # Windows
```

⏱️ First build takes 30-40 minutes. Packages land in `artifacts/packages/<Configuration>/Shipping/`.

## Step 2: Find the package version

```bash
ls artifacts/packages/Debug/Shipping/Microsoft.NETCore.App.Runtime.*.nupkg
# Example: Microsoft.NETCore.App.Runtime.osx-arm64.11.0.0-dev.nupkg
```

The version string is typically `11.0.0-dev`. Note it for the next step.

## Step 3: Update the MAUI repo to use your local packages

1. **Update `eng/Versions.props`** — change `MicrosoftNETCoreAppRefPackageVersion` and pin the derived versions:

First, note the current value of `MicrosoftNETCoreAppRefPackageVersion` (e.g. `11.0.0-preview.2.26103.111`). Then change it and pin the 4 derived versions to the original value:

```xml
<MicrosoftNETCoreAppRefPackageVersion>11.0.0-dev</MicrosoftNETCoreAppRefPackageVersion>

<!-- IMPORTANT: These 4 derive from MicrosoftNETCoreAppRefPackageVersion via $(…) but
     the runtime build does NOT produce their NuGet packages. Pin them to the original
     value you noted above to avoid NU1603 restore errors. -->
<SystemTextJsonPackageVersion>ORIGINAL_VERSION</SystemTextJsonPackageVersion>
<SystemTextEncodingsWebPackageVersion>ORIGINAL_VERSION</SystemTextEncodingsWebPackageVersion>
<MicrosoftBclAsyncInterfacesPackageVersion>ORIGINAL_VERSION</MicrosoftBclAsyncInterfacesPackageVersion>
<SystemCodeDomPackageVersion>ORIGINAL_VERSION</SystemCodeDomPackageVersion>
```

2. **Add your local packages folder as a NuGet source** and **set a local NuGet cache** in `NuGet.config` (repo root):

```xml
<configuration>
  <config>
    <!-- Use a local cache so we can delete just this folder between iterations -->
    <add key="globalPackagesFolder" value="./local-nuget-cache" />
  </config>
  <packageSources>
    <clear />
    <!-- Add this line pointing at your runtime build output -->
    <add key="local-runtime" value="/path/to/runtime/artifacts/packages/Debug/Shipping" />
    <!-- Keep existing sources below -->
    ...
  </packageSources>
</configuration>
```

Using a local `globalPackagesFolder` avoids polluting (or needing to nuke) your global NuGet cache. Between iterations, just delete this folder:

```bash
rm -rf ./local-nuget-cache
```

## Step 4: Build MAUI

```bash
./build.sh -restore -build
```

## Iterating after runtime changes

1. Rebuild the runtime: `./build.sh -s clr+libs+packs+host`
2. Delete the local NuGet cache: `rm -rf ./local-nuget-cache`
3. Rebuild MAUI: `./build.sh -restore -build`

## Troubleshooting

| Problem | Solution |
|---------|----------|
| NuGet doesn't pick up updated packages | Delete the local cache: `rm -rf ./local-nuget-cache` |
| Version mismatch errors | Ensure `MicrosoftNETCoreAppRefPackageVersion` matches the exact version from your nupkg filenames |
| MAUI build fails with missing packages | Your local runtime build may not include all packages MAUI needs — keep the `dotnet11` feed as a fallback source in `NuGet.config` |

## Notes

- The MAUI repo tracks its runtime dependency in `eng/Versions.props` → `MicrosoftNETCoreAppRefPackageVersion`. Four other package versions (`SystemTextJsonPackageVersion`, `SystemTextEncodingsWebPackageVersion`, `MicrosoftBclAsyncInterfacesPackageVersion`, `SystemCodeDomPackageVersion`) derive from this via `$(MicrosoftNETCoreAppRefPackageVersion)`. Since the runtime build doesn't produce those library NuGet packages, they must be pinned to the original version.
- The dependency normally flows via [darc](https://github.com/dotnet/arcade/blob/main/Documentation/Darc.md) from dotnet/dotnet (the VMR) → dotnet/maui. The manual approach above is simpler for one-off testing.
- When testing Android or iOS targets, runtime binaries differ per RID. Make sure the runtime was built for the right architecture (e.g., `android-arm64` for a physical Android device, `osx-arm64` for Mac Catalyst on Apple Silicon).
