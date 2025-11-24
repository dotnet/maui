using Microsoft.Maui.Controls.SourceGen.UnitTests;
using System;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Tests for Binding elements with Path attribute inside MultiBinding.
/// This scenario was causing crashes because the Path property wasn't being extracted correctly
/// when x:DataType was present on the Binding element itself.
/// </summary>
public class MultiBindingElementSyntaxTest : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void BindingElementWithPathAndDataTypeCompilesCorrectly()
	{
		// This test covers the scenario from Issue26328 where:
		// - MultiBinding contains Binding elements (not markup extension syntax)
		// - Each Binding element has both x:DataType and Path attributes
		// - The Path property needs to be extracted from a different namespace context
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage">
	<Label>
		<Label.Text>
			<MultiBinding>
				<Binding x:DataType="test:TestModel" Path="Id" />
				<Binding x:DataType="test:TestModel" Path="Name" />
			</MultiBinding>
		</Label.Text>
	</Label>
</ContentPage>
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
	public TestPage()
	{
		InitializeComponent();
	}
}

public class TestModel
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		Assert.Empty(result.Diagnostics);
		Assert.NotNull(generated);

		// The key verification is that the Path property is correctly extracted
		// When x:DataType is on the Binding element itself and Path is specified,
		// GetBindingPath should extract "Id" and "Name" correctly (not return ".")
		
		// Verify Path properties are being set correctly (not falling back to ".")
		Assert.Contains("bindingExtension.Path = \"Id\";", generated, StringComparison.Ordinal);
		Assert.Contains("bindingExtension1.Path = \"Name\";", generated, StringComparison.Ordinal);
		
		// Verify the bindings access the correct properties
		Assert.Contains("source.Id", generated, StringComparison.Ordinal);
		Assert.Contains("source.Name", generated, StringComparison.Ordinal);
		
		// The binding should NOT be treated as a self-binding (path = ".")
		// If it were, GetBindingPath would return "." and we wouldn't see Path being set
		// This test primarily ensures the Path property extraction works in this context
	}
}
