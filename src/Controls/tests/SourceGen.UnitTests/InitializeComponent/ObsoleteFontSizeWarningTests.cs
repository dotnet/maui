using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class ObsoleteFontSizeWarningTests : SourceGenXamlInitializeComponentTestBase
{
	const string DefaultCodeBehind =
"""
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
""";

	[Theory]
	[InlineData("Default")]
	[InlineData("Micro")]
	[InlineData("Small")]
	[InlineData("Medium")]
	[InlineData("Large")]
	[InlineData("Body")]
	[InlineData("Header")]
	[InlineData("Title")]
	[InlineData("Subtitle")]
	[InlineData("Caption")]
	public void NamedFontSize_EmitsObsoleteXamlWarning(string namedSize)
	{
		var xaml =
$$"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label FontSize="{{namedSize}}" />
</ContentPage>
""";

		var (result, _) = RunGenerator(xaml, DefaultCodeBehind);

		var warning = Assert.Single(result.Diagnostics, d => d.Id == "MAUIX2025");
		Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
		Assert.Contains($"NamedSize.{namedSize}", warning.GetMessage(), StringComparison.Ordinal);
		Assert.Contains("numeric FontSize", warning.GetMessage(), StringComparison.Ordinal);
		Assert.EndsWith("Test.xaml", warning.Location.GetLineSpan().Path, StringComparison.Ordinal);
		Assert.Equal(5, warning.Location.GetLineSpan().StartLinePosition.Line);
	}

	[Fact]
	public void NamedFontSize_CaseInsensitiveAndStyleSetter_EmitsObsoleteXamlWarning()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<Style TargetType="Label">
			<Setter Property="Label.FontSize" Value="Large" />
		</Style>
	</ContentPage.Resources>
	<Label FontSize="medium" />
</ContentPage>
""";

		var (result, _) = RunGenerator(xaml, DefaultCodeBehind);

		var warnings = result.Diagnostics.Where(d => d.Id == "MAUIX2025").ToArray();
		Assert.Equal(2, warnings.Length);
		Assert.Contains(warnings, w => w.GetMessage().Contains("NamedSize.Large", StringComparison.Ordinal));
		Assert.Contains(warnings, w => w.GetMessage().Contains("NamedSize.Medium", StringComparison.Ordinal));
	}

	[Theory]
	[InlineData("12")]
	[InlineData("18.5")]
	[InlineData("0")]
	public void NumericFontSize_DoesNotEmitObsoleteWarning(string fontSize)
	{
		var xaml =
$$"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label FontSize="{{fontSize}}" />
</ContentPage>
""";

		var (result, _) = RunGenerator(xaml, DefaultCodeBehind);

		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2025");
	}

	[Theory]
	[InlineData("MAUIX2025")]
	[InlineData("2025")]
	[InlineData("XC0618")]
	[InlineData("0618")]
	public void NamedFontSize_WithNoWarn_SuppressesWarning(string noWarn)
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label FontSize="Large" />
</ContentPage>
""";

		var (result, _) = RunGenerator(xaml, DefaultCodeBehind, noWarn: noWarn);

		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2025");
	}

	[Fact]
	public void NamedFontSize_DoesNotEmitCS0612FromGeneratedCode()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<VerticalStackLayout>
		<Label FontSize="Large" />
		<Button FontSize="Small" />
	</VerticalStackLayout>
</ContentPage>
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(DefaultCodeBehind));

		var xamlPath = Path.Combine(Environment.CurrentDirectory, "Test.xaml");
		var additionalFile = new AdditionalXamlFile(
			xamlPath,
			xaml,
			RelativePath: "Test.xaml",
			ManifestResourceName: $"{compilation.AssemblyName}.Test.xaml");

		GeneratorDriver driver = CSharpGeneratorDriver.Create([new XamlGenerator().AsSourceGenerator()])
			.AddAdditionalTexts([additionalFile.Text])
			.WithUpdatedAnalyzerConfigOptions(CreateAnalyzerConfigOptionsProvider(additionalFile));

		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var generatorDiagnostics);

		Assert.Equal(2, generatorDiagnostics.Count(d => d.Id == "MAUIX2025"));
		Assert.DoesNotContain(updatedCompilation.GetDiagnostics(), d => d.Id == "CS0612");
	}
}
