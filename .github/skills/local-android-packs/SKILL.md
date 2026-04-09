---
name: local-android-packs
description: "Override provisioned Android workload packs with locally built versions from dotnet/android"
metadata:
  author: dotnet-maui
  version: "1.0.0"
compatibility: "macOS, Linux, Windows — requires local dotnet/android build"
---

# Local Android Packs Overlay

Override the provisioned Android workload packs in `.dotnet/` with locally built packs from a [dotnet/android](https://github.com/dotnet/android) checkout. This lets you test android changes against MAUI without waiting for dependency flow through Maestro.

## When to Use

- **Testing local dotnet/android changes** — you've made a fix or feature in dotnet/android and want to verify it works with MAUI before submitting a PR
- **Debugging Android workload issues** — you need to add diagnostics or logging to the Android SDK and test against MAUI
- **Bisecting regressions in dotnet/android** — testing different dotnet/android commits against the same MAUI code

## Prerequisites

1. **Built dotnet/android repo** — clone and build locally:
   ```bash
   git clone https://github.com/dotnet/android ~/repos/android
   cd ~/repos/android
   dotnet build Xamarin.Android.sln -c Release
   ```
   The build output packs will be at `bin/Release/lib/packs/`.

2. **Provisioned MAUI SDK** — the `.dotnet/` directory must exist. From the MAUI repo root:
   ```bash
   ./build.sh --restore
   ```

3. **PowerShell 7+** — installed on your platform (`pwsh`). The provisioned `.dotnet/` may include it, or install via your package manager.

## Quick Start

### Overlay Release packs

```powershell
.github/skills/local-android-packs/scripts/Overlay-LocalAndroidPacks.ps1 `
    -AndroidSrcPath ~/repos/android
```

### Overlay Debug packs

```powershell
.github/skills/local-android-packs/scripts/Overlay-LocalAndroidPacks.ps1 `
    -AndroidSrcPath ~/repos/android -Config Debug
```

### Restore original packs

```powershell
.github/skills/local-android-packs/scripts/Overlay-LocalAndroidPacks.ps1 `
    -AndroidSrcPath ~/repos/android -Restore
```

## How It Works

The script uses a **manifest patching + pack placement** approach:

1. **Discovers SDK manifest path** — searches `.dotnet/sdk-manifests/*/microsoft.net.sdk.android/` for `WorkloadManifest.json`, picking the highest SDK band
2. **Reads `WorkloadManifest.json`** to get the installed pack versions and the full list of net11 packs
3. **Detects local build version** from the version subdirectory name under the local packs directory (e.g., `36.1.99-ci.main.157`)
4. **Backs up the manifest** and records original version, local version, timestamp, and list of overlaid packs in a metadata file
5. **Patches `WorkloadManifest.json`** — updates version fields for all net11 packs to the local build version. The net10 SDK entry and Templates pack are left untouched.
6. **Places packs** — copies local pack directories into `.dotnet/packs/<PackName>/<local-version>/`. The SDK pack is mapped to the platform-specific alias (e.g., `Microsoft.Android.Sdk.Darwin` on macOS).
7. **Verifies** key files exist in the overlaid packs

Everything is reversible — run with `-Restore` to undo all changes.

## What Gets Modified

| Location | Change |
|----------|--------|
| `.dotnet/sdk-manifests/<band>/microsoft.net.sdk.android/WorkloadManifest.json` | Version fields patched to local build version |
| `.dotnet/packs/<PackName>/<local-version>/` | Local pack directories placed alongside existing versions |
| `.android-packs-backup/` (repo root) | Backup of original manifest + metadata (gitignored via `.dotnet/` parent) |

## Pack Coverage

The following packs are overlaid (all net11 packs from the manifest):

| Pack ID (manifest entry) | Resolved Name | Kind |
|--------------------------|---------------|------|
| `Microsoft.Android.Sdk.net11` | `Microsoft.Android.Sdk.Darwin` / `.Linux` / `.Windows` (platform alias) | sdk |
| `Microsoft.Android.Ref.36` | `Microsoft.Android.Ref.36` | framework |
| `Microsoft.Android.Runtime.36.android` | `Microsoft.Android.Runtime.36.android` | framework |
| `Microsoft.Android.Runtime.Mono.36.android-arm` | `Microsoft.Android.Runtime.Mono.36.android-arm` | framework |
| `Microsoft.Android.Runtime.Mono.36.android-arm64` | `Microsoft.Android.Runtime.Mono.36.android-arm64` | framework |
| `Microsoft.Android.Runtime.Mono.36.android-x86` | `Microsoft.Android.Runtime.Mono.36.android-x86` | framework |
| `Microsoft.Android.Runtime.Mono.36.android-x64` | `Microsoft.Android.Runtime.Mono.36.android-x64` | framework |
| `Microsoft.Android.Runtime.CoreCLR.36.android-arm64` | `Microsoft.Android.Runtime.CoreCLR.36.android-arm64` | framework |
| `Microsoft.Android.Runtime.CoreCLR.36.android-x64` | `Microsoft.Android.Runtime.CoreCLR.36.android-x64` | framework |
| `Microsoft.Android.Runtime.NativeAOT.36.android-arm64` | `Microsoft.Android.Runtime.NativeAOT.36.android-arm64` | framework |
| `Microsoft.Android.Runtime.NativeAOT.36.android-x64` | `Microsoft.Android.Runtime.NativeAOT.36.android-x64` | framework |

> **Skipped:** `Microsoft.Android.Sdk.net10` (different version for net10 compat) and `Microsoft.Android.Templates` (template pack, not needed for builds).

## Restore / Undo

Run with `-Restore` to revert all changes:

```powershell
.github/skills/local-android-packs/scripts/Overlay-LocalAndroidPacks.ps1 `
    -AndroidSrcPath ~/repos/android -Restore
```

This will:
1. Read the backup metadata to find all overlaid pack names and versions
2. Remove the local version directories from `.dotnet/packs/`
3. Restore the original `WorkloadManifest.json` from backup
4. Delete the backup directory

If an overlay fails partway through, the script automatically attempts to roll back from the backup.

## Troubleshooting

### "Local packs directory not found"

The path `<AndroidSrcPath>/bin/<Config>/lib/packs` doesn't exist. Verify that:
- Your `-AndroidSrcPath` points to the dotnet/android repo root
- You built with the correct configuration (`-Config Release` vs `-Config Debug`)
- The build completed successfully (`dotnet build Xamarin.Android.sln -c Release`)

### "No manifest found"

No `WorkloadManifest.json` was found under `.dotnet/sdk-manifests/*/microsoft.net.sdk.android/`. Run `./build.sh --restore` from the MAUI repo root to provision the SDK.

### "Version mismatch" / unexpected behavior

If the local dotnet/android build version doesn't match what MAUI expects, builds may fail with missing API errors. Check:
- The dotnet/android branch matches the MAUI branch expectations
- The local build is fresh (clean + rebuild if needed)

### Pack not found in local build

Some packs may not be present in the local build output (e.g., NativeAOT runtime packs if not built). These are skipped with a warning — the installed version remains for those packs.

### Build errors after overlay

If MAUI builds fail after overlaying, the local android version may be incompatible. Restore with `-Restore` to go back to the known-good state.

## Notes

- **Backups are gitignored** — the `.android-packs-backup/` directory lives under the repo root, and `.dotnet/` (where the packs live) is already in `.gitignore`
- **Templates pack is skipped** — `Microsoft.Android.Templates` is not overlaid since it's only used for `dotnet new` project creation, not builds
- **net10 SDK is untouched** — `Microsoft.Android.Sdk.net10` has a separate version for backward compatibility and is not modified
- **Platform-specific SDK alias** — the script automatically detects whether you're on macOS (`Sdk.Darwin`), Linux (`Sdk.Linux`), or Windows (`Sdk.Windows`) and uses the correct pack name
