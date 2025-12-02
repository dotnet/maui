---
description: "5-minute quick start guide for PR review agents"
---

# PR Reviewer Quick Start (5 Minutes)

This is your fast-track guide to starting a PR review. Read this first, reference other files as needed.

---

## ⚡ Pre-Flight Checklist (30 seconds)

```bash
# 1. Check where you are
git branch --show-current

# 2. Verify you have the PR number
# User request should mention PR #XXXXX
```

**Output**: You know your starting branch and the PR number.

---

## 📖 Essential Reading (3 minutes)

Read these in order:

1. **Testing Approach** (30 seconds)
   - ✅ **HostApp with UI tests** = PR validation (DEFAULT for all reviews)
   - ✅ Create test page in TestCases.HostApp
   - ✅ Create NUnit test in TestCases.Shared.Tests
   - ✅ Run BuildAndRunHostApp.ps1 to build, deploy, and test
   
   **Decision**: If user says "review PR" or "test this fix" → Use HostApp UI testing
   
   **If user asks to "write UI tests" or "debug UI tests":**
   - Delegate to `uitest-coding-agent` (that's their specialty)
   
   See [testing-guidelines.md](testing-guidelines.md#testing-approach) for details.

2. **Workflow Overview** (2 minutes)
   ```
   1. Fetch PR → Analyze code
   2. Create test code → CHECKPOINT (show user, get approval)
   3. Build & Deploy → Test WITHOUT fix
   4. Test WITH fix → Compare results
   5. Write review → Eliminate redundancy
   ```

3. **UI Interaction Rule** (10 seconds)
   - ✅ **Appium** = ALL user interactions (tapping, scrolling, gestures)
   - ❌ **ADB/xcrun** = Only for device setup and log monitoring
   
   **Decision**: If you need to interact with app UI → Use Appium script
   
   See [appium-control.md](../appium-control.md) for complete guide.

4. **Special Cases** (30 seconds - read if applicable)
   - CollectionView/CarouselView PR? → Read [collectionview-handler-detection.md](collectionview-handler-detection.md)
   - SafeArea PR? → Read [safearea-testing.md](../safearea-testing.md)
   - UI test files in PR? → Read [uitests.instructions.md](../uitests.instructions.md)

---

---

## 🛑 Stop and Ask Yourself

**Before proceeding, answer this question:**

**Q: What testing approach am I using for this PR validation?**

- ✅ If you answered "HostApp with UI tests" → Correct! Proceed.
- ❌ If you answered "Manual testing" or "code-only review" → WRONG! Re-read Testing Approach above.
- ❓ If you're unsure → Default to HostApp UI testing

**Always use the UI testing infrastructure** for PR validation.

---

## 🚀 Start Working (90 seconds)

### Step 1: Fetch the PR (30 seconds)

```bash
# Save starting point
ORIGINAL_BRANCH=$(git branch --show-current)
echo "Starting from: $ORIGINAL_BRANCH"

# Fetch PR
PR_NUMBER=XXXXX  # Replace with actual number
git fetch origin pull/$PR_NUMBER/head:pr-$PR_NUMBER-temp
git checkout -b test-pr-$PR_NUMBER
git merge pr-$PR_NUMBER-temp -m "Test PR #$PR_NUMBER" --no-edit
```

### Step 2: Understand the PR (30 seconds)

- Read PR description
- Check linked issues
- Review changed files: `git diff $ORIGINAL_BRANCH..HEAD --name-only`

### Step 3: Create Initial Plan (30 seconds)

Post this to user:

```markdown
## Initial Assessment

**PR #XXXXX**: [Brief description of what it fixes]

**Testing approach**:
- Using HostApp with UI tests
- Will create test page in TestCases.HostApp
- Will create NUnit test in TestCases.Shared.Tests
- Platforms: [iOS/Android/both]
- Plan to compare WITH/WITHOUT PR changes

**Next step**: Creating test code, will show before building.

Proceed? Any concerns about this approach?
```

**⚠️ CRITICAL**: Wait for user response before continuing.

---

## 🛑 Mandatory Checkpoints

### Checkpoint 1: STOP AND ASK BEFORE BUILDING (MANDATORY)

**🚨 CRITICAL RULE: NEVER build without showing your plan and getting approval.**

After creating test code, **STOP and ask**:

```markdown
## 🛑 Checkpoint 1: Show Me Your Plan

I've created UI test code to validate this PR. Before I build (which takes 10-15 minutes), here's my approach:

**Test Page** (`TestCases.HostApp/Issues/IssueXXXXX.xaml`):
```xml
[Show relevant XAML snippet with AutomationIds]
```

**NUnit Test** (`TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`):
```csharp
[Show test logic with Appium interactions and assertions]
```

**Validation approach**:
- What the test validates: [Specific behavior being tested]
- How test verifies: [Appium element queries and assertions]
- Test sequence: [Steps the test will perform]

**Expected results**:
- WITHOUT PR fix: Test should FAIL (bug reproduces)
- WITH PR fix: Test should PASS (bug is fixed)

**Should I proceed with building?** (This will take 10-15 minutes)
```

**Why this checkpoint is mandatory**:
- ❌ Building wrong test wastes 10-15 minutes
- ❌ Measuring wrong things wastes entire test cycle
- ✅ User validates approach before expensive operation
- ✅ Catches mistakes early

**NEVER build without explicit approval at this checkpoint.**

### Checkpoint 2: Before Final Review (Optional but Recommended)

Show raw data and ask if interpretation is correct.

---

## 🔄 Reverting PR Changes for Testing

**Goal**: Test WITH and WITHOUT the PR changes to prove the fix works.

### ⚠️ Common Mistake: Partial Revert

**❌ WRONG Approach**:
```bash
# Don't just comment out "the fix" - PRs often make multiple changes
# This leaves PR infrastructure in place, giving false results
```

**✅ CORRECT Approach**:
```bash
# 1. First, see ALL changes the PR made
git diff main..HEAD -- path/to/changed/file.cs

# 2. Read the full diff - understand EVERYTHING that changed
# Don't assume you know what changed without reading it

# 3. Revert the entire file to main branch state
git checkout main -- path/to/changed/file.cs

# 4. Verify revert was complete (should show NO diff)
git diff main -- path/to/changed/file.cs
```

### Testing Workflow

**Phase 1: Test WITHOUT PR (baseline)**
```bash
# Revert to main branch code
git checkout main -- src/path/to/changed/file.cs

# Verify no differences remain
git diff main -- src/path/to/changed/file.cs  # Should be empty

# Build and test using automated script
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform [android|ios] -TestFilter "FullyQualifiedName~IssueXXXXX"

# Document results: "Bug reproduces"
```

**Phase 2: Test WITH PR (fix)**
```bash
# Restore PR changes
git checkout HEAD -- src/path/to/changed/file.cs

# Verify PR changes are back
git diff main -- src/path/to/changed/file.cs  # Should show PR changes

# Build and test using automated script
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform [android|ios] -TestFilter "FullyQualifiedName~IssueXXXXX"

# Document results: "Bug is fixed"
```

### Verification Checklist

After reverting:
- [ ] Ran `git diff main -- <file>` and saw NO output
- [ ] If you see ANY diff, the revert was incomplete
- [ ] PRs often change multiple files - check all changed files
- [ ] Don't skip verification - it's fast and catches mistakes

**Why this matters**: Partial reverts leave PR infrastructure in place, making it impossible to see if the PR actually fixes anything.

---

## 📋 Testing Command (Copy-Paste)

**🚨 MANDATORY: Always Use BuildAndRunHostApp.ps1 for PR Validation**

**There is ONLY ONE way to validate PRs - use the HostApp script:**

```powershell
# Android
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "FullyQualifiedName~IssueXXXXX"

# iOS
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "FullyQualifiedName~IssueXXXXX"
```

**What the script does for you** (so you don't do these manually):
- ✅ Detects and boots devices automatically
- ✅ Builds the TestCases.HostApp
- ✅ Deploys to device
- ✅ Runs your NUnit test via `dotnet test`
- ✅ Captures all logs to `CustomAgentLogsTmp/UITests/` directory

**❌ DO NOT do any of these manually**:
- ❌ `dotnet build ... -t:Run` - Script handles this
- ❌ `dotnet test` - Script handles this
- ❌ `adb logcat` - Script captures logs automatically
- ❌ `xcrun simctl launch` - Script handles this

**✅ YOUR JOB**: Create test page in TestCases.HostApp and NUnit test in TestCases.Shared.Tests

**Note**: This guide is for PR validation using HostApp UI tests. If you need help writing complex UI test suites, consider delegating to `uitest-coding-agent`.

See [quick-ref.md](quick-ref.md) and [Common Testing Patterns](../common-testing-patterns.md) for more details.

---

## ❌ Top 6 Mistakes to Avoid

1. ❌ **Using manual commands instead of BuildAndRunHostApp.ps1** → Script does everything automatically
2. ❌ **Building without showing test code first** → Wasted 15+ minutes if wrong
3. ❌ **Not using HostApp UI tests** → Should always use HostApp infrastructure
4. ❌ **Only testing WITH fix** → Must test baseline too
5. ❌ **Not checking current branch first** → Might already be on PR branch
6. ❌ **Forgetting to eliminate redundancy in review** → Read [output-format.md](output-format.md) before posting

---

## 📚 When to Read Other Guides

**During work** (reference as needed):
- Creating test code? → [uitests.instructions.md](../uitests.instructions.md)
- Build errors? → [error-handling.md](error-handling.md)
- Can't complete testing? → [checkpoint-resume.md](checkpoint-resume.md)

**Before final review** (always):
- Writing review? → [output-format.md](output-format.md) (eliminate redundancy!)

**For deep understanding** (optional):
- Why test deeply? → [core-guidelines.md](core-guidelines.md)
- Complete workflow details? → [testing-guidelines.md](testing-guidelines.md)

---

## ⏱️ Time Budgets

**Simple PRs** (single property fix): 30-45 minutes
**Medium PRs** (bug fix, multiple files): 1-2 hours  
**Complex PRs** (architecture, SafeArea): 2-4 hours

**Exceeding these?** Use checkpoint system and ask for help.

---

## 🚨 CRITICAL: Validation and Screenshot Rules

### Never Use Screenshots for Validation

**❌ PROHIBITED:**
- Using screenshot file sizes to determine if bug exists
- Comparing screenshots visually to validate fixes
- Making conclusions based on screenshot appearance

**✅ REQUIRED:**
- **ALWAYS use Appium element queries** to verify UI state
- Use `FindElement` to check if elements exist/don't exist
- Programmatically verify which page the app is on

**Example:**
```csharp
// ✅ RIGHT: Use Appium to verify state
try {
    driver.FindElement(MobileBy.Id("MainPageTitle"));
    Console.WriteLine("✅ On main page");
} catch {
    Console.WriteLine("❌ Not on main page - bug reproduced");
}
```

### Test Outputs and Logs

**All test outputs are automatically captured by BuildAndRunHostApp.ps1:**

- ✅ **Device logs**: `CustomAgentLogsTmp/UITests/android-device.log` or `ios-device.log`
- ✅ **Test output**: `CustomAgentLogsTmp/UITests/test-output.log`
- 📝 **Purpose**: Review logs after test runs for debugging

**VerifyScreenshot() in NUnit tests**:
- Automatically captures and compares screenshots
- No manual screenshot management needed
- Screenshots stored internally by test framework

**Automatic cleanup**: BuildAndRunHostApp.ps1 removes old logs before each test run.

---

## ✅ Ready to Start

You now know:
- ✅ Which app to use (HostApp with UI tests)
- ✅ Workflow with mandatory checkpoints
- ✅ How to validate (Appium, not screenshots)
- ✅ Where to find detailed instructions
- ✅ Common mistakes to avoid

**Next action**: Fetch the PR and create initial assessment.

**Remember**: Show test code before building. Always.
