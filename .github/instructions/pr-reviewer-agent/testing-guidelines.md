⚠️ **CRITICAL**: Read this ENTIRE file before creating any plans or taking any actions

---

# Testing Guidelines for PR Review

## 🎯 The #1 Rule: Testing Approach

### Default Answer: **HostApp with UI Tests**

Use TestCases.HostApp + TestCases.Shared.Tests for PR validation. Create test pages and NUnit tests to validate the PR.

### Quick Decision Tree:

```
What is the user asking you to do?
│
├─ "Review this PR" ────────────────────────────────────────────────────┐
├─ "Test this fix" ─────────────────────────────────────────────────────┤
├─ "Validate PR #XXXXX" ────────────────────────────────────────────────┤
├─ "Check if this works" ───────────────────────────────────────────────┼──→ Use HostApp UI tests ✅
├─ "Does this PR fix the issue?" ───────────────────────────────────────┤
└─ [Any other PR validation request] ───────────────────────────────────┘
│
├─ "Write UI tests" ────────────────────────────────────────────────────┐
├─ "Debug UI test" ─────────────────────────────────────────────────────┼──→ Delegate to uitest-coding-agent ✅
└─ "Validate UI test code" ─────────────────────────────────────────────┘
```

**Key Insight**: Always use HostApp with UI tests for PR validation.

### Testing Workflow

**Standard approach for all PR reviews:**

1. **Create test page** in `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml`
2. **Create NUnit test** in `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`
3. **Run BuildAndRunHostApp.ps1** to build, deploy, and execute tests
4. **Compare WITH/WITHOUT PR changes** to validate the fix

**If user asks to "write UI tests" or "debug UI tests":**
- Delegate to `uitest-coding-agent` (that's their specialty)

### 💰 Benefits of HostApp UI Testing

**Using HostApp with UI tests (correct approach):**
- ✅ **Reusable tests**: Your validation tests can become permanent automated tests
- ✅ **Consistent with CI**: Same testing infrastructure CI uses
- ✅ **Professional**: Proper UI testing framework, not ad-hoc scripts
- ✅ **Comprehensive**: Full Appium-based testing with proper assertions
- ✅ **Maintainable**: Tests are structured and follow established patterns

### 📋 Testing Approach Reference

| Scenario | Approach | Why |
|----------|----------|-----|
| Validating PR fix | HostApp UI tests ✅ | Standard testing infrastructure |
| Testing before/after comparison | HostApp UI tests ✅ | Proper test framework with assertions |
| User says "review this PR" | HostApp UI tests ✅ | Default for all PR validation |
| User says "write UI tests" | Delegate to uitest-coding-agent ✅ | Their specialty |
| User says "debug UI test" | Delegate to uitest-coding-agent ✅ | Their specialty |
| PR already has test files | Still create your own test ✅ | Validate the fix independently |
| Unsure what to do | HostApp UI tests ✅ | When in doubt, default here |

---

## UI Testing Infrastructure for PR Validation

**STANDARD APPROACH**: Use TestCases.HostApp + TestCases.Shared.Tests for all PR validation.

### How It Works

**Two-part requirement:**
1. **Test Page** (`src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml`) - Creates the UI scenario
2. **NUnit Test** (`src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`) - Validates behavior with Appium

### Running Tests

Use the BuildAndRunHostApp.ps1 script:

```powershell
# Android
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "FullyQualifiedName~IssueXXXXX"

# iOS  
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "FullyQualifiedName~IssueXXXXX"
```

**What the script does:**
- ✅ Builds TestCases.HostApp
- ✅ Deploys to device/simulator
- ✅ Runs your NUnit test via `dotnet test`
- ✅ Captures all logs to `CustomAgentLogsTmp/UITests/`

### When to Delegate

**If user asks "write UI tests" or "debug UI tests":**
- Delegate to `uitest-coding-agent` (that's their specialty)
- They handle the test authoring workflow

**For PR validation (default):**
- You create the test page and NUnit test yourself
- You run BuildAndRunHostApp.ps1 to validate the PR

### Examples

✅ **You handle (use HostApp UI tests)**:
- "Review PR #32372"
- "Validate the RTL CollectionView fix"
- "Test this SafeArea change on iOS"
- "Does this fix actually work?"
- "Compare before/after behavior"

✅ **Delegate to uitest-coding-agent**:
- "Write comprehensive UI tests for this PR"
- "Debug the failing UI test for Issue32310"
- "Create proper automated test coverage"

**Rule of Thumb**: 
- **Validating the PR's fix** = You create and run UI tests (99% of reviews)
- **Writing complex UI test suites** = Delegate to uitest-coding-agent (1% of reviews)

---

## 🎛️ CollectionView/CarouselView Handler Detection

**CRITICAL**: If the PR affects CollectionView or CarouselView, you MUST determine which handler implementation to enable before testing.

See **[CollectionView Handler Detection Guide](collectionview-handler-detection.md)** for complete algorithm, configuration examples, and platform-specific notes.

### Quick Summary

**Detection pattern**:
```bash
# Check which handler files were changed
git diff <base>..<pr> --name-only | grep "Handlers/Items"

# Look at path:
# "Handlers/Items/" (NOT "Items2") → CollectionViewHandler
# "Handlers/Items2/" → CollectionViewHandler2
```

**Why this matters**: iOS/MacCatalyst defaults to CollectionViewHandler2. If a PR fixes a bug in CollectionViewHandler, you MUST explicitly enable it or the bug won't reproduce.

**Configuration**: Edit `src/Controls/tests/TestCases.HostApp/MauiProgram.cs` - see [Handler Detection Guide](collectionview-handler-detection.md#configuration-examples) for complete code.

| Path Pattern | Handler to Enable |
|--------------|------------------|
| `Handlers/Items/` | CollectionViewHandler (original) |
| `Handlers/Items2/` | CollectionViewHandler2 (new) |

---

## Fetch PR Changes (Without Checking Out)

**CRITICAL**: Stay on your current branch (wherever you are when starting the review) to preserve context. Apply PR changes on top of the current branch instead of checking out the PR branch.

**FIRST STEP - Record Your Starting Branch:**
```bash
# Record what branch you're currently on - you'll need this for cleanup
ORIGINAL_BRANCH=$(git branch --show-current)
echo "Starting review from branch: $ORIGINAL_BRANCH"
# Remember this value for cleanup at the end!
```

```bash
# Get the PR number from the user's request
PR_NUMBER=XXXXX  # Replace with actual PR number

# Fetch the PR into a temporary branch
git fetch origin pull/$PR_NUMBER/head:pr-$PR_NUMBER-temp

# Check fetch succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Failed to fetch PR #$PR_NUMBER"
    exit 1
fi

# Create a test branch from current branch (preserves instruction files)
git checkout -b test-pr-$PR_NUMBER

# Check branch creation succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Failed to create test branch"
    exit 1
fi

# Merge the PR changes into the test branch
git merge pr-$PR_NUMBER-temp -m "Test PR #$PR_NUMBER" --no-edit

# Check merge succeeded (will error if conflicts)
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Merge failed with conflicts"
    echo "See section below on handling merge conflicts"
    exit 1
fi
```

**If merge conflicts occur:**
```bash
# See which files have conflicts
git status

# For simple conflicts, you can often accept the PR's version
git checkout --theirs <conflicting-file>
git add <conflicting-file>

# Complete the merge
git commit --no-edit
```

**Why this approach:**
- ✅ Preserves your current working context and branch state
- ✅ Tests PR changes on top of wherever you currently are
- ✅ Allows agent to maintain proper context across review
- ✅ Easy to clean up (just delete test branch and return to original branch)
- ✅ Can compare before/after easily
- ✅ Handles most conflicts gracefully

## ⚠️ MANDATORY Workflow with Checkpoints

### Workflow Overview

```
1. Fetch PR → Analyze code
2. Create test code
3. 🛑 CHECKPOINT 1 (MANDATORY): Show test code, get approval
4. Build & Deploy → Test WITHOUT fix  
5. Test WITH fix → Compare results
6. 🛑 CHECKPOINT 2 (Recommended): Show raw data
7. Write review → Eliminate redundancy
```

### 🛑 CHECKPOINT 1: Before Building (MANDATORY)

**After creating test page and NUnit test, STOP and post this to user:**

```markdown
## Validation Checkpoint - Before Building

**Test created in TestCases.HostApp:**

**Test Page XAML** (`Issues/IssueXXXXX.xaml`):
```xml
[Show relevant XAML snippet - UI elements with AutomationIds]
```

**NUnit Test** (`Tests/Issues/IssueXXXXX.cs`):
```csharp
[Show test logic - Appium interactions and assertions]
```

**What this test validates**: [Clear explanation of what behavior is being tested]

**Expected WITHOUT PR**: [Bug should reproduce]
**Expected WITH PR**: [Bug should be fixed]

**Build time**: ~10-15 minutes

Should I proceed with building and running the test?
```

**⚠️ DO NOT BUILD without user approval** - Building takes 10-15 minutes. If test design is wrong, this checkpoint saves that wasted time.

**User can correct at this point**:
- Wrong test approach
- Missing test cases
- Wrong AutomationIds
- Missing edge cases

### 🛑 CHECKPOINT 2: Before Final Review (Recommended)

**After testing, before posting review:**

```markdown
## Data Review Checkpoint

**Raw test results**:

WITHOUT PR:
```
[Console output/measurements]
```

WITH PR:
```
[Console output/measurements]
```

**My interpretation**: [What the data means]

**Draft recommendation**: [Approve/Request Changes/etc.]

Does this interpretation look correct?
```

## Setup Test Environment

**Complete command sequences**: See [quick-ref.md](quick-ref.md) for copy-paste workflows.

**Key patterns** (reference only - use quick-ref.md for actual commands):
- iOS: UDID extraction, boot, verify - See [quick-ref.md](quick-ref.md#ios-testing-complete-sequence)
- Android: Device detection, emulator startup - See [quick-ref.md](quick-ref.md#android-testing-complete-sequence)
- Full patterns with error checking: [common-testing-patterns.md](../common-testing-patterns.md)

**Important Android Rules**:
- ✅ **Start emulators with subshell + background**: `cd $ANDROID_HOME/emulator && (./emulator -avd Name ... &)`
- ❌ **NEVER use `adb kill-server`** - This disconnects active emulators and is almost never needed
- ❌ **NEVER use `mode="async"` for emulators** - They will be killed when the session ends
- ✅ **Check `adb devices` first** - If device is visible, no action needed

## Build and Deploy

**Complete command sequences**: See [quick-ref.md](quick-ref.md) for copy-paste workflows including error checking.

**Quick reference**:
- iOS build/deploy: [quick-ref.md](quick-ref.md#ios-testing-complete-sequence)
- Android build/deploy: [quick-ref.md](quick-ref.md#android-testing-complete-sequence)
- Error handling: [error-handling.md](error-handling.md)

## Success Verification Points

After each major step, verify success before proceeding to the next step:

**After PR Fetch:**
- ✅ Confirm branch `test-pr-[NUMBER]` exists: `git branch --list test-pr-*`
- ✅ Verify PR commits are present: `git log --oneline -5`
- ✅ Check you're on the test branch: `git branch --show-current`

**After Test Creation:**
- ✅ Test page created: `TestCases.HostApp/Issues/IssueXXXXX.xaml`
- ✅ NUnit test created: `TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`
- ✅ AutomationIds set on all interactive elements
- ✅ Test scenario matches PR description
- ✅ If uncertain about test approach, use validation checkpoint

**After Build:**
- ✅ Build succeeded with no errors (warnings are OK)
- ✅ Artifact exists:
  - iOS: `artifacts/bin/Controls.TestCases.HostApp/Debug/net10.0-ios/iossimulator-arm64/Controls.TestCases.HostApp.app`
  - Android: `artifacts/bin/Controls.TestCases.HostApp/Debug/net10.0-android/*/com.microsoft.maui.uitests-Signed.apk`
- ✅ No "0 succeeded, 1 failed" in build output

**After Deploy & Run:**
- ✅ App launched successfully (no crash on startup)
- ✅ Console output captured in log file or terminal
- ✅ Instrumentation output is visible in logs (search for "TEST OUTPUT" or your marker)
- ✅ Measurements show reasonable values (not all zeros or nulls)

**If any verification fails:**
- 🛑 **STOP immediately**
- 📝 Document what failed and the error message
- 🔍 Attempt to fix (1-2 attempts maximum)
- ❓ If still failing, ask for help

## Test WITH and WITHOUT PR Changes

1. **First**: Test WITHOUT PR changes
   ```bash
   # Revert the fix files to main branch state
   git checkout main -- src/path/to/changed/file.cs
   
   # Verify revert succeeded
   git diff main -- src/path/to/changed/file.cs  # Should be empty
   
   # Run test - should fail (bug reproduces)
   pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "FullyQualifiedName~IssueXXXXX"
   ```

2. **Capture baseline data** (test should fail, showing the bug exists)

3. **Then**: Test WITH PR changes
   ```bash
   # Restore PR changes
   git checkout HEAD -- src/path/to/changed/file.cs
   
   # Verify PR changes are back
   git diff main -- src/path/to/changed/file.cs  # Should show PR changes
   
   # Run test - should pass (bug is fixed)
   pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "FullyQualifiedName~IssueXXXXX"
   ```

4. **Capture new data** (test should pass, showing the fix works)

5. **Compare results** and include in review

6. **Clean up test branches**
   ```bash
   # Return to original branch (whatever branch you started on)
   git checkout $ORIGINAL_BRANCH
   
   # Delete test branches
   git branch -D test-pr-XXXXX baseline-test pr-XXXXX-temp
   ```
   
   **Note**: Uses `$ORIGINAL_BRANCH` variable you set at the beginning. If you didn't save it, replace with whatever branch you were on when you started the review (e.g., `main`, `pr-reviewer`, etc.)

## Include Test Results in Review

Format test data clearly:

```markdown
## Test Results

**Environment**: iOS 26.0 (iPhone 17 Pro Simulator)
**Test**: Issue XXXXX - [Description]
**Command**: `BuildAndRunHostApp.ps1 -Platform ios -TestFilter "FullyQualifiedName~IssueXXXXX"`

**WITHOUT PR (Baseline)**:
```
Test outcome: Failed
[Test output showing bug reproduces]
```
❌ Issue: [What's wrong - bug confirmed]

**WITH PR Changes**:
```
Test outcome: Passed
[Test output showing bug is fixed]
```
✅ Result: [Bug is fixed]

**Logs**: See `CustomAgentLogsTmp/UITests/` for full device logs and test output
```

## Cleanup

After testing, clean up all test artifacts:

```bash
# Return to your original branch (use the variable from the beginning)
git checkout $ORIGINAL_BRANCH  # Or manually specify: main, pr-reviewer, etc.

# Revert any changes to test files if you created temporary test pages
git checkout -- src/Controls/tests/TestCases.HostApp/
git checkout -- src/Controls/tests/TestCases.Shared.Tests/

# Delete test branches
git branch -D test-pr-XXXXX baseline-test pr-XXXXX-temp 2>/dev/null || true
```

**Important**: If you didn't save `$ORIGINAL_BRANCH` at the start, replace it with whatever branch you were on when you began the review. This ensures you return to your starting state.

## Time Budget Guidance

### Realistic Time Expectations

**Simple PRs** (property fix, single file): **30-45 minutes**
- Read & analyze: 5 min
- Create test: 5 min
- Checkpoint 1: 2 min (wait for approval)
- Build & test: 20 min
- Write review: 10 min

**Medium PRs** (bug fix, multiple files): **1-2 hours**
- Read & analyze: 10 min
- Create test: 15 min
- Checkpoint 1: 3 min (wait for approval)
- Build & test (WITH/WITHOUT): 40 min
- Edge cases: 20 min
- Write review: 15 min

**Complex PRs** (architecture change, SafeArea): **2-4 hours**
- Read & analyze: 20 min
- Create test with instrumentation: 30 min
- Checkpoint 1: 5 min (wait for approval)
- Build & test: 60 min
- Multiple edge cases: 60 min
- Write review: 30 min

**⚠️ Exceeding these times significantly?**
- Use [checkpoint-resume.md](checkpoint-resume.md) if blocked by environment
- Ask for help if stuck on errors/issues
- Consider if you're over-testing minor PRs

**✅ Have unlimited time**: But use it wisely. Don't waste time on wrong approaches.

## Edge Case Discovery

**CRITICAL**: Don't just test the PR author's scenario. Test edge cases they may have missed.

### 🔴 HIGH PRIORITY Edge Cases (Always Test)

Test these for every UI/Layout PR:

- **Empty state**: Empty collections, null values, no data
- **Property combinations**: Test the fix with other properties (e.g., RTL + IsVisible + Margin)
- **Platform-specific**: Test on all affected platforms (iOS, Android, Windows, Mac)

### 🟡 MEDIUM PRIORITY Edge Cases (Test If Time Permits)

Test these for complex PRs or if time allows:

- **Single item**: Collections with exactly one item
- **Dynamic changes**: Rapidly toggle properties (e.g., toggle FlowDirection 10 times)
- **Nested scenarios**: Control inside control (e.g., CollectionView in ScrollView)
- **Large data sets**: 100+ items to test scrolling/virtualization

### ⚫ LOW PRIORITY Edge Cases (Optional)

Test these only if you have extra time or PR is particularly risky:

- **Orientation**: Portrait vs landscape (mobile/tablet)
- **Screen sizes**: Different screen sizes and densities
- **State transitions**: Page appearing/disappearing, backgrounding/foregrounding

### For Layout/Positioning PRs, Also Consider:

**🔴 High**: Padding/Margin combinations, Parent constraints
**🟡 Medium**: Header/Footer presence, Alignment options
**⚫ Low**: Different parent sizes

### For Behavior/Interaction PRs, Also Consider:

**🔴 High**: Timing of interactions
**🟡 Medium**: Rapid interactions
**⚫ Low**: User interaction during state changes

### Document Findings

For each edge case tested, document:
- What you tested
- Expected behavior
- Actual behavior
- Whether it works correctly or reveals an issue

**Time management**: Start with high priority edge cases. If you're approaching time budget limits, document which edge cases you couldn't test and note in review.

## 🛑 Manual Verification Required When Testing Unavailable

**CRITICAL**: If you cannot run tests locally **after attempting all available solutions**, you MUST pause and create a manual verification checkpoint.

### When to Create Manual Verification Checkpoint

Create a checkpoint when:
- ❌ **Android emulator fails to start** (after following startup sequence from quick-ref.md)
- ❌ **Platform unavailable** (iOS on Linux, Windows on macOS, Mac on Windows)
- ❌ **Cannot interact with app UI** (crashes immediately, unresponsive)
- ❌ **Need visual verification** (colors, layouts, animations, positioning)
- ❌ **Environment issues** (`$ANDROID_HOME` not set, no AVDs available, SDK issues)

### 🚨 What You MUST Try First

**DO NOT create checkpoint until you've attempted these**:

**For Android**:
1. ✅ Check for device: `adb devices`
2. ✅ Attempt emulator startup (see quick-ref.md for commands)
3. ✅ Wait for full boot sequence
4. ✅ Check emulator logs if startup fails: `cat /tmp/emulator.log`

**For iOS**:
1. ✅ Check for simulators: `xcrun simctl list devices`
2. ✅ Attempt to boot simulator
3. ✅ Verify simulator state

**Only after trying these steps and hitting a genuine blocker**, create the checkpoint.

### Checkpoint Template: Cannot Validate PR

```markdown
## 🛑 CHECKPOINT: Cannot Validate PR on [Platform]

**I cannot interact with the app in this environment to validate the PR changes.**

### What I Attempted

**Platform**: [Android/iOS/Windows/Mac]

**Steps taken**:
- ✅ Checked for device: `adb devices` → [result]
- ✅ Attempted emulator startup: [command used]
- ❌ **Blocker**: [specific error or unavailability reason]

**Evidence**:
```bash
[Show actual command output, error messages, logs]
```

### PR Analysis

**PR #XXXXX**: [Brief description]

**Files changed**:
- `[file path]` - [what changed]
- `[file path]` - [what changed]

**What the fix does** (based on code analysis):
[Explain the changes in plain language]

**Expected behavior WITH fix**:
[What should happen after the fix]

**Expected behavior WITHOUT fix** (the bug):
[What currently happens that's wrong]

### Manual Verification Steps Needed

**Platform**: [Android/iOS/Windows/Mac]

**🚨 MANDATORY: Use BuildAndRunHostApp.ps1 Script**

There is **ONLY ONE WAY** to test HostApp UI tests:

```powershell
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform [android|ios] -TestFilter "FullyQualifiedName~IssueXXXXX"
```

**Do NOT do these manually**:
- ❌ `dotnet build` commands
- ❌ `dotnet test` commands manually
- ❌ Manually start Appium
- ❌ Any build/deploy steps by hand

**The script does EVERYTHING**:
- ✅ Detects and boots devices
- ✅ Builds and deploys TestCases.HostApp
- ✅ Runs your NUnit test via `dotnet test`
- ✅ Captures all logs to `CustomAgentLogsTmp/UITests/`

**To verify this PR works, you'll need to**:

1. **Review the test page**: `TestCases.HostApp/Issues/IssueXXXXX.xaml`
   - Verify it reproduces the issue scenario
   
2. **Review the NUnit test**: `TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`
   - Verify it validates the expected behavior

3. **Run the automated script**:
   ```powershell
   pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform [android|ios] -TestFilter "FullyQualifiedName~IssueXXXXX"
   ```

4. **Reproduce the original issue** (test WITHOUT PR):
   - Revert fix files: `git checkout main -- [fix files]`
   - Run test - should FAIL (confirms bug exists)

5. **Verify the fix works** (test WITH PR):
   - Restore PR: `git checkout HEAD -- [fix files]`
   - Run test - should PASS (confirms fix works)

6. **Test edge cases** (if applicable):
   - [Edge case 1 to test]
   - [Edge case 2 to test]

### What I Need From You

Please verify:
- [ ] The original issue reproduces as described (confirms bug understanding)
- [ ] PR changes fix the issue (confirms fix works)
- [ ] No regressions in similar functionality
- [ ] Behavior matches expectations

**Once you confirm the fix works**, I will:
- Provide comprehensive review feedback
- Note any code quality observations
- Recommend approval or suggest improvements

**If the fix doesn't work as expected**, I will:
- Revise my analysis
- Suggest alternative approaches
- Request clarification from PR author
```

### DO NOT Skip Testing Without Checkpoint

**❌ WRONG**:
```
"I can't test on Android, so I'll just do a code review."
"No emulator available, I'll approve based on code analysis."
"Let's skip testing and trust the CI."
```

**✅ RIGHT**:
```
"I attempted to start Android emulator but it failed (see checkpoint).
I've created a comprehensive test plan for you to verify manually."
```

### Cost of Skipping This Step

- **Without checkpoint**: PR approved with no validation → Broken code merges
- **With checkpoint**: User validates → High confidence in PR quality

---

## SafeArea Testing Guidelines

### Special Case: SafeArea Testing

**If the PR modifies SafeAreaEdges, SafeAreaRegions, or related safe area handling code:**

1. **CRITICAL: Read `.github/instructions/safearea-testing.md` FIRST** before setting up your test
2. **Measure CHILD content position**, not the parent container with SafeAreaEdges
3. **Calculate gaps from screen edges** to detect padding
4. **Use colored backgrounds** (red parent, yellow child) for visual validation

**Why this is critical**: SafeArea bugs are subtle. The parent container size stays constant - only the child content position changes. Measuring the wrong element will show no difference even when the bug exists.

See `.github/instructions/safearea-testing.md` for comprehensive guidance including:
- The "measure children, not parents" principle with visual diagrams
- Complete XAML and instrumentation code examples
- How to interpret gap measurements
- Common mistakes to avoid
- When to use the validation checkpoint for SafeArea testing
