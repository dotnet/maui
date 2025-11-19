---
description: "Common testing patterns for command sequences used across .NET MAUI AI agent instructions"
---

# Common Testing Patterns

This document consolidates recurring command patterns used across multiple instruction files. Reference these patterns instead of duplicating them.

## 1. UDID Extraction Patterns

### iOS Simulator UDID (iPhone Xs, Highest iOS Version)

**Used in**: `pr-reviewer.md`, `uitests.instructions.md`, `instrumentation.instructions.md`, `appium-control.instructions.md`

**Pattern**:
```bash
UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')

# Check UDID was found
if [ -z "$UDID" ] || [ "$UDID" = "null" ]; then
    echo "❌ ERROR: No iPhone Xs simulator found. Please create one."
    exit 1
fi

echo "Using simulator: iPhone Xs (UDID: $UDID)"
```

**When to use**: Default iOS testing on iPhone Xs with latest iOS version

---

### iOS Simulator UDID (Specific iOS Version)

**Used in**: `uitests.instructions.md`

**Pattern**:
```bash
# User specifies iOS version (e.g., "26.0") and device name
IOS_VERSION="${IOS_VERSION:-}"
DEVICE_NAME="${DEVICE_NAME:-iPhone Xs}"

if [ -z "$IOS_VERSION" ]; then
    echo "❌ ERROR: IOS_VERSION not set"
    exit 1
fi

IOS_VERSION_FILTER="iOS-${IOS_VERSION//./-}"
UDID=$(xcrun simctl list devices available --json | jq -r --arg filter "$IOS_VERSION_FILTER" --arg device "$DEVICE_NAME" '.devices | to_entries | map(select(.key | contains($filter))) | map(.value) | flatten | map(select(.name == $device)) | first | .udid')

if [ -z "$UDID" ] || [ "$UDID" = "null" ]; then
    echo "❌ ERROR: No $DEVICE_NAME simulator found for iOS $IOS_VERSION"
    exit 1
fi

echo "Using simulator: $DEVICE_NAME iOS $IOS_VERSION (UDID: $UDID)"
```

**When to use**: Testing on specific iOS version

---

### Android Device UDID

**Used in**: `pr-reviewer.md`, `uitests.instructions.md`, `instrumentation.instructions.md`, `appium-control.instructions.md`

**Pattern**:
```bash
export DEVICE_UDID=$(adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1)

# Check device was found
if [ -z "$DEVICE_UDID" ]; then
    echo "❌ ERROR: No Android device/emulator found. Start an emulator or connect a device."
    exit 1
fi

echo "Using Android device: $DEVICE_UDID"
```

**When to use**: Any Android testing

---

## 2. Device Boot Patterns

### iOS Simulator Boot with Error Checking

**Used in**: `pr-reviewer.md`, `uitests.instructions.md`, `instrumentation.instructions.md`

**Pattern**:
```bash
# Requires: $UDID already set
# Boot simulator (error if already booted is OK)
xcrun simctl boot $UDID 2>/dev/null || true

# Check simulator is booted
STATE=$(xcrun simctl list devices --json | jq -r --arg udid "$UDID" '.devices[][] | select(.udid == $udid) | .state')
if [ "$STATE" != "Booted" ]; then
    echo "❌ ERROR: Simulator failed to boot. Current state: $STATE"
    exit 1
fi

echo "Simulator is booted and ready"
```

**When to use**: Before installing/launching iOS apps

---

### Android Emulator Startup with Error Checking

**Used in**: PR reviews, investigation work

**Pattern**:
```bash
# Clean up any existing emulator processes
pkill -9 qemu-system-x86_64 2>/dev/null || true
pkill -9 emulator 2>/dev/null || true
sleep 2

# CRITICAL: Start emulator as background daemon that survives session end
# Use subshell with & to fully detach from current session
cd $ANDROID_HOME/emulator && (./emulator -avd Pixel_9 -no-snapshot-load -no-audio -no-boot-anim > /tmp/emulator.log 2>&1 &)

# Wait a moment for process to start
sleep 3

# Verify emulator process started
EMULATOR_PID=$(ps aux | grep "qemu.*Pixel_9" | grep -v grep | awk '{print $2}')
if [ -z "$EMULATOR_PID" ]; then
    echo "❌ ERROR: Emulator failed to start"
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

**When to use**: Starting Android emulator for testing

**Why this pattern is critical**:

The subshell `()` with `&` pattern is essential for emulator persistence:
- **Problem**: Using `mode="async"` attaches the emulator to the bash session, causing it to be killed when the session ends
- **Root cause**: Background processes attached to sessions are terminated during cleanup, even with `nohup`
- **Solution**: Subshell `()` creates a new process group that's detached from the current session
- **Result**: Emulator persists even when AI agent finishes responding or sessions end

**Wrong approach (emulator dies)**:
```bash
# DON'T DO THIS - emulator will be killed when session ends
bash --mode=async ./emulator -avd Pixel_9 &
```

**Correct approach (emulator persists)**:
```bash
# Subshell with & fully detaches the process
cd $ANDROID_HOME/emulator && (./emulator -avd Pixel_9 ... &)
```

**Critical details**: 
- The emulator command must be run from `$ANDROID_HOME/emulator` directory. Running from other directories causes "Qt library not found" and "qemu-system not found" errors
- **Use subshell `()` with `&`** to start emulator as true background daemon that persists after bash session ends
- **NEVER use `adb kill-server`** - This disconnects ALL active ADB connections and causes emulators to lose connection. Almost never necessary
- **NEVER use `mode="async"` for emulators** - Processes will be terminated when the session ends
- **Check first**: Run `adb devices` before starting - if device is already visible, no action needed

**Available emulators**: List with `emulator -list-avds`

**To verify persistence**: After starting emulator, note the PID, finish the current task, then check if the process still exists with `ps aux | grep <PID>`

---

## 3. Build Patterns

### Sandbox App Build (iOS)

**Used in**: `pr-reviewer.md`, `instrumentation.instructions.md`, `appium-control.instructions.md`

**Pattern**:
```bash
# Build (use --no-incremental only if app crashes on launch)
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-ios

# Check build succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build failed"
    exit 1
fi

echo "Build successful"
```

**When to use**: Building Sandbox app for iOS testing

**Troubleshooting**: If app crashes on launch, rebuild with `--no-incremental` flag

### Sandbox App Build and Deploy (Android)

**Used in**: `appium-control.instructions.md`

**Pattern**:
```bash
# Requires: $DEVICE_UDID already set
# Build, install, and launch (the -t:Run target does all three)
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-android -t:Run

# Check build succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build/deploy failed"
    exit 1
fi

echo "Build and deploy successful"

# Verify app is running
sleep 3
if adb -s $DEVICE_UDID shell pidof com.microsoft.maui.sandbox > /dev/null; then
    echo "✅ App is running"
else
    echo "❌ App failed to start"
    exit 1
fi
```

**When to use**: Building and deploying Sandbox app for Android testing

**Why `-t:Run`**: On Android, use the `Run` target which builds, installs, and launches the app in one command

**Troubleshooting**: If app crashes on launch, rebuild with `--no-incremental` flag

---

### Sandbox App Build (Android)

**Used in**: `pr-reviewer.md`, `instrumentation.instructions.md`

**Pattern**:
```bash
# Build and deploy (use --no-incremental only if app crashes on launch)
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-android -t:Run

# Check build/deploy succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build or deployment failed"
    exit 1
fi

echo "Build successful and deployed"
```

**When to use**: Building and deploying Sandbox app for Android testing

**Troubleshooting**: If app crashes on launch, rebuild with `--no-incremental` flag

---

### TestCases.HostApp Build (iOS)

**Used in**: `uitests.instructions.md`

**Pattern**:
```bash
# Use local dotnet if available, otherwise global
DOTNET_CMD="./bin/dotnet/dotnet"
if [ ! -f "$DOTNET_CMD" ]; then
    DOTNET_CMD="dotnet"
fi

$DOTNET_CMD build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-ios

# Check build succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build failed"
    exit 1
fi

echo "Build successful"
```

**When to use**: Building TestCases.HostApp for automated UI tests

**Troubleshooting**: If app crashes on launch, rebuild with `--no-incremental` flag

---

### TestCases.HostApp Build (Android)

**Used in**: `uitests.instructions.md`

**Pattern**:
```bash
# Use local dotnet if available, otherwise global
DOTNET_CMD="./bin/dotnet/dotnet"
if [ ! -f "$DOTNET_CMD" ]; then
    DOTNET_CMD="dotnet"
fi

$DOTNET_CMD build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run

# Check build/deploy succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build or deployment failed"
    exit 1
fi

echo "Build successful and deployed"
```

**When to use**: Building and deploying TestCases.HostApp for Android automated UI tests

---

## 4. App Installation Patterns

### iOS App Install with Error Checking

**Used in**: `pr-reviewer.md`, `uitests.instructions.md`, `instrumentation.instructions.md`

**Pattern**:
```bash
# Requires: $UDID already set, simulator booted
APP_PATH="${APP_PATH:-artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-ios/iossimulator-arm64/Maui.Controls.Sample.Sandbox.app}"

# Install
xcrun simctl install $UDID "$APP_PATH"

# Check install succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: App installation failed"
    exit 1
fi

echo "App installed successfully"
```

**When to use**: Installing iOS apps to simulator

---

## 5. App Launch Patterns

### iOS App Launch with Console Capture

**Used in**: `pr-reviewer.md`, `instrumentation.instructions.md`

**Pattern**:
```bash
# Requires: $UDID, $BUNDLE_ID set, app already installed
LOG_FILE="${LOG_FILE:-/tmp/ios_test.log}"
BUNDLE_ID="${BUNDLE_ID:-com.microsoft.maui.sandbox}"

# Launch with console capture
xcrun simctl launch --console-pty $UDID "$BUNDLE_ID" > "$LOG_FILE" 2>&1 &

# Check launch didn't immediately fail
if [ $? -ne 0 ]; then
    echo "❌ ERROR: App launch failed"
    exit 1
fi

# Wait for app to start
sleep 8

# Show output
cat "$LOG_FILE"
```

**When to use**: Launching iOS app and capturing console output for instrumentation

---

### Android Logcat Monitoring

**Used in**: `pr-reviewer.md`, `instrumentation.instructions.md`

**Pattern**:
```bash
# Monitor logs for specific markers
MARKER="${MARKER:-TEST OUTPUT}"
adb logcat | grep -E "($MARKER|Console|FATAL|AndroidRuntime)"
```

**When to use**: Monitoring Android console output during testing

---

## 6. Cleanup Patterns

### Sandbox App Cleanup

**Used in**: `pr-reviewer.md`, `instrumentation.instructions.md`

**Pattern**:
```bash
# Revert all changes to Sandbox app
git checkout -- src/Controls/samples/Controls.Sample.Sandbox/
```

**When to use**: After testing with Sandbox app

---

### Test Branch Cleanup

**Used in**: `pr-reviewer.md`

**Pattern**:
```bash
# Requires: $ORIGINAL_BRANCH set at start of review
# Return to original branch
git checkout $ORIGINAL_BRANCH

# Delete test branches
git branch -D test-pr-* baseline-test pr-*-temp 2>/dev/null || true
```

**When to use**: After completing PR review testing

---

## 7. Error Recovery Patterns

### Build Failure Recovery

**Used in**: `copilot-instructions.md`, `pr-reviewer.md`

**Pattern**:
```bash
# Clean and retry build
dotnet clean [project-path]
rm -rf bin/ obj/
dotnet tool restore --force
dotnet build [project-path] --verbosity normal --no-incremental

# If still failing, check for:
# - Wrong .NET SDK version (check global.json)
# - Missing build tasks (build Microsoft.Maui.BuildTasks.slnf first)
# - Platform SDK issues (check Android SDK, Xcode, etc.)
```

**When to use**: When initial build fails

---

## 8. References in Other Files

When these patterns are used in instruction files, reference them like this:

```markdown
**iOS Simulator Setup**: See [Common Testing Patterns: UDID Extraction (iOS)](common-testing-patterns.md#ios-simulator-udid-iphone-xs-highest-ios-version)

**Device Boot**: See [Common Testing Patterns: Device Boot](common-testing-patterns.md#ios-simulator-boot-with-error-checking)
```

This keeps instructions DRY while maintaining readability.

---

**Last Updated**: 2025-11-12

**Note**: These patterns include error checking at every critical step to ensure AI agents can detect and handle failures early.
