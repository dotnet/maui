# .NET MAUI Workload Discovery Process

This document describes the logical steps to discover .NET workload information, including SDK versions, workload set versions, individual workload versions, and their dependencies (Android SDK, JDK, Xcode requirements, etc.).

## Overview

The .NET workload system uses NuGet packages to distribute workload manifests and dependencies. The discovery process involves:

1. Finding the latest workload set for a .NET version
2. Downloading the workload set manifest to get individual workload versions
3. Downloading each workload's manifest and dependencies
4. Extracting dependency information (Android SDK, JDK, Xcode, etc.)

## Prerequisites

- Access to NuGet API (https://api.nuget.org)
- Ability to download and extract NuGet packages (.nupkg files are ZIP archives)
- JSON parsing capability

---

## Step 1: Determine the .NET SDK Band

The SDK band is derived from the .NET version and determines which workload packages to search for.

### Input
- .NET Version (e.g., `9.0`, `10.0`)

### Logic
```
SDK Band = {major}.0.100
Example: 9.0 → 9.0.100
Example: 10.0 → 10.0.100
```

For newer SDK versions, the band may be different (e.g., `9.0.200`, `9.0.300`). The workload set package ID includes this band.

---

## Step 2: Find the Latest Workload Set Version

Workload sets are published as NuGet packages with IDs following the pattern:
```
Microsoft.NET.Workloads.{major}.{band}
```

### NuGet Search API

**Endpoint:**
```
https://azuresearch-usnc.nuget.org/query?q={package_pattern}&prerelease={true|false}&semVerLevel=2.0.0
```

**Example Request:**
```
GET https://azuresearch-usnc.nuget.org/query?q=Microsoft.NET.Workloads.10.0&prerelease=false&semVerLevel=2.0.0
```

**Response Structure:**
```json
{
  "data": [
    {
      "id": "Microsoft.NET.Workloads.10.0.100",
      "version": "10.102.0",
      "versions": [
        { "version": "10.100.0" },
        { "version": "10.101.0" },
        { "version": "10.102.0" }
      ]
    }
  ]
}
```

### Filtering Logic

1. Search for packages matching `Microsoft.NET.Workloads.{major}.0`
2. Filter results to only include packages matching the pattern:
   - `Microsoft.NET.Workloads.{major}.{band}` (e.g., `Microsoft.NET.Workloads.10.0.100`)
   - Exclude architecture-specific packages (`.Msi.x64`, `.Msi.arm64`, etc.)
3. If no stable versions found, retry with `prerelease=true`
4. Select the package with the highest version number

### Output
- **Package ID:** `Microsoft.NET.Workloads.10.0.100`
- **NuGet Version:** `10.102.0`

---

## Step 3: Convert NuGet Version to CLI Workload Version

The NuGet package version format differs from the `dotnet workload` CLI format.

### Conversion Logic
```
NuGet Format:    {major}.{patch}.{revision}     → 10.102.0
CLI Format:      {major}.0.{patch}              → 10.0.102

Algorithm:
1. Split version by '.'
2. Extract: major={parts[0]}, patch={parts[1]}, revision={parts[2]}
3. Result: "{major}.0.{patch}"
```

### Examples
| NuGet Version | CLI Version |
|---------------|-------------|
| `10.102.0` | `10.0.102` |
| `9.309.0` | `9.0.309` |
| `9.305.0` | `9.0.305` |

---

## Step 4: Download the Workload Set Manifest

The workload set package contains a JSON file listing all workloads and their versions.

### NuGet Package Download

**Package Content API:**
```
https://api.nuget.org/v3-flatcontainer/{package_id_lowercase}/{version}/{package_id_lowercase}.{version}.nupkg
```

**Example:**
```
https://api.nuget.org/v3-flatcontainer/microsoft.net.workloads.10.0.100/10.102.0/microsoft.net.workloads.10.0.100.10.102.0.nupkg
```

### Extract Workload Set JSON

**File Path in Package:**
```
data/microsoft.net.workloads.workloadset.json
```

**File Contents:**
```json
{
  "microsoft.net.sdk.android": "35.0.50/9.0.100",
  "microsoft.net.sdk.ios": "26.2.10191/10.0.100",
  "microsoft.net.sdk.maccatalyst": "26.2.10191/10.0.100",
  "microsoft.net.sdk.macos": "26.2.10191/10.0.100",
  "microsoft.net.sdk.tvos": "26.2.10191/10.0.100",
  "microsoft.net.sdk.maui": "10.0.10/10.0.100"
}
```

### Parsing the Workload Set

Each entry has the format:
```
"{workload_id}": "{version}/{sdk_band}"
```

**Parsing Logic:**
```
Entry: "microsoft.net.sdk.ios": "26.2.10191/10.0.100"

1. Key = workload manifest ID (lowercase)
2. Value = "{version}/{sdk_band}"
3. Split value by '/'
   - version = "26.2.10191"
   - sdk_band = "10.0.100"
```

### Output
| Workload ID | Version | SDK Band |
|-------------|---------|----------|
| `microsoft.net.sdk.android` | `35.0.50` | `9.0.100` |
| `microsoft.net.sdk.ios` | `26.2.10191` | `10.0.100` |
| `microsoft.net.sdk.maccatalyst` | `26.2.10191` | `10.0.100` |
| `microsoft.net.sdk.macos` | `26.2.10191` | `10.0.100` |
| `microsoft.net.sdk.tvos` | `26.2.10191` | `10.0.100` |
| `microsoft.net.sdk.maui` | `10.0.10` | `10.0.100` |

---

## Step 5: Download Individual Workload Manifests

Each workload has its own NuGet package containing manifest and dependency information.

### Workload Manifest Package ID

**Pattern:**
```
{WorkloadId}.Manifest-{sdk_band}
```

**Examples:**
- `Microsoft.NET.Sdk.iOS.Manifest-10.0.100`
- `Microsoft.NET.Sdk.Android.Manifest-9.0.100`
- `Microsoft.NET.Sdk.Maui.Manifest-10.0.100`

### Download URL

```
https://api.nuget.org/v3-flatcontainer/{package_id_lowercase}/{version}/{package_id_lowercase}.{version}.nupkg
```

**Example:**
```
https://api.nuget.org/v3-flatcontainer/microsoft.net.sdk.ios.manifest-10.0.100/26.2.10191/microsoft.net.sdk.ios.manifest-10.0.100.26.2.10191.nupkg
```

### Files in Workload Manifest Package

| File Path | Description |
|-----------|-------------|
| `data/WorkloadManifest.json` | Workload definition and packs |
| `data/WorkloadDependencies.json` | External dependencies (SDK, JDK, Xcode) |

---

## Step 6: Extract Workload Dependencies

The `WorkloadDependencies.json` file contains information about external tool requirements.

### Android Workload Dependencies

**File:** `data/WorkloadDependencies.json` from `Microsoft.NET.Sdk.Android.Manifest-{band}`

**Structure:**
```json
{
  "microsoft.net.sdk.android": {
    "jdk": {
      "version": "[17.0,22.0)",
      "recommendedVersion": "17.0.14"
    },
    "androidsdk": {
      "packages": [
        "build-tools;35.0.0",
        "platform-tools",
        "platforms;android-35",
        "cmdline-tools;13.0"
      ],
      "apiLevel": "35",
      "buildToolsVersion": "35.0.0",
      "cmdLineToolsVersion": "13.0"
    }
  }
}
```

**Extracted Information:**
| Field | Value | Description |
|-------|-------|-------------|
| JDK Version Range | `[17.0,22.0)` | Compatible JDK versions |
| JDK Recommended | `17.0.14` | Preferred JDK version |
| Android API Level | `35` | Target Android API |
| Build Tools Version | `35.0.0` | Android build tools version |
| SDK Packages | (array) | Required Android SDK packages |

### iOS/tvOS/macOS Workload Dependencies

**File:** `data/WorkloadDependencies.json` from `Microsoft.NET.Sdk.iOS.Manifest-{band}`

**Structure:**
```json
{
  "microsoft.net.sdk.ios": {
    "workload": {
      "alias": ["ios"],
      "version": "26.2.10191"
    },
    "xcode": {
      "version": "[26.2,)",
      "recommendedVersion": "26.2"
    },
    "sdk": {
      "version": "26.2"
    }
  }
}
```

**Extracted Information:**
| Field | Value | Description |
|-------|-------|-------------|
| Xcode Version Range | `[26.2,)` | Compatible Xcode/SDK versions |
| Xcode Recommended | `26.2` | Preferred Xcode/SDK version |
| iOS SDK Version | `26.2` | iOS SDK version |

### Version Range Notation

The version ranges use NuGet/Maven interval notation:

| Notation | Meaning |
|----------|---------|
| `[16.0,17.0)` | >= 16.0 AND < 17.0 |
| `[26.2,)` | >= 26.2 (no upper bound) |
| `[17.0,22.0)` | >= 17.0 AND < 22.0 |
| `(1.0,2.0]` | > 1.0 AND <= 2.0 |

**Bracket meanings:**
- `[` = inclusive minimum
- `(` = exclusive minimum
- `]` = inclusive maximum
- `)` = exclusive maximum

---

## Complete Example: .NET 10.0 Workload Discovery

### Step-by-Step Walkthrough

**1. Start with .NET version:** `10.0`

**2. Search for workload sets:**
```bash
curl "https://azuresearch-usnc.nuget.org/query?q=Microsoft.NET.Workloads.10.0&prerelease=false"
```
Result: `Microsoft.NET.Workloads.10.0.100` version `10.102.0`

**3. Convert to CLI version:**
```
10.102.0 → 10.0.102
```

**4. Download workload set package:**
```bash
curl -o workloadset.nupkg \
  "https://api.nuget.org/v3-flatcontainer/microsoft.net.workloads.10.0.100/10.102.0/microsoft.net.workloads.10.0.100.10.102.0.nupkg"
unzip workloadset.nupkg -d workloadset/
cat workloadset/data/microsoft.net.workloads.workloadset.json
```

**5. Parse iOS workload entry:**
```json
"microsoft.net.sdk.ios": "26.2.10191/10.0.100"
```
- Version: `26.2.10191`
- SDK Band: `10.0.100`

**6. Download iOS manifest package:**
```bash
curl -o ios-manifest.nupkg \
  "https://api.nuget.org/v3-flatcontainer/microsoft.net.sdk.ios.manifest-10.0.100/26.2.10191/microsoft.net.sdk.ios.manifest-10.0.100.26.2.10191.nupkg"
unzip ios-manifest.nupkg -d ios-manifest/
cat ios-manifest/data/WorkloadDependencies.json
```

**7. Extract dependencies:**
```json
{
  "xcode": {
    "version": "[26.2,)",
    "recommendedVersion": "26.2"
  },
  "sdk": {
    "version": "26.2"
  }
}
```

### Final Output

```yaml
.NET Version: 10.0

Workload Set:
  Package: Microsoft.NET.Workloads.10.0.100
  NuGet Version: 10.102.0
  CLI Version: 10.0.102

iOS Workload:
  Manifest: Microsoft.NET.Sdk.iOS.Manifest-10.0.100
  Version: 26.2.10191
  Xcode Range: "[26.2,)"
  Xcode Recommended: 26.2
  iOS SDK: 26.2

Android Workload:
  Manifest: Microsoft.NET.Sdk.Android.Manifest-9.0.100
  Version: 35.0.50
  JDK Range: "[17.0,22.0)"
  JDK Recommended: 17.0.14
  API Level: 35
  Build Tools: 35.0.0
```

---

## API Reference Summary

### NuGet APIs

| Operation | Endpoint |
|-----------|----------|
| Search packages | `https://azuresearch-usnc.nuget.org/query?q={query}&prerelease={bool}` |
| Download package | `https://api.nuget.org/v3-flatcontainer/{id}/{version}/{id}.{version}.nupkg` |
| Get versions | `https://api.nuget.org/v3-flatcontainer/{id}/index.json` |

### Key Package Files

| Package Type | File Path | Contents |
|--------------|-----------|----------|
| Workload Set | `data/microsoft.net.workloads.workloadset.json` | Workload → version mappings |
| Workload Manifest | `data/WorkloadManifest.json` | Workload definition and packs |
| Workload Manifest | `data/WorkloadDependencies.json` | External dependencies |

---

## CLI Tool Specification

A command-line tool implementing this process should support:

### Commands

```bash
# Get latest workload set version
workload-info get-workload-set --dotnet-version 10.0

# Get all workload versions for a .NET version
workload-info list-workloads --dotnet-version 10.0

# Get dependencies for a specific workload
workload-info get-dependencies --dotnet-version 10.0 --workload ios

# Get Xcode requirements
workload-info get-xcode-requirements --dotnet-version 10.0

# Get Android SDK requirements
workload-info get-android-requirements --dotnet-version 10.0
```

### Output Formats

- `--format json` - Machine-readable JSON
- `--format yaml` - YAML format
- `--format table` - Human-readable table (default)

### Example Output

```bash
$ workload-info get-dependencies --dotnet-version 10.0 --workload ios --format json
{
  "workloadId": "microsoft.net.sdk.ios",
  "manifestVersion": "26.2.10191",
  "sdkBand": "10.0.100",
  "dependencies": {
    "xcode": {
      "versionRange": "[26.2,)",
      "recommendedVersion": "26.2"
    },
    "sdk": {
      "version": "26.2"
    }
  }
}
```

```bash
$ workload-info get-android-requirements --dotnet-version 10.0 --format json
{
  "workloadId": "microsoft.net.sdk.android",
  "manifestVersion": "35.0.50",
  "sdkBand": "9.0.100",
  "dependencies": {
    "jdk": {
      "versionRange": "[17.0,22.0)",
      "recommendedVersion": "17.0.14"
    },
    "androidSdk": {
      "apiLevel": "35",
      "buildToolsVersion": "35.0.0",
      "cmdLineToolsVersion": "13.0",
      "packages": [
        "build-tools;35.0.0",
        "platform-tools",
        "platforms;android-35",
        "cmdline-tools;13.0"
      ]
    }
  }
}
```

---

## Related Files in This Repository

| File | Purpose |
|------|---------|
| `common-functions.ps1` | PowerShell implementation of discovery logic |
| `check-workload-updates.ps1` | CI script that uses discovery to check for updates |
