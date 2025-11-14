# PR Reviewer - Quick Reference

The `pr-reviewer` agent conducts thorough code reviews with hands-on device/simulator testing and validation.

## How It Works

The agent always performs **thorough reviews** which include:

1. **Code Analysis** - Review code for correctness, style, and best practices
2. **Build & Deploy** - Build the Sandbox app and deploy to simulator/emulator  
3. **Real Testing** - Test the PR changes on actual devices with measurements
4. **Before/After Comparison** - Compare behavior with and without PR changes
5. **Edge Case Testing** - Test scenarios not mentioned by the PR author
6. **Documented Results** - Provide review with actual test data and evidence

## Usage Examples

**Basic review:**
```
Please review PR #32372
```

**Platform-specific testing:**
```
Test PR #32205 on iOS 26.0 specifically
```

**Cross-platform validation:**
```
Test PR #12345 on both iOS and Android to verify cross-platform behavior
```

**Margin/Layout Issues:**
```
Review PR #32205 and instrument the Sandbox app to capture actual frame positions
```

**Before/After Comparison:**
```
Test PR #32205 comparing behavior WITH and WITHOUT the changes
```

## What Happens If Build Fails?

If the agent encounters build errors during testing:

1. It will try to fix obvious issues (1-2 attempts)
2. If errors persist, it will **STOP** and ask for your help
3. It will **NOT** silently switch to code-only review
4. You'll get a clear error report with options to proceed

This ensures you always know when testing couldn't be completed.

## Expected Output

Every review includes:

- **Test Results** - Actual console output and measurements from simulator/device
- **Environment Details** - Which platform/version was tested
- **Comparison** - Behavior with and without PR changes
- **Issues & Suggestions** - Validated through real testing
- **Recommendation** - Based on both code review and hands-on validation
