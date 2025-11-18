# Testing Guidelines

## Which App to Use for Testing

**CRITICAL DISTINCTION**: There are two testing apps in the repository, and choosing the wrong one wastes significant time (20+ minutes for unnecessary builds).

**🟢 Sandbox App (`src/Controls/samples/Controls.Sample.Sandbox/`) - USE THIS FOR PR VALIDATION**

**When to use**:
- ✅ **DEFAULT**: Validating PR changes and testing scenarios
- ✅ Reproducing the issue described in the PR
- ✅ Testing edge cases not covered by the PR author
- ✅ Comparing behavior WITH and WITHOUT PR changes
- ✅ Instrumenting code to capture measurements
- ✅ Any time you're validating if a fix actually works
- ✅ Manual testing of the PR's scenario

**Why**: 
- Builds in ~2 minutes (fast iteration)
- Simple, empty app you can modify freely
- Easy to instrument and capture measurements
- Designed for quick testing and validation

**🔴 TestCases.HostApp (`src/Controls/tests/TestCases.HostApp/`) - DO NOT USE FOR PR VALIDATION**

**When to use**:
- ❌ **NEVER** for validating PR changes or testing scenarios
- ✅ **ONLY** when explicitly asked to write UI tests
- ✅ **ONLY** when explicitly asked to validate UI tests
- ✅ **ONLY** when running automated Appium tests via `dotnet test`

**Why NOT to use for PR validation**:
- Takes 20+ minutes to build for iOS (extremely slow)
- Contains 100+ test pages (complex, hard to modify)
- Designed for automated UI tests, not manual validation
- Running automated tests is not part of PR review (that's what CI does)

**Decision Tree**:

```
User asks to review PR #XXXXX
    │
    ├─ User explicitly says "write UI tests" or "validate the UI tests"?
    │   └─ YES → Use TestCases.HostApp (and TestCases.Shared.Tests)
    │
    └─ Otherwise (normal PR review with testing)?
        └─ Use Sandbox app for validation
```

**Examples**:

✅ **Use Sandbox app**:
- "Review PR #32372" 
- "Validate the RTL CollectionView fix"
- "Test this SafeArea change on iOS"
- "Review and test this PR"
- "Does this fix actually work?"
- "Compare before/after behavior"

❌ **Use TestCases.HostApp** (only for these explicit requests):
- "Write UI tests for this PR"
- "Validate the UI tests in this PR work correctly"
- "Run the automated UI tests"
- "Create an Issue32372.xaml test page"

**Rule of Thumb**: 
- **Validating the PR's fix** = Sandbox app (99% of reviews)
- **Writing/validating automated tests** = TestCases.HostApp (1% of reviews, only when explicitly asked)

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

## Setup Test Environment

**iOS Testing**:
```bash
# Find iPhone Xs with highest iOS version
UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')

# Check UDID was found
if [ -z "$UDID" ] || [ "$UDID" = "null" ]; then
    echo "❌ ERROR: No iPhone Xs simulator found. Please create one."
    exit 1
fi

# Boot simulator
xcrun simctl boot $UDID 2>/dev/null || true

# Check simulator is booted
STATE=$(xcrun simctl list devices --json | jq -r --arg udid "$UDID" '.devices[][] | select(.udid == $udid) | .state')
if [ "$STATE" != "Booted" ]; then
    echo "❌ ERROR: Simulator failed to boot. Current state: $STATE"
    exit 1
fi
```

**Android Testing**:
```bash
# Get connected device/emulator
export DEVICE_UDID=$(adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1)

# Check device was found
if [ -z "$DEVICE_UDID" ]; then
    echo "❌ ERROR: No Android device/emulator found. Start an emulator or connect a device."
    exit 1
fi
```

## Build and Deploy

**iOS**:
```bash
# Build
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-ios

# Check build succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build failed"
    exit 1
fi

# Install
xcrun simctl install $UDID artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-ios/iossimulator-arm64/Maui.Controls.Sample.Sandbox.app

# Check install succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: App installation failed"
    exit 1
fi

# Launch with console capture
xcrun simctl launch --console-pty $UDID com.microsoft.maui.sandbox > /tmp/ios_test.log 2>&1 &

# Check launch didn't immediately fail
if [ $? -ne 0 ]; then
    echo "❌ ERROR: App launch failed"
    exit 1
fi

sleep 8
cat /tmp/ios_test.log
```

**Android**:
```bash
# Build and deploy
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-android -t:Run

# Check build/deploy succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build or deployment failed"
    exit 1
fi

# Monitor logs
adb logcat | grep -E "(YourMarker|Frame|Console)"
```

## Success Verification Points

After each major step, verify success before proceeding to the next step:

**After PR Fetch:**
- ✅ Confirm branch `test-pr-[NUMBER]` exists: `git branch --list test-pr-*`
- ✅ Verify PR commits are present: `git log --oneline -5`
- ✅ Check you're on the test branch: `git branch --show-current`

**After Sandbox Modification:**
- ✅ Files modified: `MainPage.xaml` and `MainPage.xaml.cs`
- ✅ Instrumentation code includes `Console.WriteLine` statements
- ✅ Test scenario matches PR description
- ✅ If uncertain about test approach, consider using validation checkpoint

**After Build:**
- ✅ Build succeeded with no errors (warnings are OK)
- ✅ Artifact exists:
  - iOS: `artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-ios/iossimulator-arm64/Maui.Controls.Sample.Sandbox.app`
  - Android: `artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-android/*/com.microsoft.maui.sandbox-Signed.apk`
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
   # On test-pr-XXXXX branch, temporarily revert the PR commits
   # Identify how many commits came from the PR
   NUM_COMMITS=$(git log --oneline pr-reviewer..HEAD | wc -l)
   
   # Create a temporary branch at the commit before PR changes
   git checkout -b baseline-test HEAD~$NUM_COMMITS
   
   # Build and test to capture baseline data
   ```

2. **Capture baseline data** (build, deploy, run with instrumentation)

3. **Then**: Test WITH PR changes
   ```bash
   # Switch back to test branch with PR changes
   git checkout test-pr-XXXXX
   
   # Build and test with PR changes
   ```

4. **Capture new data** (build, deploy, run with instrumentation)

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
**Test Scenario**: [Description]

**WITHOUT PR (Current Main)**:
```
[Actual console output or measurements]
```
❌ Issue: [What's wrong]

**WITH PR Changes**:
```
[Actual console output or measurements]
```
✅ Result: [What changed]
```

## Cleanup

After testing, clean up all test artifacts:

```bash
# Return to your original branch (use the variable from the beginning)
git checkout $ORIGINAL_BRANCH  # Or manually specify: main, pr-reviewer, etc.

# Revert any changes to Sandbox app
git checkout -- src/Controls/samples/Controls.Sample.Sandbox/

# Delete test branches
git branch -D test-pr-XXXXX baseline-test pr-XXXXX-temp 2>/dev/null || true

# Clean build artifacts if needed
dotnet clean
```

**Important**: If you didn't save `$ORIGINAL_BRANCH` at the start, replace it with whatever branch you were on when you began the review. This ensures you return to your starting state.

## Edge Case Discovery

**CRITICAL**: Don't just test the PR author's scenario. Test edge cases they may have missed.

### Required edge cases for UI/Layout PRs:

- **Empty state**: Empty collections, null values, no data
- **Single item**: Collections with exactly one item
- **Large data sets**: 100+ items to test scrolling/virtualization
- **Dynamic changes**: Rapidly toggle properties (e.g., toggle FlowDirection 10 times)
- **Property combinations**: Test the fix with other properties (e.g., RTL + IsVisible + Margin)
- **Nested scenarios**: Control inside control (e.g., CollectionView in ScrollView)
- **Platform-specific**: Test on all affected platforms (iOS, Android, Windows, Mac)
- **Orientation**: Portrait vs landscape (mobile/tablet)
- **Screen sizes**: Different screen sizes and densities

### For layout/positioning PRs, also test:

- **Header/Footer**: With and without headers/footers
- **Padding/Margin**: Various padding and margin combinations
- **Alignment**: Different HorizontalOptions/VerticalOptions
- **Parent constraints**: Different parent sizes and constraints

### For behavior/interaction PRs, also test:

- **Timing**: Rapid interactions, delayed interactions
- **State transitions**: Page appearing/disappearing, backgrounding/foregrounding
- **User interaction**: Tap, scroll, swipe during state changes

### Document findings:

For each edge case tested, document:
- What you tested
- Expected behavior
- Actual behavior
- Whether it works correctly or reveals an issue

## SafeArea Testing Guidelines

### Special Case: SafeArea Testing

**If the PR modifies SafeAreaEdges, SafeAreaRegions, or related safe area handling code:**

1. **CRITICAL: Read `.github/instructions/safearea-testing.instructions.md` FIRST** before setting up your test
2. **Measure CHILD content position**, not the parent container with SafeAreaEdges
3. **Calculate gaps from screen edges** to detect padding
4. **Use colored backgrounds** (red parent, yellow child) for visual validation

**Why this is critical**: SafeArea bugs are subtle. The parent container size stays constant - only the child content position changes. Measuring the wrong element will show no difference even when the bug exists.

See `.github/instructions/safearea-testing.instructions.md` for comprehensive guidance including:
- The "measure children, not parents" principle with visual diagrams
- Complete XAML and instrumentation code examples
- How to interpret gap measurements
- Common mistakes to avoid
- When to use the validation checkpoint for SafeArea testing
