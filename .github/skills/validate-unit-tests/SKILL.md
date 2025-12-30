---
name: validate-unit-tests
description: Validates that unit tests correctly fail without a fix and pass with a fix. Use after assess-test-type confirms unit tests are appropriate.
metadata:
  author: dotnet-maui
  version: "2.0"
compatibility: Requires dotnet CLI. No device/simulator needed.
---

# Validate Unit Tests

This skill validates that unit tests in a PR correctly catch regressions by:
1. Running tests WITH the fix (should pass)
2. Reverting the fix and running tests WITHOUT the fix (should fail)
3. Confirming the failure reason matches the expected issue

## When to Use

- "Validate the unit tests"
- "Check if unit tests catch the regression"
- "Verify unit tests fail without the fix"
- After `assess-test-type` confirms unit tests are appropriate

## Prerequisites

- PR is checked out
- Fix files and test files are identified
- `assess-test-type` has confirmed these should be unit tests

## Quick Method: Use the Skill Script (Recommended)

The fastest way to validate unit tests is to use this skill's script:

```bash
# Validate unit tests catch the regression
pwsh .github/skills/validate-unit-tests/scripts/validate-regression.ps1 \
    -TestProject "src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj" \
    -TestFilter "MyTestClass" \
    -FixFiles @("src/Controls/src/Core/SomeFile.cs")
```

The script will:
1. ✅ Build and run tests WITH fix (verify they pass)
2. ✅ Automatically revert fix files to main
3. ✅ Rebuild and run tests WITHOUT fix (verify they fail)
4. ✅ Restore fix files
5. ✅ Report whether validation passed or failed

**Output:**
```
╔═══════════════════════════════════════════════════════════╗
║              VALIDATION PASSED ✅                         ║
╠═══════════════════════════════════════════════════════════╣
║  Tests correctly catch the regression:                    ║
║  - PASS with fix                                          ║
║  - FAIL without fix                                       ║
╚═══════════════════════════════════════════════════════════╝
```

---

## Manual Method (Step-by-Step)

If you need more control or want to debug issues:

### Step 1: Identify Fix Files and Test Files

```bash
# Find the fix files (non-test code changes)
git diff main --name-only | grep -v "Test"

# Find the unit test files
git diff main --name-only | grep -E "UnitTests.*\.cs$"
```

### Step 2: Identify the Test Project

Common unit test projects:
- `src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj`
- `src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj`
- `src/Core/tests/UnitTests/Core.UnitTests.csproj`
- `src/Essentials/test/UnitTests/Essentials.UnitTests.csproj`

```bash
# Find the test project based on test file location
TEST_FILE="path/to/TestFile.cs"
TEST_PROJECT=$(find . -name "*.UnitTests.csproj" -path "*$(dirname $TEST_FILE)*" | head -1)
```

### Step 3: Run Tests WITH Fix (Baseline)

```bash
# Run specific test
dotnet test $TEST_PROJECT --filter "FullyQualifiedName~TestClassName" --no-build

# Or run by test name
dotnet test $TEST_PROJECT --filter "TestMethodName" --no-build
```

**Expected**: Tests should PASS

Record the output:
```bash
dotnet test $TEST_PROJECT --filter "TestName" 2>&1 | tee test-with-fix.log
```

### Step 4: Revert the Fix

```bash
# Revert only the fix files (not the test files)
git checkout main -- path/to/fix/File1.cs path/to/fix/File2.cs
```

### Step 5: Build and Run Tests WITHOUT Fix

```bash
# Rebuild the affected project
dotnet build $TEST_PROJECT

# Run tests again - should FAIL
dotnet test $TEST_PROJECT --filter "TestName" --no-build 2>&1 | tee test-without-fix.log
```

**Expected**: Tests should FAIL with a meaningful assertion error

### Step 6: Verify Failure Reason

```bash
# Check the failure message
grep -A10 "Failed\|Assert\|Error" test-without-fix.log
```

**Good failure examples:**
- ✅ `Assert.Equal() Failure: Expected: "value", Actual: null`
- ✅ `Expected collection to contain 3 items but found 0`
- ✅ `PropertyChanged event was not raised`

**Bad failure examples:**
- ❌ `Build failed` - Compilation issue, not test failure
- ❌ `Test not found` - Test discovery issue
- ❌ `NullReferenceException in setup` - Test infrastructure problem

### Step 7: Restore the Fix

```bash
git checkout HEAD -- path/to/fix/File1.cs path/to/fix/File2.cs
```

## Output Format

```markdown
## Unit Test Validation Results

**Test Project**: `[Project path]`
**Test Filter**: `[Filter used]`

### Regression Test Validation

| Scenario | Expected | Actual | Status |
|----------|----------|--------|--------|
| With fix | PASS | PASS/FAIL | ✅/❌ |
| Without fix | FAIL | PASS/FAIL | ✅/❌ |

### Failure Analysis (Without Fix)

**Assertion Error**:
```
[Quote the actual assertion failure message]
```

**Does failure match the issue?**
- ✅ Yes - Failure directly relates to the reported bug
- OR
- ❌ No - Failure is unrelated because [reason]

### Conclusion

- ✅ Unit tests correctly validate the fix
- OR
- ⚠️ Tests need improvement because [reason]
```

## Common Issues

### Tests Pass Without Fix

**Possible causes:**
- Test checking wrong property/value
- Test has incorrect assertions
- Issue is in rendering (should be UI test)

**Solution:** Review test assertions, consider if UI test is needed

### Tests Fail With Fix

**Possible causes:**
- Fix is incomplete
- Test has bugs
- Test expectations are wrong

**Solution:** Debug the test, verify fix addresses all cases

### Build Fails After Revert

**Possible causes:**
- Fix includes new APIs that tests depend on
- Breaking change in fix

**Solution:** Note this in review - tests may need adjustment

### Test Not Found

**Possible causes:**
- Wrong filter syntax
- Test class/method name mismatch
- Test project not built

**Solution:** Verify test name, rebuild project

## Quick Reference

```bash
# Common test projects
dotnet test src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj --filter "TestName"
dotnet test src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj --filter "TestName"
dotnet test src/Core/tests/UnitTests/Core.UnitTests.csproj --filter "TestName"

# Filter examples
--filter "FullyQualifiedName~ClassName"
--filter "FullyQualifiedName~ClassName.MethodName"
--filter "TestCategory=CategoryName"
```
