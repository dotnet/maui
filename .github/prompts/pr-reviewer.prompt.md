# PR Reviewer - Quick Reference

Quick reference for using the `pr-reviewer` agent with different review modes.

## Usage Patterns

### Quick Review (Code Analysis Only)
Use when you want a fast code review without building or testing.

**Example prompts:**
```
Please review PR #32205
```
```
Quick review of PR #12345 focusing on test coverage
```

### Thorough Review (With Real Testing)
Use when you need validation on actual devices/simulators with measurements.

**Example prompts:**
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

**Quick Mode**: Default, or use "quick", "fast", "overview"
**Thorough Mode**: Use "test", "verify", "validate", "run", "deploy", "simulator", "device"
**Deep Mode**: Use "deep", "comprehensive", "performance", "profile", "memory"

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
