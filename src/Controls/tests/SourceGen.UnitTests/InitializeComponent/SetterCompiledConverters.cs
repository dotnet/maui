using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class SetterCompiledConverters : SourceGenXamlInitializeComponentTestBase
{
[Fact]
public void SetterWithCompiledConverters_DoesNotGenerateDeadCode()
{
var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage 
xmlns="http://schemas.microsoft.com/dotnet/2021/maui" 
xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml" 
x:Class="Test.TestPage">
<ContentPage.Resources>
<ResourceDictionary>
<Style x:Key="testStyle" TargetType="Label">
<Setter Property="FontSize" Value="16" />
<Setter Property="TextColor" Value="Red" />
</Style>
</ResourceDictionary>
</ContentPage.Resources>
</ContentPage>
""";

var code =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
public TestPage()
{
InitializeComponent();
}
}
""";

var (result, generated) = RunGenerator(xaml, code);
Assert.False(result.Diagnostics.Any());

// Verify trimmable style constructor is used (not typeof)
Assert.Contains("new global::Microsoft.Maui.Controls.Style(\"Microsoft.Maui.Controls.Label, Microsoft.Maui.Controls\")", generated, StringComparison.Ordinal);

// Verify setters are generated with compiled property references
Assert.Contains("Label.FontSizeProperty", generated, StringComparison.Ordinal);
Assert.Contains("Label.TextColorProperty", generated, StringComparison.Ordinal);
Assert.Contains("16D", generated, StringComparison.Ordinal);
Assert.Contains("Colors.Red", generated, StringComparison.Ordinal);

// Verify lazy initialization pattern for trimmable styles
Assert.Contains("LazyInitialization = (__style, __target) =>", generated, StringComparison.Ordinal);
Assert.Contains("__style.Setters", generated, StringComparison.Ordinal);

// Explicitly verify that XamlTypeResolver is not used anywhere in the generated code
// This is critical because XamlTypeResolver is not AOT-compatible
Assert.DoesNotContain("XamlTypeResolver", generated, StringComparison.Ordinal);
}
}
