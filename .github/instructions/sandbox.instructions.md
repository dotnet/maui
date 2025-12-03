---
description: "Comprehensive guide for working with the .NET MAUI Sandbox app for testing, validation, and experimentation"
applyTo: "src/Controls/samples/Controls.Sample.Sandbox/**"
---

# Sandbox Testing Guide

Comprehensive guide for working with the .NET MAUI Sandbox app for manual testing, PR validation, issue reproduction, and experimentation with MAUI features.

## When This Applies

This guide applies when you:
- Work with files in `src/Controls/samples/Controls.Sample.Sandbox/`
- User asks to "test this PR" or "validate PR #XXXXX" in Sandbox
- User asks to "reproduce issue #XXXXX" in Sandbox
- User wants to deploy to iOS/Android for manual testing
- User mentions Sandbox app by name in testing context

## 🚨 CRITICAL VALIDATION RULES - READ FIRST

**YOU MUST FOLLOW THESE RULES WHEN RUNNING SANDBOX TESTS:**

### What You NEVER Do (Absolute Rules)

- ❌ **NEVER** assume test completion without validation
- ❌ **NEVER** claim success based on HTTP 200 responses alone (element found ≠ test completed)
- ❌ **NEVER** skip the mandatory validation checklist
- ❌ **NEVER** proceed without verifying device logs show expected behavior
- ❌ **NEVER** assume Appium connection means test finished
- ❌ **NEVER** claim button was tapped without checking device logs

### What You ALWAYS Do (Mandatory Steps)

- ✅ **ALWAYS** save full output to file for analysis
- ✅ **ALWAYS** check for errors/exceptions FIRST before claiming success
- ✅ **ALWAYS** verify "Test completed" marker appears in output
- ✅ **ALWAYS** verify expected test actions in logs (Tapping, Screenshot, etc.)
- ✅ **ALWAYS** check device logs for Console.WriteLine markers
- ✅ **ALWAYS** verify artifacts exist (screenshots, if test captures them)

### Rule 1: NEVER ASSUME TEST COMPLETION
- ❌ **DO NOT** assume the test completed successfully just because Appium connected
- ❌ **DO NOT** assume success based on HTTP 200 responses (element found ≠ test completed)
- ✅ **DO** verify test completion by checking for completion markers in output
- ✅ **DO** search for "Test completed", "═══════", or final summary messages

### Rule 2: ALWAYS VALIDATE TEST OUTPUT
After running BuildAndRunSandbox.ps1, you MUST:
1. **Save full output to file**: `pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform ios > CustomAgentLogsTmp/Sandbox/build-run-output.log 2>&1`
2. **Check for errors FIRST**: `grep -E "ERROR|Exception|failed" CustomAgentLogsTmp/Sandbox/build-run-output.log`
3. **Verify completion markers**: `grep "Test completed\|══════" CustomAgentLogsTmp/Sandbox/build-run-output.log`
4. **Check for expected actions**: `grep "Tapping\|Screenshot saved\|switched to" CustomAgentLogsTmp/Sandbox/build-run-output.log`

### Rule 3: VALIDATE DEVICE LOGS FOR EXPECTED BEHAVIOR
- ✅ **DO** check device logs confirm your expected test actions happened
- ✅ **DO** grep for your Console.WriteLine markers (e.g., "SANDBOX.*CLICKED")
- ❌ **DO NOT** claim the test worked without verifying device logs show the action

### Rule 4: SYSTEMATIC VALIDATION CHECKLIST
After EVERY test run, verify ALL of these:

```bash
# 1. Check for errors/exceptions
grep -iE "error|exception|failed" CustomAgentLogsTmp/Sandbox/build-run-output.log | head -20

# 2. Verify test completed
grep "Test completed" CustomAgentLogsTmp/Sandbox/build-run-output.log

# 3. Check screenshots were saved (if test captures them)
ls -lh CustomAgentLogsTmp/Sandbox/*.png

# 4. Verify expected test actions in device logs
grep "SANDBOX" CustomAgentLogsTmp/Sandbox/ios-device.log  # or android-device.log

# 5. Check exit code
echo $?  # Should be 0 for success
```

**If ANY of these checks fail, the test DID NOT complete successfully. Investigate and fix before proceeding.**

---

## Purpose

Work with the Sandbox app for manual testing, PR validation, issue reproduction, and experimentation with MAUI features.

## When to Use Sandbox Testing

- ✅ User asks to "test this PR" (functional testing, not code review)
- ✅ User asks to "validate PR #XXXXX" or "validate PR #XXXXX in Sandbox"
- ✅ User asks to "reproduce issue #XXXXX" or "try out issue #XXXXX"
- ✅ User asks to "try out" or "experiment with" a feature in Sandbox
- ✅ PR modifies core MAUI functionality (controls, layouts, platform code)
- ✅ Need to manually verify a fix works on device/simulator
- ✅ Need to create a quick test scenario for hands-on validation

## When NOT to Use Sandbox

- ❌ User asks to "review PR #XXXXX" → Use **pr-reviewer** agent for code review
- ❌ User asks to "write UI tests" or "create automated tests" → Use **uitest-coding-agent**
- ❌ User asks to "validate the UI tests" or "verify test quality" → Review test code instead
- ❌ User asks to "fix issue #XXXXX" → Use **issue-resolver** agent
- ❌ PR only adds documentation (no code changes to test)
- ❌ PR only modifies build scripts (no functional changes)

## Distinction: Code Review vs. Functional Testing

**Code Review** (pr-reviewer agent):
- Analyzes code quality, patterns, best practices
- Reviews test coverage and correctness
- Checks for potential bugs or issues in the code itself
- Trigger: "review PR", "review pull request", "code review"

**Functional Testing** (sandbox-agent):
- Builds and deploys PR to device/simulator
- Manually validates the fix works as expected
- Reproduces issues and verifies they're resolved
- Trigger: "test this PR", "validate PR in Sandbox", "reproduce issue"

## 🚨 Critical Requirements for Android Testing

**ANDROID-ONLY REQUIREMENT - appium:noReset**

⚠️ **This ONLY applies to Android, NOT iOS**

When testing on Android, the Appium test script **MUST** have this capability:

```csharp
// ANDROID ONLY - Do NOT add this for iOS
if (PLATFORM == "android")
{
    options.AddAdditionalAppiumOption("appium:noReset", true);
}
```

**Why this is critical for Android:**
- Without `noReset`, Appium clears app data between runs
- This breaks .NET MAUI's Fast Deployment mechanism on Android
- App crashes with: `"No assemblies found in '.../__override__/...' ... Assuming this is part of Fast Deployment. Exiting..."`
- The app will crash immediately on launch before any test can run

**iOS does NOT need this** - iOS deployment works differently and doesn't use Fast Deployment

**Where to set it:**
- Template: `.github/scripts/templates/RunWithAppiumTest.template.cs` (line ~68, Android section only)
- Active test: `CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs` (Android section only)

**Platform detection is automatic** - The template automatically detects Android vs iOS from the UDID format, so you don't need to manually set the platform. The `if (PLATFORM == "android")` block will execute automatically when testing on Android.

**⚠️ NEVER REMOVE THIS CAPABILITY FROM ANDROID** - All Android tests depend on it

---

## Core Workflow

**🚨 CRITICAL RULE FOR ENTIRE WORKFLOW:**
- **ALWAYS use BuildAndRunSandbox.ps1 script** for building, deploying, and testing
- **NEVER use manual `dotnet build`, `adb`, or `xcrun` commands**
- The script handles device detection, build, deployment, and test execution automatically
- See "BuildAndRunSandbox.ps1 Script" section below for full details

---

### Step 1: Checkout PR and Understand Issue

```bash
# Fetch and checkout PR
gh pr checkout <PR_NUMBER>
```

**Understand the issue thoroughly:**
- Read PR description and linked issue report
- Identify what bug is being fixed
- Note affected platforms
- Look for reproduction steps in the issue
- Review PR changes to understand the fix

---

### Step 2: Create Test Scenario in Sandbox

**Choose test scenario source (in priority order):**

1. **From Issue Reproduction** (Preferred)
   - Look for "Reproduction" or "Steps to Reproduce" in the linked issue
   - Use the exact scenario the user reported
   - This proves you're testing what the user experienced

2. **From PR's UI Tests** (Alternative)
   - Check if PR includes files in `TestCases.HostApp/Issues/IssueXXXXX.*`
   - Adapt the test page code to Sandbox
   - Simplify if needed for manual testing

3. **Create Your Own** (Last Resort)
   - If no repro available, design scenario based on PR changes
   - Focus on the specific code paths modified by the fix
   - Keep it simple and focused

**Files to modify**:
- `src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml[.cs]` - UI and code for reproduction
- `CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs` - Appium test script (see setup below)

**Setting up the Appium test file:**
1. If `CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs` does NOT exist:
   - Create directory: `mkdir -p CustomAgentLogsTmp/Sandbox`
   - Copy template: `cp .github/scripts/templates/RunWithAppiumTest.template.cs CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs`
2. If it already exists, edit it directly (it persists between runs)
3. Customize the test logic for your scenario

**Implementation**:
- Copy reproduction code from issue or UITest
- Add `AutomationId` to interactive elements
- Add console logging for debugging
- See [Instrumentation Guide](instrumentation.md) for patterns and examples

**🚨 CRITICAL - Document Your Test Scenario:**

You MUST include in your final report:
- ✅ **Source**: Where did the test scenario come from? (issue reproduction / PR UITest / custom)
- ✅ **Why**: Why did you choose that source? (e.g., "Issue #XXXXX provides detailed repro steps")
- ✅ **What**: What specific actions does your test perform? (e.g., "Tap button → verify label changes to 'Success'")
- ✅ **Expected**: What behavior should occur? (from issue description or PR changes)

Without this documentation, user cannot verify you tested the right thing.

---

### Step 3: Test WITH PR Fix

**Platform Selection Decision Tree:**

Follow this flowchart in order - stop at the first match:

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. Does PR title have platform tag? [Android], [iOS], etc.     │
│    YES → Test that platform ONLY                                │
│    NO  → Continue to step 2                                     │
├─────────────────────────────────────────────────────────────────┤
│ 2. Are ALL modified files in platform-specific paths?          │
│    (Platform/Android/, Platform/iOS/, *.Android.cs, etc.)      │
│    YES → Test that platform ONLY                                │
│    NO  → Continue to step 3                                     │
├─────────────────────────────────────────────────────────────────┤
│ 3. Does issue report mention a specific platform?              │
│    YES (one platform)  → Test that platform ONLY               │
│    YES (multiple)      → Test Android + iOS                    │
│    NO  → Continue to step 4                                     │
├─────────────────────────────────────────────────────────────────┤
│ 4. Is this high-risk cross-platform code?                      │
│    (Controls/, Core/, layout, navigation, critical controls)   │
│    YES → Test Android + iOS                                     │
│    NO  → Test Android ONLY (default - faster)                  │
└─────────────────────────────────────────────────────────────────┘
```

**Platform-specific path indicators:**
- `Platform/Android/` or `Platform/iOS/` → Platform-specific
- Files with `.Android.`, `.iOS.`, `.MacCatalyst.` in name → Platform-specific
- `Controls/`, `Core/` without platform subfolders → Cross-platform

**Hard rule:** Never test more than 2 platforms unless user explicitly requests it.

**Run Test on Specific iOS Device/Version:**

When user requests a specific iOS version or device:

1. **Find the UDID for that device/version combination**:
   ```bash
   # Example: Find iPhone Xs with iOS 18.5
   UDID=$(xcrun simctl list devices available --json | jq -r '
     .devices 
     | to_entries 
     | map(select(.key | contains("iOS-18-5"))) 
     | map(.value) 
     | flatten 
     | map(select(.name == "iPhone Xs")) 
     | first 
     | .udid
   ')
   
   echo "Found UDID: $UDID"
   ```

2. **Pass the UDID to the script**:
   ```bash
   pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform ios -DeviceUdid "$UDID"
   ```

**Examples:**
- **"Run on iOS 18.5"** → Find iPhone Xs with iOS 18.5, get UDID, pass to script
- **"Run on iPhone 15"** → Find iPhone 15 (any iOS), get UDID, pass to script
- **"Run on iPhone 16 Pro with iOS 18.0"** → Find iPhone 16 Pro with iOS 18.0, get UDID, pass to script

---

## How the Template Works

**The template ALWAYS does the same thing:**

1. ✅ Verifies app launched successfully (WaitForElement)
2. ✅ Optionally runs automated UI tests (if you add them)
3. ✅ Exits WITHOUT closing the app (stays running for manual validation)

**Usage:**
```bash
# Copy the template
cp .github/scripts/templates/RunWithAppiumTest.template.cs CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs

# OPTIONAL: Add automated test logic in the TEST LOGIC section
# If you don't add test logic, it just verifies launch and exits

# Run the script
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform ios
```

**Result:**
- App launches and stays running
- You can manually validate in the simulator
- Test script exits without closing the app

---

## 🚨 CRITICAL: BuildAndRunSandbox.ps1 Script - ONLY Way to Deploy Sandbox

**YOU MUST ALWAYS USE THIS SCRIPT. NEVER USE MANUAL `dotnet build`, `adb`, or `xcrun` COMMANDS.**

### Script Location
`.github/scripts/BuildAndRunSandbox.ps1`

### Basic Usage
```powershell
# Android
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform Android

# iOS (auto-detects device)
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform iOS

# iOS with specific device
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform iOS -DeviceUdid "YOUR-DEVICE-UDID"
```

### What the Script Does Automatically
- ✅ **Device detection and boot** - Finds and boots simulator/emulator
- ✅ **UDID extraction** - Sets DEVICE_UDID environment variable
- ✅ **Fresh app build** - Builds Sandbox project for target platform
- ✅ **App deployment** - Installs and launches app
- ✅ **Appium server management** - Starts/stops Appium automatically
- ✅ **Log capture** - Saves device and Appium logs to `CustomAgentLogsTmp/Sandbox/`
- ✅ **Test execution** - Runs your Appium test script

### Requirements Before Running
**You MUST create the Appium test file first:**

```bash
# Create directory if needed
mkdir -p CustomAgentLogsTmp/Sandbox

# Copy template
cp .github/scripts/templates/RunWithAppiumTest.template.cs CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs

# OPTIONAL: Add automated test logic (or leave as-is for manual testing only)
# Edit CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs to add UI automation
```

**What the template does:**
- ✅ Verifies app launched (always)
- ✅ Runs any automated tests you add (optional)
- ✅ Exits without closing app (always)

**🚨 CRITICAL - MANDATORY POST-TEST VALIDATION:**

After BuildAndRunSandbox.ps1 completes, you **MUST** run these validation commands:

```bash
# Step 1: Save full output to file for analysis
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform ios > CustomAgentLogsTmp/Sandbox/build-run-output.log 2>&1

# Step 2: Check for errors/exceptions FIRST (do this before anything else)
grep -iE "error|exception|failed" CustomAgentLogsTmp/Sandbox/build-run-output.log | head -20

# Step 3: Verify test completion marker
grep "Test completed" CustomAgentLogsTmp/Sandbox/build-run-output.log

# Step 4: Verify expected test actions in output
grep -E "Found|Tapping|Screenshot saved|switched" CustomAgentLogsTmp/Sandbox/build-run-output.log

# Step 5: Verify device logs show expected behavior
grep "SANDBOX" CustomAgentLogsTmp/Sandbox/ios-device.log  # or android-device.log

# Step 6: Check if screenshots were created (if test captures them)
ls -lh CustomAgentLogsTmp/Sandbox/*.png
```

**Validation Checklist - ALL must pass:**
- ✅ No errors/exceptions in test output
- ✅ "Test completed" message appears
- ✅ Expected test actions appear in output (Tapping, Screenshot, etc.)
- ✅ Device logs show your SANDBOX markers for button clicks/actions
- ✅ Screenshots exist (if test captures them)
- ✅ Exit code is 0 (check: `echo $?` after script)

**If ANY validation fails:**
1. ❌ **DO NOT** claim test success
2. ❌ **DO NOT** proceed to summary
3. ✅ **DO** investigate the specific failure
4. ✅ **DO** fix the issue (test script, path bug, etc.)
5. ✅ **DO** rerun and revalidate

**What Appium HTTP 200 means:**
- ✅ Element was found on the screen
- ❌ Does NOT mean test completed
- ❌ Does NOT mean button was tapped
- ❌ Does NOT mean expected behavior occurred

**Always validate the FULL test execution, not just element finding.**

---

**What to validate (after passing above checklist):**
- ✅ App launches successfully
- ✅ **CRITICAL**: Verify app is actually running before proceeding:
  - Check device logs show MainPage initialization
  - Confirm Appium can find your initial test element
  - If element not found: STOP and investigate logs (see Troubleshooting)
- ✅ Test scenario completes without crashes/hangs
- ✅ Appium test finds expected elements
- ✅ Behavior matches expected fix

**If Appium can't find initial element:**
🚨 **DO NOT PROCEED** - Something is wrong:
1. Check device logs for crashes/exceptions
2. Verify XAML AutomationIds match test code
3. Confirm app actually loaded MainPage (check logs for "MainPage" messages)
4. Check if XAML has matching event handler (e.g., `Clicked="OnNavigateClicked"` needs method in code-behind)
5. See [Element Not Found Troubleshooting](#element-not-found-on-first-screen) section

**What to document in your summary:**
- ✅ Validation checklist results (all passed)
- ✅ Which test scenario you created (from issue/UITest/custom)
- ✅ Specific actions your test performs
- ✅ What behavior you observed (from device logs and test output)
- ✅ Whether fix works as expected
- ✅ Any warnings, errors, or unexpected behavior

---

### 📝 Note for User

**Test scenario is ready in Sandbox for manual verification.**

**To verify bug reproduction (optional):**
```bash
# 1. Revert the PR fix files
git checkout main -- [list specific fix files from PR]

# 2. Rerun test - bug should appear
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform [android|ios]

# 3. Restore fix
git checkout HEAD -- [fix files]

# 4. Rerun test - bug should be gone
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform [android|ios]
```

This proves the test scenario correctly reproduces the bug.

---

## Output Format

Provide a concise test summary:

```markdown
## PR Testing Summary

**PR**: #XXXXX - [Title]
**Platform Tested**: Android/iOS
**Issue**: [Brief description]

---

### Test Scenario Setup

**🚨 REQUIRED - Source of Test Scenario**:
- **Source**: [From issue reproduction / From PR UITest / Custom scenario]
- **Why this source**: [e.g., "Issue #XXXXX provides detailed repro steps" / "PR includes UITest that demonstrates the fix" / "No repro available, created scenario based on PR code changes"]
- **Link to source**: [URL to issue comment with repro, or path to UITest file]

**What was tested**:
- [Specific actions taken - e.g., "Tap 'Toggle RTL' button, then tap 'Show Dialog' button"]
- [UI elements involved - e.g., "Button with AutomationId='ToggleButton', Dialog with Label"]
- [Expected behavior - e.g., "Dialog should appear with correct RTL padding on label"]

---

### Test Results WITH PR Fix

**Observed Behavior**:
- [What happened when running the test]
- [Appium test results]
- [Relevant log excerpts]

**Screenshots**: [Reference if taken, but not for validation]

---

### Verdict

✅ **FIX VALIDATED** - Test scenario completes successfully, expected behavior observed
OR
⚠️ **PARTIAL** - Fix appears to work but [note any concerns]
OR
❌ **ISSUES FOUND** - [Specific problems encountered]
OR
🚫 **CANNOT TEST** - [Build failures, setup issues, etc.]

---

### Notes for User
- Test scenario is set up in Sandbox and ready for manual verification if needed
- To verify bug reproduction without fix, revert PR changes: `git checkout main -- [fix files]`
- Then rerun: `pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform [android|ios]`
```

---

## Best Practices

1. **Use issue reproduction when available** - Most reliable test scenario
2. **Adapt PR's UITests if no repro** - They're already designed to test the fix
3. **Validate programmatically, reference visually** - Use Appium element queries for validation, screenshots only for additional context
4. **Use colored backgrounds** - Makes layout issues visible
5. **Add console markers** - Easy to grep logs  
6. **Test multiple iterations** - Race conditions need multiple runs (3-5 times)
7. **Leave Sandbox as-is** - User will iterate on it after your testing
8. **Document your test scenario thoroughly** - Include source (issue/UITest/custom), why you chose it, specific actions, and expected behavior so user can verify

**Screenshot Usage**:
- ✅ Take screenshots for **context and reference** (e.g., showing layout before/after)
- ✅ Include in report if they provide **additional insight** beyond what logs show
- ❌ Do NOT rely on screenshots as **primary validation** - use Appium element queries and log analysis
- ❌ Do NOT take screenshots just to show "it works" - validation should come from test assertions

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

## 🚨 CRITICAL RULE: Never Run Manual Build/Deploy/Capture Commands

The BuildAndRunSandbox.ps1 script handles build, deploy, and log capture:
- ✅ Building the app
- ✅ Deploying to device
- ✅ Capturing logs (device + Appium)
- ✅ Running Appium tests
- ✅ Cleaning logcat before tests

**❌ NEVER RUN THESE BUILD/DEPLOY/CAPTURE COMMANDS DURING TEST WORKFLOW:**
- `adb logcat` - Script already captures to `CustomAgentLogsTmp/Sandbox/android-device.log`
- `adb logcat -c` - Script already clears logcat before test
- `adb install` - Script handles deployment
- `adb shell am start` - Script launches the app
- `dotnet build` - Script builds the app
- `dotnet run` - Script runs the Appium test
- `xcrun simctl install` - Script deploys to simulator
- `xcrun simctl spawn booted log stream` - Script captures iOS logs

**⚠️ EXCEPTION - Pre-test device setup (when user requests specific device/version):**

These commands ARE allowed ONLY for finding/booting a specific device BEFORE running BuildAndRunSandbox.ps1:
- `xcrun simctl list devices` - To find UDID for specific iOS version/device
- `xcrun simctl boot <UDID>` - To boot a specific simulator before passing UDID to script

See "Run Test on Specific iOS Device/Version" section above for when this is needed.

**Why this matters:**
- Running `adb logcat` manually captures DIFFERENT logs than what the script captured
- Manual logcat shows CURRENT state, not what happened during the test
- Script filters logs to Sandbox app only and includes PID
- You'll miss the actual crash/error if you run logcat after the fact

**✅ DO RUN THE SCRIPT TO BUILD/DEPLOY/TEST:**
```bash
# Let the script handle everything
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform android
```

**✅ DO RUN ANALYSIS COMMANDS TO READ CAPTURED LOGS:**
```bash
# The script ALREADY captured logs. Now analyze them:
grep -i "error" CustomAgentLogsTmp/Sandbox/android-device.log
grep -i "exception" CustomAgentLogsTmp/Sandbox/android-device.log
grep -i "FATAL" CustomAgentLogsTmp/Sandbox/android-device.log
tail -50 CustomAgentLogsTmp/Sandbox/android-device.log
cat CustomAgentLogsTmp/Sandbox/appium.log

# Search for your console markers:
grep "SANDBOX" CustomAgentLogsTmp/Sandbox/android-device.log

# These commands READ existing files. They don't capture new logs.
```

**Expected files after BuildAndRunSandbox.ps1:**
```
CustomAgentLogsTmp/Sandbox/
├── RunWithAppiumTest.cs           # Your Appium test script
├── android-device.log             # Android device logs (filtered to Sandbox app)
│   OR ios-device.log              # iOS simulator logs (filtered to Sandbox app)
├── appium.log                     # Appium server logs
├── issue_XXXXX_before.png         # Screenshot before test (if test captures it)
└── issue_XXXXX_after.png          # Screenshot after test (if test captures it)
```

**If logs are missing or empty:**

This indicates a bug in BuildAndRunSandbox.ps1 that MUST be fixed:

1. **Check script output** - Look for "Logcat dumped to:" or "iOS logs saved to:" messages
2. **Verify variable is defined** - Script must set `$deviceLogFile` before using it:
   ```powershell
   $deviceLogFile = Join-Path $SandboxAppiumDir "$Platform-device.log"
   ```
3. **Check file permissions** - Ensure `CustomAgentLogsTmp/Sandbox/` directory is writable
4. **Report the bug** - If script doesn't generate logs, this is a script bug that blocks testing

**How to debug:**

1. **Run BuildAndRunSandbox.ps1** - It builds, deploys, captures logs, and runs tests
2. **Read script output** - Shows build errors, deployment status, test results
3. **Verify log files exist** - Check `ls -lh CustomAgentLogsTmp/Sandbox/` for log files
4. **Analyze captured logs** - Use `grep`, `cat`, `tail` to read log files in `CustomAgentLogsTmp/Sandbox/`:
   - `android-device.log` or `ios-device.log` - Device logs
   - `appium.log` - Appium server logs
5. **Fix issues** - Update test code, MainPage, or report if blocked
6. **Rerun script** - It will rebuild, redeploy, recapture logs, and retest

**Summary:**
- ❌ Don't run commands that BUILD, DEPLOY, or CAPTURE logs
- ✅ DO run commands that ANALYZE already-captured logs
- ✅ Always use BuildAndRunSandbox.ps1 for build/deploy/test cycle
- ✅ If log files are missing, this is a script bug - fix or report it

---

## Common Error Handling Patterns

### PublicAPI Analyzer Failures

**Don't turn off analyzers** - Fix the PublicAPI.Unshipped.txt files properly:

```bash
# Use dotnet format to fix analyzer issues
dotnet format analyzers Microsoft.Maui.sln

# If that doesn't work, revert and manually add entries
git checkout -- src/*/PublicAPI.Unshipped.txt
# Then add only the new public APIs
```

### What to Fix Based on Logs

**Build failures** - Analyze error in script output, check if SDK mismatch, fix project files if needed, retry
**App crashes** - Read device log from `CustomAgentLogsTmp/Sandbox/`, check stack trace, fix test code or reproduction scenario
**Fast Deployment errors (Android)** - See "Critical Requirements for Android Testing" section at top of this document
**Cannot find reproduction scenario** - Try alternative approaches (UITests, code analysis)
**Test code errors** - Fix Appium script or MainPage code based on error messages

---

## When to Report vs. When to Escalate

### ✅ Just Report (Continue Testing)
- Minor warnings in logs (unrelated to fix)
- Small visual glitches (if not what's being fixed)
- Non-critical Appium delays or timeouts that eventually succeed
- Platform-specific behavior differences (document them)

### 🔧 Debug and Fix (Try These Before Reporting)

**Retry Policy**: Attempt up to **3 times** before reporting as blocked.

After each failure:
1. Analyze logs (device + Appium)
2. Identify root cause
3. Fix the issue (test code, XAML, capabilities)
4. Rerun BuildAndRunSandbox.ps1

**Recovery Steps by Issue Type**:

| Issue | Recovery Action | Max Retries |
|-------|----------------|-------------|
| Build error | Check SDK version, restore tools, clean build | 2 |
| App crash | Fix test code/XAML based on stack trace | 3 |
| Element not found | Verify AutomationIds, check page loaded | 2 |
| Fast Deployment error (Android) | See "Critical Requirements for Android Testing" section | 1 |
| Appium timeout | Increase wait time, check app state | 2 |
| XAML parse error | Fix event handler or binding syntax | 2 |

**When to stop and report**: After max retries OR if you determine:
- Issue is outside your control (SDK mismatch, device unavailable)
- Root cause unclear despite log analysis
- Issue appears to be a PR bug (not test infrastructure)

### 🚫 STOP and Report (Cannot Proceed After Attempts)
- Cannot checkout PR (git errors)
- BuildAndRunSandbox.ps1 doesn't exist
- No device/simulator available
- SDK version mismatch with no resolution
- Build failures persist after troubleshooting attempts
- App crashes that appear unrelated to test code or fix

---

## Detailed Troubleshooting

### Build Fails
**Action**: Stop and report to user

```markdown
❌ Build failed

**Error**: [Full error message]

**Common Causes**:
- .NET SDK version mismatch (check global.json)
- Missing dependencies
- Corrupted build cache

**Recommended Actions**:
1. Verify .NET SDK version: `dotnet --version`
2. Should match value in global.json
3. Run: `dotnet tool restore`

Unable to proceed with validation. Please advise.
```

### Element Not Found on First Screen
**Action**: ALWAYS investigate - Do NOT assume app is working

🚨 **CRITICAL**: If Appium can't find your initial element (e.g., test button), the app is NOT running correctly.

**DO NOT ASSUME**:
- ❌ "App is just loading slowly" - Wait longer
- ❌ "AutomationId is wrong" - Just try different locators
- ❌ "Maybe the page didn't navigate yet" - Try other elements

**IMMEDIATELY CHECK**:

1. **Check device logs for crashes**:
   ```bash
   # Android - Look for "FATAL", "crash", or "Exception"
   grep -i "FATAL\|crash\|exception" CustomAgentLogsTmp/Sandbox/android-device.log | tail -20
   
   # iOS - Look for "Terminating app" or "exception"
   grep -i "terminating\|exception\|crash" CustomAgentLogsTmp/Sandbox/ios-device.log | tail -20
   ```

2. **Verify app actually launched**:
   ```bash
   # Android - Check if MainPage initialized
   grep "SANDBOX.*MainPage" CustomAgentLogsTmp/Sandbox/android-device.log
   
   # iOS - Check if app process is present
   grep "Maui.Controls.Sample.Sandbox" CustomAgentLogsTmp/Sandbox/ios-device.log | head -5
   ```

3. **Common Root Causes**:
   - **App crashed on launch** - Check logs for exception/crash
   - **XAML parse error** - Missing event handler in code-behind
   - **AutomationId mismatch** - XAML has different name than test expects
   - **Wrong page displayed** - App navigated somewhere else
   - **Android Fast Deployment issue** - Missing `noReset` capability

**Debugging Steps**:

1. Check logs first (see commands above)
2. If crashed: Find and fix the exception
3. If XAML error: Verify event handler exists and matches
4. If no crash: Verify AutomationIds match between XAML and test
5. If still unclear: Check Appium page source: `driver.PageSource`

---

### App Crashes Immediately on Launch (Android)
**Action**: Check for Fast Deployment error first

```bash
# Search device log for Fast Deployment error
grep "No assemblies found" CustomAgentLogsTmp/Sandbox/android-device.log
# OR
grep "Abort message" CustomAgentLogsTmp/Sandbox/android-device.log
```

**If you see: "No assemblies found in '.../__override__/...' ... Fast Deployment"**

This is a missing `appium:noReset` capability issue. See "Critical Requirements for Android Testing" section at the top of this document for the fix.

**This is NOT a PR bug** - It's a test infrastructure issue. Once fixed, retry the test.

---

### App Crashes During Test (Other Reasons)
**Action**: Analyze crash logs and report

```markdown
❌ App crashed during testing

**When**: [Specific action that caused crash]

**Error from logs**: [Exception/crash details]

**Analysis**: [Your interpretation - does this seem related to the PR changes?]

**Recommendation**: [Does this indicate the fix doesn't work, or is it an unrelated issue?]
```

### Cannot Find Reproduction Steps
**Action**: Try alternative approaches

1. Check linked issue for "Reproduction" section
2. Look for PR's UITest files in `TestCases.HostApp/Issues/`
3. Examine PR code changes to understand what's being fixed
4. Create minimal test scenario based on code analysis
5. Report to user if reproduction scenario is unclear

### Test Shows Unexpected Behavior
**Action**: Document and report

```markdown
⚠️ Unexpected behavior during testing

**What I expected**: [Based on issue description]

**What I observed**: [Actual behavior]

**Test scenario**: [What was tested]

**Logs**: [Relevant excerpts]

**Question for user**: Is this expected behavior, or does this indicate an issue?
```

---

## Common Mistakes to Avoid

- ❌ Using TestCases.HostApp for manual PR validation (use Sandbox)
- ❌ Manual build/deploy commands instead of BuildAndRunSandbox.ps1
- ❌ Testing only one platform when PR affects multiple
- ❌ Using screenshots for validation (use Appium element queries)
- ❌ Creating test scenario without checking issue for reproduction steps
- ❌ Ignoring PR's existing UITests when available
- ❌ Cleaning up or reverting Sandbox changes (user will iterate on it)

---

## Key Resources

### Must-Read Before Testing
- [Instrumentation Guide](instrumentation.md) - How to add logging and measurements
- [Appium Control Scripts](appium-control.md) - UI automation patterns

### Read When Relevant
- [SafeArea Testing](safearea-testing.md) - If PR involves SafeArea

---

## Related Documentation

- [Instrumentation Guide](instrumentation.md) - How to add logging and measurements to Sandbox app
- [Appium Control Scripts](appium-control.md) - UI automation with Appium
- [SafeArea Testing](safearea-testing.md) - Testing SafeArea-specific issues

---

## Appendix: Cleanup (Only When User Requests)

⚠️ **DO NOT clean up after testing** - Leave Sandbox as-is so user can iterate on it.

Only use these commands if the **user explicitly requests cleanup**:

### Sandbox App Cleanup (User Request Only)
```bash
# Revert all changes to Sandbox app
git checkout -- src/Controls/samples/Controls.Sample.Sandbox/
```

### Sandbox Test Files Cleanup (User Request Only)
```bash
# Remove Appium test directory (gitignored)
rm -rf CustomAgentLogsTmp/Sandbox/
```
