# Link Construction Guide

## NuGet Package Links

### Workload Set Package
```
https://www.nuget.org/packages/Microsoft.NET.Workloads.{major}.0.{band}/{nuget_version}
```
Example: `https://www.nuget.org/packages/Microsoft.NET.Workloads.10.0.100/10.102.0`

### Workload Manifest Packages
```
https://www.nuget.org/packages/Microsoft.NET.Sdk.{Workload}.Manifest-{band}/{version}
```
Examples:
- `https://www.nuget.org/packages/Microsoft.NET.Sdk.Maui.Manifest-10.0.100/10.0.1`
- `https://www.nuget.org/packages/Microsoft.NET.Sdk.iOS.Manifest-10.0.100/26.2.10191`

### MAUI NuGet Packages
```
https://www.nuget.org/packages/{PackageName}/{version}
```

---

## GitHub Release Links

### MAUI Releases
Tag = version number directly
```
https://github.com/dotnet/maui/releases/tag/{version}
```
Example: `https://github.com/dotnet/maui/releases/tag/10.0.30`

### Android Releases
Tag = version number directly
```
https://github.com/dotnet/android/releases/tag/{version}
```
Example: `https://github.com/dotnet/android/releases/tag/36.1.12`

### Apple Platform Releases (iOS, Mac Catalyst, tvOS, macOS)

Tag format: `dotnet-{dotnetVersion}.0.1xx-xcode{xcodeVersion}-{buildNumber}`

**Parsing workload version** (e.g., `26.2.10191`):
- Parts: `xcodeVersion = 26.2`, `buildNumber = 10191`
- Tag: `dotnet-10.0.1xx-xcode26.2-10191`

```python
def apple_release_tag(workload_version, dotnet_major):
    parts = workload_version.split('.')
    xcode = f"{parts[0]}.{parts[1]}"
    build = parts[2]
    return f"dotnet-{dotnet_major}.0.1xx-xcode{xcode}-{build}"
```

Examples:
- `26.2.10191` on .NET 10 → `dotnet-10.0.1xx-xcode26.2-10191`
- `26.0.9783` on .NET 9 → `dotnet-9.0.1xx-xcode26.0-9783`
- `26.2.11425-net11-p2` on .NET 11 → `dotnet-11.0.1xx-preview2-11425`
---

## Formatting Conventions

| NuGet Notation | Display Format |
|----------------|----------------|
| `[17.0,22.0)` | `≥ 17.0 and < 22.0` |
| `[26.2,)` | `≥ 26.2` |

- Use `≥` symbol (not `>=`)
- Bold workload/component names in first column
- Backticks for package names and versions inline
- `---` horizontal rules between major .NET version sections
- Date format: `Month Day, Year` (e.g., `January 19, 2026`)
