using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class Maui32879Tests : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void StyleSetterWithAttachedPropertyContentSyntax()
	{
		// Test reproducing issue #32879: Style with setter on AbsoluteLayout.LayoutBounds using content property syntax
		// Previously generated invalid code like:
		// .Value = "10,10,20,20";
		// var setter = new Setter {...};
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<ResourceDictionary>
			<Style x:Key="NetworkIndicator" TargetType="Image">
				<Setter Property="AbsoluteLayout.LayoutBounds">10,10,20,20</Setter>
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
		Assert.Contains("new global::Microsoft.Maui.Controls.Style(\"Microsoft.Maui.Controls.Image, Microsoft.Maui.Controls\")", generated, StringComparison.Ordinal);
		Assert.Contains("AbsoluteLayout.LayoutBoundsProperty", generated, StringComparison.Ordinal);
		Assert.Contains("BoundsTypeConverter().ConvertFromInvariantString(\"10,10,20,20\")", generated, StringComparison.Ordinal);
		Assert.Contains("resourceDictionary[\"NetworkIndicator\"]", generated, StringComparison.Ordinal);
		Assert.Contains("style.LazyInitialization = styleInitializer", generated, StringComparison.Ordinal);
		
		// Verify lazy behavior: Initializer is set but NOT called immediately
		Assert.DoesNotContain("styleInitializer(style, new global::Microsoft.Maui.Controls.Label())", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("style.LazyInitialization = null!", generated, StringComparison.Ordinal);
	}
}
