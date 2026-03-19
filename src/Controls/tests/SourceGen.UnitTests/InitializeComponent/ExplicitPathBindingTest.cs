using Microsoft.Maui.Controls.SourceGen.UnitTests;
using System;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Tests for bindings with explicit Path= syntax.
/// Fixed: GetBindingPath now uses XmlName(null, "Path") to correctly extract the Path property.
/// </summary>
public class ExplicitPathBindingTest : SourceGenXamlInitializeComponentTestBase
{
	[Theory]
	[InlineData("{Binding Name}")]
	[InlineData("{Binding Path=Name}")]
	public void BindingCompilesToTypedBinding(string bindingExpression)
	{
		// This test verifies that both {Binding PropertyName} and {Binding Path=PropertyName} compile correctly
		var xaml =
$$"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:TestPage"
	Title="{{bindingExpression}}"/>
""";

		var code =
"""
#nullable enable
#pragma warning disable CS0219 // Variable is assigned but its value is never used
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

		// Should compile as a property binding to Name property
		Assert.Contains("TypedBinding<global::Test.TestPage, string>", generated, StringComparison.Ordinal);
		Assert.Contains("source.Name", generated, StringComparison.Ordinal);
	}
}
