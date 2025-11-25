# .NET MAUI Custom Agents

This directory contains specialized agents for different workflows in the .NET MAUI repository. Each agent is focused on a specific type of work.

## Available Agents

#### 1. sandbox-pr-tester
**File**: `sandbox-pr-tester.md`  
**Purpose**: Test PRs manually using the Sandbox app  
**Use when**:
- Testing a PR fix
- Reproducing an issue
- Validating changes work correctly

**Example**: `@sandbox-pr-tester please test PR #32479`

#### 2. uitest-coding-agent
**File**: `uitest-coding-agent.md`  
**Purpose**: Write and run UI tests following .NET MAUI conventions  
**Use when**:
- Need to create new automated tests
- Adding test coverage
- Running existing UI tests from a PR
- Validating UI tests work correctly
- Writing tests for a fix

**Example**: 
- `@uitest-coding-agent please write UI tests for issue #12345`
- `@uitest-coding-agent please run the UI tests from PR #32479`

## Quick Decision Guide

**Choose the agent based on what you need to do:**

| What do you want to do? | Use this agent |
|------------------------|----------------|
| Test a PR manually with Sandbox | `sandbox-pr-tester` |
| Reproduce an issue manually | `sandbox-pr-tester` |
| Validate a fix works manually | `sandbox-pr-tester` |
| Write new UI tests | `uitest-coding-agent` |
| Run existing UI tests | `uitest-coding-agent` |
| Validate UI tests from a PR | `uitest-coding-agent` |
| Add test coverage | `uitest-coding-agent` |
| Review test code quality | `uitest-coding-agent` |
| Investigate an issue | `issue-resolver` |

## Other Agents

### issue-resolver
**File**: `issue-resolver.md`  
**Purpose**: Investigate and resolve community-reported issues  
**Use when**: Working on issue reports from GitHub

## Key Differences

### Sandbox vs HostApp

| Aspect | Sandbox | TestCases.HostApp |
|--------|---------|-------------------|
| **Purpose** | Manual PR testing | Automated UI tests |
| **Used by** | sandbox-pr-tester | uitest-coding-agent |
| **Location** | `src/Controls/samples/Controls.Sample.Sandbox/` | `src/Controls/tests/TestCases.HostApp/` |
| **Test Type** | Manual validation | Automated NUnit tests |
| **Clean up** | Revert changes after testing | Commit test files to repo |

### Manual Testing vs Automated Testing

**Manual Testing** (Sandbox):
- Quick validation
- Visual inspection
- Interactive testing
- Temporary test code
- Use BuildAndRunSandbox.ps1

**Automated Testing** (HostApp):
- CI/CD validation
- Regression prevention  
- Permanent test coverage
- Committed to repository
- Runs in CI pipeline

## Usage Examples

### Example 1: Testing a PR manually
```
@sandbox-pr-tester please test PR #32479
```

### Example 2: Writing new UI tests
```
@uitest-coding-agent please write UI tests for issue #12345 about button clicks
```

### Example 3: Running existing UI tests
```
@uitest-coding-agent please run the UI tests from PR #32479
```

### Example 4: Investigating an issue
```
@issue-resolver please investigate issue #12345
```

## Agent Files Reference

All agent files are in `.github/agents/`:

```
.github/agents/
├── README.md (this file)
├── issue-resolver.md (issue investigation)
├── sandbox-pr-tester.md (manual testing)
└── uitest-coding-agent.md (write and run UI tests)
```

## Related Documentation

- [Instrumentation Guide](../instructions/instrumentation.md)
- [UI Testing Guide](../instructions/uitests.instructions.md)
- [Common Testing Patterns](../instructions/common-testing-patterns.md)
- [Appium Control Scripts](../instructions/appium-control.md)
