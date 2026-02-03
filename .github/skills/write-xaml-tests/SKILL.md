---
name: write-xaml-tests
description: Creates XAML unit tests for GitHub issues in the Controls.Xaml.UnitTests project. Tests XAML parsing, compilation (XamlC), and source generation. Use when testing XAML-specific behavior, not UI interactions.
metadata:
  author: Stephane Delcroix
  version: "1.0"
compatibility: Requires .NET SDK for building and running xUnit tests.
---

# Write XAML Tests Skill

Creates XAML unit tests that verify XAML parsing, XamlC compilation, and source generation behavior.

## When to Use

- ✅ Testing XAML parsing/inflation behavior
- ✅ Testing XamlC (IL generation) correctness
- ✅ Testing Source Generator output
- ✅ XAML-specific bugs (bindings, markup extensions, x:Name, etc.)

## When NOT to Use

- ❌ Testing UI interactions or visual behavior → Use `write-ui-tests` skill
- ❌ Testing runtime behavior after page loads → Use `write-ui-tests` skill
- ❌ Testing platform-specific rendering → Use `write-ui-tests` skill

## Required Input

Before invoking, ensure you have:
- **Issue number** (e.g., 12345)
- **Issue description** - what XAML behavior is broken
- **Expected vs actual behavior**

## Workflow

### Step 1: Read the XAML Unit Test Guidelines

```bash
cat .github/instructions/xaml-unittests.instructions.md
```

This contains the authoritative conventions for:
- File naming (`MauiXXXXX.xaml` and `MauiXXXXX.xaml.cs`)
- File location (`src/Controls/tests/Xaml.UnitTests/Issues/`)
- Test patterns with `[Values] XamlInflator`
- XamlC testing with `MockCompiler`
- Source Generator testing with `MockSourceGenerator`
- Special file extensions for invalid codegen (`.rt.xaml`, `.rtsg.xaml`, `.rtxc.xaml`)

### Step 2: Create Test Files

Following the conventions from Step 1, create:
- `src/Controls/tests/Xaml.UnitTests/Issues/MauiXXXXX.xaml`
- `src/Controls/tests/Xaml.UnitTests/Issues/MauiXXXXX.xaml.cs`

### Step 3: Verify Tests Compile and Run

```bash
# Build the test project
dotnet build src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj -c Debug --no-restore -v q

# Run specific test
dotnet test src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj --filter "FullyQualifiedName~MauiXXXXX" --no-build
```

### Step 4: Verify Test Behavior

- **For bug reproduction tests**: Tests should FAIL before fix, PASS after fix
- **For regression tests**: Tests should PASS to confirm behavior works

## Output

After completion, report:

```markdown
✅ XAML unit test created for Issue #XXXXX

**Files:**
- `src/Controls/tests/Xaml.UnitTests/Issues/MauiXXXXX.xaml`
- `src/Controls/tests/Xaml.UnitTests/Issues/MauiXXXXX.xaml.cs`

**Test method:** `DescriptiveTestName`
**Inflators tested:** Runtime, XamlC, SourceGen
**Verification:** Tests [PASS/FAIL] as expected
```

## References

- **Full conventions:** `.github/instructions/xaml-unittests.instructions.md`
- **Test project:** `src/Controls/tests/Xaml.UnitTests/`
- **Existing issue tests:** `src/Controls/tests/Xaml.UnitTests/Issues/`
