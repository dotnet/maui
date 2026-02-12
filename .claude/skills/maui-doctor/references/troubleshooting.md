# Troubleshooting .NET MAUI Environment Issues

Common problems and solutions when setting up or using .NET MAUI.

## .NET SDK Issues

### "dotnet: command not found"

**Cause**: .NET SDK not installed or not in PATH.

**Solution**:
```bash
# Check if dotnet exists
which dotnet

# macOS/Linux - Add to PATH
export PATH="$PATH:$HOME/.dotnet"

# Or install SDK
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0
```

### "The required workload is not installed"

**Cause**: MAUI workload not installed.

**Solution**:
```bash
dotnet workload install maui --version $WORKLOAD_VERSION
```

### "Workload version mismatch"

**Cause**: Workloads from different SDK versions or incomplete installation.

**Solution**: Reinstall workloads with explicit version:
```bash
# First, find the correct workload set version for your SDK
# Query NuGet APIs (see workload-dependencies-discovery.md)
# or use dotnet-workload-info skill

# Then reinstall with explicit version
dotnet workload install maui android ios maccatalyst --version $WORKLOAD_VERSION
```

**Note**: Avoid `dotnet workload update` or `dotnet workload repair` as they can cause version inconsistencies.

### SDK version conflict with global.json

**Cause**: Project requires specific SDK version not installed.

**Solution**:
```bash
# Check required version
cat global.json

# Install specific version
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version X.Y.Z
```

---

## Java JDK Issues

**IMPORTANT: Only Microsoft Build of OpenJDK is supported.** Other JDK vendors (Oracle, Azul, Amazon Corretto, Temurin) are NOT supported for .NET MAUI development.

See `microsoft-openjdk.md` for complete installation paths by platform.

### "JAVA_HOME is not set"

**This is usually NOT a problem.** The .NET MAUI toolchain and AndroidSdk.Tool auto-detect JDK installations without needing `JAVA_HOME`.

**When JAVA_HOME matters:**
- ❌ `JAVA_HOME` is set but points to a **non-Microsoft JDK** → Fix by unsetting or pointing to Microsoft JDK
- ✅ `JAVA_HOME` is not set → Fine, tools will auto-detect
- ✅ `JAVA_HOME` points to Microsoft JDK → Fine

**Solution (only if JAVA_HOME is set to wrong JDK):**
```bash
# Option 1: Unset JAVA_HOME (let tools auto-detect)
unset JAVA_HOME

# Option 2: Point to Microsoft JDK (macOS)
export JAVA_HOME=/Library/Java/JavaVirtualMachines/microsoft-17.jdk/Contents/Home

# Option 2: Point to Microsoft JDK (Linux)
export JAVA_HOME=/usr/lib/jvm/msopenjdk-17

# Option 2: Point to Microsoft JDK (Windows PowerShell)
$env:JAVA_HOME = "C:\Program Files\Microsoft\jdk-17.0.14+7"
# Or remove it: Remove-Item Env:JAVA_HOME
```

### "Unsupported Java version" or "Wrong JDK vendor"

**Cause**: JDK version outside required range OR non-Microsoft JDK installed.

> **⚠️ TEMPORARY**: Recommend JDK 21 (not 17) until WorkloadDependencies.json is updated.

**Solution**: Install Microsoft OpenJDK 21:
```bash
# macOS
brew install --cask microsoft-openjdk@21

# Windows
winget install Microsoft.OpenJDK.21

# Ubuntu/Debian
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb && rm packages-microsoft-prod.deb
sudo apt update && sudo apt install msopenjdk-21
```

### Non-Microsoft JDK detected

**Cause**: Oracle, Azul, Corretto, or other non-Microsoft JDK is installed and selected.

**How to identify**: Run `java -version` - if output does NOT contain "Microsoft", wrong JDK is selected.

**Solution**:
1. Install Microsoft OpenJDK 21 (see commands above)
2. Set `JAVA_HOME` to Microsoft JDK path:
   - macOS: `/Library/Java/JavaVirtualMachines/microsoft-{VERSION}.jdk/Contents/Home`
   - Windows: `C:\Program Files\Microsoft\jdk-{VERSION}\`
   - Linux: `/usr/lib/jvm/msopenjdk-{VERSION}`
3. Optionally uninstall the non-Microsoft JDK

### Multiple JDKs installed, wrong one selected

**Solution**:
```bash
# If AndroidSdk.Tool installed - lists all JDKs with vendor info
android jdk list
android jdk dotnet-prefer --home /path/to/microsoft-jdk

# Fallback (macOS) - find Microsoft JDK
/usr/libexec/java_home -V 2>&1 | grep -i microsoft

# Fallback (Linux) - set Microsoft as default
sudo update-java-alternatives --set msopenjdk-21-amd64
```

---

## Android SDK Issues

### "Android SDK not found"

**Cause**: SDK not installed or path not configured.

**Solution**:
```bash
# If AndroidSdk.Tool installed
android sdk info
android sdk download

# Fallback - check environment variables
echo $ANDROID_HOME
echo $ANDROID_SDK_ROOT

# Fallback - common SDK locations
# macOS: ~/Library/Android/sdk
# Linux: ~/Android/Sdk
# Windows: %LOCALAPPDATA%\Android\Sdk
```

### "Failed to find Build Tools"

**Cause**: Required build-tools package not installed.

**Solution**:
```bash
# Get build tools version from WorkloadDependencies.json (androidsdk.buildToolsVersion)

# If AndroidSdk.Tool installed
android sdk install --package "build-tools;$BUILD_TOOLS_VERSION"

# Fallback - use sdkmanager
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager "build-tools;$BUILD_TOOLS_VERSION"
```

### "License not accepted"

**Cause**: Android SDK licenses not accepted.

**Solution**:
```bash
# If AndroidSdk.Tool installed
android sdk accept-licenses

# Fallback - use sdkmanager
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager --licenses
```

### "Platform not found: android-XX"

**Cause**: Target platform not installed.

**Solution**:
```bash
# Get required API level from WorkloadDependencies.json (androidsdk.apiLevel)

# If AndroidSdk.Tool installed
android sdk install --package "platforms;android-$API_LEVEL"

# Fallback - use sdkmanager
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager "platforms;android-$API_LEVEL"
```

### Emulator won't start

**Causes & Solutions**:

1. **HAXM/KVM not enabled**:
   ```bash
   # Linux - check KVM
   kvm-ok
   
   # Enable KVM
   sudo modprobe kvm
   ```

2. **Hyper-V conflict (Windows)**:
   - Use Android Emulator Hypervisor Driver instead of HAXM
   - Or disable Hyper-V: `bcdedit /set hypervisorlaunchtype off`

3. **Insufficient disk space**:
   - Clear AVD cache: `~/.android/avd/`

---

## Xcode Issues (macOS)

### "xcode-select: error: no developer tools found"

**Solution**:
```bash
xcode-select --install
```

### "Xcode not found at expected location"

**Solution**:
```bash
# If AppleDev.Tools installed
apple xcode list

# Fallback - list Xcode installations
ls -d /Applications/Xcode*.app 2>/dev/null

# Set active Xcode
sudo xcode-select -s /Applications/Xcode.app/Contents/Developer
```

### "Unable to boot simulator"

**Causes & Solutions**:

1. **No simulators installed**:
   ```bash
   # List available
   xcrun simctl list devices available
   
   # Create new simulator
   xcrun simctl create "iPhone 16" "com.apple.CoreSimulator.SimDeviceType.iPhone-16"
   ```

2. **Simulator runtime not installed**:
   - Open Xcode → Settings → Platforms → Download iOS runtime

3. **Corrupted simulator**:
   ```bash
   xcrun simctl erase all
   ```

### "Code signing error"

**Cause**: Missing or invalid provisioning profile.

**Solution**:
1. Open Xcode
2. Go to Settings → Accounts
3. Add/refresh Apple Developer account
4. Download provisioning profiles

---

## Build Errors

### "The target framework 'net10.0-android' is not available"

**Cause**: Android workload not installed for this SDK.

**Solution**:
```bash
dotnet workload install android --version $WORKLOAD_VERSION
```

### "Could not find android.jar"

**Cause**: Android platform not installed.

**Solution**:
```bash
android sdk install --package "platforms;android-35"
```

### "MSB4019: The imported project was not found"

**Cause**: Workload not properly installed.

**Solution**: Reinstall with explicit version:
```bash
# Get correct workload version for your SDK band from NuGet APIs, then:
dotnet workload install maui --version $WORKLOAD_VERSION
```

### "NETSDK1147: To build this project, the following workloads must be installed"

**Solution**: Install the listed workloads with explicit version:
```bash
dotnet workload install [workload-name] --version $WORKLOAD_VERSION
```

---

## Performance Issues

### Slow builds

**Solutions**:
1. Enable incremental builds (default)
2. Use Hot Reload during development
3. Build only necessary platforms:
   ```bash
   dotnet build -f net10.0-android
   ```

### Slow Android emulator

**Solutions**:
1. Enable hardware acceleration (HAXM/KVM/Hyper-V)
2. Use x86_64 system image (not ARM on Intel)
3. Increase emulator RAM in AVD settings
4. Use physical device for testing

### Slow iOS simulator

**Solutions**:
1. Close other resource-intensive apps
2. Use recent simulator device (not legacy)
3. Reduce debugger verbosity
4. Use physical device for performance testing

---

## Environment Variable Reference

| Variable | Purpose | Required | Notes |
|----------|---------|----------|-------|
| `JAVA_HOME` | JDK location | No | Only problematic if set to non-Microsoft JDK |
| `ANDROID_HOME` | Android SDK location | No | Auto-detected |
| `ANDROID_SDK_ROOT` | Android SDK location | No | Alternative to ANDROID_HOME |
| `DOTNET_ROOT` | .NET SDK location | No | Usually auto-detected |
| `PATH` | Must include dotnet | Yes | Required for CLI access |

**Key point about JAVA_HOME:**
- ✅ Not set → Fine, tools auto-detect Microsoft JDK
- ✅ Set to Microsoft JDK path → Fine
- ❌ Set to non-Microsoft JDK → Problem! Unset or fix it

**Note**: AndroidSdk.Tool and AppleDev.Tools auto-detect most paths. Only set these manually if auto-detection fails or wrong JDK is being selected.

---

## Getting Help

### Diagnostic Commands

```bash
# Full .NET info
dotnet --info

# Workload status
dotnet workload list

# JDK info (if AndroidSdk.Tool installed)
android jdk list
# Fallback
java -version
/usr/libexec/java_home -V  # macOS

# Android SDK info (if AndroidSdk.Tool installed)
android sdk info
android sdk list --installed
# Fallback
echo $ANDROID_SDK_ROOT
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager --list_installed

# Xcode info (macOS)
xcodebuild -version
xcode-select -p
# If AppleDev.Tools installed
apple xcode list
```

### Log Locations

| Platform | Log Location |
|----------|--------------|
| macOS | `~/Library/Logs/Xamarin/` |
| Windows | `%LOCALAPPDATA%\Xamarin\Logs\` |
| Linux | `~/.local/share/Xamarin/` |

### Resources

- [.NET MAUI GitHub Issues](https://github.com/dotnet/maui/issues)
- [Stack Overflow - maui tag](https://stackoverflow.com/questions/tagged/maui)
- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
