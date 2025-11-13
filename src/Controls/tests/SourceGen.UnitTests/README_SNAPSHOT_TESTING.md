# Snapshot Testing Guide

This document explains how to write and maintain snapshot tests for source generator code.

## Using SnapshotAssert

The `SnapshotAssert` utility provides improved assertion methods for comparing generated code snapshots. It addresses common pain points:

- ✅ **Whitespace normalization**: Avoids brittle tests due to indentation differences
- ✅ **Better diffs**: Uses xUnit's built-in diff output showing exact differences
- ✅ **Easy to use**: Simple static methods with clear intent
- ✅ **Line ending normalization**: Works consistently across platforms

## Quick Start

### Recommended: Use `SnapshotAssert.Equal()`

For most snapshot tests, use `SnapshotAssert.Equal()`:

```csharp
[Fact]
public void GeneratesExpectedCode()
{
    var generatedCode = MySourceGenerator.Generate(...);
    
    SnapshotAssert.Equal(
        """
        namespace MyNamespace
        {
            public class GeneratedClass
            {
                public void Method() { }
            }
        }
        """,
        generatedCode);
}
```

**What it does**:
- Trims leading and trailing whitespace from each line
- Removes empty lines
- Normalizes line endings (\r\n, \r, \n → \n)
- Compares the normalized strings

This means your test will pass regardless of:
- Indentation (tabs vs spaces, number of spaces)
- Extra blank lines
- Line ending differences (Windows vs Unix)

### When to Use Other Methods

#### `EqualIgnoringLineEndings()` - When whitespace matters

Use this when the exact whitespace is semantically important but line endings may vary:

```csharp
[Fact]
public void GeneratesCodeWithExactIndentation()
{
    var generatedCode = MySourceGenerator.Generate(...);
    
    SnapshotAssert.EqualIgnoringLineEndings(
        "    public class MyClass\n    {\n    }",
        generatedCode);
}
```

**What it does**:
- Only normalizes line endings
- Preserves all other whitespace

#### `StrictEqual()` - When everything matters

Use this when you need an exact match including line endings:

```csharp
[Fact]
public void GeneratesExactOutput()
{
    var generatedCode = MySourceGenerator.Generate(...);
    
    SnapshotAssert.StrictEqual(
        "public class MyClass\n{\n}",
        generatedCode);
}
```

**What it does**:
- No normalization
- Requires exact character-by-character match

## Migration Guide

### Old Pattern (AssertExtensions.CodeIsEqual)

```csharp
[Fact]
public void OldTest()
{
    var code = BindingCodeWriter.GenerateCommonCode();
    AssertExtensions.CodeIsEqual(
        """
        namespace Test
        {
            class MyClass { }
        }
        """,
        code);
}
```

### New Pattern (SnapshotAssert.Equal)

```csharp
[Fact]
public void NewTest()
{
    var code = BindingCodeWriter.GenerateCommonCode();
    SnapshotAssert.Equal(
        """
        namespace Test
        {
            class MyClass { }
        }
        """,
        code);
}
```

**Benefits of migration**:
1. **Better error messages**: xUnit shows the exact position where strings differ
2. **More resilient**: Automatically handles whitespace and line ending variations
3. **Cleaner**: No need to worry about trailing whitespace in expected strings

## Best Practices

### 1. Use raw string literals (""")

C# 11+ raw string literals make multi-line expected values cleaner:

```csharp
// ✅ Good - raw string literal
SnapshotAssert.Equal(
    """
    namespace Test
    {
        class MyClass { }
    }
    """,
    generatedCode);

// ❌ Avoid - escaped strings
SnapshotAssert.Equal(
    "namespace Test\n{\n    class MyClass { }\n}",
    generatedCode);
```

### 2. Don't worry about indentation in expected strings

Since `SnapshotAssert.Equal()` normalizes whitespace, you can write expected values naturally:

```csharp
// Both of these work the same:
SnapshotAssert.Equal(
    """
    namespace Test
    {
        class MyClass { }
    }
    """,
    generatedCode);

SnapshotAssert.Equal(
    """
namespace Test
{
    class MyClass { }
}
    """,
    generatedCode);
```

### 3. Use descriptive test names

```csharp
// ✅ Good - clear what's being tested
[Fact]
public void GeneratesBindingWithConditionalNullChecks()

// ❌ Avoid - unclear purpose
[Fact]
public void Test1()
```

### 4. Keep expected snapshots focused

```csharp
// ✅ Good - focused on key parts
SnapshotAssert.Equal(
    """
    public static void SetBinding(
        this BindableObject bindableObject,
        BindableProperty bindableProperty,
        Func<TSource, TProperty> getter)
    {
        // Test the key logic
    }
    """,
    GetMethodCode(generatedClass));

// ❌ Avoid - testing too much at once
// (entire file with multiple methods and boilerplate)
```

## Troubleshooting

### Test fails with "Strings differ"

1. **Check the diff output**: xUnit shows the exact position where strings differ
2. **Verify the generated code**: Print or debug the actual generated code
3. **Consider content differences**: If only whitespace differs, `SnapshotAssert.Equal()` should handle it
4. **Check for actual logic issues**: The diff might reveal a real bug in code generation

### Test passes locally but fails in CI

This usually indicates line ending issues:
- Use `SnapshotAssert.Equal()` (recommended) or `EqualIgnoringLineEndings()`
- Avoid `StrictEqual()` unless you specifically need exact line endings

## Examples

See the following test files for more examples:
- `SnapshotAssertTests.cs` - Comprehensive tests of SnapshotAssert behavior
- `BindingCodeWriterTestsWithSnapshotAssert.cs` - Migration examples from old to new pattern
