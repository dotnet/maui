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
		var result = SourceGeneratorDriver.RunGenerator<CodeBehindGenerator>(compilation, cssFile);

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
		var result = SourceGeneratorDriver.RunGeneratorWithChanges<CodeBehindGenerator>(compilation, ApplyChanges, cssFile);

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
