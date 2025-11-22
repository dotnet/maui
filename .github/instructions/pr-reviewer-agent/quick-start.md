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

1. **App Selection Rule** (30 seconds)
   - ✅ **Sandbox app** = PR validation (DEFAULT, 99% of reviews)
   - ❌ **HostApp** = Only when explicitly asked to "write UI tests" or "validate UI tests"
   
   **Decision**: If user says "review PR" or "test this fix" → Use Sandbox
   
   See [testing-guidelines.md](testing-guidelines.md#app-selection) for details.

2. **Workflow Overview** (2 minutes)
   ```
   1. Fetch PR → Analyze code
   2. Create test code → CHECKPOINT (show user, get approval)
   3. Build & Deploy → Test WITHOUT fix
   4. Test WITH fix → Compare results
   5. Write review → Eliminate redundancy
   ```

3. **Special Cases** (30 seconds - read if applicable)
   - CollectionView/CarouselView PR? → Read [collectionview-handler-detection.md](collectionview-handler-detection.md)
   - SafeArea PR? → Read [safearea-testing.instructions.md](../safearea-testing.instructions.md)
   - UI test files in PR? → Read [uitests.instructions.md](../uitests.instructions.md)

4. **UI Interaction Rule** (10 seconds)
   - ✅ **Appium** = ALL user interactions (tapping, scrolling, gestures)
   - ❌ **ADB/xcrun** = Only for device setup and log monitoring
   
   **Decision**: If you need to interact with app UI → Use Appium script
   
   See [appium-control.instructions.md](../appium-control.instructions.md) for complete guide.

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
- Using Sandbox app (not HostApp)
- Will test scenario: [description]
- Platforms: [iOS/Android/both]
- Plan to compare WITH/WITHOUT PR changes

**Next step**: Creating test code, will show before building.

Proceed? Any concerns about this approach?
```

**⚠️ CRITICAL**: Wait for user response before continuing.

---

## 🛑 Mandatory Checkpoints

### Checkpoint 1: Before Building (MANDATORY)

After creating test code, **STOP and show user**:

```markdown
## Validation Checkpoint - Before Building

**Test code created**:

XAML:
```xml
[Show relevant XAML snippet]
```

Code:
```csharp
[Show instrumentation code]
```

**What I'm measuring**: [Explain]

**Expected WITHOUT PR**: [What you expect]
**Expected WITH PR**: [What should change]

Should I proceed with building? (Build takes 10-15 minutes)
```

**Do NOT build without approval.**

### Checkpoint 2: Before Final Review (Optional but Recommended)

Show raw data and ask if interpretation is correct.

---

## 📋 Common Commands (Copy-Paste)

See [quick-ref.md](quick-ref.md) for complete command sequences.

**iOS Testing**:
```bash
# Complete workflow - see quick-ref.md for full version
UDID=$(xcrun simctl list devices available --json | jq -r '...')
# ... build, install, launch
```

**Android Testing**:
```bash
# 1. Check for device
export DEVICE_UDID=$(adb devices | grep device | awk '{print $1}' | head -1)

# 2. If no device, START EMULATOR (don't skip!)
if [ -z "$DEVICE_UDID" ]; then
    echo "No device found. Starting emulator..."
    cd $ANDROID_HOME/emulator && (./emulator -avd Pixel_9 -no-snapshot-load -no-audio -no-boot-anim > /tmp/emulator.log 2>&1 &)
    adb wait-for-device
fi

# 3. Build and deploy
# ... see quick-ref.md for complete workflow
```

---

## ❌ Top 5 Mistakes to Avoid

1. ❌ **Building without showing test code first** → Wasted 15+ minutes if wrong
2. ❌ **Using HostApp for PR validation** → Should use Sandbox
3. ❌ **Only testing WITH fix** → Must test baseline too
4. ❌ **Not checking current branch first** → Might already be on PR branch
5. ❌ **Forgetting to eliminate redundancy in review** → Read [output-format.md](output-format.md) before posting

---

## 📚 When to Read Other Guides

**During work** (reference as needed):
- Creating test code? → [sandbox-setup.md](sandbox-setup.md)
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

## ✅ Ready to Start

You now know:
- ✅ Which app to use (Sandbox, not HostApp)
- ✅ Workflow with mandatory checkpoints
- ✅ Where to find detailed instructions
- ✅ Common mistakes to avoid

**Next action**: Fetch the PR and create initial assessment.

**Remember**: Show test code before building. Always.
