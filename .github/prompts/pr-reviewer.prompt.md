---
description: "Conduct thorough PR reviews with hands-on testing on real devices/simulators"
name: pr-reviewer
agent: pr-reviewer
---

# PR Reviewer

Reviews .NET MAUI pull requests through code analysis AND hands-on device testing with instrumentation.

## What It Does

**Thorough Testing-Based Reviews:**
1. 📖 Fetches and analyzes PR changes
2. 🏗️ Builds Sandbox app with PR modifications
3. 📱 Deploys to iOS/Android simulators
4. 🔬 Uses **Appium** for reliable UI interaction (never manual ADB commands)
5. 📊 Instruments code to capture actual measurements
6. ⚖️ Compares WITH/WITHOUT PR changes
7. 🧪 Tests edge cases PR author may have missed
8. 📝 Documents findings with real data

**Key Principles:**
- ✅ Uses **Sandbox app** for validation (not TestCases.HostApp)
- ✅ Uses **Appium** for ALL UI interactions (taps, swipes, rotations)
- ✅ Uses **background daemon pattern** for Android emulators (survives sessions)
- ✅ Never kills ADB server unnecessarily
- ✅ Pauses and asks for help if stuck (never gives up and switches to code-only review)

## Usage Examples

**Basic review:**
```
/pr-reviewer Please review PR #32372
```

**Platform-specific:**
```
/pr-reviewer Test PR #32205 on iOS 26.0
```

**Cross-platform validation:**
```
/pr-reviewer Validate PR #12345 on both iOS and Android
```

**Layout/SafeArea issues:**
```
/pr-reviewer Review PR #32275 and test Shell flyout in landscape with display notch
```

**Specific scenario:**
```
/pr-reviewer Test PR #32205 with RTL layout and capture frame measurements
```

**Before/after comparison:**
```
/pr-reviewer Compare behavior WITH and WITHOUT PR #12345 changes
```

## What You'll Get

**Comprehensive Review Document** (`Review_Feedback_Issue_XXXXX.md`) including:

- ✅ **Code Analysis** - Style, correctness, best practices review
- ✅ **Test Environment** - Platform/version/device used
- ✅ **Actual Measurements** - Console output, frame positions, inset values
- ✅ **Before/After Comparison** - Behavior with and without PR
- ✅ **Edge Cases** - Additional scenarios tested beyond PR description
- ✅ **Screenshots** - Visual evidence when relevant
- ✅ **Issues Found** - Problems discovered through testing
- ✅ **Recommendations** - Accept/reject/modify based on evidence

## Expectations

**Timeline:**
- ⏱️ Reviews take 15-45 minutes (building, deploying, testing multiple scenarios)
- ✅ Agent has **unlimited time** - will not rush or skip testing
- ✅ You'll see progress updates as testing proceeds

**Testing Approach:**
- 🤖 **Appium-based** - All UI automation through Appium (reliable, verifiable)
- 📱 **Real devices** - Tests on actual iOS simulators or Android emulators
- 📏 **Instrumented** - Adds logging to capture measurements
- 🔄 **Comparative** - Tests both with and without PR changes

**If Issues Occur:**
- Build fails → Agent attempts 1-2 fixes, then **pauses** for your input
- Emulator crashes → Agent reports issue and asks for guidance
- Unexpected behavior → Agent documents and asks for validation
- **Never silently falls back to code-only review**

## Common Scenarios

**SafeArea/Layout PRs:**
- Agent instruments views to capture padding, margins, frame positions
- Tests in portrait and landscape orientations
- Verifies safe area insets are applied correctly
- Measures child vs parent view positions

**UI Component PRs:**
- Opens Sandbox app with test scenario
- Uses Appium to interact with controls (taps, swipes, text entry)
- Captures screenshots and measurements
- Tests on target platform (iOS, Android, or both)

**Shell/Navigation PRs:**
- Sets up Shell in Sandbox with flyout items
- Uses Appium to open menus, navigate, test gestures
- Verifies behavior in different orientations
- Tests with and without headers/footers

**Cross-Platform PRs:**
- Tests same scenario on iOS AND Android
- Compares platform-specific behavior
- Verifies consistent user experience
- Notes any platform differences

## Tips for Best Results

✅ **Be specific** about what to test:
```
/pr-reviewer Test PR #12345 focusing on RTL layout behavior
```

✅ **Mention platform** if it matters:
```
/pr-reviewer Test PR #32275 on Android with display notch
```

✅ **Request measurements** for layout issues:
```
/pr-reviewer Capture actual frame positions and padding values for PR #32205
```

✅ **Ask for edge cases**:
```
/pr-reviewer Test PR #12345 including rotation, RTL, and empty state scenarios
```

❌ **Don't rush the agent** - Testing takes time, and thoroughness prevents bugs

## Related Resources

- [Core Guidelines](../instructions/pr-reviewer-agent/core-guidelines.md) - Full workflow details
- [Testing Guidelines](../instructions/pr-reviewer-agent/testing-guidelines.md) - Platform setup, build patterns
- [Appium Instructions](../instructions/appium-control.instructions.md) - UI automation patterns
- [Common Testing Patterns](../instructions/common-testing-patterns.md) - Device setup, error checking
