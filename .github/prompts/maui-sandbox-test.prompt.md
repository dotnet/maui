---
description: "Validate a PR by testing it in the .NET MAUI Sandbox app for manual verification"
name: maui-sandbox-test
agent: sandbox-agent
---

# Sandbox PR Testing

Test a pull request using the .NET MAUI Sandbox app for manual validation and issue reproduction.

## Usage

```
@workspace /maui-sandbox-test <PR_NUMBER>
@workspace /maui-sandbox-test <PR_NUMBER> <platform>
```

## What This Does

1. Checks out the PR branch
2. Analyzes the issue and PR changes
3. Creates a test scenario in the Sandbox app
4. Builds and deploys to device/simulator
5. Runs automated Appium tests
6. Provides a validation summary

**Timeline:** 5-10 minutes for first test (build + deploy), 2-3 minutes for subsequent tests

## When to Use

✅ **Use this prompt when:**
- Validate PR fixes work as expected
- Reproduce reported issues
- Manually verify behavior
- Experiment with features

❌ **DON'T use this prompt when:**
- Write UI tests → Use `@workspace /maui-uitest-write` instead
- Review code quality → Use `@workspace /maui-pr-reviewer` instead
- Documentation-only changes → No testing needed

## Platform Selection

- **Automatically detects platform** from PR title tags (`[Android]`, `[iOS]`, etc.) or modified file paths
- **Defaults to Android** if cross-platform (faster setup, better device availability)
- **You can specify manually**: `/sandbox-test 23456 ios`

## Examples

```
@workspace /maui-sandbox-test 23456
@workspace /maui-sandbox-test 23456 android
@workspace /maui-sandbox-test 23456 ios
```

## Test Scenario Sources

The agent will prioritize:
1. **Issue reproduction steps** (most reliable)
2. **PR's existing UI tests** (if available)
3. **Custom scenario** based on PR changes (last resort)

## What You'll Get

- ✅ **Test scenario** - Set up in `src/Controls/samples/Controls.Sample.Sandbox/`
- ✅ **Appium test script** - Generated in `CustomAgentLogsTmp/Sandbox/`
- ✅ **Captured logs** - Device/simulator logs for analysis
- ✅ **Validation summary** - Pass/fail verdict with evidence

## Documentation

**Complete agent instructions:**
- [sandbox-agent](../agents/sandbox-agent.md) - Full agent definition with workflow and troubleshooting

**Related guides:**
- [Sandbox Testing Patterns](../instructions/sandbox-testing-patterns.md) - Build and deployment commands
- [Instrumentation Guide](../instructions/instrumentation.md) - Adding logging for debugging

## Notes

- Test scenarios remain in Sandbox for your iteration
- To verify bug reproduction: revert fix, rerun test, restore fix, rerun again
- Script used: `pwsh .github/scripts/BuildAndRunSandbox.ps1`
