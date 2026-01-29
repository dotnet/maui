# Installation Commands Reference

Complete installation commands for all .NET MAUI development dependencies.

## Contents

- [.NET SDK](#net-sdk)
- [.NET Workloads](#net-workloads)
- [Java JDK (Microsoft OpenJDK)](#java-jdk-microsoft-openjdk-only)
- [Android SDK](#android-sdk)
- [Xcode (macOS)](#xcode-macos-only)
- [iOS Simulators](#ios-simulators-macos-only)
- [Windows Development](#windows-development-windows-only)
- [Helper Tools (Third-Party)](#helper-tools-third-party---optional)
- [Quick Setup Scripts](#quick-setup-scripts)

---

**Important**: All specific versions shown below are placeholders. Always discover the actual versions to use:
- **SDK/Workload versions**: Query releases-index.json and NuGet APIs (see `workload-dependencies-discovery.md`)
- **Android SDK packages**: From `androidsdk` in WorkloadDependencies.json
- **JDK version**: From `jdk.version` in WorkloadDependencies.json

## .NET SDK

### macOS / Linux

```bash
# Install latest LTS
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel LTS

# Install specific channel (replace MAJOR.MINOR with actual version)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel $DOTNET_CHANNEL

# Install specific version (replace with actual version from releases-index.json)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version $SDK_VERSION

# Add to PATH (add to ~/.bashrc or ~/.zshrc)
export PATH="$PATH:$HOME/.dotnet"
export DOTNET_ROOT="$HOME/.dotnet"
```

### Windows

```powershell
# Using winget (replace X with major version)
winget install Microsoft.DotNet.SDK.$DOTNET_MAJOR

# Using PowerShell script
Invoke-WebRequest -Uri https://dot.net/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1
./dotnet-install.ps1 -Channel $DOTNET_CHANNEL
```

### Homebrew (macOS)

```bash
brew install --cask dotnet-sdk
```

---

## .NET Workloads

### Install MAUI Workloads

**Always use explicit workload set version** to ensure consistent, reproducible installs.

First, find the latest workload set version using the process in `workload-dependencies-discovery.md`:
```bash
# Query NuGet for latest workload set
# SDK band = first 2 segments of SDK version (e.g., 10.0 from 10.0.102)
curl -s "https://azuresearch-usnc.nuget.org/query?q=Microsoft.NET.Workloads.$SDK_BAND&prerelease=false" | \
  jq '.data[] | select(.id | test("^Microsoft.NET.Workloads.$SDK_BAND.[0-9]+$")) | {id, version}'

# Convert NuGet version to CLI version:
# NuGet A.B.C → CLI A.0.B (e.g., NuGet 10.102.0 → CLI 10.0.102)
```

Then install with explicit version:
```bash
# Full MAUI installation (recommended)
dotnet workload install maui --version $WORKLOAD_VERSION

# Individual workloads
dotnet workload install android --version $WORKLOAD_VERSION
dotnet workload install ios --version $WORKLOAD_VERSION           # macOS only meaningful
dotnet workload install maccatalyst --version $WORKLOAD_VERSION   # macOS only meaningful

# Multiple at once
dotnet workload install maui android ios maccatalyst --version $WORKLOAD_VERSION
```

### List Installed Workloads

```bash
dotnet workload list
```

### ⚠️ Commands to Avoid

**Never use these commands** - they can cause version inconsistencies:
- ❌ `dotnet workload update` - Can introduce mixed versions
- ❌ `dotnet workload repair` - May not fix version issues
- ❌ `dotnet workload install` without `--version` - Gets unpredictable versions

**Instead**: Always reinstall with explicit `--version` to fix workload issues.

---

## Java JDK (Microsoft OpenJDK ONLY)

**CRITICAL: Only Microsoft Build of OpenJDK is supported.** Other JDK vendors (Oracle, Azul, Amazon Corretto, Temurin, etc.) are NOT supported for .NET MAUI development.

See `microsoft-openjdk.md` for complete installation paths and detection scripts.

### Why Microsoft OpenJDK Only?
- Tested and validated with .NET MAUI toolchain
- Consistent behavior across all platforms
- Long-term support with security updates
- Official recommendation from Microsoft

### macOS

```bash
# Homebrew (recommended)
brew install --cask microsoft-openjdk@17
# or
brew install --cask microsoft-openjdk@21

# Verify it's Microsoft JDK
java -version 2>&1 | grep -i microsoft

# Installation path: /Library/Java/JavaVirtualMachines/microsoft-{VERSION}.jdk/Contents/Home
```

### Windows

```powershell
# Using winget (recommended)
winget install Microsoft.OpenJDK.17
# or
winget install Microsoft.OpenJDK.21

# Verify it's Microsoft JDK
java -version 2>&1 | Select-String "Microsoft"

# Installation path: C:\Program Files\Microsoft\jdk-{VERSION}\
```

### Ubuntu / Debian

```bash
# Add Microsoft repository
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install
sudo apt update
sudo apt install msopenjdk-17
# or
sudo apt install msopenjdk-21

# Verify it's Microsoft JDK
java -version 2>&1 | grep -i microsoft

# Installation path: /usr/lib/jvm/msopenjdk-{VERSION}/
```

### Fedora / RHEL

```bash
# Add Microsoft repository
sudo rpm -Uvh https://packages.microsoft.com/config/rhel/8/packages-microsoft-prod.rpm

# Install
sudo dnf install msopenjdk-17
# or
sudo dnf install msopenjdk-21

# Verify it's Microsoft JDK
java -version 2>&1 | grep -i microsoft
```

### Verify JDK Installation

**IMPORTANT**: Verify the output contains "Microsoft"!

```bash
# Check version - MUST show "Microsoft" in output
java -version

# Expected output example:
# openjdk version "17.0.14" 2025-01-21 LTS
# OpenJDK Runtime Environment Microsoft-XXXXXXX (build 17.0.14+7-LTS)
# OpenJDK 64-Bit Server VM Microsoft-XXXXXXX (build 17.0.14+7-LTS, mixed mode, sharing)

# If using AndroidSdk.Tool (shows vendor info)
android jdk list
android jdk find --version 17
```

---

## Android SDK

### Using AndroidSdk.Tool (Optional - Third-Party)

**Note**: This is a third-party tool, not published by Microsoft. Ask user before installing.

```bash
# Check if already installed
dotnet tool list -g | grep -i androidsdk

# Install the tool globally (only if user agrees)
dotnet tool install -g AndroidSdk.Tool

# Download Android SDK
android sdk download

# Install required packages (get exact versions from WorkloadDependencies.json)
# androidsdk.packages array contains required packages
# androidsdk.buildToolsVersion contains build tools version
# androidsdk.apiLevel contains API level for platforms;android-XX
android sdk install --package "platform-tools"
android sdk install --package "build-tools;$BUILD_TOOLS_VERSION"
android sdk install --package "platforms;android-$API_LEVEL"
android sdk install --package "cmdline-tools;$CMDLINE_TOOLS_VERSION"

# Accept all licenses
android sdk accept-licenses
```

### Using Android Studio

1. Download from https://developer.android.com/studio
2. Install and run Android Studio
3. Go to Settings → SDK Manager
4. Install required SDK packages (versions from WorkloadDependencies.json):
   - Android SDK Platform (from `androidsdk.apiLevel`)
   - Android SDK Build-Tools (from `androidsdk.buildToolsVersion`)
   - Android SDK Platform-Tools
   - Android SDK Command-line Tools

### Using sdkmanager (Manual)

```bash
# Set SDK root
export ANDROID_SDK_ROOT="$HOME/Android/Sdk"

# Download command-line tools
# From: https://developer.android.com/studio#command-tools

# Install packages (get versions from WorkloadDependencies.json)
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager "platform-tools"
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager "build-tools;$BUILD_TOOLS_VERSION"
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager "platforms;android-$API_LEVEL"

# Accept licenses
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager --licenses
```

### Verify Android SDK

```bash
# Using AndroidSdk.Tool (if installed)
android sdk info
android sdk list --installed

# Fallback - Check ADB
adb --version

# Fallback - Use sdkmanager
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager --list_installed
```

---

## Xcode (macOS Only)

### Install from App Store

1. Open App Store
2. Search for "Xcode"
3. Click "Get" / "Install"

### Install Command Line Tools

```bash
xcode-select --install
```

### Using AppleDev.Tools (Optional - Third-Party)

**Note**: This is a third-party tool, not published by Microsoft. Ask user before installing.

```bash
# Check if already installed
dotnet tool list -g | grep -i appledev

# Install the tool (only if user agrees)
dotnet tool install -g AppleDev.Tools

# List installed Xcode versions
apple xcode list

# Locate best available
apple xcode locate --best
```

**Fallback commands** (if user declines or tool not available):
```bash
# List Xcode installations
ls -d /Applications/Xcode*.app 2>/dev/null

# Get Xcode version
xcodebuild -version

# Get selected Xcode
xcode-select -p
```

### Set Active Xcode Version

```bash
sudo xcode-select -s /Applications/Xcode.app/Contents/Developer
```

### Accept Xcode License

```bash
sudo xcodebuild -license accept
```

### Verify Xcode Installation

```bash
xcodebuild -version
xcrun simctl list devices available
```

---

## iOS Simulators (macOS Only)

### Using Xcode

1. Open Xcode
2. Go to Settings → Platforms
3. Download desired iOS runtime versions
4. Simulators are automatically available

### Using Command Line

```bash
# List available device types
xcrun simctl list devicetypes

# List available runtimes
xcrun simctl list runtimes

# Create a simulator
xcrun simctl create "iPhone 16 Pro" "com.apple.CoreSimulator.SimDeviceType.iPhone-16-Pro" "com.apple.CoreSimulator.SimRuntime.iOS-18-0"

# Boot simulator
xcrun simctl boot "iPhone 16 Pro"

# Open Simulator app
open -a Simulator
```

---

## Windows Development (Windows Only)

### Visual Studio

```powershell
# Using winget
winget install Microsoft.VisualStudio.2022.Community

# Then add MAUI workload via Visual Studio Installer
# Or via command line:
& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vs_installer.exe" modify --installPath "C:\Program Files\Microsoft Visual Studio\2022\Community" --add Microsoft.VisualStudio.Workload.NetCrossPlat
```

### Windows App SDK

```powershell
# Usually installed with Visual Studio MAUI workload
# Or via NuGet in your project (check latest version on NuGet.org):
# <PackageReference Include="Microsoft.WindowsAppSDK" Version="$LATEST_VERSION" />
```

---

## Helper Tools (Third-Party - Optional)

These tools are **not published by Microsoft**. They provide convenience features but are optional.
**Always ask the user before installing.**

### AndroidSdk.Tool

Provides better JDK and Android SDK detection/management.

- **Author**: Third-party (community)
- **NuGet**: https://www.nuget.org/packages/AndroidSdk.Tool

```bash
# Check if installed
dotnet tool list -g | grep -i androidsdk

# Install globally (only if user agrees)
dotnet tool install -g AndroidSdk.Tool

# Or update existing
dotnet tool update -g AndroidSdk.Tool

# Verify
android --help
```

**If user declines**: Use `sdkmanager` directly (see "Using sdkmanager" section above).

### AppleDev.Tools (macOS)

Provides better Xcode detection/management.

- **Author**: Third-party (community)
- **NuGet**: https://www.nuget.org/packages/AppleDev.Tools

```bash
# Check if installed
dotnet tool list -g | grep -i appledev

# Install globally (only if user agrees)
dotnet tool install -g AppleDev.Tools

# Or update existing
dotnet tool update -g AppleDev.Tools

# Verify
apple --help
```

**If user declines**: Use `xcode-select`, `xcodebuild`, and `xcrun` commands directly.

---

## Quick Setup Scripts

**Note**: These scripts use placeholder variables. Before running, you must:
1. Query NuGet APIs to get the latest workload set version (see `workload-dependencies-discovery.md`)
2. Update the `WORKLOAD_VERSION` variable with the actual version
3. Update Android SDK package versions from WorkloadDependencies.json

**Third-party tools**: These scripts use AndroidSdk.Tool and AppleDev.Tools for convenience.
If you prefer not to use these, replace those commands with the fallback commands (sdkmanager, xcode-select, etc.)

### macOS Complete Setup

```bash
#!/bin/bash
set -e

# Configuration - DISCOVER THESE VALUES from NuGet APIs before running
# See workload-dependencies-discovery.md for the process
WORKLOAD_VERSION=""        # e.g., "10.0.102" from NuGet version 10.102.0
BUILD_TOOLS_VERSION=""     # From WorkloadDependencies.json androidsdk.buildToolsVersion
API_LEVEL=""               # From WorkloadDependencies.json androidsdk.apiLevel
JDK_MAJOR=""               # From WorkloadDependencies.json jdk.recommendedVersion major

# Optional: Set to "yes" to use third-party helper tools, "no" to use native CLIs
USE_HELPER_TOOLS="yes"

if [ -z "$WORKLOAD_VERSION" ]; then
  echo "ERROR: Set WORKLOAD_VERSION before running"
  exit 1
fi

# .NET SDK
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel LTS
export PATH="$PATH:$HOME/.dotnet"

# Workloads (with explicit version)
dotnet workload install maui android ios maccatalyst --version $WORKLOAD_VERSION

# JDK (Microsoft OpenJDK)
brew install --cask microsoft-openjdk@$JDK_MAJOR

# Android SDK
if [ "$USE_HELPER_TOOLS" = "yes" ]; then
  # Using AndroidSdk.Tool (third-party, optional)
  dotnet tool install -g AndroidSdk.Tool
  android sdk download
  android sdk install --package "platform-tools" "build-tools;$BUILD_TOOLS_VERSION" "platforms;android-$API_LEVEL"
  android sdk accept-licenses
else
  # Native approach - download cmdline-tools from developer.android.com
  export ANDROID_SDK_ROOT="$HOME/Library/Android/sdk"
  mkdir -p "$ANDROID_SDK_ROOT"
  # User must download and extract cmdline-tools manually, then:
  $ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager "platform-tools" "build-tools;$BUILD_TOOLS_VERSION" "platforms;android-$API_LEVEL"
  $ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager --licenses
fi

# Apple tools (optional third-party)
if [ "$USE_HELPER_TOOLS" = "yes" ]; then
  dotnet tool install -g AppleDev.Tools
fi

# Xcode CLI tools
xcode-select --install

echo "✅ MAUI development environment ready!"
```

### Windows Complete Setup (PowerShell)

```powershell
# Configuration - DISCOVER THESE VALUES from NuGet APIs before running
# See workload-dependencies-discovery.md for the process
$WORKLOAD_VERSION = ""     # e.g., "10.0.102" from NuGet version 10.102.0
$BUILD_TOOLS_VERSION = ""  # From WorkloadDependencies.json androidsdk.buildToolsVersion
$API_LEVEL = ""            # From WorkloadDependencies.json androidsdk.apiLevel

# Optional: Set to $true to use third-party helper tools, $false to use native CLIs
$USE_HELPER_TOOLS = $true

if (-not $WORKLOAD_VERSION) {
  Write-Error "ERROR: Set WORKLOAD_VERSION before running"
  exit 1
}

# .NET SDK
winget install Microsoft.DotNet.SDK.Preview  # Or specific version

# Workloads (with explicit version)
dotnet workload install maui android --version $WORKLOAD_VERSION

# JDK (version from WorkloadDependencies.json jdk.recommendedVersion)
winget install Microsoft.OpenJDK.17

# Android SDK
if ($USE_HELPER_TOOLS) {
  # Using AndroidSdk.Tool (third-party, optional)
  dotnet tool install -g AndroidSdk.Tool
  android sdk download
  android sdk install --package "platform-tools" "build-tools;$BUILD_TOOLS_VERSION" "platforms;android-$API_LEVEL"
  android sdk accept-licenses
} else {
  # Native approach - download cmdline-tools from developer.android.com
  $env:ANDROID_SDK_ROOT = "$env:LOCALAPPDATA\Android\Sdk"
  # User must download and extract cmdline-tools manually, then:
  & "$env:ANDROID_SDK_ROOT\cmdline-tools\latest\bin\sdkmanager.bat" "platform-tools" "build-tools;$BUILD_TOOLS_VERSION" "platforms;android-$API_LEVEL"
  & "$env:ANDROID_SDK_ROOT\cmdline-tools\latest\bin\sdkmanager.bat" --licenses
}

Write-Host "✅ MAUI development environment ready!" -ForegroundColor Green
```

### Linux Complete Setup (Ubuntu)

```bash
#!/bin/bash
set -e

# Configuration - DISCOVER THESE VALUES from NuGet APIs before running
# See workload-dependencies-discovery.md for the process
WORKLOAD_VERSION=""        # e.g., "10.0.102" from NuGet version 10.102.0
BUILD_TOOLS_VERSION=""     # From WorkloadDependencies.json androidsdk.buildToolsVersion
API_LEVEL=""               # From WorkloadDependencies.json androidsdk.apiLevel

# Optional: Set to "yes" to use third-party helper tools, "no" to use native CLIs
USE_HELPER_TOOLS="yes"

if [ -z "$WORKLOAD_VERSION" ]; then
  echo "ERROR: Set WORKLOAD_VERSION before running"
  exit 1
fi

# .NET SDK
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel LTS
export PATH="$PATH:$HOME/.dotnet"

# Workloads (Android only on Linux - use maui-android, NOT maui)
dotnet workload install maui-android android --version $WORKLOAD_VERSION

# JDK (Microsoft OpenJDK)
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
sudo apt install -y msopenjdk-17

# Android SDK
if [ "$USE_HELPER_TOOLS" = "yes" ]; then
  # Using AndroidSdk.Tool (third-party, optional)
  dotnet tool install -g AndroidSdk.Tool
  android sdk download
  android sdk install --package "platform-tools" "build-tools;$BUILD_TOOLS_VERSION" "platforms;android-$API_LEVEL"
  android sdk accept-licenses
else
  # Native approach - download cmdline-tools from developer.android.com
  export ANDROID_SDK_ROOT="$HOME/Android/Sdk"
  mkdir -p "$ANDROID_SDK_ROOT"
  # User must download and extract cmdline-tools manually, then:
  $ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager "platform-tools" "build-tools;$BUILD_TOOLS_VERSION" "platforms;android-$API_LEVEL"
  $ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager --licenses
fi

# KVM for emulator (optional)
sudo apt install -y qemu-kvm

echo "✅ MAUI development environment ready (Android only on Linux)!"
```
