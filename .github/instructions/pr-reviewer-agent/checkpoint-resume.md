# Checkpoint/Resume System

When you encounter environment limitations that prevent completing the review, you can pause and create a checkpoint for someone else to complete the testing.

## When to Create a Checkpoint

Create a checkpoint when you cannot proceed due to:
- **Physical device required** but only simulator/emulator available
- **Platform unavailable** (e.g., need Windows/Mac but working from Linux)
- **Specific OS version needed** that isn't installed
- **Hardware requirements** (e.g., specific Android API level, iOS version)
- **Performance testing** that requires real device
- **Specialized hardware** (e.g., specific screen resolution, foldable device)

## How to Create a Checkpoint

1. **Stop at the blocking point** - Don't try to work around limitations
2. **Document current state** - What you've completed so far
3. **Specify required action** - Exactly what needs to be done
4. **Provide resume instructions** - How to continue after the action is complete

## Checkpoint Format

When creating a checkpoint, output this in your response:

````markdown
## 🛑 CHECKPOINT: [Brief Description]

### Current State
- ✅ **Completed**: [List what you've done]
  - Fetched PR #XXXXX
  - Created HostApp test page and NUnit test
  - Analyzed code changes

- ⏸️ **Paused At**: [Where you stopped]
  - Ready to test on [platform/device]
  - Built app is ready for deployment

### Required Action
**Platform**: [iOS/Android/Windows/Mac]
**Device**: [Physical device / Specific simulator / Specific API level]
**Why**: [Brief explanation of why this specific environment is needed]

**Steps to complete**:
1. [Specific command or action needed]
2. [What to observe/measure]
3. [What data to capture]

### How to Resume

After completing the required action, provide these results in a comment:

```
**Test Results from [Device/Platform]**:

**Console Output**:
```
[Paste console output here]
```

**Observed Behavior**:
- [What happened]
- [Measurements/observations]

**Screenshots** (if applicable):
[Attach screenshots]
```

Then tag me to resume: @copilot please resume review
````

## Resume Process

When you receive checkpoint results:
1. **Acknowledge the results** - Confirm you received the data
2. **Validate completeness** - Ensure all required data was captured
3. **Continue workflow** - Pick up where you left off
4. **Complete the review** - Finish remaining steps and provide final review

## Example Checkpoints

### Example 1: Physical iOS Device Testing

```markdown
## 🛑 CHECKPOINT: Physical iOS Device Testing Required

### Current State
- ✅ **Completed**:
  - Fetched PR #25789 - Fix RTL layout on iOS
  - Created test page in TestCases.HostApp with RTL CollectionView
  - Created NUnit test in TestCases.Shared.Tests
  - Built app successfully for iOS
  - Analyzed code changes in `CollectionViewHandler.iOS.cs`

- ⏸️ **Paused At**: Ready to deploy to physical iPhone

### Required Action
**Platform**: iOS
**Device**: Physical iPhone (iOS 17+)
**Why**: RTL layout rendering differences between simulator and device have been reported in the issue

**Steps to complete**:
1. Deploy built app to physical iPhone:

2. Launch app and observe:
   - CollectionView with RTL FlowDirection
   - Check padding values (should be 16px left, 0px right)
   - Scroll behavior in RTL mode
   - Visual alignment of items

### How to Resume

Provide observations, then tag @copilot to resume.
```

### Example 2: Specific Android API Level

```markdown
## 🛑 CHECKPOINT: Android API 31 Testing Required

### Current State
- ✅ **Completed**:
  - Fetched PR #26543 - Fix Entry focus on Android 12
  - Created test page in TestCases.HostApp with Entry focus scenario
  - Created NUnit test in TestCases.Shared.Tests
  - Built app for Android
  - Analyzed handler changes

- ⏸️ **Paused At**: Need Android 12 (API 31) emulator

### Required Action
**Platform**: Android
**Device**: Emulator with API 31 (Android 12)
**Why**: Bug only reproduces on Android 12, not on newer versions

**Steps to complete**:
1. Create and start Android 12 emulator:

2. Test Entry focus behavior:
   - Tap Entry control
   - Observe keyboard appearance
   - Check console for STATE CAPTURE logs

### How to Resume

Provide observations, then tag @copilot to resume.
```

### Example 3: Windows-Specific Testing

```markdown
## 🛑 CHECKPOINT: Windows Platform Testing Required

### Current State
- ✅ **Completed**:
  - Fetched PR #27821 - Fix Window sizing on Windows
  - Created test page in TestCases.HostApp with Window size scenario
  - Created NUnit test in TestCases.Shared.Tests
  - Built app for Windows (from macOS)
  - Reviewed WinUI changes

- ⏸️ **Paused At**: Need to test on Windows machine

### Required Action
**Platform**: Windows 11
**Device**: Windows development machine
**Why**: WinUI-specific behavior cannot be tested on macOS

**Steps to complete**:
1. On Windows machine, clone and build:

2. Observe Window behavior:
   - Window opens at correct size
   - Resizing works smoothly
   - Check console for measurements

3. Capture console output

### How to Resume

Provide console output and window behavior observations, then tag @copilot to resume.
```

## Checkpoint Best Practices

### DO:
- ✅ Be specific about exact requirements (iOS version, device type, API level)
- ✅ Provide complete file paths and commands
- ✅ Explain WHY the specific environment is needed
- ✅ Make it easy for someone else to complete the action
- ✅ Specify exactly what data you need to resume
- ✅ Include console output patterns to look for
- ✅ Mention if screenshots are needed

### DON'T:
- ❌ Create checkpoints for minor inconveniences
- ❌ Use vague requirements ("test on a device")
- ❌ Skip checkpoints when truly needed
- ❌ Forget to specify resume instructions
- ❌ Assume knowledge about the environment
- ❌ Make resume process complicated

## Special Cases

### Multiple Checkpoints

If you need testing on multiple platforms, create separate checkpoints:

```markdown
## 🛑 CHECKPOINT 1/2: iOS Physical Device Testing
[Details...]

## 🛑 CHECKPOINT 2/2: Android API 31 Testing
[Details...]
```

Resume after each checkpoint is completed.

### Partial Results

If someone provides partial results, acknowledge and request remaining data:

```markdown
✅ **Received**: iOS test results
⏸️ **Still Need**: Android API 31 test results

Please complete checkpoint 2/2 before I can finalize the review.
```

## Integration with Review Workflow

Checkpoints should fit naturally into the review workflow:

1. **Standard Review** (no checkpoint needed):
   - Fetch PR → Create UI tests → Build → Test → Review

2. **Review with Checkpoint**:
   - Fetch PR → Create UI tests → **🛑 CHECKPOINT** → Build → [Wait for manual testing] → Resume → Review

3. **Review with Multiple Checkpoints**:
   - Fetch PR → Create tests → Build → **🛑 CHECKPOINT 1** → [Wait] → Resume → **🛑 CHECKPOINT 2** → [Wait] → Resume → Review

The final review should acknowledge checkpoint assistance:

```markdown
## Review Summary

**Testing Assistance**: Special thanks to @username for testing on physical iPhone (iOS 17.2)
and @username2 for Android API 31 validation.

[Rest of review...]
```
