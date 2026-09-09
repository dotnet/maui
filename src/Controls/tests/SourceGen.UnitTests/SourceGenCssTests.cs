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
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, cssFile);

		Assert.False(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase) && !gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

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
		var output1 = result1.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase) && !gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();
		var output2 = result2.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase) && !gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

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

	[Fact]
	public void CompiledCss_GeneratesFactory()
	{
		var css =
"""
.primary { background-color: #FF5733; font-size: 16; }
label { color: white; }
#myButton { opacity: 0.8; }
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var cssFile = new AdditionalCssFile("Styles.css", css, ManifestResourceName: "MyApp.Styles.css", TargetPath: "Styles.css");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, cssFile);

		Assert.False(result.Diagnostics.Any());

		var compiledSource = result.Results.Single().GeneratedSources
			.Single(gs => gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

		// Should contain the factory class
		Assert.Contains("__CompiledCss_Styles", compiledSource, StringComparison.Ordinal);
		Assert.Contains("GetStyleSheet", compiledSource, StringComparison.Ordinal);
		Assert.Contains("CreateCompiled", compiledSource, StringComparison.Ordinal);
		Assert.Contains("RegisterCompiled", compiledSource, StringComparison.Ordinal);

		// Should contain the pre-parsed selector and declarations
		Assert.Contains(".primary", compiledSource, StringComparison.Ordinal);
		Assert.Contains("background-color", compiledSource, StringComparison.Ordinal);
		Assert.Contains("#FF5733", compiledSource, StringComparison.Ordinal);
		Assert.Contains("font-size", compiledSource, StringComparison.Ordinal);
		Assert.Contains("label", compiledSource, StringComparison.Ordinal);
		Assert.Contains("color", compiledSource, StringComparison.Ordinal);
		Assert.Contains("#myButton", compiledSource, StringComparison.Ordinal);
		Assert.Contains("opacity", compiledSource, StringComparison.Ordinal);

		// Should also still have XamlResourceId for backward compat
		Assert.Contains("XamlResourceId", compiledSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CompiledCss_EmptyStylesheet()
	{
		var css = "/* just a comment */";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var cssFile = new AdditionalCssFile("Empty.css", css, ManifestResourceName: "MyApp.Empty.css", TargetPath: "Empty.css");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, cssFile);

		Assert.False(result.Diagnostics.Any());

		var compiledSource = result.Results.Single().GeneratedSources
			.Single(gs => gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

		Assert.Contains("__CompiledCss_Empty", compiledSource, StringComparison.Ordinal);
		Assert.Contains("Array.Empty", compiledSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CompiledCss_MultipleSelectors()
	{
		var css =
"""
.a, .b { color: red; }
stacklayout > label { font-size: 20; }
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var cssFile = new AdditionalCssFile("Multi.css", css, ManifestResourceName: "MyApp.Multi.css", TargetPath: "Multi.css");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, cssFile);

		Assert.False(result.Diagnostics.Any());

		var compiledSource = result.Results.Single().GeneratedSources
			.Single(gs => gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

		Assert.Contains(".a, .b", compiledSource, StringComparison.Ordinal);
		Assert.Contains("stacklayout > label", compiledSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CompiledCss_CommentsAreStripped()
	{
		var css =
"""
/* This is a header comment */
.primary {
    /* inline comment */
    color: red;
    background-color: blue; /* trailing comment */
}
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var cssFile = new AdditionalCssFile("Comments.css", css, ManifestResourceName: "MyApp.Comments.css", TargetPath: "Comments.css");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, cssFile);

		Assert.False(result.Diagnostics.Any());

		var compiledSource = result.Results.Single().GeneratedSources
			.Single(gs => gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

		// Comments should not appear in output
		Assert.DoesNotContain("header comment", compiledSource, StringComparison.Ordinal);
		Assert.DoesNotContain("inline comment", compiledSource, StringComparison.Ordinal);
		Assert.DoesNotContain("trailing comment", compiledSource, StringComparison.Ordinal);

		// But properties should
		Assert.Contains("color", compiledSource, StringComparison.Ordinal);
		Assert.Contains("background-color", compiledSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CompiledCss_ModifiedCss_RegeneratesFactory()
	{
		var css = ".a { color: red; }";
		var newCss = ".a { color: blue; } .b { font-size: 14; }";

		var cssFile = new AdditionalCssFile("Mod.css", css, ManifestResourceName: "MyApp.Mod.css", TargetPath: "Mod.css");
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(compilation, ApplyChanges, cssFile);

		var compiled1 = result.result1.Results.Single().GeneratedSources
			.Single(gs => gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();
		var compiled2 = result.result2.Results.Single().GeneratedSources
			.Single(gs => gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

		// Output should differ because CSS changed
		Assert.NotEqual(compiled1, compiled2);

		// New version should have the new rule
		Assert.Contains(".b", compiled2, StringComparison.Ordinal);
		Assert.Contains("blue", compiled2, StringComparison.Ordinal);

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			var newCssFile = new AdditionalCssFile("Mod.css", newCss, ManifestResourceName: "MyApp.Mod.css", TargetPath: "Mod.css");
			driver = driver.ReplaceAdditionalText(cssFile.Text, newCssFile.Text);
			return (driver, compilation);
		}
	}

	[Fact]
	public void CompiledCss_WithVariables_ResolvedAtCompileTime()
	{
		var css =
"""
:root { --primary: #FF5733; --text-color: white; }
.card { background-color: var(--primary); color: var(--text-color); }
label { color: var(--missing, blue); }
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var cssFile = new AdditionalCssFile("Theme.css", css, ManifestResourceName: "MyApp.Theme.css", TargetPath: "Theme.css");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, cssFile);

		Assert.False(result.Diagnostics.Any());

		var compiledSource = result.Results.Single().GeneratedSources
			.Single(gs => gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

		// :root selector and custom property declarations should still be in output
		Assert.Contains(":root", compiledSource, StringComparison.Ordinal);
		Assert.Contains("--primary", compiledSource, StringComparison.Ordinal);
		Assert.Contains("#FF5733", compiledSource, StringComparison.Ordinal);
		Assert.Contains("--text-color", compiledSource, StringComparison.Ordinal);

		// var() references should be RESOLVED to their values at compile time
		Assert.DoesNotContain("var(--primary)", compiledSource, StringComparison.Ordinal);
		Assert.DoesNotContain("var(--text-color)", compiledSource, StringComparison.Ordinal);
		Assert.DoesNotContain("var(--missing, blue)", compiledSource, StringComparison.Ordinal);

		// The resolved values should appear directly in declarations
		// .card background-color should be #FF5733, color should be white
		// label color should be blue (fallback for --missing)
		Assert.Contains("blue", compiledSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CompiledCss_RecursiveVariables_ResolvedAtCompileTime()
	{
		var css =
"""
:root { --base: green; --primary: var(--base); }
label { color: var(--primary); }
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var cssFile = new AdditionalCssFile("Recursive.css", css, ManifestResourceName: "MyApp.Recursive.css", TargetPath: "Recursive.css");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, cssFile);

		Assert.False(result.Diagnostics.Any());

		var compiledSource = result.Results.Single().GeneratedSources
			.Single(gs => gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

		// var(--primary) and var(--base) should both be resolved to "green"
		Assert.DoesNotContain("var(--primary)", compiledSource, StringComparison.Ordinal);
		Assert.DoesNotContain("var(--base)", compiledSource, StringComparison.Ordinal);

		// "green" should appear as the resolved value for both the --primary declaration and label color
		Assert.Contains("green", compiledSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CompiledCss_ShorthandExpanded_AtCompileTime()
	{
		var css =
"""
button { border: 2 solid red; }
label { font: italic bold 16 Arial; }
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var cssFile = new AdditionalCssFile("Shorthand.css", css, ManifestResourceName: "MyApp.Shorthand.css", TargetPath: "Shorthand.css");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, cssFile);

		Assert.False(result.Diagnostics.Any());

		var compiledSource = result.Results.Single().GeneratedSources
			.Single(gs => gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

		// border shorthand should NOT appear — should be expanded to border-width and border-color
		Assert.DoesNotContain("\"border\"", compiledSource, StringComparison.Ordinal);
		Assert.Contains("border-width", compiledSource, StringComparison.Ordinal);
		Assert.Contains("border-color", compiledSource, StringComparison.Ordinal);

		// font shorthand should NOT appear — should be expanded
		Assert.DoesNotContain("\"font\"", compiledSource, StringComparison.Ordinal);
		Assert.Contains("font-style", compiledSource, StringComparison.Ordinal);
		Assert.Contains("font-weight", compiledSource, StringComparison.Ordinal);
		Assert.Contains("font-size", compiledSource, StringComparison.Ordinal);
		Assert.Contains("font-family", compiledSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CompiledCss_ImportantTracked_AtCompileTime()
	{
		var css =
"""
label { color: red !important; background-color: blue; }
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var cssFile = new AdditionalCssFile("Important.css", css, ManifestResourceName: "MyApp.Important.css", TargetPath: "Important.css");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, cssFile);

		Assert.False(result.Diagnostics.Any());

		var compiledSource = result.Results.Single().GeneratedSources
			.Single(gs => gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

		// !important should be stripped from the value
		Assert.DoesNotContain("!important", compiledSource, StringComparison.Ordinal);
		// But importantProperties array should be emitted
		Assert.Contains("\"color\"", compiledSource, StringComparison.Ordinal);
		// The value should be clean
		Assert.Contains("red", compiledSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CompiledCss_CalcAndUnits_ResolvedAtCompileTime()
	{
		var css =
"""
label { font-size: 1.5rem; margin: calc(1rem + 4); padding: 20px; }
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var cssFile = new AdditionalCssFile("Units.css", css, ManifestResourceName: "MyApp.Units.css", TargetPath: "Units.css");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, cssFile);

		Assert.False(result.Diagnostics.Any());

		var compiledSource = result.Results.Single().GeneratedSources
			.Single(gs => gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

		// 1.5rem should be resolved to 24 (1.5 * 16)
		Assert.Contains("24", compiledSource, StringComparison.Ordinal);
		// calc(1rem + 4) = calc(16 + 4) = 20
		Assert.Contains("20", compiledSource, StringComparison.Ordinal);
		// raw units should NOT appear
		Assert.DoesNotContain("1.5rem", compiledSource, StringComparison.Ordinal);
		Assert.DoesNotContain("calc(", compiledSource, StringComparison.Ordinal);
		Assert.DoesNotContain("20px", compiledSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CompiledCss_MediaQuery_Parsed()
	{
		var css =
"""
label { color: red; }
@media (min-width: 768px) {
  label { color: blue; font-size: 20; }
  .container { max-width: 720; }
}
@charset "UTF-8";
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var cssFile = new AdditionalCssFile("Media.css", css, ManifestResourceName: "MyApp.Media.css", TargetPath: "Media.css");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, cssFile);

		Assert.False(result.Diagnostics.Any());

		var compiledSource = result.Results.Single().GeneratedSources
			.Single(gs => gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

		// Should contain base rule
		Assert.Contains("\"color\"", compiledSource, StringComparison.Ordinal);
		Assert.Contains("\"red\"", compiledSource, StringComparison.Ordinal);

		// Should contain CompiledCssMediaGroup with the condition
		Assert.Contains("CompiledCssMediaGroup", compiledSource, StringComparison.Ordinal);
		Assert.Contains("min-width: 768px", compiledSource, StringComparison.Ordinal);

		// Should contain media group rules
		Assert.Contains("\"blue\"", compiledSource, StringComparison.Ordinal);
		Assert.Contains(".container", compiledSource, StringComparison.Ordinal);

		// @charset should be silently skipped
		Assert.DoesNotContain("charset", compiledSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CompiledCss_PrefersColorScheme_MediaQuery()
	{
		var css =
"""
:root { --bg: white; }
@media (prefers-color-scheme: dark) {
  :root { --bg: #222; }
}
label { background-color: var(--bg); }
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var cssFile = new AdditionalCssFile("Theme.css", css, ManifestResourceName: "MyApp.Theme.css", TargetPath: "Theme.css");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, cssFile);

		Assert.False(result.Diagnostics.Any());

		var compiledSource = result.Results.Single().GeneratedSources
			.Single(gs => gs.HintName.Contains("compiled", StringComparison.Ordinal)).SourceText.ToString();

		// Base label should have resolved var(--bg) → "white"
		Assert.DoesNotContain("var(--bg)", compiledSource, StringComparison.Ordinal);
		Assert.Contains("white", compiledSource, StringComparison.Ordinal);

		// Media group with dark theme condition
		Assert.Contains("prefers-color-scheme: dark", compiledSource, StringComparison.Ordinal);
		Assert.Contains("#222", compiledSource, StringComparison.Ordinal);
	}
}
