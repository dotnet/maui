---
description: "Guidelines for writing XAML unit tests in the Controls.Xaml.UnitTests project"
---

# XAML Unit Testing Guidelines

This guide provides conventions for writing XAML unit tests in `src/Controls/tests/Xaml.UnitTests/`.

## 1. File Naming and Location for Issue Tests

When writing a test for a GitHub issue:

- **Location**: Place the test file in the `Issues/` folder
- **Naming convention**: `MauiXXXXX.xaml` and `MauiXXXXX.xaml.cs` (where XXXXX is the GitHub issue number)

**Example:**
```
src/Controls/tests/Xaml.UnitTests/Issues/
├── Maui12345.xaml
└── Maui12345.xaml.cs
```

## 2. Basic Test Pattern

Most unit tests will:
1. Check that the XAML page can be created/inflated
2. Perform some assertions on the created page
3. Apply to all `[XamlInflator]` test variations (runtime, XamlC, SourceGen)

**XAML file (Maui12345.xaml):**
```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.Maui12345">
    <!-- Your test content -->
</ContentPage>
```

**Code-behind (Maui12345.xaml.cs):**
```csharp
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui12345 : ContentPage
{
    public Maui12345()
    {
        InitializeComponent();
    }

    [TestFixture]
    class Tests
    {
        [Test]
        public void MyTest([Values] XamlInflator inflator)
        {
            var page = new Maui12345(inflator);
            Assert.That(page, Is.Not.Null);
            // Additional assertions
        }
    }
}
```

**Note**: Only add `[SetUp]` and `[TearDown]` methods if your test requires special initialization (e.g., setting up `DispatcherProvider` or `Application.Current`). Most tests don't need them.

## 3. Testing IL Correctness (XamlC) or Source Generation

When the test specifically validates compile-time behavior for a specific inflator:

**XamlC (IL generation):**
```csharp
[Test]
public void MyTest([Values] XamlInflator inflator)
{
    if (inflator == XamlInflator.XamlC)
        Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Maui12345)));
    else
    {
        var page = new Maui12345(inflator);
        Assert.That(page, Is.Not.Null);
    }
}
```

**Source Generation:**
```csharp
using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

[Test]
public void MyTest([Values] XamlInflator inflator)
{
    if (inflator == XamlInflator.SourceGen)
    {
        var result = CreateMauiCompilation()
            .WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Maui12345 : ContentPage
{
    public Maui12345() => InitializeComponent();
}
""")
            .RunMauiSourceGenerator(typeof(Maui12345));
        Assert.That(result.Diagnostics, Is.Empty); // or Is.Not.Empty for error cases
    }
    else
    {
        var page = new Maui12345(inflator);
        Assert.That(page, Is.Not.Null);
    }
}
```

## 4. Handling Invalid Code Generation

When testing scenarios where code generation is **expected to fail** or produce invalid code, use special file extensions to prevent build failures:

| Extension | Behavior | Use Case |
|-----------|----------|----------|
| `.rt.xaml` | Runtime inflation only | XamlC/SourceGen should fail, but runtime should work |
| `.rtsg.xaml` | Runtime + SourceGen only | XamlC should fail |
| `.rtxc.xaml` | Runtime + XamlC only | SourceGen should fail |

**Why?** These extensions prevent the compiler and build tasks from processing the XAML file, allowing your test to execute and verify the expected behavior.

**Example:**
```
Issues/
├── Maui12345.rt.xaml      # Only inflated at runtime (compiler/sourcegen skip)
└── Maui12345.xaml.cs
```

## Quick Reference

| Scenario | Location | Naming | Special Extension |
|----------|----------|--------|-------------------|
| Issue test | `Issues/` | `MauiXXXXX.*` | None (unless invalid codegen) |
| Invalid XamlC | `Issues/` | `MauiXXXXX.rt.xaml` | `.rt.xaml` |
| Invalid SourceGen | `Issues/` | `MauiXXXXX.rt.xaml` or `.rtxc.xaml` | Depends on what should fail |
| Test XamlC IL | Anywhere | Any | Use `MockCompiler` |
| Test SourceGen | Anywhere | Any | Use `MockSourceGenerator` |

## Related Documentation

- `.github/copilot-instructions.md` - General MAUI development guidelines
- `.github/instructions/uitests.instructions.md` - UI testing guidelines (different from XAML unit tests)
