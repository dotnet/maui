# Platform Requirements for .NET MAUI

Platform-specific requirements for .NET MAUI development. **All specific versions must be discovered from WorkloadDependencies.json** - see `workload-dependencies-discovery.md`.

## macOS

### Required Components

| Component | Requirement | Notes |
|-----------|-------------|-------|
| macOS | Recent version | ARM64 or Intel |
| .NET SDK | Active support | Query releases-index.json for latest |
| Xcode | Per WorkloadDependencies | From App Store |
| Command Line Tools | Match Xcode | `xcode-select --install` |

### Optional IDE/Editor

| Editor | Notes |
|--------|-------|
| Visual Studio Code | With C# Dev Kit extension |
| JetBrains Rider | Full .NET MAUI support |
| Visual Studio for Mac | **EOL** - no longer recommended |

**Note**: No IDE is required. You can build and run .NET MAUI apps using `dotnet` CLI alone.

### Required Workloads

| Workload | Required | Purpose |
|----------|----------|---------|
| `maui` | ✅ Yes | Core MAUI framework |
| `android` | ✅ Yes | Android targets |
| `ios` | ✅ Yes | iOS targets |
| `maccatalyst` | Recommended | Mac Catalyst targets |

### Android Development (macOS)

| Component | Source | Notes |
|-----------|--------|-------|
| Java JDK | `jdk.version` from WorkloadDependencies | Microsoft OpenJDK **only** |
| Android SDK | `androidsdk` from WorkloadDependencies | Use packages array |
| Platform Tools | `androidsdk.packages` | ADB, fastboot |
| Build Tools | `androidsdk.buildToolsVersion` | AAPT2, dx |

### iOS/macOS Development

| Component | Source | Notes |
|-----------|--------|-------|
| Xcode | `xcode.version` from WorkloadDependencies | From iOS workload manifest |
| iOS SDK | `sdk.version` from WorkloadDependencies | Bundled with Xcode |
| iOS Simulator | Any | At least one device |

---

## Windows

### Required Components

| Component | Requirement | Notes |
|-----------|-------------|-------|
| Windows | 10 or later | 64-bit required |
| .NET SDK | Active support | Query releases-index.json for latest |
| Windows App SDK | Current | Required for WinUI 3 / Windows targets |

### Optional IDE/Editor

| Editor | Notes |
|--------|-------|
| Visual Studio 2022 | With ".NET Multi-platform App UI development" workload |
| Visual Studio Code | With C# Dev Kit extension |
| JetBrains Rider | Full .NET MAUI support |

**Note**: No IDE is required. You can build and run .NET MAUI apps using `dotnet` CLI alone. However, Visual Studio provides the best debugging experience on Windows.

### Required Workloads

| Workload | Required | Purpose |
|----------|----------|---------|
| `maui` or `maui-windows` | ✅ Yes | Core MAUI framework |
| `android` | ✅ Yes | Android targets |
| `ios` | Optional | iOS targets (requires Mac build host) |
| `maccatalyst` | Optional | Mac Catalyst (requires Mac build host) |

### Android Development (Windows)

| Component | Source | Notes |
|-----------|--------|-------|
| Java JDK | `jdk.version` from WorkloadDependencies | Microsoft OpenJDK **only** |
| Android SDK | `androidsdk` from WorkloadDependencies | Use packages array |
| Android Emulator | Latest | With HAXM or Hyper-V |
| Platform Tools | `androidsdk.packages` | ADB, fastboot |
| Build Tools | `androidsdk.buildToolsVersion` | AAPT2, dx |

### Windows App Development

| Component | Requirement | Notes |
|-----------|-------------|-------|
| Windows App SDK | Current | Required for WinUI 3 |
| Windows SDK | Recent | Windows 10+ SDK |

---

## Linux

⚠️ **Linux has limited support** - Android targets only.

### Required Components

| Component | Requirement | Notes |
|-----------|-------------|-------|
| .NET SDK | Active support | Query releases-index.json |
| Java JDK | Per WorkloadDependencies | Microsoft OpenJDK **only** |
| Android SDK | Per WorkloadDependencies | Use packages array |

### Optional IDE/Editor

| Editor | Notes |
|--------|-------|
| Visual Studio Code | With C# Dev Kit extension |
| JetBrains Rider | Full .NET MAUI support |

**Note**: No IDE is required. You can build and run .NET MAUI apps using `dotnet` CLI alone.

### Required Workloads

| Workload | Required | Purpose |
|----------|----------|---------|
| `maui-android` | ✅ Yes | MAUI for Android |
| `android` | ✅ Yes | Android targets |

**Important**: Use `maui-android` NOT `maui` on Linux. The `maui` workload is a meta-workload that includes iOS/Mac dependencies which won't install on Linux.

### Limitations

⚠️ **Linux only supports Android targets**:
- ❌ No iOS support (requires macOS)
- ❌ No Mac Catalyst support (requires macOS)
- ❌ No Windows support (requires Windows)

### Android Development (Linux)

| Component | Source | Notes |
|-----------|--------|-------|
| Java JDK | `jdk.version` from WorkloadDependencies | Microsoft OpenJDK **only** |
| Android SDK | `androidsdk` from WorkloadDependencies | Use packages array |
| Platform Tools | `androidsdk.packages` | ADB, fastboot |
| Build Tools | `androidsdk.buildToolsVersion` | AAPT2, dx |
| KVM | Enabled | For emulator acceleration |

---

## How to Get Current Requirements

All specific version requirements must be discovered dynamically:

1. **Query releases-index.json** for supported .NET SDK versions
2. **Download WorkloadDependencies.json** from workload manifests for:
   - JDK version range and recommended version
   - Android SDK packages, API level, build tools version
   - Xcode version range and recommended version

See `workload-dependencies-discovery.md` for the complete NuGet API discovery process.

---

## Official Documentation Links

- [.NET MAUI Installation](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation)
- [.NET SDK Downloads](https://dotnet.microsoft.com/download)
- [Microsoft OpenJDK](https://learn.microsoft.com/en-us/java/openjdk/install)
- [Android Studio & SDK](https://developer.android.com/studio)
- [Xcode Downloads](https://developer.apple.com/xcode/)
