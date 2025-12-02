---
description: "Common testing patterns and scripts for .NET MAUI AI agent testing workflows"
---

# Common Testing Patterns

This document consolidates testing patterns and scripts used across AI agent instructions.

## 1. BuildAndRunSandbox.ps1 (Sandbox App Testing)

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
- Must have `CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs` file (use `.github/scripts/RunWithAppiumTest.template.cs` as starting point)

**When to use**:
- ✅ Issue reproduction with Sandbox app
- ✅ Manual testing and debugging
- ✅ PR validation with custom UI scenarios
- ❌ NOT for automated UI tests (use BuildAndRunHostApp.ps1)

---

## 2. BuildAndRunHostApp.ps1 (Automated UI Tests)

**CRITICAL**: For running automated UI tests from TestCases.HostApp, use this script.

**Script Location**: `.github/scripts/BuildAndRunHostApp.ps1`

**Usage**:
```powershell
# Run specific test by name
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform Android -TestFilter "FullyQualifiedName~Issue12345"

# Run tests by category
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform iOS -TestFilter "Category=Button"
```

**What the script handles**:
- Device detection, boot, and UDID extraction
- HostApp building (always fresh build)
- App installation
- Appium server management (auto-start/stop)
- Running `dotnet test` with specified filter
- Complete log capture to `CustomAgentLogsTmp/UITests/` directory:
  - `appium.log` - Appium server logs
  - `android-device.log` or `ios-device.log` - Device logs filtered to HostApp
  - `test-output.log` - Test execution results

**When to use**:
- ✅ Running automated UI tests
- ✅ Validating test changes
- ✅ PR test validation

---

## 3. Cleanup Patterns

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

### HostApp Test Logs Cleanup
```bash
# Remove test logs directory (gitignored)
rm -rf CustomAgentLogsTmp/UITests/
```

---

## 4. Common Error Handling Patterns

### App Crashes on Launch

**CRITICAL**: When an app crashes, the PowerShell scripts automatically capture all crash logs.

**Workflow**:
1. Run BuildAndRunSandbox.ps1 or BuildAndRunHostApp.ps1
2. Script will capture crash in device logs
3. Read the crash logs:
   - **Android**: `CustomAgentLogsTmp/Sandbox/android-device.log` or `CustomAgentLogsTmp/UITests/android-device.log`
   - **iOS**: `CustomAgentLogsTmp/Sandbox/ios-device.log` or `CustomAgentLogsTmp/UITests/ios-device.log`
4. Find the exception stack trace in the logs
5. Investigate root cause from the exception
6. Fix the underlying issue (null reference, missing resource, etc.)

**Why**: Crashes are caused by actual code issues, not build artifacts. The exception tells you exactly what's wrong.

**DO NOT**: Use `--no-incremental` or `dotnet clean` as first solution for crashes.

---

### PublicAPI Analyzer Failures

**Symptom**:
```
error RS0016: Symbol 'X' is not marked as public API
```

**Solution**:
```bash
# Let analyzers fix PublicAPI.Unshipped.txt files
dotnet format analyzers Microsoft.Maui.sln
```

**Why**: See `.github/copilot-instructions.md` section "PublicAPI.Unshipped.txt File Management"

**NEVER**: Disable the analyzer or add `#pragma warning disable`

---

### Appium Server Issues

**Note**: The PowerShell scripts automatically manage Appium (start if needed, stop if we started it).

**If you see Appium errors**: Check the `appium.log` file in `CustomAgentLogsTmp/Sandbox/` or `CustomAgentLogsTmp/UITests/` directories.

---

## 5. Related Documentation

- **Issue Resolution**: `.github/instructions/issue-resolver-agent/` - Full workflow and guidelines
- **UI Tests**: `.github/instructions/uitests.instructions.md` - Writing automated UI tests
- **Instrumentation**: `.github/instructions/instrumentation.md` - Adding debug instrumentation
- **Troubleshooting**: `.github/copilot-instructions.md` - General development troubleshooting

---

**Last Updated**: 2025-11-24

**Note**: The PowerShell scripts handle all build, deployment, and log capture. Agents should use these scripts instead of manual commands.
