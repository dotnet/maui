---
description: "Common error handling patterns shared across PR reviewer and issue resolver agents"
---

# Common Error Handling

This document contains error handling patterns that are common to both PR reviewing and issue resolution workflows.

**Agent-specific errors**: See agent-specific error-handling.md files for unique issues

---

## Table of Contents

- [Build Errors](#build-errors)
- [Deployment Errors](#deployment-errors)
- [Runtime Errors](#runtime-errors)
- [Testing Errors](#testing-errors)
- [Recovery Patterns](#recovery-patterns)

---

## Build Errors

### Error: Build Tasks Not Found

**Symptom**:
```
error: Could not find Microsoft.Maui.Resizetizer.BuildTasks
error: Task 'Something' not found
```

**Cause**: Build tasks project hasn't been compiled

**Solution**:
```bash
# Rebuild build tasks
dotnet build ./Microsoft.Maui.BuildTasks.slnf

# Check build succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build tasks compilation failed"
    exit 1
fi

echo "✅ Build tasks compiled successfully"

# Now retry your original build
dotnet build [your-project-path]
```

---

### Error: Dependency Version Conflicts

**Symptom**:
```
error: Package version conflict detected
error: Unable to resolve dependencies
error NU1107: Version conflict detected
```

**Cause**: Cached packages or corrupted dependencies

**Solution**:
```bash
# Remove build artifacts
rm -rf bin/ obj/

# Force restore dependencies
dotnet restore Microsoft.Maui.sln --force

# Check restore succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Dependency restore failed"
    exit 1
fi

echo "✅ Dependencies restored"

# Retry build
dotnet build [your-project-path]
```

---

### Error: PublicAPI Analyzer Failures

**Symptom**:
```
error RS0016: Symbol 'MyNewMethod' is not marked as public API
error RS0016: Symbol 'NewProperty' should be removed from PublicAPI.Unshipped.txt
```

**Cause**: Public API changes not reflected in PublicAPI.Unshipped.txt

**Solution - Use dotnet format analyzers (RECOMMENDED)**:
```bash
# Let analyzers fix PublicAPI.Unshipped.txt files automatically
dotnet format analyzers Microsoft.Maui.sln

# Check if it fixed the issue
dotnet build [your-project-path]

if [ $? -eq 0 ]; then
    echo "✅ PublicAPI errors fixed"
else
    echo "⚠️ Some errors remain, see solution below"
fi
```

**Solution - Manual fix if needed**:
```bash
# If dotnet format doesn't work, revert and re-add
git checkout -- **/PublicAPI.Unshipped.txt

# Run format analyzers again
dotnet format analyzers Microsoft.Maui.sln

# Build to verify
dotnet build [your-project-path]
```

**⚠️ NEVER**:
- Disable the analyzer (`#pragma warning disable RS0016`)
- Add `<NoWarn>RS0016</NoWarn>` to project file
- Manually edit PublicAPI.Unshipped.txt without understanding the changes

**Why**: PublicAPI files track breaking changes and are critical for .NET versioning

**Reference**: `.github/copilot-instructions.md` section "PublicAPI.Unshipped.txt File Management"

---

### Error: Platform SDK Not Found

#### iOS - Xcode Not Found

**Symptom**:
```
error: Unable to find Xcode
error: xcode-select: error: tool 'xcodebuild' requires Xcode
```

**Solution**:
```bash
# Check Xcode installation
xcode-select --print-path

# If not found, install Xcode command line tools
xcode-select --install

# Or install full Xcode from App Store

# Verify installation
xcodebuild -version
```

#### Android - SDK Not Found

**Symptom**:
```
error: ANDROID_HOME not set
error: Unable to find Android SDK
```

**Solution**:
```bash
# Check ANDROID_HOME environment variable
echo $ANDROID_HOME

# If not set, find Android SDK location
# Usually: ~/Library/Android/sdk (Mac) or ~/Android/Sdk (Linux)

# Set environment variable
export ANDROID_HOME=/path/to/android-sdk

# Verify
echo $ANDROID_HOME

# Or use Android SDK Manager
android  # Opens SDK Manager UI
```

#### Windows - SDK Not Found

**Symptom**:
```
error: Windows SDK not found
error: Could not find Windows.winmd
```

**Solution**:
- Install Windows SDK from Visual Studio Installer
- Or download standalone: https://developer.microsoft.com/windows/downloads/windows-sdk/

---

### Error: Wrong .NET SDK Version

**Symptom**:
```
error NETSDK1045: The current .NET SDK does not support targeting .NET 10.0
```

**Cause**: Wrong .NET SDK version installed

**Solution**:
```bash
# Check required version
cat global.json | grep -A 1 '"dotnet"'

# Check installed version
dotnet --version

# If mismatch, install correct version:
# - Download from https://dotnet.microsoft.com/download
# - Or use dotnet-install script

# Verify after installation
dotnet --version
```

**Reference**: `.github/copilot-instructions.md` section "Repository Overview"

---

## Deployment Errors

### Error: App Installation Failed (iOS)

**Symptom**:
```
error: Failed to install app to simulator
An error was encountered processing the command (domain=com.apple.CoreSimulator.SimError, code=164)
```

**Cause**: Simulator not booted or app already installed with different signature

**Solution**:
```bash
# Requires: $UDID already set

# Ensure simulator is booted
xcrun simctl boot $UDID 2>/dev/null || true

# Wait a moment for boot
sleep 3

# Verify booted
STATE=$(xcrun simctl list devices --json | jq -r --arg udid "$UDID" '.devices[][] | select(.udid == $udid) | .state')
if [ "$STATE" != "Booted" ]; then
    echo "❌ ERROR: Simulator not booted. State: $STATE"
    exit 1
fi

echo "✅ Simulator is booted"

# Uninstall previous version (if exists)
xcrun simctl uninstall $UDID com.microsoft.maui.sandbox 2>/dev/null || true

# Retry install
xcrun simctl install $UDID [path-to-.app]

if [ $? -eq 0 ]; then
    echo "✅ App installed successfully"
fi
```

---

### Error: App Installation Failed (Android)

**Symptom**:
```
adb: failed to install [app]: INSTALL_FAILED_UPDATE_INCOMPATIBLE
adb: failed to install [app]: INSTALL_FAILED_INSUFFICIENT_STORAGE
```

**Solution - Update Incompatible**:
```bash
# Requires: $DEVICE_UDID already set

# Uninstall previous version
adb -s $DEVICE_UDID uninstall com.microsoft.maui.sandbox

# Check uninstall succeeded
if [ $? -eq 0 ]; then
    echo "✅ Uninstalled previous version"
fi

# Retry installation (use -t:Run to rebuild and install)
dotnet build [project-path] -f net10.0-android -t:Run
```

**Solution - Insufficient Storage**:
```bash
# Clear app data and cache
adb shell pm clear com.microsoft.maui.sandbox

# Or wipe emulator and start fresh
xcrun simctl erase $UDID  # iOS
# or for Android, create new emulator
```

---

### Error: App Crashes on Launch

**Symptom**: App installs successfully but crashes immediately when launched

**Solution**: Read the crash logs to find the exception

#### iOS Crash Logs

```bash
# Requires: $UDID already set

# Capture crash logs
xcrun simctl spawn booted log stream \
  --predicate 'processImagePath contains "Maui.Controls.Sample.Sandbox"' \
  --level=debug > /tmp/ios_crash.log 2>&1 &
LOG_PID=$!

# Try to launch the app
xcrun simctl launch $UDID com.microsoft.maui.sandbox

# Wait for crash
sleep 3

# Stop log capture
kill $LOG_PID

# Find the exception
cat /tmp/ios_crash.log | grep -A 20 -B 5 "Exception"
cat /tmp/ios_crash.log | grep -A 20 -B 5 "FATAL"
```

#### Android Crash Logs

```bash
# Monitor crash logs in real-time
adb logcat | grep -E "(FATAL|AndroidRuntime|Exception|Error|Crash)"

# Or capture to file
adb logcat > /tmp/android_crash.log 2>&1 &
LOGCAT_PID=$!

# Launch app
dotnet build [project-path] -f net10.0-android -t:Run

# Wait for crash
sleep 3

# Stop logcat
kill $LOGCAT_PID

# Review crash
cat /tmp/android_crash.log | grep -A 30 "FATAL EXCEPTION"
cat /tmp/android_crash.log | grep -A 30 "AndroidRuntime"
```

#### Common Crash Causes

**After finding the exception, investigate:**

1. **Null Reference**: Are required objects initialized?
2. **Missing Resources**: Do all resource IDs exist?
3. **Platform API Incompatibility**: Is the code compatible with target OS version?
4. **Threading Issue**: Are UI updates on main thread?
5. **Handler Lifecycle**: Is handler properly connected?

**Next steps**:
- Identify the exact line throwing the exception
- Understand why it's failing (null value? wrong state?)
- Add null checks or initialization as needed
- Test the fix

**What NOT to do**:
- ❌ Clean/rebuild without reading the exception
- ❌ Assume it's a "build cache issue"
- ❌ Skip reading logs and guess at the problem

---

## Runtime Errors

### Error: Console Output Not Captured

#### iOS - No Console Output

**Symptom**: App runs but no Console.WriteLine output appears

**Cause**: Not using `--console-pty` flag

**Solution**:
```bash
# Requires: $UDID already set

# Launch with console capture
xcrun simctl launch --console-pty $UDID com.microsoft.maui.sandbox > /tmp/output.log 2>&1 &

# Wait for app to start
sleep 5

# Check log file has content
if [ ! -s /tmp/output.log ]; then
    echo "⚠️ WARNING: Log file is empty"
else
    echo "✅ Console output captured"
fi

# View output
cat /tmp/output.log
```

#### Android - No Console Output

**Symptom**: App runs but no Console.WriteLine output in logcat

**Cause**: Logcat not started before app launch

**Solution**:
```bash
# Start logcat BEFORE launching app
adb logcat > /tmp/android.log 2>&1 &
LOGCAT_PID=$!

echo "✅ Logcat started (PID: $LOGCAT_PID)"

# Now launch app
dotnet build [project-path] -f net10.0-android -t:Run

# Wait for logging
sleep 5

# Stop logcat
kill $LOGCAT_PID

# Check log
cat /tmp/android.log | grep "YourMarker"
```

---

### Error: Measurements Show Zero or Null

**Symptom**: Layout measurements are 0x0 or properties are null

**Cause**: Measuring before layout completes

**Solution**:
```csharp
// ❌ DON'T measure immediately
private void OnLoaded(object sender, EventArgs e)
{
    // Layout not complete yet!
    Console.WriteLine($"Bounds: {TestElement.Bounds}");  // Might be 0x0
}

// ✅ DO wait for layout
private void OnLoaded(object sender, EventArgs e)
{
    // Wait for layout to complete
    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
    {
        Console.WriteLine($"Bounds: {TestElement.Bounds}");  // Actual values
    });
}
```

**See**: [Instrumentation Instructions](../instrumentation.instructions.md#timing-when-to-measure) for proper timing patterns

---

## Testing Errors

### Error: Appium Server Not Starting

**Symptom**:
```
AppiumServerHasNotBeenStartedLocallyException
System.InvalidOperationException: The HTTP request to the remote WebDriver server timed out
```

**Cause**: Existing Appium process blocking port 4723

**Solution**:
```bash
# Kill existing Appium processes
lsof -i :4723 | grep LISTEN | awk '{print $2}' | xargs kill -9 2>/dev/null

# Verify port is free
lsof -i :4723
# Should show nothing

# Wait a moment
sleep 2

# Start Appium fresh
appium --log-level error &

# Wait for startup
sleep 3

# Verify it's running
curl http://localhost:4723/status

if [ $? -eq 0 ]; then
    echo "✅ Appium server running"
fi
```

**Why**: UITest framework needs to start its own Appium server. Existing processes block it.

**Reference**: `.github/instructions/uitests.instructions.md` - "Prerequisites: Kill Existing Appium Processes"

---

### Error: DEVICE_UDID Not Set

**Symptom**:
```
ERROR: DEVICE_UDID environment variable not set
Test failed: Could not find device
```

**Cause**: Environment variable not exported before running tests

**Solution**:
```bash
# iOS - Set DEVICE_UDID before running tests
export DEVICE_UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')

# Android - Set DEVICE_UDID before running tests
export DEVICE_UDID=$(adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1)

# Verify it's set
echo "DEVICE_UDID: $DEVICE_UDID"

if [ -z "$DEVICE_UDID" ]; then
    echo "❌ ERROR: DEVICE_UDID is empty"
    exit 1
fi

# Now run tests
dotnet test [test-project]
```

---

### Error: Test App Not Installed

**Symptom**:
```
error: App not found on device
error: Could not launch app with bundle ID 'com.microsoft.maui.uitests'
```

**Cause**: TestCases.HostApp not built/installed before running tests

**Solution**:
```bash
# iOS - Build and install HostApp first
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-ios

xcrun simctl install $UDID artifacts/bin/Controls.TestCases.HostApp/Debug/net10.0-ios/iossimulator-arm64/Controls.TestCases.HostApp.app

# Android - Build and install HostApp first
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run

# Now run tests
dotnet test [test-project]
```

---

## Recovery Patterns

### Decision Tree: What to Do When Errors Occur

```
Error occurs during testing
    │
    ├─ Build error?
    │  ├─ Build tasks missing → Rebuild Microsoft.Maui.BuildTasks.slnf
    │  ├─ Dependency conflict → Clean + restore --force
    │  ├─ PublicAPI error → dotnet format analyzers
    │  └─ Platform SDK missing → Install/configure SDK
    │
    ├─ Deployment error?
    │  ├─ Install failed (iOS) → Check simulator booted, uninstall old version
    │  ├─ Install failed (Android) → Uninstall old version, check storage
    │  └─ App crashes → Read crash logs for exception
    │
    ├─ Runtime error?
    │  ├─ No console output → Check --console-pty (iOS) or logcat (Android)
    │  ├─ Zero measurements → Add Dispatcher.DispatchDelayed delay
    │  └─ Unexpected behavior → Add more instrumentation
    │
    └─ Testing error?
       ├─ Appium won't start → Kill existing Appium processes
       ├─ DEVICE_UDID missing → Set environment variable
       └─ Test app missing → Build and install HostApp first
```

---

### Build Failure Recovery Steps

**Step 1**: Try simple fixes
```bash
# Clean and restore
rm -rf bin/ obj/
dotnet restore --force
dotnet build [project-path]
```

**Step 2**: Check build tasks
```bash
dotnet build ./Microsoft.Maui.BuildTasks.slnf
dotnet build [project-path]
```

**Step 3**: Check .NET SDK version
```bash
cat global.json | grep -A 1 '"dotnet"'
dotnet --version
# If mismatch, install correct version
```

**Step 4**: Build with verbose output
```bash
dotnet build [project-path] --verbosity normal
# Look for specific error messages
```

---

### When to Ask for Help

**Stop and ask for guidance if:**

1. **Error persists after 2-3 fix attempts** - Don't waste time
2. **Error message is cryptic** - "Unknown error" or stack traces without context
3. **Platform-specific issue unfamiliar** - E.g., iOS code signing, Android NDK
4. **Suspected infrastructure problem** - .NET SDK corruption, simulator issues
5. **Multiple errors compound** - Fixing one reveals another

**How to ask**:
```markdown
## Error Encountered

**Command that failed**:
```bash
[exact command]
```

**Error output**:
```
[relevant error message with stack trace]
```

**What I've tried**:
1. [First attempt] - [result]
2. [Second attempt] - [result]
3. [Third attempt] - [result]

**Environment**:
- Platform: [iOS/Android/Windows/Mac]
- .NET SDK: [version from `dotnet --version`]
- Device: [simulator/emulator/physical]
- Branch: [current branch name]

**Request**: [What you need help with]
```

---

## Error Handling Principles

1. **Check success after every critical step** - Use `if [ $? -ne 0 ]`
2. **Exit early on errors** - Don't continue if prerequisite failed
3. **Provide clear error messages** - Explain what failed and why
4. **Try simple fixes first** - Clean/rebuild before complex debugging
5. **Know when to ask for help** - 2-3 attempts maximum before escalating
6. **Read error messages carefully** - They usually tell you what's wrong

---

## Related Documentation

- [Platform Workflows](platform-workflows.md) - Complete build/deploy workflows
- [Instrumentation Guide](../instrumentation.instructions.md) - Adding debug logging
- [Common Testing Patterns](../common-testing-patterns.md) - Additional error patterns
- [Copilot Instructions](../../copilot-instructions.md) - Troubleshooting section

**Agent-Specific Error Handling**:
- [PR Reviewer Error Handling](../pr-reviewer-agent/error-handling.md) - Review-specific errors
- [Issue Resolver Error Handling](../issue-resolver-agent/error-handling.md) - Fix development errors

---

**Last Updated**: November 2025
