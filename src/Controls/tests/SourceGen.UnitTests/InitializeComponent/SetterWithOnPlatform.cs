using System;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class SetterWithOnPlatform : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void SetterWithOnPlatformExtensionSetsPropertyCorrectly()
	{
		var xaml = """
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			 xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			 x:Class="Test.TestPage">
	<ContentPage.Resources>
		<Style TargetType="Label">
			<Setter Property="FontSize" Value="{OnPlatform iOS=36, Android=24}" />
			<Setter Property="TextColor" Value="{OnPlatform iOS=Red, Android=Blue}" />
		</Style>
	</ContentPage.Resources>
</ContentPage>
""";

		var code = """
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
		
		// Verify that XamlTypeResolver is not present (AOT-compatible)
		Assert.DoesNotContain("XamlTypeResolver", generated, StringComparison.Ordinal);
		
		// Verify that OnPlatform extensions are processed correctly
		// The Property must be set on the Setter so OnPlatform can determine the target type
		Assert.Contains("setter.Property = global::Microsoft.Maui.Controls.Label.FontSizeProperty", generated, StringComparison.Ordinal);
		Assert.Contains("setter1.Property = global::Microsoft.Maui.Controls.Label.TextColorProperty", generated, StringComparison.Ordinal);
		
		// Verify OnPlatform is processed
		Assert.Contains("OnPlatformExtension", generated, StringComparison.Ordinal);
	}
}
