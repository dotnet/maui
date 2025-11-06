# PR Reviewer - Quick Reference

Quick reference for using the `pr-reviewer` agent with different review modes.

## 🔒 Mode Selection (Read This First!)

The agent detects mode in this priority order:

1. **Explicit Bracket Notation** (HIGHEST PRIORITY):
   - `[quick]` = Quick Mode (code only)
   - `[thorough]` = Thorough Mode (with testing)
   - `[deep]` = Deep Mode (comprehensive)
   - Example: `"Can you [quick] check and test PR #123"` → Quick Mode (despite "test")

2. **Keywords**: "quick"/"code only" → Quick, "deep"/"comprehensive" → Deep

3. **Default**: No mode specified → **Thorough Mode** (with testing)

**Mode is LOCKED after selection** - the agent will NOT switch modes during review.

## Usage Patterns

### Quick Review (Code Analysis Only)
⚠️ **NOT RECOMMENDED** - Only use when you explicitly want code-only review without testing.

**Example prompts:**
```
[quick] Review PR #32205
```
```
Quick code review of PR #12345 - skip testing
```
```
Code-only review, don't test
```

**Note**: Simply saying "review PR #12345" defaults to Thorough Mode (with testing).
**Tip**: Use `[quick]` bracket notation to force Quick Mode even if other keywords are present.

### Thorough Review (With Real Testing)
**This is the DEFAULT mode** - Use when you need validation on actual devices/simulators with measurements.

**Example prompts:**
```
Please review PR #32205
```
```
[thorough] Review PR #32205
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

**Note**: `[thorough]` bracket notation forces Thorough Mode even if no testing keywords present.

### Deep Review (Comprehensive Analysis)
Use for critical changes that need performance analysis and edge case testing.

**Example prompts:**
```
[deep] Review PR #32205
```
```
Comprehensive review of PR #32205 with performance analysis
```
```
Deep review of PR #12345 including memory profiling and edge cases
```

**Note**: `[deep]` bracket notation forces Deep Mode.

## Mode Selection Examples

**Bracket notation overrides everything:**
- `"[quick] check and test PR #123"` → Quick Mode (despite "test")
- `"[deep] review PR #456"` → Deep Mode (despite default being Thorough)
- `"[thorough] quick look at PR #789"` → Thorough Mode (despite "quick")

**Without brackets, keywords determine mode:**
- `"quick code review of PR #123"` → Quick Mode
- `"review PR #123"` → Thorough Mode (default)
- `"deep review of PR #123"` → Deep Mode
- `"comprehensive analysis of PR #123"` → Deep Mode

## Important: Default Behavior

**When you simply say "review this PR", the agent defaults to Thorough Mode (with real device/simulator testing).**

This means:
- ✅ "Please review PR #32205" → **Thorough Mode** (builds and tests)
- ✅ "Review and validate PR #32205" → **Thorough Mode** (builds and tests)
- ✅ "Check PR #32205" → **Thorough Mode** (builds and tests)

To get Quick Mode (code-only), explicitly request it:
- ✅ "[quick] Review PR #32205" → **Quick Mode** (code only, no testing)
- ✅ "Quick code review of PR #32205" → **Quick Mode** (code only, no testing)
- ✅ "Code-only review, skip testing" → **Quick Mode** (code only, no testing)

**Remember**: Mode is LOCKED once selected - the agent will not switch modes mid-review.

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
