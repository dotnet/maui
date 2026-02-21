---
name: dotnet-workload-info
description: Discover .NET SDK versions, workload sets, manifest versions, and workload dependencies (Xcode, JDK, Android SDK) from live NuGet APIs. Use when asked about: .NET SDK requirements/versions, workload set versions, workload manifest versions, Xcode version requirements, JDK version requirements, Android SDK packages, or MAUI NuGet package versions. Triggers on questions like "What Xcode is required for .NET 10?" or "What's the latest workload set?"
---

# .NET Workload Info Discovery

Query live NuGet APIs to discover authoritative .NET SDK versions, workload sets, and dependency requirements.

## Inputs

| Parameter | Required | Example | Notes |
|-----------|----------|---------|-------|
| dotnetVersion | yes | 9.0, 10.0 | Major.minor format |
| prerelease | no | true/false | Default false |
| workload | no | ios, android, maui | Alias or full id |

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

Extract `latest-sdk` and derive SDK band:
- `10.0.102` → band `10.0.100` (hundreds digit)
- `10.0.205` → band `10.0.200`

### Step 2: Find Workload Set Package

```bash
curl -s "https://azuresearch-usnc.nuget.org/query?q=Microsoft.NET.Workloads.{MAJOR}.0&prerelease=false&semVerLevel=2.0.0"
```

Filter: Match `Microsoft.NET.Workloads.{major}.{band}`, exclude `.Msi.*`, pick highest version.

**Version conversion**: NuGet `10.102.0` → CLI `10.0.102` (swap middle segments)

### Step 3: Download Workload Set Manifest

```bash
curl -o workloadset.nupkg "https://api.nuget.org/v3-flatcontainer/microsoft.net.workloads.{band}/{version}/microsoft.net.workloads.{band}.{version}.nupkg"
unzip -p workloadset.nupkg data/microsoft.net.workloads.workloadset.json
```

Format: `"{workload_id}": "{manifestVersion}/{sdkBand}"`

### Step 4: Download Workload Manifest

Build package id: `{WorkloadId}.Manifest-{sdkBand}` (e.g., `Microsoft.NET.Sdk.iOS.Manifest-10.0.100`)

```bash
curl -o manifest.nupkg "https://api.nuget.org/v3-flatcontainer/{packageid}/{version}/{packageid}.{version}.nupkg"
unzip -p manifest.nupkg data/WorkloadDependencies.json
```

### Step 5: Parse Dependencies

**Android** (`microsoft.net.sdk.android`):
```json
{
  "jdk": { "version": "[17.0,22.0)", "recommendedVersion": "17.0.14" },
  "androidsdk": {
    "packages": ["build-tools;35.0.0", "platform-tools", "platforms;android-35"],
    "apiLevel": "35", "buildToolsVersion": "35.0.0"
  }
}
```

**iOS/macOS** (`microsoft.net.sdk.ios`):
```json
{
  "xcode": { "version": "[26.2,)", "recommendedVersion": "26.2" },
  "sdk": { "version": "26.2" }
}
```

**Version ranges**: `[17.0,22.0)` = >=17.0 AND <22.0; `[26.2,)` = >=26.2

---

## MAUI NuGet Packages

MAUI packages may be newer than workload versions. Query for latest:

```bash
curl -s "https://azuresearch-usnc.nuget.org/query?q=packageid:Microsoft.Maui.Controls&prerelease=false" | \
  jq '.data[0].versions | map(select(.version | startswith("{MAJOR}."))) | last'
```

Key packages: `Microsoft.Maui.Controls`, `Microsoft.Maui.Essentials`, `Microsoft.Maui.Graphics`

To use newer version than workload:
```xml
<PackageReference Include="Microsoft.Maui.Controls" Version="10.0.30" />
```

---

## Output Format

```json
{
  "dotnetVersion": "10.0",
  "latestSdk": "10.0.102",
  "sdkBand": "10.0.100",
  "workloadSet": { "packageId": "...", "nugetVersion": "10.102.0", "cliVersion": "10.0.102" },
  "workloads": [{
    "workloadId": "microsoft.net.sdk.ios",
    "manifestVersion": "26.2.10191",
    "sdkBand": "10.0.100",
    "dependencies": { "xcode": { "versionRange": "[26.2,)", "recommendedVersion": "26.2" } }
  }],
  "mauiNugets": [{ "packageId": "Microsoft.Maui.Controls", "latestVersion": "10.0.30" }]
}
```

---

## Error Handling

- No results → retry with `prerelease=true`
- Missing WorkloadDependencies.json → report explicitly
- Missing dependency key → note which keys absent

## Best Practices

- ALWAYS fetch live data; never hardcode versions
- ALWAYS include sdkBand with manifest versions
- Show exact URLs used for transparency

## Reference

See `references/workload-discovery-process.md` for detailed NuGet API documentation and complete examples.