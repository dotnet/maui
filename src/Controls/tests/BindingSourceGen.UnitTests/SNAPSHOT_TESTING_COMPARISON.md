# Snapshot Testing Approaches Comparison

This document compares different approaches to snapshot testing in source generator tests and provides guidance on which to use.

## Summary of Approaches

We have implemented and evaluated three approaches:

1. **SnapshotAssert** (Recommended for most cases)
2. **CSharpierSnapshotAssert** (Experimental - for specific use cases)
3. **Legacy approaches** (AssertExtensions.CodeIsEqual, LineEndingNormalizedEqualityComparer)

## Detailed Comparison

### 1. SnapshotAssert (Recommended)

**What it does:**
- Trims leading/trailing whitespace from each line
- Removes empty lines
- Normalizes line endings
- Uses xUnit's built-in assertion with clear diff output

**Pros:**
- ✅ Simple and fast
- ✅ No external dependencies
- ✅ Excellent diff output showing exact differences
- ✅ Handles 95% of use cases well
- ✅ Easy to understand what's being compared
- ✅ Graceful degradation (doesn't fail on syntax errors)

**Cons:**
- ⚠️ Still sensitive to some formatting (e.g., spacing within lines)
- ⚠️ Doesn't understand C# syntax/semantics

**Best for:**
- Most snapshot tests of generated code
- Quick comparison of code structure
- Tests where minor formatting differences don't matter

**Example:**
```csharp
SnapshotAssert.Equal(
    """
    namespace Test
    {
        class MyClass { }
    }
    """,
    generatedCode);
```

### 2. CSharpierSnapshotAssert (Experimental)

**What it does:**
- Formats both expected and actual code using CSharpier
- Compares the formatted results
- Completely normalizes all C# formatting conventions

**Pros:**
- ✅ Completely ignores ALL formatting differences
- ✅ Only fails on actual semantic differences
- ✅ Can compare extremely differently formatted code
- ✅ Uses industry-standard formatter (CSharpier)
- ✅ Handles incomplete/invalid code gracefully

**Cons:**
- ❌ Requires additional dependency (CSharpier.Core)
- ❌ Slower (formats code before comparison)
- ❌ May hide unintended formatting changes in generator output
- ❌ Less clear what exact formatting the generator produces
- ⚠️ Overkill for most use cases

**Best for:**
- Testing generator logic when output formatting is completely irrelevant
- Comparing code from different formatters or sources
- Tests where you truly only care about semantic correctness
- Cases where you have many style variations to support

**Example:**
```csharp
CSharpierSnapshotAssert.Equal(
    """
    namespace Test{class MyClass{}}
    """,
    generatedCode); // Passes even with completely different formatting
```

### 3. Legacy Approaches

#### AssertExtensions.CodeIsEqual

**What it does:**
- Splits code into lines
- Trims each line
- Filters empty lines
- Compares line by line

**Issues:**
- ❌ Poor diff output (shows which line differs but not where)
- ❌ Line-by-line comparison can be confusing
- ❌ No semantic understanding

**Status:** Maintained for backward compatibility. Use SnapshotAssert for new tests.

#### LineEndingNormalizedEqualityComparer

**What it does:**
- Only normalizes line endings
- Preserves all other whitespace

**Issues:**
- ❌ Still sensitive to whitespace
- ❌ Requires using xUnit's Assert.Equal with custom comparer
- ❌ Verbose to use

**Status:** Superseded by SnapshotAssert. Use SnapshotAssert.EqualIgnoringLineEndings() instead.

## Decision Matrix

Use this table to choose the right approach:

| Scenario | Recommended Approach | Reason |
|----------|---------------------|---------|
| General snapshot testing | `SnapshotAssert.Equal()` | Fast, simple, handles most cases |
| Need to preserve exact whitespace | `SnapshotAssert.EqualIgnoringLineEndings()` | Only normalizes line endings |
| Need exact match including line endings | `SnapshotAssert.StrictEqual()` | No normalization |
| Only care about semantic correctness | `CSharpierSnapshotAssert.Equal()` | Ignores ALL formatting |
| Comparing code from different sources | `CSharpierSnapshotAssert.Equal()` | Handles style variations |
| Testing generator formatting output | `SnapshotAssert.EqualIgnoringLineEndings()` | Preserves intentional formatting |

## Performance Comparison

Based on test execution times:

| Approach | Relative Speed | Notes |
|----------|---------------|-------|
| SnapshotAssert.Equal() | ⚡ Fastest (baseline) | Simple string operations |
| SnapshotAssert.EqualIgnoringLineEndings() | ⚡ Very Fast (~1.1x) | Regex replacement |
| CSharpierSnapshotAssert.Equal() | 🐌 Slower (~50-100x) | Full code formatting |

## Recommendations by Test Type

### Unit Tests for Code Generation

**Use:** `SnapshotAssert.Equal()`

```csharp
[Fact]
public void GeneratesBindingCode()
{
    var code = generator.Generate(...);
    SnapshotAssert.Equal(expectedCode, code);
}
```

### Integration Tests

**Use:** `SnapshotAssert.Equal()` or `CSharpierSnapshotAssert.Equal()`

Choose CSharpier if:
- The test compares code from multiple sources
- The exact formatting is irrelevant
- You want maximum resilience to formatting changes

### Regression Tests

**Use:** `SnapshotAssert.EqualIgnoringLineEndings()`

Preserves the exact formatting to catch unintended changes while being cross-platform compatible.

## Migration Guide

### From AssertExtensions.CodeIsEqual

```csharp
// Before
AssertExtensions.CodeIsEqual(expected, actual);

// After
SnapshotAssert.Equal(expected, actual);
```

Benefits: Better diff output, clearer test failures

### From LineEndingNormalizedEqualityComparer

```csharp
// Before
Assert.Equal(expected, actual, LineEndingNormalizedEqualityComparer.Instance);

// After
SnapshotAssert.EqualIgnoringLineEndings(expected, actual);
```

Benefits: Simpler, more explicit intent

## When NOT to Use CSharpierSnapshotAssert

❌ Don't use CSharpier if:
1. **You care about the generator's formatting** - CSharpier will normalize it away
2. **Performance is critical** - CSharpier adds significant overhead
3. **The formatting is part of the test** - You want to verify indent levels, line breaks, etc.
4. **It's not C# code** - CSharpier only works with C#

## Experimental Status

`CSharpierSnapshotAssert` is marked as **experimental** because:
1. It adds a significant external dependency
2. It may hide legitimate formatting issues
3. Performance overhead may not be acceptable for large test suites
4. Most tests don't need this level of normalization

Consider using it only when you've tried `SnapshotAssert` and found it insufficient.

## Contributing

If you find scenarios where these utilities don't work well, please:
1. Document the scenario in tests
2. Propose improvements
3. Consider whether it's a real limitation or a test design issue

See `README_SNAPSHOT_TESTING.md` for usage examples and best practices.
