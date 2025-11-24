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

#### 2. uitest-pr-validator
**File**: `uitest-pr-validator.md`  
**Purpose**: Validate existing UI tests in a PR  
**Use when**:
- PR contains test files
- Need to verify tests are correct
- Want to run tests and check they pass

**Example**: `@uitest-pr-validator please validate the tests in this PR`

#### 3. uitest-coding-agent
**File**: `uitest-coding-agent.md`  
**Purpose**: Write new UI tests following .NET MAUI conventions  
**Use when**:
- Need to create automated tests
- Adding test coverage
- Writing tests for a fix

**Example**: `@uitest-coding-agent please write UI tests for issue #12345`

## Quick Decision Guide

**Choose the agent based on what you need to do:**

| What do you want to do? | Use this agent |
|------------------------|----------------|
| Test a PR manually | `sandbox-pr-tester` |
| Reproduce an issue | `sandbox-pr-tester` |
| Validate a fix works | `sandbox-pr-tester` |
| Check if tests are correct | `uitest-pr-validator` |
| Run existing tests | `uitest-pr-validator` |
| Review test code quality | `uitest-pr-validator` |
| Write new UI tests | `uitest-coding-agent` |
| Add test coverage | `uitest-coding-agent` |
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
| **Used by** | sandbox-pr-tester | uitest-pr-validator<br>uitest-coding-agent |
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

### Example 1: Testing a PR
```
@sandbox-pr-tester please test PR #32479
```

### Example 2: Validating tests in a PR
```
@uitest-pr-validator please validate the tests in this PR
```

### Example 3: Writing new tests
```
@uitest-coding-agent please write UI tests for issue #12345 about button clicks
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
├── uitest-coding-agent.md (write tests)
└── uitest-pr-validator.md (validate tests)
```

## Related Documentation

- [Instrumentation Guide](../instructions/instrumentation.md)
- [UI Testing Guide](../instructions/uitests.instructions.md)
- [Common Testing Patterns](../instructions/common-testing-patterns.md)
- [Appium Control Scripts](../instructions/appium-control.md)
