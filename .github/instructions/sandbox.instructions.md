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
- ❌ **NEVER** switch branches (e.g., `git checkout main`) during reproduction - stay on current branch

### What You ALWAYS Do (Mandatory Steps)

- ✅ **ALWAYS** save full output to file for analysis
- ✅ **ALWAYS** check for errors/exceptions FIRST before claiming success
- ✅ **ALWAYS** verify "Test completed" marker appears in output
- ✅ **ALWAYS** verify expected test actions in logs (Tapping, Screenshot, etc.)
- ✅ **ALWAYS** check device logs for Console.WriteLine markers (e.g., "SANDBOX: ...")
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
After EVERY test run, verify ALL of these **IN THIS ORDER**:

```bash
# Step 1: Check for errors/exceptions FIRST
grep -iE "error|exception|failed" CustomAgentLogsTmp/Sandbox/build-run-output.log | grep -v "no such element" | head -20

# Step 2: Verify expected test actions (MOST IMPORTANT - proves test actually ran)
grep -E "Tapping|Screenshot saved|Found.*element|Clicking|Entering text" CustomAgentLogsTmp/Sandbox/build-run-output.log

# Step 3: Verify test completion marker
grep "Test completed" CustomAgentLogsTmp/Sandbox/build-run-output.log

# Step 4: Verify device logs show expected behavior
grep "SANDBOX" CustomAgentLogsTmp/Sandbox/android-device.log  # or ios-device.log

# Step 5: Check screenshots were saved (if test captures them)
ls -lh CustomAgentLogsTmp/Sandbox/*.png

# Step 6: Check exit code
echo $?  # Should be 0 for success
```

**CRITICAL**: If Step 2 shows NO test actions, the test didn't actually run even if it "completed successfully". Update your Appium test and rerun.

**If ANY of these checks fail, the test DID NOT complete successfully. Investigate and fix before proceeding.**

---

## 🚨 WARNING: "Test Completed Successfully" ≠ Test Actually Worked

**CRITICAL UNDERSTANDING**: The message "✅ Test completed successfully" only means:
- ✅ Appium test script finished running without crashing
- ✅ Script exit code was 0

**It does NOT mean**:
- ❌ Appium found your UI elements
- ❌ Buttons were clicked
- ❌ Navigation happened
- ❌ Your test scenario actually ran

### Example of False Success

**What you see in output**:
```
✅ Test completed successfully

╔═══════════════════════════════════════════════════════════╗
║                    Test Summary                           ║
╠═══════════════════════════════════════════════════════════╣
║  Platform:     ANDROID                                    ║
║  Device:       emulator-5554                              ║
║  Result:       SUCCESS ✅                                 ║
╚═══════════════════════════════════════════════════════════╝
```

**What actually happened**:
```bash
# Check the logs:
grep "no such element" CustomAgentLogsTmp/Sandbox/build-run-output.log
# Result: 20+ lines of "no such element" errors

# The test looked for "InstructionLabel" which doesn't exist in MainPage
# Appium never found ANY elements
# Test script just gave up and exited with code 0
# NO ACTUAL TESTING WAS PERFORMED
```

### How to Detect False Success

**MANDATORY check after EVERY "successful" test**:
```bash
# Look for actual test actions in output
grep -E "Tapping|Clicking|Found element|Screenshot saved" CustomAgentLogsTmp/Sandbox/build-run-output.log
```

**If grep returns NOTHING → FALSE SUCCESS**:
- Test didn't actually do anything
- Template test is looking for elements that don't exist
- You MUST update `CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs` to match your MainPage
- Rerun BuildAndRunSandbox.ps1 after updating test

**If grep returns multiple lines → REAL SUCCESS**:
- Test found elements and interacted with them
- Proceed with full validation checklist

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

- ❌ User asks to "review PR #XXXXX" → Use **pr** agent for code review
- ❌ User asks to "write tests" or "create automated tests" → Use **write-tests-agent**
- ❌ User asks to "validate the UI tests" or "verify test quality" → Review test code instead
- ❌ User asks to "fix issue #XXXXX" (no PR exists) → Suggest `/delegate` command
- ❌ PR only adds documentation (no code changes to test)
- ❌ PR only modifies build scripts (no functional changes)

## Distinction: Code Review vs. Functional Testing

**Code Review** (pr-review skill):
- Analyzes code quality, patterns, best practices
- Reviews test coverage and correctness
- Checks for potential bugs or issues in the code itself
- Trigger: "review PR", "work on PR"

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

**🚨 CRITICAL RULES FOR ENTIRE WORKFLOW:**
- **ALWAYS use BuildAndRunSandbox.ps1 script** for building, deploying, and testing
- **NEVER use manual `dotnet build`, `adb`, or `xcrun` commands**
- **NEVER switch branches during reproduction** - stay on the current branch
- **ALWAYS stop and ask user if you cannot reproduce** - do not try alternative branches
- The script handles device detection, build, deployment, and test execution automatically
- See "BuildAndRunSandbox.ps1 Script" section below for full details

---

### Step 1: Understand Issue (DO NOT Checkout PR Unless Instructed)

**⚠️ IMPORTANT**: Only checkout a PR if the user explicitly asks you to test a specific PR. For general issue reproduction, work on the current branch.

```bash
# ONLY if user explicitly asks to test a PR:
gh pr checkout <PR_NUMBER>
```

**Understand the issue thoroughly:**
- Read issue report or PR description
- Identify what bug needs to be reproduced
- Note affected platforms
- Look for reproduction steps in the issue
- If testing a PR: Review PR changes to understand the fix

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
- `CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs` - Appium test script (MANDATORY - see setup below)

**Setting up the Appium test file (MANDATORY):**

🚨 **CRITICAL**: Update Appium test BEFORE running script. Template will give FALSE SUCCESS otherwise.

1. **Create test file**:
   ```bash
   mkdir -p CustomAgentLogsTmp/Sandbox
   cp .github/scripts/templates/RunWithAppiumTest.template.cs CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs
   ```

2. **Update test to match MainPage**:
   - Check AutomationIds: `grep AutomationId MainPage.xaml`
   - Update test to use those IDs (not template defaults)
   - Add test logic: tap buttons, verify labels
   - Add Console.WriteLine markers for debugging

3. **Example**:
   ```bash
   # Check MainPage
   grep 'AutomationId=' MainPage.xaml
   # Update test: App.WaitForElement("NavigateButton");
   ```

**Checklist**:
- ✅ Add AutomationIds to MainPage.xaml elements
- ✅ Update RunWithAppiumTest.cs to match
- ✅ Add SANDBOX markers for debugging

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
Copy and update test file (see Step 2 above). Template looks for "InstructionLabel" which doesn't exist - update first!

**🚨 POST-TEST VALIDATION (MANDATORY):**

After script completes, run Rule 4 validation checklist (see above). If ANY check fails: investigate, fix, rerun.

**Key reminders**:
- HTTP 200 = element found, NOT test completed
- If no test actions in logs = FALSE SUCCESS
- If Appium can't find initial element = app crashed or AutomationIds wrong

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

## 🔄 Iterative Testing Workflow (MANDATORY PROCESS)

**🚨 CRITICAL**: This is THE workflow for Sandbox testing. Do NOT use manual `adb`/`xcrun` commands to bypass it.

### The Required Loop

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Update MainPage.xaml[.cs] with your test scenario       │
│    - Add UI elements for reproduction                       │
│    - Add AutomationIds to all interactive elements          │
│    - Add Console.WriteLine markers for debugging            │
├─────────────────────────────────────────────────────────────┤
│ 2. Update Appium test to match your MainPage               │
│    CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs         │
│    - Update element locators to match AutomationIds        │
│    - Add test logic (tap buttons, verify labels)           │
├─────────────────────────────────────────────────────────────┤
│ 3. Run BuildAndRunSandbox.ps1                               │
│    pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform X  │
├─────────────────────────────────────────────────────────────┤
│ 4. Validate results using SYSTEMATIC CHECKLIST             │
│    - Step 1: Check for errors/exceptions                   │
│    - Step 2: Verify test actions (Tapping, etc.)           │
│    - Step 3: Verify "Test completed"                       │
│    - Step 4: Check device logs for SANDBOX markers         │
│    - Step 5-6: Screenshots and exit code                   │
├─────────────────────────────────────────────────────────────┤
│ 5. Did ALL validation checks pass?                         │
│    YES → Report success with summary (go to step 8)        │
│    NO  → Continue to step 6                                │
├─────────────────────────────────────────────────────────────┤
│ 6. Investigate failure from captured logs                  │
│    - Read CustomAgentLogsTmp/Sandbox/android-device.log    │
│    - Read CustomAgentLogsTmp/Sandbox/build-run-output.log  │
│    - Identify root cause (element not found? crash?)       │
├─────────────────────────────────────────────────────────────┤
│ 7. Fix the issue and LOOP BACK TO STEP 3                   │
│    - Update MainPage if UI/code issue                      │
│    - Update RunWithAppiumTest.cs if test issue             │
│    - Update both if AutomationId mismatch                  │
│    - Max 3 iterations before reporting as blocked          │
├─────────────────────────────────────────────────────────────┤
│ 8. Report comprehensive summary to user                    │
│    - Test scenario source and justification                │
│    - Validation results                                    │
│    - Verdict (success/partial/issues/blocked)              │
└─────────────────────────────────────────────────────────────┘
```

### ❌ What NOT To Do

**Never use manual commands during testing**:
- ❌ `adb logcat`, `adb shell`, `adb install`
- ❌ `xcrun simctl spawn`, `xcrun simctl install`
- ❌ `dotnet build`, `dotnet run`

**Why**: Script already captured everything. Manual commands show CURRENT state, not test execution state.

**✅ Correct**: Edit files → Rerun `BuildAndRunSandbox.ps1`

### ✅ Correct Iteration Example

**Most common case - Test fails to find element**:
```bash
# 1. Check what went wrong
grep "no such element" CustomAgentLogsTmp/Sandbox/build-run-output.log

# 2. Check what DOES exist in MainPage
grep AutomationId src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml

# 3. Fix MainPage: Add AutomationIds
# Edit MainPage.xaml: <Button AutomationId="NavigateButton" ...

# 4. Fix test: Update to match
# Edit RunWithAppiumTest.cs: App.WaitForElement("NavigateButton");

# 5. Rerun
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform android
```

**When validation passes**: Report success with comprehensive summary

### When to Stop Iterating

- ✅ **All validation checks pass** → Report success with detailed summary
- ❌ **Max 3 iterations reached** → **STOP and report**: "I wasn't able to reproduce the issue. What should I try next?"
- ❌ **Build fails repeatedly** → **STOP and report**: "I wasn't able to reproduce the issue due to build failures. What should I try next?"
- ❌ **Root cause unclear after log analysis** → **STOP and report**: "I wasn't able to reproduce the issue. What should I try next?"
- ❌ **Issue appears to be PR bug, not test** → **STOP and report findings to user**: "I wasn't able to reproduce the issue - it appears there may be an issue with [details]. What should I try next?"
- ❌ **Cannot reproduce the issue** → **STOP immediately**: "I wasn't able to reproduce the issue. What should I try next?"

**CRITICAL**: When you cannot reproduce or hit blockers, **PAUSE and ask the user**. Do NOT:
- ❌ Switch to a different branch (e.g., `git checkout main`)
- ❌ Try alternative approaches without asking first
- ❌ Change the workflow significantly without user guidance

### Mental Model: The Script is Your Robot

BuildAndRunSandbox.ps1 handles: build → deploy → capture logs → run test → report

**Your workflow**: Edit files → Run script → Read logs → Fix issues → Repeat

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

### 📝 Adding Debug Logging to Your Test Scenario

**Use `Console.WriteLine` for logging** - it works on all platforms.

```csharp
// Use a unique prefix for easy grep
Console.WriteLine("SANDBOX: Button clicked");
Console.WriteLine($"SANDBOX: Value is {myValue}");
```

**Searching logs:**
```bash
grep "SANDBOX" CustomAgentLogsTmp/Sandbox/android-device.log
grep "SANDBOX" CustomAgentLogsTmp/Sandbox/ios-device.log
grep "SANDBOX" CustomAgentLogsTmp/Sandbox/catalyst-device.log
```

---

## 🚨 ABSOLUTE RULE: BuildAndRunSandbox.ps1 is THE ONLY Deployment Method

**THIS IS MANDATORY. NOT A SUGGESTION.**

❌ If typing `adb`/`xcrun` commands during testing → STOP. You're violating the workflow.

**Why**: Manual commands show CURRENT state, not test execution state. Script already captured correct logs during test.

**❌ NEVER during testing**: `adb logcat`, `adb install`, `adb shell`, `xcrun simctl install/spawn`, `dotnet build/run`

**✅ ONLY exception**: Finding/booting specific iOS device BEFORE running script (`xcrun simctl list/boot`)

**Correct workflow**:
1. Edit files (MainPage, RunWithAppiumTest.cs)
2. Run: `pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform X`
3. Analyze logs in `CustomAgentLogsTmp/Sandbox/` (android-device.log, appium.log)
4. Fix issues, rerun script

**Expected logs**: `android-device.log` or `ios-device.log`, `appium.log`, `RunWithAppiumTest.cs`, optional screenshots

---

## Troubleshooting & Recovery

**Retry up to 3 times before reporting as blocked.** After each failure: analyze logs → fix → rerun script.

### Common Issues

| Issue | Recovery | Max Retries |
|-------|----------|-------------|
| Build error | Check SDK version, `dotnet tool restore` | 2 |
| App crash | Check stack trace in device log, fix code/XAML | 3 |
| Element not found | Verify AutomationIds match, check app loaded | 2 |
| Fast Deployment (Android) | Add `appium:noReset` capability | 1 |
| XAML parse error | Verify event handler exists in code-behind | 2 |

### Element Not Found Debugging

🚨 If Appium can't find initial element, app is NOT running correctly.

**Check**:
```bash
# Look for crashes
grep -i "FATAL\|crash\|exception" CustomAgentLogsTmp/Sandbox/android-device.log | tail -20

# Verify app launched
grep "SANDBOX.*MainPage" CustomAgentLogsTmp/Sandbox/android-device.log
```

**Root causes**: App crashed, XAML parse error, AutomationId mismatch, Android Fast Deployment

### When to Stop & Report
- ✅ **Continue**: Minor warnings, non-critical timeouts, platform differences
- ❌ **Stop and ask user**: Can't checkout PR, build fails after max retries, SDK mismatch, root cause unclear, cannot reproduce issue

**When blocked, ALWAYS report to user with this format**:
```markdown
I wasn't able to reproduce the issue. Here's what I tried:

1. [What I attempted]
2. [Issues encountered]
3. [Current state of reproduction attempt]

What should I try next?
```

**DO NOT** try alternative approaches without asking first. **DO NOT** switch branches.

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

**Testing Tips**:
- For layout bugs: Use `element.GetRect()` to measure positions
- For SafeArea PRs: Measure child content position, not parent size
- Add `Console.WriteLine("SANDBOX: ...")` markers for debugging

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
