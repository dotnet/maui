using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class SourceGenCssTests : SourceGenTestsBase
{
	private record AdditionalCssFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Css", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName ?? Path, TargetFramework: TargetFramework, NoWarn: "");

	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName ?? Path, TargetFramework: TargetFramework, NoWarn: "");

	[Fact]
	public void TestStyleSheetSource_RootedPathCompilesWithoutInternalApiCalls()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    x:Class="Test.MainPage">
    <ContentPage.Resources>
        <StyleSheet Source="/Resources/Styles/Styles.css" />
    </ContentPage.Resources>
</ContentPage>
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var xamlFile = new AdditionalXamlFile("Views/MainPage.xaml", xaml, RelativePath: @"Views\MainPage.xaml");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, xamlFile, assertNoCompilationErrors: true);

		Assert.Empty(result.Diagnostics);

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		Assert.Contains("global::Microsoft.Maui.Controls.StyleSheets.StyleSheet.FromResource(@\"Resources/Styles/Styles.css\", typeof(global::Test.MainPage).Assembly,", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("RDSourceTypeConverter.GetResourcePath", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("XamlResourceIdAttribute.GetPathForType", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void TestStyleSheetSource_RelativePathResolvesAgainstXamlPath()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    x:Class="Test.SubPage">
    <ContentPage.Resources>
        <StyleSheet Source="../Resources/Styles/Styles.css" />
        <StyleSheet Source="Local.css" />
    </ContentPage.Resources>
</ContentPage>
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var xamlFile = new AdditionalXamlFile("Views/SubPage.xaml", xaml, RelativePath: @"Views\SubPage.xaml");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, xamlFile, assertNoCompilationErrors: true);

		Assert.Empty(result.Diagnostics);

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		Assert.Contains("global::Microsoft.Maui.Controls.StyleSheets.StyleSheet.FromResource(@\"Resources/Styles/Styles.css\", typeof(global::Test.SubPage).Assembly,", generated, StringComparison.Ordinal);
		Assert.Contains("global::Microsoft.Maui.Controls.StyleSheets.StyleSheet.FromResource(@\"Views/Local.css\", typeof(global::Test.SubPage).Assembly,", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void TestCodeBehindGenerator_BasicCss()
	{
		var css =
"""
h1 {color: purple;
    background-color: lightcyan;
    font-weight: 800;
}
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var cssFile = new AdditionalCssFile("Test.css", css);
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, cssFile);

		Assert.False(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		Assert.Contains($"XamlResourceId(\"{cssFile.ManifestResourceName}\", \"{cssFile.Path}\"", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void TestCodeBehindGenerator_ModifiedCss()
	{
		var css =
"""
h1 {color: purple;
    background-color: lightcyan;
    font-weight: 800;
}
""";
		var newCss =
"""
h1 {color: red;
    background-color: lightcyan;
    font-weight: 800;
}
""";
		var cssFile = new AdditionalCssFile("Test.css", css);
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(compilation, ApplyChanges, cssFile);

		var result1 = result.result1.Results.Single();
		var result2 = result.result2.Results.Single();
		var output1 = result1.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();
		var output2 = result2.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		// Assert.True(result1.TrackedSteps.All(s => s.Value.Single().Outputs.Single().Reason == IncrementalStepRunReason.New));
		Assert.Equal(output1, output2);

		Assert.Contains($"XamlResourceId(\"{cssFile.ManifestResourceName}\", \"{cssFile.Path}\"", output1, StringComparison.Ordinal);

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			var newCssFile = new AdditionalCssFile("Test.css", newCss);
			driver = driver.ReplaceAdditionalText(cssFile.Text, newCssFile.Text);
			return (driver, compilation);
		}

		var expectedReasons = new Dictionary<string, IncrementalStepRunReason>
		{
			{ TrackingNames.ProjectItemProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.CssProjectItemProvider, IncrementalStepRunReason.Modified }
		};

		VerifyStepRunReasons(result2, expectedReasons);
	}
}
