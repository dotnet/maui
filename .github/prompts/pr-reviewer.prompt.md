# PR Reviewer - Quick Reference

Quick reference for using the `pr-reviewer` agent with different review modes.

## Usage Patterns

### Quick Review (Code Analysis Only)
⚠️ **NOT RECOMMENDED** - Only use when you explicitly want code-only review without testing.

**Example prompts:**
```
Quick code review of PR #32205
```
```
Code-only review of PR #12345 - skip testing
```
```
Just review the code structure, don't test
```

**Note**: Simply saying "review PR #12345" will default to Thorough Mode (with testing).

### Thorough Review (With Real Testing)
**This is the DEFAULT mode** - Use when you need validation on actual devices/simulators with measurements.

**Example prompts:**
```
Please review PR #32205
```
```
Review and validate PR #32205
```
```
Please review and TEST PR #32205 on iOS 26
```
```
Review PR #12345 and VERIFY your suggestions work in the Sandbox app
```
```
Test PR #32205 and measure the actual frame positions WITH and WITHOUT the changes
```

### Deep Review (Comprehensive Analysis)
Use for critical changes that need performance analysis and edge case testing.

**Example prompts:**
```
Comprehensive review of PR #32205 with performance analysis
```
```
Deep review of PR #12345 including memory profiling and edge cases
```

## Mode Triggers

The agent automatically detects the mode from your prompt keywords:

**Quick Mode**: Use "quick", "fast", "code only", "skip testing"
**Thorough Mode**: Default for PR reviews, or use "test", "verify", "validate", "run", "deploy", "simulator", "device", "thorough"
**Deep Mode**: Use "deep", "comprehensive", "performance", "profile", "memory"

## Important: Default Behavior

**When you simply say "review this PR", the agent defaults to Thorough Mode (with real device/simulator testing).**

This means:
- ✅ "Please review PR #32205" → **Thorough Mode** (builds and tests)
- ✅ "Review and validate PR #32205" → **Thorough Mode** (builds and tests)  
- ✅ "Check PR #32205" → **Thorough Mode** (builds and tests)

To get Quick Mode (code-only), you must explicitly request it:
- ✅ "Quick code review of PR #32205" → **Quick Mode** (code only, no testing)
- ✅ "Code-only review, skip testing" → **Quick Mode** (code only, no testing)

## Platform-Specific Testing

```
Test PR #32205 on iOS 26.0 specifically
```
```
Test PR #12345 on both iOS and Android to verify cross-platform behavior
```

## Common Scenarios

**Margin/Layout Issues:**
```
Review PR #32205 and instrument the Sandbox app to capture actual frame positions
```

**Performance-Sensitive Changes:**
```
Deep review of PR #12345 - this changes layout code, measure performance impact
```

**Before/After Comparison:**
```
Test PR #32205 comparing behavior WITH and WITHOUT the changes
```

## What Happens If Build Fails?

If the agent encounters build errors during Thorough/Deep mode testing:

1. It will try to fix obvious issues (1-2 attempts)
2. If errors persist, it will **STOP** and ask for your help
3. It will **NOT** silently switch to code-only review
4. You'll get a clear error report with options to proceed

This ensures you always know when testing couldn't be completed.
