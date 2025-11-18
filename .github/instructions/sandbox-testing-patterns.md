---
description: "Sandbox app testing patterns for manual PR validation and issue reproduction"
---

# Sandbox Testing Patterns

Common patterns for testing with the Sandbox app. This is specifically for **manual testing** and **PR validation**.

## BuildAndRunSandbox.ps1 Script

**CRITICAL**: For all Sandbox app testing and reproduction work, use this script instead of manual commands.

**Script Location**: `.github/scripts/BuildAndRunSandbox.ps1`

**Usage**:
```powershell
# Android
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform Android

# iOS  
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform iOS
```

**What the script handles**:
- Device detection, boot, and UDID extraction
- App building (always fresh build)
- App installation and deployment
- Appium server management (auto-start/stop)
- Running Appium test (`CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs`)
- Complete log capture to `CustomAgentLogsTmp/Sandbox/` directory:
  - `appium.log` - Appium server logs
  - `android-device.log` or `ios-device.log` - Device logs filtered to Sandbox app
  - `RunWithAppiumTest.cs` - Your test script (preserved after run)

**Requirements**:
- Must have `CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs` file (use `.github/scripts/templates/RunWithAppiumTest.template.cs` as starting point)

**When to use**:
- ✅ Issue reproduction with Sandbox app
- ✅ Manual testing and debugging
- ✅ PR validation with custom UI scenarios

---

## Cleanup Patterns

### Sandbox App Cleanup
```bash
# Revert all changes to Sandbox app
git checkout -- src/Controls/samples/Controls.Sample.Sandbox/
```

### Sandbox Test Files Cleanup
```bash
# Remove Appium test directory (gitignored)
rm -rf CustomAgentLogsTmp/Sandbox/
```

---

## Log Capture and Review

### Where Logs Are Saved

After running BuildAndRunSandbox.ps1, all logs are in `CustomAgentLogsTmp/Sandbox/`:

1. **Android**: `CustomAgentLogsTmp/Sandbox/android-device.log`
2. **iOS**: `CustomAgentLogsTmp/Sandbox/ios-device.log`
3. **Appium**: `CustomAgentLogsTmp/Sandbox/appium.log`

### Viewing Logs

```bash
# View device logs
cat CustomAgentLogsTmp/Sandbox/android-device.log
# or
cat CustomAgentLogsTmp/Sandbox/ios-device.log

# Search for specific output
grep "TEST OUTPUT" CustomAgentLogsTmp/Sandbox/android-device.log

# View Appium logs
cat CustomAgentLogsTmp/Sandbox/appium.log
```

---

## Common Error Handling Patterns

### App Crashes on Launch

**Android**:
```bash
# Check crash logs
grep -E "(FATAL|AndroidRuntime|Exception)" CustomAgentLogsTmp/Sandbox/android-device.log

# Look for the actual exception
adb logcat | grep -A 20 "FATAL"
```

**iOS**:
```bash
# Capture crash log
xcrun simctl spawn booted log stream --predicate 'processImagePath contains "Sandbox"' --level=debug > /tmp/crash.log 2>&1 &
LOG_PID=$!

# Launch app manually to trigger crash
xcrun simctl launch $UDID com.microsoft.maui.sandbox

# Wait and review
sleep 3
kill $LOG_PID
cat /tmp/crash.log | grep -A 20 "Exception"
```

### PublicAPI Analyzer Failures

**Don't turn off analyzers** - Fix the PublicAPI.Unshipped.txt files properly:

```bash
# Use dotnet format to fix analyzer issues
dotnet format analyzers Microsoft.Maui.slnx

# If that doesn't work, revert and manually add entries
git checkout -- src/*/PublicAPI.Unshipped.txt
# Then add only the new public APIs
```

### Appium Server Issues

**"Appium server not started" error**:
```bash
# Kill existing Appium processes
lsof -i :4723 | grep LISTEN | awk '{print $2}' | xargs kill -9

# Verify port is free
lsof -i :4723
```

**If you see Appium errors**: Check the `appium.log` file in `CustomAgentLogsTmp/Sandbox/` directory.

---

## Platform-Specific Patterns

### Android Device Detection
```bash
# Get connected device
export DEVICE_UDID=$(adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1)

# Verify device
echo "Using device: $DEVICE_UDID"
```

### iOS Simulator Detection
```bash
# Find iPhone Xs with highest iOS version
UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')

# Boot simulator
xcrun simctl boot $UDID 2>/dev/null || true
```

---

## Related Documentation

- [Instrumentation Guide](instrumentation.md) - How to add logging and measurements to Sandbox app
- [Appium Control Scripts](appium-control.md) - UI automation with Appium
- [SafeArea Testing](safearea-testing.md) - Testing SafeArea-specific issues
