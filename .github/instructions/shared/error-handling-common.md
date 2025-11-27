---
description: "Common error handling patterns shared across PR reviewer and issue resolver agents"
---

# Common Error Handling

**🚨 CRITICAL**: Most build/deploy/log errors are now handled automatically by PowerShell scripts. This document covers only the errors that still require manual intervention.

---

## Table of Contents

- [Build Errors](#build-errors)
- [App Crash Analysis](#app-crash-analysis)
- [When to Ask for Help](#when-to-ask-for-help)

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
dotnet build ./Microsoft.Maui.BuildTasks.slnf
```

---

### Error: PublicAPI Analyzer Failures

**Symptom**:
```
error RS0016: Symbol 'MyNewMethod' is not marked as public API
error RS0016: Symbol 'NewProperty' should be removed from PublicAPI.Unshipped.txt
```

**Solution**:
```bash
# Let analyzers fix PublicAPI.Unshipped.txt files automatically
dotnet format analyzers Microsoft.Maui.slnx
```

**If that doesn't work**:
```bash
# Revert and re-add
git checkout -- **/PublicAPI.Unshipped.txt
dotnet format analyzers Microsoft.Maui.sln
```

**⚠️ NEVER**:
- Disable the analyzer
- Add `<NoWarn>RS0016</NoWarn>` to project file
- Manually edit PublicAPI.Unshipped.txt without understanding changes

**Reference**: `.github/copilot-instructions.md` section "PublicAPI.Unshipped.txt File Management"

---

## App Crash Analysis

### Reading Crash Logs

**🚨 CRITICAL**: When apps crash, the PowerShell scripts automatically capture all logs. Your job is to READ and ANALYZE them, not rebuild.

#### Sandbox App Crashes

**Log locations** (after running BuildAndRunSandbox.ps1):
- `CustomAgentLogsTmp/Sandbox/appium.log` - Appium server logs
- `CustomAgentLogsTmp/Sandbox/android-device.log` - Android crash logs (filtered to app PID)
- `CustomAgentLogsTmp/Sandbox/ios-device.log` - iOS crash logs (filtered to app bundle)

#### HostApp Test Crashes

**Log locations** (after running BuildAndRunHostApp.ps1):
- `CustomAgentLogsTmp/UITests/appium.log` - Appium server logs
- `CustomAgentLogsTmp/UITests/android-device.log` - Android crash logs
- `CustomAgentLogsTmp/UITests/ios-device.log` - iOS crash logs
- `CustomAgentLogsTmp/UITests/test-output.log` - Test execution output

### Common Crash Causes

**After finding the exception in logs, investigate:**

1. **Null Reference**: Are required objects initialized?
2. **Missing Resources**: Do all resource IDs exist?
3. **Platform API Incompatibility**: Is code compatible with target OS version?
4. **Threading Issue**: Are UI updates on main thread?
5. **Handler Lifecycle**: Is handler properly connected?

**Next steps**:
- Identify the exact line throwing the exception (from stack trace)
- Understand WHY it's failing (null value? wrong state? missing resource?)
- Add null checks or initialization as needed
- Test the fix by running the PS1 script again

**What NOT to do**:
- ❌ Clean/rebuild without reading the exception
- ❌ Assume it's a "build cache issue"
- ❌ Skip reading logs and guess at the problem
- ❌ Use `--no-incremental` or `dotnet clean` as first solution

---

## When to Ask for Help

**Stop and ask for guidance if:**

1. **Error persists after 2-3 fix attempts** - Don't waste time
2. **Error message is cryptic** - "Unknown error" or stack traces without clear cause
3. **Platform-specific issue unfamiliar** - E.g., iOS code signing, Android NDK
4. **Suspected infrastructure problem** - .NET SDK corruption, simulator/emulator issues
5. **Build failures with no clear error** - Verbose output doesn't help

**How to ask**:
```markdown
## Error Encountered

**What I was doing**:
[Running BuildAndRunSandbox.ps1 / BuildAndRunHostApp.ps1 / other command]

**Error output**:
```
[paste relevant error message with stack trace]
```

**What I've tried**:
1. [First attempt] - [result]
2. [Second attempt] - [result]
3. [Third attempt] - [result]

**Log files**:
- Checked [CustomAgentLogsTmp/Sandbox/android-device.log or other log file]
- Key exception: [paste exception if found]

**Environment**:
- Platform: [iOS/Android]
- .NET SDK: [from `dotnet --version`]
- Branch: [current branch name]

**Request**: [What you need help with]
```

---

## Error Handling Principles

1. **Read logs FIRST** - PS1 scripts capture everything you need
2. **Understand the exception** - Don't guess at fixes
3. **Try 2-3 attempts maximum** - Then ask for help
4. **Never skip log analysis** - The answer is usually in the logs

---

## Related Documentation

- [BuildAndRunSandbox.ps1](../../scripts/BuildAndRunSandbox.ps1) - Sandbox app testing script
- [BuildAndRunHostApp.ps1](../../scripts/BuildAndRunHostApp.ps1) - HostApp UI test script
- [Instrumentation Guide](../instrumentation.md) - Adding debug logging

**Agent-Specific Error Handling**:
- [Issue Resolver Error Handling](../issue-resolver-agent/error-handling.md) - Fix development errors

---

**Last Updated**: November 2025
