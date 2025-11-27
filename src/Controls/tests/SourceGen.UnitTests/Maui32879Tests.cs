using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class Maui32879Tests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn);

	[Fact]
	public void StyleSetterWithAttachedPropertyContentSyntax()
	{
		// Test reproducing issue #32879: Style with setter on AbsoluteLayout.LayoutBounds using content property syntax
		// fails to compile because SourceGen produces invalid code like:
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
	<AbsoluteLayout>
		<Image Style="{StaticResource NetworkIndicator}" Source="dotnet_bot.png"/>
	</AbsoluteLayout>
</ContentPage>
""";

		var compilation = CreateMauiCompilation();
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		// This should NOT produce any compilation errors in the generated code
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
	}

	[Fact]
	public void StyleSetterWithAttachedPropertyAttributeSyntax()
	{
		// Verify that attribute syntax for Setter Value still works
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage2">
	<ContentPage.Resources>
		<ResourceDictionary>
			<Style x:Key="NetworkIndicator" TargetType="Image">
				<Setter Property="AbsoluteLayout.LayoutBounds" Value="10,10,20,20"/>
			</Style>
		</ResourceDictionary>
	</ContentPage.Resources>
	<AbsoluteLayout>
		<Image Style="{StaticResource NetworkIndicator}" Source="dotnet_bot.png"/>
	</AbsoluteLayout>
</ContentPage>
""";

		var compilation = CreateMauiCompilation();
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test2.xaml", xaml));

		// This should NOT produce any compilation errors
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
	}
}
