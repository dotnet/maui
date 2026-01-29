# Workload Dependencies Discovery

This reference describes how to discover authoritative version requirements from NuGet APIs. All JDK, Android SDK, and Xcode requirements come from WorkloadDependencies.json - never hardcode versions.

## Workload Aliases

| Alias | Full ID |
|-------|---------|
| ios | microsoft.net.sdk.ios |
| android | microsoft.net.sdk.android |
| maccatalyst | microsoft.net.sdk.maccatalyst |
| macos | microsoft.net.sdk.macos |
| tvos | microsoft.net.sdk.tvos |
| maui | microsoft.net.sdk.maui |

---

## Discovery Process

### Step 1: Get Latest SDK Version

```bash
curl -s "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json" | \
  jq '.["releases-index"][] | select(.["channel-version"]=="{MAJOR}.0")'
```

Response fields:
| Field | Description |
|-------|-------------|
| `channel-version` | Major.minor (e.g., "10.0") |
| `latest-sdk` | Current stable SDK version |
| `support-phase` | "active", "maintenance", "eol" |

Extract `latest-sdk` and derive SDK band:
- `10.0.102` → band `10.0.100` (hundreds digit)
- `10.0.205` → band `10.0.200`

### Step 2: Find Workload Set Package

```bash
curl -s "https://azuresearch-usnc.nuget.org/query?q=Microsoft.NET.Workloads.{MAJOR}.0&prerelease=false&semVerLevel=2.0.0"
```

Filter results:
- Match `Microsoft.NET.Workloads.{major}.{band}` (e.g., `Microsoft.NET.Workloads.10.0.100`)
- Exclude `.Msi.*` packages
- Pick highest version

**Version conversion**: NuGet `10.102.0` → CLI `10.0.102`
```
parts = nugetVersion.split('.')
cliVersion = parts[0] + ".0." + parts[1]
```

### Step 3: Download Workload Set Manifest

```bash
curl -o workloadset.nupkg "https://api.nuget.org/v3-flatcontainer/microsoft.net.workloads.{band}/{version}/microsoft.net.workloads.{band}.{version}.nupkg"
unzip -p workloadset.nupkg data/microsoft.net.workloads.workloadset.json
```

Contents format: `"{workload_id}": "{manifestVersion}/{sdkBand}"`

Example:
```json
{
  "microsoft.net.sdk.android": "35.0.50/9.0.100",
  "microsoft.net.sdk.ios": "26.2.10191/10.0.100",
  "microsoft.net.sdk.maui": "10.0.10/10.0.100"
}
```

### Step 4: Download Workload Manifest

Build package id: `{WorkloadId}.Manifest-{sdkBand}`

Examples:
- `Microsoft.NET.Sdk.iOS.Manifest-10.0.100`
- `Microsoft.NET.Sdk.Android.Manifest-9.0.100`

```bash
curl -o manifest.nupkg "https://api.nuget.org/v3-flatcontainer/{packageid}/{version}/{packageid}.{version}.nupkg"
unzip -p manifest.nupkg data/WorkloadDependencies.json
```

### Step 5: Parse WorkloadDependencies.json

**Android workload** (`microsoft.net.sdk.android`):
```json
{
  "microsoft.net.sdk.android": {
    "jdk": {
      "version": "[17.0,22.0)",
      "recommendedVersion": "17.0.14"
    },
    "androidsdk": {
      "packages": ["build-tools;35.0.0", "platform-tools", "platforms;android-35", "cmdline-tools;13.0"],
      "apiLevel": "35",
      "buildToolsVersion": "35.0.0",
      "cmdLineToolsVersion": "13.0"
    }
  }
}
```

**iOS workload** (`microsoft.net.sdk.ios`):
```json
{
  "microsoft.net.sdk.ios": {
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

### Version Range Notation

| Notation | Meaning |
|----------|---------|
| `[17.0,22.0)` | >= 17.0 AND < 22.0 |
| `[26.2,)` | >= 26.2 (no upper bound) |

Brackets: `[` = inclusive, `(` = exclusive

---

## Complete Example

**Goal**: Find requirements for .NET 10

```bash
# Step 1: Get SDK info
curl -s "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json" | \
  jq '.["releases-index"][] | select(.["channel-version"]=="10.0") | .["latest-sdk"]'
# Result: "10.0.102" → band "10.0.100"

# Step 2: Find workload set
curl -s "https://azuresearch-usnc.nuget.org/query?q=Microsoft.NET.Workloads.10.0&prerelease=false" | \
  jq '.data[] | select(.id=="Microsoft.NET.Workloads.10.0.100") | {id, version}'
# Result: { "id": "Microsoft.NET.Workloads.10.0.100", "version": "10.102.0" }
# CLI version: 10.0.102

# Step 3: Download workload set manifest
curl -so workloadset.nupkg "https://api.nuget.org/v3-flatcontainer/microsoft.net.workloads.10.0.100/10.102.0/microsoft.net.workloads.10.0.100.10.102.0.nupkg"
unzip -p workloadset.nupkg data/microsoft.net.workloads.workloadset.json | jq '."microsoft.net.sdk.android"'
# Result: "35.0.50/9.0.100"

# Step 4: Download Android manifest
curl -so android.nupkg "https://api.nuget.org/v3-flatcontainer/microsoft.net.sdk.android.manifest-9.0.100/35.0.50/microsoft.net.sdk.android.manifest-9.0.100.35.0.50.nupkg"
unzip -p android.nupkg data/WorkloadDependencies.json | jq '.["microsoft.net.sdk.android"]'
```

**Result**: Authoritative JDK, Android SDK, and Xcode requirements from live NuGet data.

---

## NuGet API Reference

| Operation | Endpoint |
|-----------|----------|
| .NET releases | `https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json` |
| Search packages | `https://azuresearch-usnc.nuget.org/query?q={query}&prerelease={bool}` |
| Download package | `https://api.nuget.org/v3-flatcontainer/{id}/{version}/{id}.{version}.nupkg` |

**Important**: Package IDs must be lowercase in download URLs.

---

## Best Practices

- **ALWAYS** fetch live data from NuGet APIs
- **NEVER** hardcode version requirements
- **ALWAYS** include SDK band with manifest versions
- Show exact URLs used for transparency
