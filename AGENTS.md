# AGENTS.md

This file provides guidance to AI coding assistants when working with code in this repository.

**🔄 IMPORTANT: Synchronization with Copilot Instructions**

When updating this file, you MUST:
1. Analyze ALL copilot instruction files:
   - `.github/copilot-instructions.md` (repository-wide instructions)
   - `.github/instructions/*.instructions.md` (path-specific instructions)
2. Determine which information is useful for AI coding assistants and should be added to AGENTS.md
3. Update `.github/copilot-instructions.md` with corresponding changes to keep both files synchronized
4. Ensure no duplication - AGENTS.md should contain universal guidance, while copilot-instructions.md may have GitHub Copilot-specific details

## Project Overview

.NET MAUI is a cross-platform framework for creating mobile and desktop applications with C# and XAML. This repository contains the core framework code that enables development for Android, iOS, iPadOS, macOS, and Windows from a single shared codebase.

### Key Technologies

- **.NET SDK** - Version always defined in `global.json` at repository root
- **C#** and **XAML** for application development
- **Cake build system** for compilation and packaging
- **MSBuild** with custom build tasks
- **xUnit + NUnit** for testing (xUnit for unit tests, NUnit for UI tests)
- **Appium WebDriver** for UI test automation

## Essential Setup Commands

### Initial Repository Setup

**🚨 CRITICAL**: Before any build operation, verify .NET SDK version:
```bash
# Check required version (always defined in global.json)
cat global.json | grep -A 1 '"dotnet"'

# Verify installed version matches
dotnet --version
```

**Before opening the solution in any IDE**, you MUST build the build tasks first:

```bash
dotnet tool restore
dotnet build ./Microsoft.Maui.BuildTasks.slnf
```

**⚠️ FAILURE RECOVERY**: If restore or build fails:
```bash
# Clean and retry
dotnet clean
rm -rf bin/ obj/
dotnet tool restore --force
dotnet build ./Microsoft.Maui.BuildTasks.slnf --verbosity normal
```

### Primary Build Commands

```bash
# Build everything using Cake (recommended)
dotnet cake

# Pack NuGet packages
dotnet cake --target=dotnet-pack

# Clean incremental builds when switching branches
dotnet cake --clean

# Build for specific platforms
dotnet cake --target=VS --android --ios
```

### Code Formatting

Before committing any changes:

```bash
dotnet format Microsoft.Maui.sln --no-restore --exclude Templates/src --exclude-diagnostics CA1822
```

### PublicAPI Management

When adding new public APIs:

```bash
# For a specific project
dotnet build ./src/Controls/src/Core/Controls.Core.csproj /p:PublicApiType=Generate

# Or use Cake to regenerate all PublicAPI files
dotnet cake --target=publicapi
```

## Project Architecture

### Handler-Based Architecture

MAUI uses a Handler-based architecture that replaces Xamarin.Forms renderers. Key concepts:

- **Handlers**: Map cross-platform controls to platform-specific implementations
- **Mappers**: Define property and command mappings between virtual and platform views
- **Handler files**: Located in `src/Core/src/Handlers/[ControlName]/`
  - Interface: `I[ControlName]Handler.cs`
  - Platform-specific: `[ControlName]Handler.Android.cs`, `[ControlName]Handler.iOS.cs`, etc.
  - Shared logic: `[ControlName]Handler.cs`

### Platform-Specific Code Organization

**File Extensions** (automatic platform targeting):
- `.android.cs` → Android only
- `.ios.cs` → iOS and MacCatalyst
- `.windows.cs` → Windows only
- `.MacCatalyst.cs` → MacCatalyst only

**Folder Organization**:
- Platform-specific code in `Android/`, `iOS/`, `MacCatalyst/`, `Windows/` folders
- Shared code at the folder root or in `Shared/`

### Major Components

- **`src/Core/`** - Core framework, handlers, platform abstractions
- **`src/Controls/`** - UI controls, XAML infrastructure
  - `src/Core/` - Core controls (Button, Label, etc.)
  - `src/Xaml/` - XAML loader and compilation
  - `src/Build.Tasks/` - MSBuild tasks for XAML compilation
- **`src/Essentials/`** - Platform APIs (GPS, accelerometer, etc.)
- **`src/Graphics/`** - Cross-platform graphics abstractions
- **`src/Compatibility/`** - Xamarin.Forms compatibility layer

### Solution Files

- `Microsoft.Maui-windows.slnf` - Windows development
- `Microsoft.Maui-mac.slnf` - Mac development
- `Microsoft.Maui.BuildTasks.slnf` - Build tasks only (must build first)

### Sample Projects for Testing

- `src/Controls/samples/Controls.Sample.Sandbox` - **Empty project for testing/reproduction** (preferred for debugging)
- `src/Controls/samples/Controls.Sample` - Full gallery sample
- `src/Essentials/samples/Essentials.Sample` - Essentials API demonstrations

**Important**: Do not commit changes to the Sandbox project in PRs.

## Testing

### Unit Tests

Key test projects to ensure pass:
- `src/Core/tests/UnitTests/Core.UnitTests.csproj`
- `src/Essentials/test/UnitTests/Essentials.UnitTests.csproj`
- `src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj`
- `src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj`

### Running UI Tests

**Option 1: Using Cake build system (for full CI-like test runs):**

```bash
# Android
./build.ps1 -Script eng/devices/android.cake --target=uitest

# iOS
./build.ps1 -Script eng/devices/ios.cake --target=uitest

# Windows
./build.ps1 -Script eng/devices/windows.cake --target=uitest

# MacCatalyst
./build.ps1 -Script eng/devices/catalyst.cake --target=uitest

# Filter by category
dotnet cake eng/devices/android.cake --target=uitest --test-filter="TestCategory=Button"

# Filter by test name
dotnet cake eng/devices/android.cake --target=uitest --test-filter="FullyQualifiedName~Issue12345"
```

**Option 2: Running specific tests directly (for rapid development):**

**Android:**

1. Deploy the TestCases.HostApp:
   ```bash
   # Use local dotnet if available, otherwise use global dotnet
   ./bin/dotnet/dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run
   # OR:
   dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run
   ```

2. Run your specific test:
   ```bash
   # Set DEVICE_UDID environment variable so Appium tests know which device to use
   # Get the device ID from: adb devices
   export DEVICE_UDID=$(adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1)

   # Run the test
   dotnet test src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj --filter "FullyQualifiedName~Issue12345"
   ```

**iOS (4-step process):**

**Important: Device and iOS Version Selection**

When the user requests to run tests on iOS:
- **Default behavior (no device, no iOS version specified)**: Use iPhone Xs with the highest available iOS version
- **User specifies iOS version only** (e.g., "iOS 26.0", "iOS 18.4"):
  - First, try to find iPhone Xs with that iOS version
  - If iPhone Xs not available for that iOS version, use ANY available device with that iOS version
  - Set `IOS_VERSION` variable, leave `DEVICE_NAME` empty to allow fallback
- **User specifies device only** (e.g., "iPhone 16 Pro", "iPhone 15"):
  - Set `DEVICE_NAME` variable with the specified device
  - Use highest available iOS version for that device
- **User specifies both device AND iOS version**: Set both `IOS_VERSION` and `DEVICE_NAME` variables

Examples of interpreting user requests:
- "Run on iOS 26.0" → Set `IOS_VERSION="26.0"`, leave `DEVICE_NAME` empty (will try iPhone Xs first, then fallback to any iOS 26.0 device)
- "Run on iPhone 16 Pro" → Set `DEVICE_NAME="iPhone 16 Pro"`, use highest iOS version
- "Run on iPhone 15 with iOS 18.0" → Set both `DEVICE_NAME="iPhone 15"` and `IOS_VERSION="18.0"`
- No specific request → Use defaults (iPhone Xs with highest iOS)

**Step 1: Find iOS Simulator**

```bash
# Set device name and iOS version based on user request
# Leave DEVICE_NAME empty when user only specifies iOS version (to allow fallback)
DEVICE_NAME="${DEVICE_NAME:-}"
IOS_VERSION="${IOS_VERSION:-}"

# Determine search strategy
if [ -z "$IOS_VERSION" ]; then
    # No iOS version specified - use iPhone Xs with highest available iOS
    DEVICE_NAME="${DEVICE_NAME:-iPhone Xs}"
    JQ_FILTER='
      .devices
      | to_entries
      | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS")))
      | map({
          key: .key,
          version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)),
          devices: .value
        })
      | sort_by(.version)
      | reverse
      | map(select(.devices | any(.name == "'"$DEVICE_NAME"'")))
      | first
      | .devices[]
      | select(.name == "'"$DEVICE_NAME"'")
      | .udid'
else
    # Specific iOS version requested
    IOS_VERSION_FILTER="iOS-${IOS_VERSION//./-}"

    if [ -z "$DEVICE_NAME" ]; then
        # iOS version specified, but no device - try iPhone Xs first, then fallback to any device
        JQ_FILTER='
          .devices
          | to_entries
          | map(select(.key | contains("'"$IOS_VERSION_FILTER"'")))
          | first
          | .value
          | (map(select(.name == "iPhone Xs")) + .)[0]
          | .udid'
    else
        # Both iOS version and device specified
        JQ_FILTER='
          .devices
          | to_entries
          | map(select(.key | contains("'"$IOS_VERSION_FILTER"'")))
          | map(.value)
          | flatten
          | map(select(.name == "'"$DEVICE_NAME"'"))
          | first
          | .udid'
    fi
fi

# Extract UDID using the constructed filter
UDID=$(xcrun simctl list devices available --json | jq -r "$JQ_FILTER")

# Get the actual device name that was found
if [ ! -z "$UDID" ] && [ "$UDID" != "null" ]; then
    FOUND_DEVICE=$(xcrun simctl list devices available --json | jq -r --arg udid "$UDID" '.devices[][] | select(.udid == $udid) | .name')
fi

# Verify UDID was found and is not empty
if [ -z "$UDID" ] || [ "$UDID" = "null" ]; then
    if [ -z "$IOS_VERSION" ]; then
        DEVICE_NAME="${DEVICE_NAME:-iPhone Xs}"
        echo "ERROR: No $DEVICE_NAME simulator found. Please create a $DEVICE_NAME simulator before running iOS tests."
    elif [ -z "$DEVICE_NAME" ]; then
        echo "ERROR: No simulator found for iOS $IOS_VERSION. Please install iOS $IOS_VERSION runtime."
    else
        echo "ERROR: No $DEVICE_NAME simulator found for iOS $IOS_VERSION. Please create one before running iOS tests."
    fi
    exit 1
fi

echo "Using $FOUND_DEVICE with UDID: $UDID"
```

**Examples of device/version selection:**
```bash
# Default: iPhone Xs with highest iOS version
# (no environment variables needed)

# Specific iOS version (will try iPhone Xs first, then any device):
export IOS_VERSION="26.0"
# Leave DEVICE_NAME unset

# Specific device with highest iOS version:
export DEVICE_NAME="iPhone 16 Pro"
# Leave IOS_VERSION unset

# Specific device AND specific iOS version:
export DEVICE_NAME="iPhone 15"
export IOS_VERSION="18.0"
```

**Step 2: Build the iOS app**
```bash
# Use local dotnet if available, otherwise use global dotnet
./bin/dotnet/dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-ios
# OR:
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-ios
```

**Step 3: Boot simulator and install app**
```bash
# Boot the simulator (will error if already booted, which is fine)
xcrun simctl boot $UDID 2>/dev/null || true

# Verify simulator is booted
STATE=$(xcrun simctl list devices --json | jq -r --arg udid "$UDID" '.devices[][] | select(.udid == $udid) | .state')
if [ "$STATE" != "Booted" ]; then
    echo "ERROR: Simulator failed to boot. Current state: $STATE"
    exit 1
fi
echo "Simulator is booted and ready"

# Install the app to the simulator
xcrun simctl install $UDID artifacts/bin/Controls.TestCases.HostApp/Debug/net10.0-ios/iossimulator-arm64/Controls.TestCases.HostApp.app
```

**Step 4: Run your specific test**
```bash
# Set DEVICE_UDID environment variable so Appium tests know which device to use
export DEVICE_UDID=$UDID

# Run the test
dotnet test src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj --filter "FullyQualifiedName~Issue12345"
```

**MacCatalyst:**

**Step 1: Deploy TestCases.HostApp to MacCatalyst**
```bash
# Use local dotnet if available, otherwise use global dotnet
./bin/dotnet/dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-maccatalyst -t:Run
# OR:
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-maccatalyst -t:Run
```

**Step 2: Run your specific test**
```bash
dotnet test src/Controls/tests/TestCases.Mac.Tests/Controls.TestCases.Mac.Tests.csproj --filter "FullyQualifiedName~Issue12345"
```

**Troubleshooting UI Tests:**

**Android App Crashes on Launch:**

If you encounter navigation fragment errors or resource ID issues:
```
java.lang.IllegalArgumentException: No view found for id 0x7f0800f8 (com.microsoft.maui.uitests:id/inward) for fragment NavigationRootManager_ElementBasedFragment
```

**Solution:** Build with `--no-incremental` to force a clean build:
```bash
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run --no-incremental
```

**Other debugging steps:**
1. Monitor logcat: `adb logcat | grep -E "(FATAL|AndroidRuntime|Exception|Error|Crash)"`
2. Try clean build: `dotnet clean src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj`
3. Check emulator: `adb devices`

### UI Tests

**CRITICAL**: UI tests require code in TWO separate projects:

1. **HostApp UI Test Page** (`src/Controls/tests/TestCases.HostApp/Issues/`)
   - Create XAML page with `AutomationId` attributes
   - Naming: `IssueXXXXX.xaml` and `IssueXXXXX.xaml.cs`

2. **NUnit Test Implementation** (`src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/`)
   - Inherit from `_IssuesUITest`
   - Naming: `IssueXXXXX.cs` (matches HostApp file)
   - Include ONE `[Category(UITestCategories.XYZ)]` attribute

**Platform Coverage**: Avoid platform-specific directives (`#if ANDROID`, `#if IOS`, etc.) unless absolutely necessary. Tests should run on all applicable platforms (iOS, Android, Windows, MacCatalyst) by default.

**Important**: When adding a new UI test category to `UITestCategories.cs`, also update `eng/pipelines/common/ui-tests.yml`.

**For comprehensive UI testing documentation**, see [docs/UITesting-Guide.md](docs/UITesting-Guide.md).

## Branching Strategy

- **`main`** - Bug fixes without API changes (pinned to latest stable .NET SDK)
- **`net10.0`** - New features and API changes (next version development)

## Critical File Management Rules

### Files to NEVER Commit

- `cgmanifest.json` - Auto-generated during CI builds
- `templatestrings.json` - Auto-generated localization files

### File Reset Guidelines for AI Agents

Since coding agents function as both CI and pair programmers, they need to handle CI-generated files appropriately:

- **Always reset changes to `cgmanifest.json` files** - These are generated during CI builds and should not be committed by coding agents
- **Always reset changes to `templatestrings.json` files** - These localization files are auto-generated and should not be committed by coding agents

### PublicAPI.Unshipped.txt Management

- Never disable analyzers or set nowarn to bypass PublicAPI errors
- Use `dotnet format analyzers` if having trouble
- **Revert and re-add approach**: Revert all changes to PublicAPI.Unshipped.txt files, then make only necessary additions

## PR Requirements

All PRs must include this note at the top of the description:

```markdown
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!
```

## Handling Existing PRs

**CRITICAL REQUIREMENT**: Always develop your own solution first, then compare with existing PRs.

1. Develop your own solution independently
2. Search for existing PRs addressing the same issue
3. Compare solutions thoroughly
4. Document your decision in PR description
5. Explain why you chose your approach over alternatives

## Code Style

Follow [.NET Foundation coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md) with exceptions:
- Do not use the `private` keyword (default accessibility)
- Use hard tabs over spaces

## Platform-Specific Development

### Android
- Requires Android SDK and OpenJDK 17
- Set `ANDROID_HOME` environment variable
- Install Android SDK components via Android SDK Manager

### iOS (requires macOS)
- Requires current stable Xcode installation
- Xcode command-line tools must be installed
- Pair to Mac required when developing on Windows

### Windows
- Requires Windows SDK
- Visual Studio workloads must include .NET MAUI development

### macOS/Mac Catalyst
- Requires Xcode installation

## Troubleshooting

### Common Build Issues

**Issue: "Build tasks not found"**
```bash
# Solution: Rebuild build tasks
dotnet clean ./Microsoft.Maui.BuildTasks.slnf
dotnet build ./Microsoft.Maui.BuildTasks.slnf
```

**Issue: "Dependency version conflicts"**
```bash
# Solution: Full clean and restore
dotnet clean Microsoft.Maui.sln
rm -rf bin/ obj/
dotnet restore Microsoft.Maui.sln --force
```

**Issue: "PublicAPI analyzer failures"**
```bash
# Solution: Use format analyzers first
dotnet format analyzers Microsoft.Maui.sln
# If still failing, check build output for required API entries
```

### Platform-Specific Troubleshooting

**iOS/Mac Build Issues:**
- Verify Xcode command line tools: `xcode-select --install`
- Check Xcode version compatibility with .NET MAUI version
- Ensure provisioning profiles are current (for device testing)

**Android Build Issues:**
- Verify OpenJDK 17 installation: `java -version`
- Check ANDROID_HOME environment variable
- Update Android SDK components via Android SDK Manager

**Windows Build Issues:**
- Ensure Windows SDK is installed
- Verify Visual Studio workloads include .NET MAUI development
- Check for missing NuGet packages: `dotnet restore --force`

## Additional Resources

- [UI Testing Guide](docs/UITesting-Guide.md)
- [UI Testing Architecture](docs/design/UITesting-Architecture.md)
