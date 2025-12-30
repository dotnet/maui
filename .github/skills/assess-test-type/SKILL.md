---
name: assess-test-type
description: Determines whether a test should be a UI test or unit test based on what behavior is being validated. Use when analyzing tests, reviewing PRs with tests, deciding test strategy, or determining appropriate test type for a fix.
metadata:
  author: dotnet-maui
  version: "1.1"
compatibility: Requires ability to read and analyze C# test code.
---

# Assess Test Type

This skill determines the appropriate test type (UI test vs unit test) for tests in a PR based on what behavior they're validating.

## When to Use

- "Should this be a UI test or unit test?"
- "What type of test is appropriate here?"
- "Assess the test type for this PR"
- Before running test validation

## Test Type Decision Framework

### When to Use Unit Tests

Unit tests (xUnit) are appropriate when testing:

| Scenario | Example | Location |
|----------|---------|----------|
| Pure logic/algorithms | String parsing, math calculations | `*.UnitTests.csproj` |
| Property change notifications | `INotifyPropertyChanged` behavior | `Controls.Core.UnitTests` |
| Binding expressions | Binding path resolution | `Controls.Core.UnitTests` |
| XAML parsing | XAML to object graph | `Controls.Xaml.UnitTests` |
| View model behavior | Command execution, state changes | `*.UnitTests.csproj` |
| Platform-agnostic handlers | Handler mapping logic | `Core.UnitTests` |

**Key indicators for unit tests:**
- ✅ No visual rendering required
- ✅ No platform-specific behavior being tested
- ✅ Can mock dependencies
- ✅ Tests run in milliseconds
- ✅ No device/simulator needed

### When to Use UI Tests

UI tests (NUnit + Appium) are appropriate when testing:

| Scenario | Example | Location |
|----------|---------|----------|
| Visual rendering | Element appears on screen | `TestCases.HostApp` + `TestCases.Shared.Tests` |
| User interaction | Tap, scroll, gesture response | `TestCases.Shared.Tests` |
| Platform-specific behavior | iOS SafeArea, Android back button | `TestCases.Shared.Tests` |
| Layout measurement | Element size, position | `TestCases.Shared.Tests` |
| Navigation | Page transitions, Shell routing | `TestCases.Shared.Tests` |
| Focus/keyboard | Entry focus, keyboard appearance | `TestCases.Shared.Tests` |

**Key indicators for UI tests:**
- ✅ Requires visual tree to be rendered
- ✅ Tests user-visible behavior
- ✅ Platform-specific rendering matters
- ✅ Interaction timing matters
- ✅ Needs real device/simulator

## Instructions

### Step 1: Identify Test Files in PR

```bash
# Find test files
git diff main --name-only | grep -E "Test|Issue"

# Categorize by location
# UI tests: TestCases.HostApp/, TestCases.Shared.Tests/
# Unit tests: *.UnitTests/
```

### Step 2: Analyze Each Test

For each test, answer these questions:

1. **Does it require rendering on screen?**
   - YES → Likely UI test
   - NO → Likely unit test

2. **Does it test user interaction?**
   - YES → UI test
   - NO → Could be either

3. **Is it testing platform-specific behavior?**
   - YES → UI test
   - NO → Likely unit test

4. **Can it run without a device/simulator?**
   - YES → Unit test
   - NO → UI test

### Step 3: Decision Flowchart

```
Does it require rendering on screen?
├── NO → Unit Test
└── YES → Does it test user interaction?
          ├── YES → UI Test
          └── NO → Does platform-specific rendering matter?
                   ├── YES → UI Test
                   └── NO → Could be Unit Test (prefer Unit if possible)
```

### Step 4: Check Current vs Recommended

Compare what the PR has vs what it should have:

```bash
# Check current test type
ls src/Controls/tests/TestCases.HostApp/Issues/Issue*.xaml 2>/dev/null && echo "Has UI test"
ls src/Controls/tests/*UnitTests/**/Issue*.cs 2>/dev/null && echo "Has unit test"
```

## Output Format

```markdown
## Test Type Assessment

### Tests in PR

| Test | Current Type | Recommended | Reasoning |
|------|--------------|-------------|-----------|
| [TestName] | UI Test | UI Test ✅ | Requires user interaction |
| [TestName] | UI Test | Unit Test ❌ | Pure logic, no rendering needed |
| [TestName] | Unit Test | Unit Test ✅ | Tests binding resolution |

### Recommendation

- ✅ Test types are appropriate
- OR
- ⚠️ Consider changing [TestName] from [current] to [recommended] because [reason]

### Next Steps

Based on test types present:
- For UI tests → Use `validate-ui-tests` skill
- For unit tests → Use `validate-unit-tests` skill
```

## Examples

### Example 1: RadioButton Binding in CollectionView

**Issue**: RadioButtonGroup.SelectedValue binding doesn't update when RadioButton is checked inside CollectionView

**Analysis**:
- Requires visual rendering? YES (RadioButtons must appear)
- Tests user interaction? YES (tapping RadioButton)
- Platform-specific? NO (cross-platform binding issue)

**Verdict**: UI Test ✅

### Example 2: String Parsing Logic

**Issue**: Custom markup extension fails to parse certain string formats

**Analysis**:
- Requires visual rendering? NO
- Tests user interaction? NO
- Platform-specific? NO

**Verdict**: Unit Test ✅

### Example 3: Layout Calculation

**Issue**: Grid column width incorrect when using star sizing

**Analysis**:
- Requires visual rendering? YES (layout must be measured)
- Tests user interaction? NO
- Platform-specific? MAYBE (could differ per platform)

**Verdict**: UI Test ✅ (layout measurement requires rendering)
