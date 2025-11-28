# Running UI Tests by Category

## Overview

The `run-ui-tests-for-category` script allows you to run UI tests for specific categories from the command line, which is much faster than running the entire test suite.

## Quick Start

### List Available Categories

```bash
# PowerShell
./eng/scripts/run-ui-tests-for-category.ps1 -ListCategories

# Bash (macOS/Linux)
./eng/scripts/run-ui-tests-for-category.sh -ListCategories
```

### Run Tests for a Single Category

```bash
# Run Button tests on Android
./eng/scripts/run-ui-tests-for-category.ps1 -Category Button -Platform android

# Run Entry tests on iOS
./eng/scripts/run-ui-tests-for-category.ps1 -Category Entry -Platform ios

# Run SafeAreaEdges tests on Windows
./eng/scripts/run-ui-tests-for-category.ps1 -Category SafeAreaEdges -Platform windows
```

### Run Tests for Multiple Categories

```bash
# Run Button, Label, and Entry tests
./eng/scripts/run-ui-tests-for-category.ps1 -CategoryGroup "Button,Label,Entry" -Platform android

# Run navigation-related tests
./eng/scripts/run-ui-tests-for-category.ps1 -CategoryGroup "Shell,Navigation,TabbedPage" -Platform ios
```

### Run Tests Based on PR Changes

```bash
# Analyze PR and run only affected tests
./eng/scripts/run-ui-tests-for-category.ps1 -PrNumber 12345 -Platform android

# This will:
# 1. Analyze what files changed in PR #12345
# 2. Determine which test categories are affected
# 3. Run only those category tests
```

## Parameters

### Required (choose one)

- **`-Category <string>`** - Single category to test
  - Examples: `Button`, `Label`, `Entry`, `CollectionView`
  
- **`-CategoryGroup <string>`** - Comma-separated list of categories
  - Examples: `"Button,Label"`, `"Entry,Editor,SearchBar"`
  
- **`-PrNumber <int>`** - PR number to analyze for intelligent selection
  - Example: `12345`
  
- **`-ListCategories`** - Display all available categories and exit

### Optional

- **`-Platform <string>`** - Platform to test (default: `android`)
  - Options: `android`, `ios`, `windows`, `catalyst`
  
- **`-Configuration <string>`** - Build configuration (default: `Release`)
  - Options: `Debug`, `Release`

## Examples

### Android Testing

```bash
# Run Button tests
./eng/scripts/run-ui-tests-for-category.ps1 -Category Button -Platform android

# Run multiple categories
./eng/scripts/run-ui-tests-for-category.ps1 -CategoryGroup "Button,Label,Entry" -Platform android

# Analyze PR and run affected tests
./eng/scripts/run-ui-tests-for-category.ps1 -PrNumber 12345 -Platform android
```

### iOS Testing

```bash
# Prerequisites: Boot an iOS simulator first
# Find simulator UDID:
UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | sort_by(.key) | reverse | first | .value[] | select(.name == "iPhone Xs") | .udid')
echo "Using simulator: $UDID"

# Boot the simulator
xcrun simctl boot $UDID 2>/dev/null || true

# Set environment variable for Appium
export DEVICE_UDID=$UDID

# Run tests
./eng/scripts/run-ui-tests-for-category.ps1 -Category Button -Platform ios
```

### Windows Testing

```bash
# Run from Windows machine or Windows VM
./eng/scripts/run-ui-tests-for-category.ps1 -Category Button -Platform windows
```

### Mac Catalyst Testing

```bash
# Run from macOS
./eng/scripts/run-ui-tests-for-category.ps1 -Category Button -Platform catalyst
```

## How It Works

### 1. Category Selection

The script translates categories into NUnit test filters:

- Single category: `TestCategory=Button`
- Multiple categories: `TestCategory=Button|TestCategory=Label|TestCategory=Entry`

### 2. Build Check

Before running tests, the script checks if the HostApp is built for the target platform:

- If not built: Automatically builds the HostApp
- If built: Skips build and runs tests immediately

### 3. Test Execution

Uses the existing Cake build system (`eng/devices/*.cake`) to:

- Deploy the HostApp to the target device/simulator
- Run the test project with the specified filter
- Collect results in `test-results/` directory

### 4. PR Analysis (Optional)

When `-PrNumber` is provided:

1. Calls `analyze-pr-changes.ps1` to fetch changed files
2. Maps changes to affected test categories
3. Runs tests for each category sequentially
4. Reports overall pass/fail status

## Platform-Specific Requirements

### Android

**Prerequisites:**
- Android SDK installed
- Android emulator running or device connected
- `adb devices` shows available device

**Setup:**
```bash
# List available devices
adb devices

# Start an emulator (if needed)
emulator -avd Pixel_5_API_30 &

# Verify device is ready
adb shell getprop sys.boot_completed
# Should output: 1
```

### iOS

**Prerequisites:**
- macOS with Xcode installed
- iOS simulator available

**Setup:**
```bash
# List available simulators
xcrun simctl list devices available

# Boot a simulator
UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | sort_by(.key) | reverse | first | .value[] | select(.name == "iPhone Xs") | .udid')
xcrun simctl boot $UDID

# Set for Appium
export DEVICE_UDID=$UDID
```

### Windows

**Prerequisites:**
- Windows 10/11 with Windows SDK
- Visual Studio or Build Tools installed

**Setup:**
```powershell
# Run from PowerShell
# No additional setup needed
```

### Mac Catalyst

**Prerequisites:**
- macOS with Xcode installed

**Setup:**
```bash
# No additional setup needed
# App runs directly on macOS
```

## Performance

### Time Comparison

| Test Scope | Categories | Approximate Time |
|-----------|-----------|-----------------|
| Single category (Button) | 1 | ~10-15 minutes |
| Small group (Entry,Editor,SearchBar) | 3 | ~25-35 minutes |
| Medium group (Navigation,Shell,Page) | 3-5 | ~40-60 minutes |
| Large group (10+ categories) | 10+ | ~2-3 hours |
| **Full suite (all categories)** | **19** | **~4+ hours** |

**Recommendation:** Run only the categories affected by your changes for maximum efficiency.

## Troubleshooting

### "HostApp not found" Error

**Solution:** The script will automatically build the HostApp, but you can also build it manually:

```bash
# Android
./bin/dotnet/dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run

# iOS
./bin/dotnet/dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-ios
```

### "Test project not found" Error

**Cause:** Test project path incorrect for platform

**Solution:** Verify the platform parameter is correct and the test project exists:
- Android: `src/Controls/tests/TestCases.Android.Tests/`
- iOS: `src/Controls/tests/TestCases.iOS.Tests/`
- Windows: `src/Controls/tests/TestCases.WinUI.Tests/`
- Catalyst: `src/Controls/tests/TestCases.Mac.Tests/`

### Android Emulator Not Found

**Solution:**
```bash
# Check if emulator is running
adb devices

# If not, start one
emulator -avd Pixel_5_API_30 &

# Wait for boot
adb wait-for-device
```

### iOS Simulator Not Booted

**Solution:**
```bash
# Get UDID and boot
UDID=$(xcrun simctl list devices available --json | jq -r '.devices[][] | select(.name == "iPhone Xs") | .udid' | head -1)
xcrun simctl boot $UDID

# Set environment variable
export DEVICE_UDID=$UDID
```

### "GitHub CLI not authenticated" (when using -PrNumber)

**Solution:**
```bash
# Set GitHub token
export GITHUB_TOKEN="your-github-pat"

# Or authenticate with gh CLI
gh auth login
```

### Tests Failing

**Debug steps:**

1. **Check app is installed:**
   ```bash
   # Android
   adb shell pm list packages | grep microsoft.maui.uitests
   
   # iOS
   xcrun simctl listapps $DEVICE_UDID | grep Controls.TestCases.HostApp
   ```

2. **Check Appium logs:**
   ```bash
   # Appium logs are in the test output
   # Look for connection errors or app launch failures
   ```

3. **Run with verbose output:**
   ```bash
   # The script already uses --verbosity=diagnostic
   # Check test-results/ for detailed logs
   ```

## Integration with Development Workflow

### Typical Development Flow

1. **Make code changes** to a control (e.g., Button)
2. **Run affected tests** locally:
   ```bash
   ./eng/scripts/run-ui-tests-for-category.ps1 -Category Button -Platform android
   ```
3. **Fix any failures** and repeat
4. **Create PR** - CI will run intelligent test selection automatically
5. **Monitor CI results** - only affected categories run

### Pre-PR Validation

Before creating a PR, validate your changes:

```bash
# Option 1: Run specific categories you changed
./eng/scripts/run-ui-tests-for-category.ps1 -CategoryGroup "Button,Label" -Platform android

# Option 2: Simulate what CI will run (if PR already exists)
./eng/scripts/run-ui-tests-for-category.ps1 -PrNumber 12345 -Platform android

# Option 3: Run on multiple platforms
for platform in android ios; do
    ./eng/scripts/run-ui-tests-for-category.ps1 -Category Button -Platform $platform
done
```

## Advanced Usage

### Running All Categories (Full Suite)

To run the full test suite locally:

```bash
# Run all category groups sequentially
./eng/scripts/run-ui-tests-for-category.ps1 -CategoryGroup "Accessibility,ActionSheet,ActivityIndicator,Animation,AppLinks" -Platform android
./eng/scripts/run-ui-tests-for-category.ps1 -CategoryGroup "Border,BoxView,Brush,Button" -Platform android
# ... continue with all groups
```

**Note:** This will take 4+ hours. Only do this if absolutely necessary.

### Custom Category Combinations

Create your own category combinations:

```bash
# Text input controls
./eng/scripts/run-ui-tests-for-category.ps1 -CategoryGroup "Entry,Editor,SearchBar" -Platform android

# Collection controls
./eng/scripts/run-ui-tests-for-category.ps1 -CategoryGroup "CollectionView,CarouselView,ListView" -Platform android

# Navigation controls
./eng/scripts/run-ui-tests-for-category.ps1 -CategoryGroup "Shell,Navigation,TabbedPage,FlyoutPage" -Platform android
```

### Continuous Testing During Development

Use a watch-like pattern (requires manual script):

```bash
#!/bin/bash
# watch-and-test.sh

while true; do
    clear
    echo "Running Button tests..."
    ./eng/scripts/run-ui-tests-for-category.ps1 -Category Button -Platform android
    echo "Tests complete. Waiting 60 seconds..."
    sleep 60
done
```

## Output and Results

### Console Output

The script provides detailed progress:
```
=== UI Test Category Runner ===
Running tests for category: Button
Platform: android
Configuration: Release
Test Filter: TestCategory=Button

Checking if HostApp is built...
HostApp found at: artifacts/bin/Controls.TestCases.HostApp/Release/net10.0-android

=== Running Tests ===

[Test execution output...]

✅ Tests completed successfully!

Test results: /Users/you/maui/test-results
Build logs: /Users/you/maui/artifacts/log
```

### Test Results Location

- **Test results (TRX):** `test-results/*.trx`
- **Build logs:** `artifacts/log/*.binlog`
- **Screenshots:** `test-results/screenshots/` (if tests failed)

### Viewing Results

```bash
# View test results summary
cat test-results/*.trx | grep -E "(passed|failed)"

# Open in VS Code
code test-results/

# View binlog (requires dotnet-binlog tool)
dotnet tool install -g dotnet-binlog
dotnet binlog test-results/*.binlog
```

## CI Integration

This script is designed for **local development**. For CI:

- Use the intelligent pipeline: `eng/pipelines/common/ui-tests-intelligent.yml`
- CI automatically analyzes PRs and runs affected categories
- This script can simulate CI behavior locally with `-PrNumber`

## Best Practices

1. **Run locally before pushing** - Catch failures early
2. **Test on primary platform first** - Usually Android or iOS
3. **Use category groups** - More efficient than individual categories
4. **Leverage PR analysis** - Let the tool decide what to run
5. **Keep HostApp built** - Saves time on repeated test runs
6. **Monitor test results** - Check TRX files for detailed failures

## Related Documentation

- [Intelligent Test Execution](../pipelines/INTELLIGENT-TEST-EXECUTION.md)
- [UI Testing Guide](../../.github/instructions/uitests.instructions.md)
- [UITestCategories.cs](../../src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs)

## Quick Reference

```bash
# List categories
./eng/scripts/run-ui-tests-for-category.ps1 -ListCategories

# Run single category
./eng/scripts/run-ui-tests-for-category.ps1 -Category Button -Platform android

# Run multiple categories
./eng/scripts/run-ui-tests-for-category.ps1 -CategoryGroup "Button,Label" -Platform android

# Run based on PR
./eng/scripts/run-ui-tests-for-category.ps1 -PrNumber 12345 -Platform android

# Different platforms
-Platform android    # Default
-Platform ios        # Requires macOS + Xcode
-Platform windows    # Requires Windows
-Platform catalyst   # Requires macOS
```
