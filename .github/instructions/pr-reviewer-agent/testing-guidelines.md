⚠️ **CRITICAL**: Read this ENTIRE file before creating any plans or taking any actions

---

# Testing Guidelines for PR Review

## 🎯 The #1 Rule: Which App to Use

### Default Answer: **Sandbox App**

Use `src/Controls/samples/Controls.Sample.Sandbox/` for PR validation **UNLESS** you are explicitly asked to write or validate UI tests.

### Quick Decision Tree:

```
Are you writing/debugging UI tests? 
├─ YES → Use TestCases.HostApp
└─ NO  → Use Sandbox app ✅ (99% of PR reviews)
```

### ⚠️ Common Confusion: "But the PR has test files!"

**Scenario**: PR adds files to `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXX.cs`

❌ **WRONG THINKING**: "The PR adds test files to HostApp, so I should use HostApp"
✅ **RIGHT THINKING**: "The PR adds automated test files. I use Sandbox to manually validate the fix."

**Why**: 
- Those test files are for the AUTOMATED UI testing framework
- You are doing MANUAL validation with real testing
- HostApp is only needed when writing/debugging those automated tests

### 💰 Cost of Wrong App Choice

**Using HostApp when you should use Sandbox:**
- ⏱️ Wasted time: 15+ minutes building
- 📦 Unnecessary complexity: 1000+ tests in project
- 🐛 Harder debugging: Can't isolate behavior
- 😞 User frustration: Obvious mistake

**Using Sandbox (correct choice):**
- ⏱️ Fast builds: 2-3 minutes
- 🎯 Focused testing: Only your test code
- 🔍 Easy debugging: Clear isolation
- ✅ Professional approach

### 📋 App Selection Reference

| Scenario | Correct App | Why |
|----------|------------|-----|
| Validating PR fix | Sandbox ✅ | Quick, isolated, easy to instrument |
| Testing before/after comparison | Sandbox ✅ | Can modify without affecting tests |
| User says "review this PR" | Sandbox ✅ | Default for all PR validation |
| User says "write a UI test" | HostApp ✅ | That's what HostApp is for |
| User says "validate the UI test" | HostApp ✅ | Testing the test itself |
| PR adds test files | Sandbox ✅ | Test files ≠ what you test with |
| Unsure which to use | Sandbox ✅ | When in doubt, default here |

---

## Which App to Use for Testing (Detailed)

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

**Configuration**: Edit `src/Controls/samples/Controls.Sample.Sandbox/MauiProgram.cs` - see [Handler Detection Guide](collectionview-handler-detection.md#configuration-examples) for complete code.

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

**After creating test code, STOP and post this to user:**

```markdown
## Validation Checkpoint - Before Building

**Test code created** (Sandbox app modified):

**XAML**:
```xml
[Show relevant XAML snippet - what you're testing]
```

**Instrumentation**:
```csharp
[Show measurement code - what you'll capture]
```

**What I'm measuring**: [Clear explanation of data you'll collect]

**Expected WITHOUT PR**: [Baseline behavior/measurements]
**Expected WITH PR**: [How it should change with the fix]

**Build time**: ~10-15 minutes

Should I proceed with building?
```

**⚠️ DO NOT BUILD without user approval** - Building takes 10-15 minutes. If test design is wrong, this checkpoint saves that wasted time.

**User can correct at this point**:
- Wrong test approach
- Missing test cases
- Measuring wrong thing
- Wrong app choice

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
