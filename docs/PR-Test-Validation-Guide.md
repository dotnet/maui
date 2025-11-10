# PR Test Validation Guide

## Overview

This guide provides detailed procedures for validating that UI tests in pull requests actually test what they claim to test. A test that passes both with and without a fix is **not testing anything** and provides false confidence.

## Critical Principle: Revert Testing

**Every bug fix PR with UI tests MUST be validated using the revert testing process.**

The test validation ensures:
1. The test **fails** without the fix (proves it catches the bug)
2. The test **passes** with the fix (proves the fix works)

## Validation Process

### Step 1: Understand the PR Structure

Identify which files contain:
- **Test files**: Always in `src/Controls/tests/TestCases.{HostApp,Shared.Tests}/**`
- **Fix files**: Typically in `src/Core`, `src/Controls`, or `src/Essentials`

Example for RTL padding bug fix:
- Test files: 
  - `src/Controls/tests/TestCases.HostApp/Issues/Issue32316.cs`
  - `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue32316.cs`
- Fix files:
  - `src/Core/src/Platform/Android/TextViewExtensions.cs`
  - `src/Core/src/Platform/iOS/MauiLabel.cs`

### Step 2: Save Current State

Create a temporary commit to preserve the PR state:

```bash
# Save all PR changes
git add -A
git commit -m "temp: PR changes for validation"
```

### Step 3: Revert ONLY the Fix (Keep Tests)

Revert the fix files while keeping the test files:

```bash
# Revert only the fix files (NOT the test files)
git checkout HEAD~1 -- src/Core/src/Platform/Android/TextViewExtensions.cs
git checkout HEAD~1 -- src/Core/src/Platform/iOS/MauiLabel.cs

# Verify what was reverted
git status
git diff
```

**Important**: Only revert the actual fix files. Do NOT revert test files.

### Step 4: Run Test - MUST FAIL

Build and run the test. It **MUST FAIL** at this point:

**Android:**
```bash
# Build test app
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj \
  -f net10.0-android -t:Run

# Run the specific test
dotnet test src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj \
  --filter "FullyQualifiedName~IssueXXXXX"
```

**iOS:**
```bash
# Find iPhone simulator
UDID=$(xcrun simctl list devices available | grep "iPhone Xs" | tail -1 | \
  sed -n 's/.*(\([A-F0-9-]*\)).*/\1/p')

# Build app
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj \
  -f net10.0-ios

# Boot simulator and install
xcrun simctl boot $UDID 2>/dev/null || true
xcrun simctl install $UDID \
  artifacts/bin/Controls.TestCases.HostApp/Debug/net10.0-ios/iossimulator-arm64/Controls.TestCases.HostApp.app

# Run test
dotnet test src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj \
  --filter "FullyQualifiedName~IssueXXXXX"
```

**MacCatalyst:**
```bash
# Build and run app
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj \
  -f net10.0-maccatalyst -t:Run

# Run test
dotnet test src/Controls/tests/TestCases.Mac.Tests/Controls.TestCases.Mac.Tests.csproj \
  --filter "FullyQualifiedName~IssueXXXXX"
```

**Expected Result**: ❌ Test FAILS

**What this proves**: The test correctly detects the bug that the PR is fixing.

### Step 5: Restore the Fix

Restore the original PR state:

```bash
# Restore everything to the PR state
git reset --hard HEAD
```

### Step 6: Run Test - MUST PASS

Build and run the test again with the fix in place:

```bash
# Same commands as Step 4, but test should now PASS
```

**Expected Result**: ✅ Test PASSES

**What this proves**: The fix resolves the issue that was detected by the test.

## Validation Results

### ✅ Valid Test

**Scenario**: Test fails without fix, passes with fix

**Action**: Approve the test - it correctly validates the bug fix

**Documentation**: In your review comment, note:
```markdown
✅ **Test Validation**: Confirmed test fails without fix and passes with fix
```

### ❌ Invalid Test - Passes Both Times

**Scenario**: Test passes both with and without the fix

**Problem**: Test is not actually validating the fix - it provides false confidence

**Action**: Request changes with specific feedback:
```markdown
❌ **Test Validation Failed**: The test passes even without the fix, which means it's not 
actually testing the bug that was reported. The test needs to be updated to properly 
validate the fix.

**Validation Steps Performed**:
1. Reverted fix files (kept tests)
2. Ran test - it PASSED (should have failed)
3. This indicates the test doesn't capture the bug

**Recommendation**: 
- Review the test implementation to ensure it actually triggers the bug condition
- Ensure AutomationIds are correct
- Verify the test is checking the right behavior
```

### ❌ Invalid Test - Fails Both Times

**Scenario**: Test fails both with and without the fix

**Problem**: Either the test is broken or the fix doesn't actually work

**Action**: Request changes:
```markdown
❌ **Test Validation Failed**: The test fails both with and without the fix.

**Possible causes**:
1. The fix doesn't actually resolve the issue
2. The test has a bug (wrong AutomationId, timing issue, etc.)
3. There's a platform-specific issue

**Next Steps**:
- Verify the fix actually resolves the reported issue manually
- Debug why the test is failing with the fix in place
- Check test logs for specific failure reasons
```

## Platform-Specific Validation

### When to Test Each Platform

**RTL Issues (FlowDirection)**:
- Minimum: Test on platforms listed in `PlatformAffected` attribute
- Recommended: Test on both Android and iOS if both are affected

**Platform-Specific Issues**:
- Only test on the affected platform

**Cross-Platform Issues**:
- Test on at least one platform (preferably the primary affected platform)
- Ideally test on multiple platforms to ensure consistency

### Platform Selection Guide

| Issue Type | Platforms to Test |
|------------|------------------|
| RTL (Android + iOS affected) | Android (primary), iOS (recommended) |
| Android-only | Android only |
| iOS-only | iOS only |
| Layout issues | At least one platform, preferably multiple |
| Control-specific | Platform where issue is most reproducible |

## Common Validation Issues

### Issue 1: Test Timing Problems

**Symptom**: Test fails intermittently or only in CI

**Solution**:
```csharp
// Ensure proper waits
App.WaitForElement("ButtonId"); // Wait before interacting

// For animations or state changes
await Task.Delay(500); // May need to add delays in test page
```

### Issue 2: Wrong AutomationId

**Symptom**: Test can't find elements

**Solution**:
```csharp
// In test page - ensure AutomationId is set
var button = new Button
{
    Text = "Click Me",
    AutomationId = "TestButton"  // Must match test
};

// In test - use exact same ID
App.WaitForElement("TestButton"); // Must match exactly
```

### Issue 3: Test Not Actually Triggering Bug

**Symptom**: Test passes without fix

**Solution**: Review the bug report and ensure the test actually reproduces the issue:
- Check if the right property is being set
- Verify the bug condition is being triggered
- Ensure visual changes are being validated with `VerifyScreenshot()`

### Issue 4: Platform Differences

**Symptom**: Test fails on one platform but passes on another

**Solution**: 
- Check if platform-specific code is correct
- Verify screenshot baselines exist for all platforms
- Consider platform-specific conditionals if behavior legitimately differs

## Validation Checklist

Before approving a PR with UI tests:

- [ ] Identified test files vs fix files
- [ ] Created temporary commit of PR state
- [ ] Reverted ONLY fix files (kept tests)
- [ ] Built and ran test without fix
- [ ] Verified test FAILED without fix
- [ ] Restored fix
- [ ] Built and ran test with fix
- [ ] Verified test PASSED with fix
- [ ] Documented validation results in PR review
- [ ] If test is invalid, provided specific feedback on how to fix it

## Automation Opportunity

This process could potentially be automated in CI:
1. Detect PRs with new UI tests
2. Run tests with fix files temporarily reverted
3. Verify tests fail as expected
4. Comment on PR with validation results

Until automated, this is a **manual reviewer responsibility**.

## Examples

### Example 1: Valid RTL Test Validation

```bash
# PR #32333 - RTL label padding fix

# Step 1: Save state
git add -A
git commit -m "temp: PR 32333 for validation"

# Step 2: Revert fix files only
git checkout HEAD~1 -- src/Core/src/Platform/Android/TextViewExtensions.cs
git checkout HEAD~1 -- src/Core/src/Platform/iOS/MauiLabel.cs

# Step 3: Run test (should FAIL)
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj \
  -f net10.0-android -t:Run
dotnet test src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj \
  --filter "FullyQualifiedName~Issue32316"

# Result: Test FAILED ✅ (proves test detects bug)

# Step 4: Restore fix
git reset --hard HEAD

# Step 5: Run test (should PASS)
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj \
  -f net10.0-android -t:Run
dotnet test src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj \
  --filter "FullyQualifiedName~Issue32316"

# Result: Test PASSED ✅ (proves fix works)

# Conclusion: Valid test, ready to approve
```

### Example 2: Invalid Test (Passes Without Fix)

```bash
# Step 3 result: Test PASSED (should have FAILED)
# This means the test doesn't actually validate the fix

# Review feedback:
"The test passes even without the fix applied. This indicates the test 
is not properly validating the bug fix. Please review the test to ensure 
it actually reproduces the reported issue."
```

## Quick Reference

**One-liner validation for experienced reviewers:**

```bash
# Save, revert fix, test (should fail), restore, test (should pass)
git add -A && git commit -m "temp" && \
git checkout HEAD~1 -- <fix-files> && \
dotnet test <test-project> --filter "~IssueXXXX" && \
git reset --hard HEAD && \
dotnet test <test-project> --filter "~IssueXXXX"
```

Replace `<fix-files>` and `<test-project>` with actual paths.

## Summary

Valid UI tests are critical for preventing regressions. The revert testing process ensures:
- Tests actually capture the bugs they claim to fix
- Fixes actually resolve the issues
- Future code changes won't reintroduce the bug

**Remember**: A test that passes without the fix is worse than no test at all - it provides false confidence.
