---
description: "Shared platform-specific workflows for iOS, Android, and MacCatalyst reproduction and testing"
---

# Platform-Specific Workflows

This document contains platform-specific workflows shared by both PR reviewer and issue resolver agents. Use these workflows for reproducing issues, building apps, and capturing diagnostics.

---

## Table of Contents

- [iOS Workflows](#ios-workflows)
- [Android Workflows](#android-workflows)
- [MacCatalyst Workflows](#maccatalyst-workflows)
- [Platform Commands Reference](#platform-commands-reference)

---

## iOS Workflows

### Complete iOS Reproduction Workflow

**Use for**: Reproducing issues, testing fixes, validating PRs on iOS

```bash
# 1. Find iPhone Xs with highest iOS version
UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')

# Verify UDID was found
if [ -z "$UDID" ] || [ "$UDID" = "null" ]; then
    echo "❌ ERROR: No iPhone Xs simulator found. Please create one."
    exit 1
fi

echo "✅ Using iPhone Xs: $UDID"

# 2. Boot simulator
xcrun simctl boot $UDID 2>/dev/null || true

# 3. Verify booted
STATE=$(xcrun simctl list devices --json | jq -r --arg udid "$UDID" '.devices[][] | select(.udid == $udid) | .state')
if [ "$STATE" != "Booted" ]; then
    echo "❌ ERROR: Simulator not booted. State: $STATE"
    exit 1
fi

echo "✅ Simulator is booted and ready"

# 4. Build app (choose one based on your needs)
# For Sandbox app:
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-ios

# For HostApp (UI tests):
# dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-ios

if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build failed"
    exit 1
fi

echo "✅ Build successful"

# 5. Install app (adjust path based on which app you built)
# For Sandbox:
xcrun simctl install $UDID artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-ios/iossimulator-arm64/Maui.Controls.Sample.Sandbox.app

# For HostApp:
# xcrun simctl install $UDID artifacts/bin/Controls.TestCases.HostApp/Debug/net10.0-ios/iossimulator-arm64/Controls.TestCases.HostApp.app

if [ $? -ne 0 ]; then
    echo "❌ ERROR: App installation failed"
    exit 1
fi

echo "✅ App installed successfully"

# 6. Launch with console capture (choose bundle ID based on app)
# For Sandbox:
xcrun simctl launch --console-pty $UDID com.microsoft.maui.sandbox > /tmp/ios_test.log 2>&1 &

# For HostApp:
# xcrun simctl launch --console-pty $UDID com.microsoft.maui.uitests > /tmp/ios_test.log 2>&1 &

# 7. Wait and show output
sleep 8
cat /tmp/ios_test.log

# 8. Filter for test markers (if using instrumentation)
grep "TEST OUTPUT" /tmp/ios_test.log
```

---

### iOS Simulator Error Monitoring

**Use for**: Capturing crashes and error logs during testing

```bash
# Requires: $UDID already set

# Start log monitoring (run in separate terminal or background)
xcrun simctl spawn $UDID log stream \
  --predicate 'processImagePath endswith "Maui.Controls.Sample.Sandbox"' \
  --level=error > /tmp/ios_errors.log 2>&1 &

LOG_PID=$!
echo "✅ Log monitoring started (PID: $LOG_PID)"

# ... run your tests ...

# Stop log monitoring
kill $LOG_PID

# View errors
cat /tmp/ios_errors.log
```

---

### iOS Simulator Cleanup

```bash
# Requires: $UDID already set

# Uninstall app
xcrun simctl uninstall $UDID com.microsoft.maui.sandbox

# Shutdown simulator
xcrun simctl shutdown $UDID

# Erase simulator (full reset - use with caution)
xcrun simctl erase $UDID
```

---

## Android Workflows

### Complete Android Reproduction Workflow

**Use for**: Reproducing issues, testing fixes, validating PRs on Android

```bash
# 1. Check for connected device/emulator
export DEVICE_UDID=$(adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1)

if [ -z "$DEVICE_UDID" ]; then
    echo "❌ ERROR: No Android device/emulator found"
    exit 1
fi

echo "✅ Using Android device: $DEVICE_UDID"

# 2. Build and deploy app (choose one based on your needs)
# For Sandbox app (Run target builds, installs, and launches):
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj \
  -f net10.0-android -t:Run

# For HostApp (UI tests):
# dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj \
#   -f net10.0-android -t:Run

if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build/deploy failed"
    exit 1
fi

echo "✅ Build and deploy successful"

# 3. Monitor logs (in separate terminal or background)
adb logcat | grep -E "(TEST OUTPUT|Console|FATAL|AndroidRuntime)" > /tmp/android_test.log 2>&1 &
LOGCAT_PID=$!

echo "✅ Logcat monitoring started (PID: $LOGCAT_PID)"

# ... run your tests ...

# Stop logcat
kill $LOGCAT_PID

# View output
cat /tmp/android_test.log | grep "TEST OUTPUT"
```

---

### Android Emulator Startup

**Use when**: No emulator is running and you need to start one

**⚠️ CRITICAL**: This pattern ensures emulator persists after session ends

```bash
# Clean up any existing emulator processes
pkill -9 qemu-system-x86_64 2>/dev/null || true
pkill -9 emulator 2>/dev/null || true
sleep 2

# CRITICAL: Use subshell with & to fully detach from session
# This ensures emulator survives when agent finishes
cd $ANDROID_HOME/emulator && (./emulator -avd Pixel_9 -no-snapshot-load -no-audio -no-boot-anim > /tmp/emulator.log 2>&1 &)

# Wait for process to start
sleep 3

# Verify emulator process started
EMULATOR_PID=$(ps aux | grep "qemu.*Pixel_9" | grep -v grep | awk '{print $2}')
if [ -z "$EMULATOR_PID" ]; then
    echo "❌ ERROR: Emulator failed to start"
    cat /tmp/emulator.log
    exit 1
fi

echo "✅ Emulator started as background daemon (PID: $EMULATOR_PID)"

# Wait for device to appear
echo "Waiting for device to appear..."
adb wait-for-device

# Wait for boot to complete
echo "Waiting for boot to complete..."
until [ "$(adb shell getprop sys.boot_completed 2>/dev/null)" = "1" ]; do
    sleep 2
    echo -n "."
done
echo ""

# Get device UDID
export DEVICE_UDID=$(adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1)

# Verify device is ready
if [ -z "$DEVICE_UDID" ]; then
    echo "❌ ERROR: Emulator started but device not found"
    exit 1
fi

# Check API level
API_LEVEL=$(adb shell getprop ro.build.version.sdk)
echo "✅ Emulator ready: $DEVICE_UDID (API $API_LEVEL)"
```

**Why this pattern is critical**:
- **Subshell `()` with `&`** creates detached process that survives session end
- **Must run from `$ANDROID_HOME/emulator`** to avoid "Qt library not found" errors
- **NEVER use `adb kill-server`** - Disconnects ALL active ADB connections

**Available emulators**: List with `emulator -list-avds`

---

### Android Logcat Filtering

**Common patterns for filtering logcat output:**

```bash
# Filter for specific app only
adb logcat | grep "com.microsoft.maui"

# Filter for errors and crashes
adb logcat | grep -E "(FATAL|AndroidRuntime|Exception|Error|Crash)"

# Filter for your test markers
adb logcat | grep "TEST OUTPUT"

# Multiple filters
adb logcat | grep -E "(TEST OUTPUT|Console|FATAL)"

# Save to file and monitor
adb logcat > /tmp/android.log 2>&1 &
LOGCAT_PID=$!
# ... run tests ...
kill $LOGCAT_PID
cat /tmp/android.log | grep "TEST OUTPUT"
```

---

### Android Cleanup

```bash
# Requires: $DEVICE_UDID already set

# Uninstall app
adb -s $DEVICE_UDID uninstall com.microsoft.maui.sandbox

# Clear app data (if keeping app installed)
adb -s $DEVICE_UDID shell pm clear com.microsoft.maui.sandbox

# Stop app
adb -s $DEVICE_UDID shell am force-stop com.microsoft.maui.sandbox
```

---

## MacCatalyst Workflows

### Complete MacCatalyst Reproduction Workflow

**Use for**: Reproducing issues, testing fixes, validating PRs on MacCatalyst

```bash
# 1. Build and run app (choose one based on your needs)
# For Sandbox app:
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj \
  -f net10.0-maccatalyst -t:Run

# For HostApp (UI tests):
# dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj \
#   -f net10.0-maccatalyst -t:Run

if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build/run failed"
    exit 1
fi

echo "✅ Build successful, app should launch automatically"

# 2. Monitor logs in Console.app
# Open Console.app and filter for your app name
# Or use log command:
log stream --predicate 'processImagePath contains "Maui.Controls.Sample.Sandbox"' --level=error
```

---

## Platform Commands Reference

### UDID Extraction Commands

**iOS (iPhone Xs, highest iOS version):**
```bash
UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')
```

**iOS (specific iOS version):**
```bash
IOS_VERSION="18.0"  # Set your desired version
DEVICE_NAME="iPhone Xs"
IOS_VERSION_FILTER="iOS-${IOS_VERSION//./-}"

UDID=$(xcrun simctl list devices available --json | jq -r --arg filter "$IOS_VERSION_FILTER" --arg device "$DEVICE_NAME" '.devices | to_entries | map(select(.key | contains($filter))) | map(.value) | flatten | map(select(.name == $device)) | first | .udid')
```

**Android:**
```bash
export DEVICE_UDID=$(adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1)
```

---

### Device State Verification

**iOS - Check simulator state:**
```bash
STATE=$(xcrun simctl list devices --json | jq -r --arg udid "$UDID" '.devices[][] | select(.udid == $udid) | .state')
echo "Simulator state: $STATE"
# Expected: "Booted"
```

**Android - Check device state:**
```bash
adb devices
# Should show "device" not "offline"

# Check boot completed
adb shell getprop sys.boot_completed
# Should return "1"
```

---

### Build Commands Summary

| Platform | Sandbox App | HostApp |
|----------|-------------|---------|
| **iOS** | `dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-ios` | `dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-ios` |
| **Android** | `dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-android -t:Run` | `dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run` |
| **MacCatalyst** | `dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-maccatalyst -t:Run` | `dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-maccatalyst -t:Run` |

**Note**: Android and MacCatalyst use `-t:Run` which builds, installs, and launches in one command

---

### Installation Paths

**iOS Sandbox:**
```
artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-ios/iossimulator-arm64/Maui.Controls.Sample.Sandbox.app
```

**iOS HostApp:**
```
artifacts/bin/Controls.TestCases.HostApp/Debug/net10.0-ios/iossimulator-arm64/Controls.TestCases.HostApp.app
```

---

### Bundle IDs

| App | Bundle ID |
|-----|-----------|
| Sandbox | `com.microsoft.maui.sandbox` |
| HostApp | `com.microsoft.maui.uitests` |

---

## Quick Reference Table

| Operation | iOS | Android | MacCatalyst |
|-----------|-----|---------|-------------|
| **List devices** | `xcrun simctl list devices` | `adb devices` | N/A (runs on host Mac) |
| **Boot/start** | `xcrun simctl boot $UDID` | Start emulator or connect device | N/A |
| **Install app** | `xcrun simctl install $UDID path.app` | Done by `-t:Run` | Done by `-t:Run` |
| **Launch app** | `xcrun simctl launch $UDID bundle.id` | Done by `-t:Run` | Done by `-t:Run` |
| **View logs** | `xcrun simctl spawn $UDID log stream` | `adb logcat` | `log stream` or Console.app |
| **Uninstall** | `xcrun simctl uninstall $UDID bundle.id` | `adb uninstall bundle.id` | Manually delete from Applications |

---

## Related Documentation

- [Common Testing Patterns](../common-testing-patterns.md) - Additional patterns with error handling
- [Instrumentation Guide](../instrumentation.instructions.md) - Adding console logging for debugging
- [Error Handling](../shared/error-handling-common.md) - Troubleshooting build and deploy errors

---

**Last Updated**: November 2025
