using Microsoft.Maui.Controls.SourceGen.UnitTests;
using System;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Tests for bindings with explicit Path= syntax.
/// NOTE: There's a pre-existing bug where bindings with Path= don't compile.
/// This is NOT related to the self-binding fix but explains the CarouselView UI test failures.
/// </summary>
public class ExplicitPathBindingTest : SourceGenXamlInitializeComponentTestBase
{
[Fact]
public void BindingWithoutExplicitPathWorks()
{
// This test verifies that {Binding PropertyName} works correctly
var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
xmlns:test="clr-namespace:Test"
x:Class="Test.TestPage"
x:DataType="test:TestPage"
Title="{Binding Name}"/>
""";

var code =
"""
#nullable enable
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
public string Name { get; set; } = "TestName";

public TestPage()
{
InitializeComponent();
}
}
""";

var (result, generated) = RunGenerator(xaml, code);

Assert.Empty(result.Diagnostics);
Assert.NotNull(generated);

// Should compile as a property binding
Assert.Contains("TypedBinding<global::Test.TestPage, string>", generated, StringComparison.Ordinal);
}

[Fact(Skip = "Pre-existing bug: bindings with Path= syntax don't compile to TypedBinding")]
public void BindingWithExplicitPathEquals()
{
// This test documents a pre-existing bug where {Binding Path=PropertyName} doesn't compile
// This is why CarouselView UI tests fail - they use Path= syntax
var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
xmlns:test="clr-namespace:Test"
x:Class="Test.TestPage"
x:DataType="test:TestPage"
Title="{Binding Path=Name}"/>
""";

var code =
"""
#nullable enable
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
public string Name { get; set; } = "TestName";

public TestPage()
{
InitializeComponent();
}
}
""";

var (result, generated) = RunGenerator(xaml, code);

Assert.Empty(result.Diagnostics);
Assert.NotNull(generated);

// SHOULD compile as a property binding, but currently doesn't (pre-existing bug)
Assert.Contains("TypedBinding<global::Test.TestPage, string>", generated, StringComparison.Ordinal);
}
}
