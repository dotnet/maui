---
description: "Conduct thorough PR reviews with hands-on testing using Sandbox app"
name: maui-pr-reviewer
agent: pr-reviewer
---

# PR Reviewer

Reviews .NET MAUI pull requests through code analysis AND hands-on Sandbox testing.

## What This Does

1. 📖 Fetches and analyzes PR changes
2. 🏗️ Creates test scenario in Sandbox app
3. 🧪 Runs automated Appium tests via BuildAndRunSandbox.ps1
4. ⚖️ Compares behavior WITH/WITHOUT PR changes
5. 📝 Provides comprehensive review with evidence

**Key Points:**
- Uses Sandbox app for PR validation (NOT HostApp)
- All testing automated via single script command
- Captures logs, screenshots, and test results
- Tests on real devices/simulators with Appium

**Timeline:** First build 60-90s, subsequent tests 20-30s, full review 10-20 min

## When to Use

✅ **Use this prompt when:**
- Need comprehensive PR review with testing
- Want to validate fixes work as expected
- Need before/after behavior comparison
- Testing layout, navigation, or UI changes

❌ **DON'T use this prompt when:**
- "Write UI tests" → Use `@workspace /maui-uitest-write` instead
- "Test this PR" (no review needed) → Use `@workspace /maui-sandbox-test` instead
- Documentation-only changes → No testing needed

## Usage Examples

```
@workspace /maui-pr-reviewer Please review PR #32479
@workspace /maui-pr-reviewer Test PR #32205 on iOS
@workspace /maui-pr-reviewer Validate PR #12345 on both iOS and Android
@workspace /maui-pr-reviewer Review PR #32275 and test Shell flyout in landscape
@workspace /maui-pr-reviewer Test PR #32205 with RTL layout and capture measurements
@workspace /maui-pr-reviewer Compare behavior WITH and WITHOUT PR #12345 changes
```

## What You'll Get

- ✅ **Code Analysis** - Changes reviewed for correctness and style
- ✅ **Test Execution** - Real device testing with Appium
- ✅ **Captured Logs** - Device logs, Appium logs, test scripts, screenshots
- ✅ **Before/After Comparison** - Behavior with and without PR
- ✅ **Issues Found** - Problems discovered through testing
- ✅ **Recommendation** - Accept/modify/reject based on evidence

All logs and artifacts saved to `CustomAgentLogsTmp/Sandbox/`

## Tips for Best Results

- **Be specific** about what to test (e.g., "focusing on RTL layout behavior")
- **Mention platform** if it matters (iOS, Android, or both)
- **Request measurements** for layout issues (padding values, positions)
- **Ask for edge cases** (rotation, RTL, light/dark mode)
- Testing is thorough - first build takes 60-90 seconds

## Documentation

**Complete agent instructions:**
- [pr-reviewer agent](../agents/pr-reviewer.md) - Full agent definition with complete workflow
- [Quick Start](../instructions/pr-reviewer-agent/quick-start.md) - Essential reading (5 min)

**Specialized guides:**
- [Sandbox Setup](../instructions/pr-reviewer-agent/sandbox-setup.md) - Test scenario examples
- [SafeArea Testing](../instructions/safearea-testing.md) - SafeArea-specific guidance
- [Error Handling](../instructions/pr-reviewer-agent/error-handling.md) - Troubleshooting

**Quick references:**
- [Common Commands](../instructions/pr-reviewer-agent/quick-ref.md) - Build and test commands
- [Instrumentation Guide](../instructions/instrumentation.md) - Adding debug logging
