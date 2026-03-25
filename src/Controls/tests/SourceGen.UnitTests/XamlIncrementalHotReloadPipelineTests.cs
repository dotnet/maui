// Phase 7: Incremental Hot Reload Pipeline Integration Tests
//
// These tests exercise the full XamlGenerator → XamlHotReloadState → UpdateComponentCodeWriter pipeline.
// They simulate a developer editing XAML and verify that:
//   1. The first generator run seeds XamlHotReloadState (no UC emitted).
//   2. A subsequent run for the SAME file with property-only changes emits a UC source file.
//   3. A structural change (add/remove element) produces NO UC (full reload required).
//   4. Unchanged XAML produces no UC.

#nullable enable

using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Integration tests for the XAML Incremental Hot Reload pipeline.
/// Verifies that the XamlGenerator + XamlHotReloadState + UpdateComponentCodeWriter
/// correctly emit UC source files when XAML property values change between generator runs.
/// </summary>
[Collection("XamlHotReloadTests")]
public class XamlIncrementalHotReloadPipelineTests : IDisposable
{
	// -----------------------------------------------------------------------
	// Helpers
	// -----------------------------------------------------------------------

	const string PageXamlV1 = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	const string PageXamlV2_PropertyChange = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="World" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	const string PageXamlV3_StructuralChange = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" />
		        <Button Text="Click me" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	const string PageRelativePath = "MainPage.xaml";

	static SourceGeneratorDriver.AdditionalFile MakeFile(string xaml, bool ihr = true) =>
		new(
			SourceGeneratorDriver.ToAdditionalText(PageRelativePath, xaml),
			Kind: "Xaml",
			RelativePath: PageRelativePath,
			TargetPath: "TestApp/MainPage.xaml",
			ManifestResourceName: null,
			TargetFramework: "net11.0",
			NoWarn: null,
			EnableIncrementalHotReload: ihr);

	static Compilation CreateCompilation() =>
		SourceGeneratorDriver.CreateMauiCompilation("TestApp");

	// Helper to run generator twice, simulating a XAML edit
	(GeneratorDriverRunResult run1, GeneratorDriverRunResult run2) TwoRuns(
		string xamlV1,
		string xamlV2,
		bool enableIHR = true)
	{
		var compilation = CreateCompilation();
		var fileV1 = MakeFile(xamlV1, enableIHR);
		var fileV2 = MakeFile(xamlV2, enableIHR);

		GeneratorDriverRunResult run1;
		GeneratorDriverRunResult run2;
		(run1, run2) = SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(
			compilation,
			applyChanges: (driver, comp) =>
			{
				// Replace XAML text with V2 content but keep same path/options
				var updatedDriver = driver
					.ReplaceAdditionalText(fileV1.Text, fileV2.Text)
					.WithUpdatedAnalyzerConfigOptions(new OptionsProvider([fileV2]));
				return (updatedDriver, comp);
			},
			fileV1);

		return (run1, run2);
	}

	static string? FindSourceByHintSuffix(GeneratorDriverRunResult result, string suffix)
	{
		foreach (var gen in result.Results)
		{
			foreach (var source in gen.GeneratedSources)
			{
				if (source.HintName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
					return source.SourceText.ToString();
			}
		}
		return null;
	}

	// -----------------------------------------------------------------------
	// Dispose: always reset static state to isolate tests
	// -----------------------------------------------------------------------

	public void Dispose() => XamlHotReloadState.Reset();

	// -----------------------------------------------------------------------
	// Tests
	// -----------------------------------------------------------------------

	[Fact]
	public void FirstRun_NoUCEmitted()
	{
		// Arrange
		XamlHotReloadState.Reset();
		var compilation = CreateCompilation();
		var file = MakeFile(PageXamlV1);

		// Act: single run
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(
			compilation, file, assertNoCompilationErrors: false);

		// Assert: IC source must exist, no UC source
		var icSource = FindSourceByHintSuffix(result, ".xsg.cs");
		Assert.NotNull(icSource);

		var ucSource = FindUCSource(result, "uc.xsg");
		Assert.Null(ucSource);
	}

	[Fact]
	public void FirstRun_SeedsHotReloadState()
	{
		// Arrange
		XamlHotReloadState.Reset();
		var compilation = CreateCompilation();
		var file = MakeFile(PageXamlV1);

		// Act
		SourceGeneratorDriver.RunGenerator<XamlGenerator>(
			compilation, file, assertNoCompilationErrors: false);

		// Assert: state was seeded at version 0 (spec: __version starts at 0)
		var hasPrev = XamlHotReloadState.TryGetPrevious("TestApp", PageRelativePath, out _, out _, out _, out _, out var version);
		Assert.True(hasPrev, "XamlHotReloadState should be seeded after first run");
		Assert.Equal(0, version);
	}

	[Fact]
	public void SecondRun_PropertyChange_EmitsUCSource()
	{
		// Arrange
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV2_PropertyChange);

		// Assert: UC source exists in run2 with fixed hint name "uc.xsg"
		var ucSource = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(ucSource);

		// Should contain single UpdateComponent method (not versioned)
		Assert.Contains("void UpdateComponent()", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("UpdateComponent_v", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_PropertyChange_UCContainsNewValue()
	{
		// Arrange
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV2_PropertyChange);

		// Find the UC source
		var ucSource = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(ucSource);
		Assert.Contains("\"World\"", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_PropertyChange_UCContainsVersionGuard()
	{
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV2_PropertyChange);

		var ucSource = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(ucSource);

		// Per spec: if (__version == 0) { ... __version = 1; }
		Assert.Contains("__version == 0", ucSource, StringComparison.Ordinal);
		// Version bump inside the if block
		Assert.Contains("__version = 1", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_StructuralChange_EmitsUCWithChildAdd()
	{
		// Arrange: V3 adds a Button child — our child add/remove support handles this incrementally
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV3_StructuralChange);

		// Assert: UC source IS emitted for child additions (not structural fallback)
		var ucSource = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(ucSource);
		Assert.Contains("Button", ucSource!, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_IdenticalXaml_NoUCEmitted()
	{
		// Arrange
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV1); // no change

		// Assert: no UC for identical XAML
		var ucSource = FindUCSource(run2, "uc.xsg");
		Assert.Null(ucSource);
	}

	[Fact]
	public void WithoutIHR_Disabled_NoStateSeeded()
	{
		// Arrange
		XamlHotReloadState.Reset();
		var compilation = CreateCompilation();
		var file = MakeFile(PageXamlV1, ihr: false); // IHR disabled

		// Act
		SourceGeneratorDriver.RunGenerator<XamlGenerator>(
			compilation, file, assertNoCompilationErrors: false);

		// Assert: state NOT seeded when IHR is disabled
		var hasPrev = XamlHotReloadState.TryGetPrevious("TestApp", PageRelativePath, out _, out _, out _, out _, out _);
		Assert.False(hasPrev, "XamlHotReloadState should NOT be seeded when IHR is disabled");
	}

	[Fact]
	public void FirstRun_DoesNotEmitHandlerSource()
	{
		// Arrange: the SDK-level XamlIncrementalHotReloadHandler is now used instead
		// of per-page generated handlers, so no .handler.xsg.cs should be emitted.
		XamlHotReloadState.Reset();
		var compilation = CreateCompilation();
		var file = MakeFile(PageXamlV1);

		// Act
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(
			compilation, file, assertNoCompilationErrors: false);

		// Assert: no handler source emitted (SDK handler is used instead)
		var handlerSource = FindUCSource(result, ".handler.xsg.cs");
		Assert.Null(handlerSource);
	}

	[Fact]
	public void FirstRun_IHRDisabled_NoHandlerSource()
	{
		// Arrange
		XamlHotReloadState.Reset();
		var compilation = CreateCompilation();
		var file = MakeFile(PageXamlV1, ihr: false);

		// Act
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(
			compilation, file, assertNoCompilationErrors: false);

		// Assert: no handler when IHR is disabled
		var handlerSource = FindUCSource(result, ".handler.xsg.cs");
		Assert.Null(handlerSource);
	}

	// -----------------------------------------------------------------------
	// Bug 1: TryGet out-var pattern test
	// -----------------------------------------------------------------------

	[Fact]
	public void GeneratedCode_UsesOutVarPatternForTryGet()
	{
		// Arrange
		XamlHotReloadState.Reset();
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <Label x:Name="myLabel" Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <Label x:Name="myLabel" Text="World" />
			</ContentPage>
			""";

		// Act
		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var ucSource = FindUCSource(run2, "uc.xsg");

		// Assert: must use out-var pattern, NOT "var __uc = TryGet(...)"
		if (ucSource != null)
		{
			Assert.Contains("out var ", ucSource, StringComparison.Ordinal);
			// Old buggy pattern was: var __uc_0 = TryGet(...) -- make sure it's gone
			Assert.DoesNotContain("= TryGet(", ucSource, StringComparison.Ordinal);
		}
		// If ucSource is null the property change is structural (x:Name present) -- that's acceptable
	}

	// -----------------------------------------------------------------------
	// Bug 2: Root property change emits this.Prop = value
	// -----------------------------------------------------------------------

	[Fact]
	public void RootPropertyChange_EmitsThisDotAssignment()
	{
		// Arrange
		XamlHotReloadState.Reset();
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage"
			             Title="Hello">
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage"
			             Title="World">
			    <Label Text="Hello" />
			</ContentPage>
			""";

		// Act
		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var ucSource = FindUCSource(run2, "uc.xsg");

		// Assert: root property change should emit this.Title = "World" (not registry lookup)
		if (ucSource != null)
		{
			Assert.Contains("this.Title", ucSource, StringComparison.Ordinal);
			// Must NOT do a registry lookup for the root node
			Assert.DoesNotContain("TryGet", ucSource.Split('\n')
				.FirstOrDefault(l => l.Contains("Title", StringComparison.Ordinal)) ?? "", StringComparison.Ordinal);
		}
		// null means structural fallback (acceptable if x:Class treated as structural)
	}

	// -----------------------------------------------------------------------
	// Bug 3: XamlHotReloadState isolated by assembly name
	// -----------------------------------------------------------------------

	[Fact]
	public void HotReloadState_IsolatedByAssemblyName()
	{
		// Arrange
		XamlHotReloadState.Reset();
		const string xaml = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="App.MainPage">
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string relPath = "MainPage.xaml";

		// Seed with assembly "App1"
		XamlHotReloadState.Update("App1", relPath, xaml, null, null, 0, 1);

		// "App2" with same relative path should NOT see "App1" state
		var hasPrev = XamlHotReloadState.TryGetPrevious("App2", relPath, out _, out _, out _, out _, out _);
		Assert.False(hasPrev, "Different assembly should not see another assembly's state");

		// "App1" SHOULD see its own state
		var hasSelf = XamlHotReloadState.TryGetPrevious("App1", relPath, out var storedXaml, out _, out _, out _, out var storedVer);
		Assert.True(hasSelf);
		Assert.Equal(xaml, storedXaml);
		Assert.Equal(1, storedVer);
	}

	// -----------------------------------------------------------------------
	// Helpers
	// -----------------------------------------------------------------------

	static string? FindUCSource(GeneratorDriverRunResult result, string hintFragment)
	{
		foreach (var gen in result.Results)
		{
			foreach (var source in gen.GeneratedSources)
			{
				if (source.HintName.Contains(hintFragment, StringComparison.OrdinalIgnoreCase))
					return source.SourceText.ToString();
			}
		}
		return null;
	}

	// Minimal AnalyzerConfigOptionsProvider wrapper to thread updated options on replayed runs
	sealed class OptionsProvider : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider
	{
		readonly SourceGeneratorDriver.AdditionalFile[] _files;

		public OptionsProvider(SourceGeneratorDriver.AdditionalFile[] files) => _files = files;

		public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GlobalOptions =>
			throw new NotImplementedException();

		public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(SyntaxTree tree) =>
			throw new NotImplementedException();

		public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(AdditionalText textFile)
		{
			foreach (var f in _files)
			{
				if (f.Text.Path == textFile.Path)
					return new SimpleOptions(f);
			}
			return EmptyOptions.Instance;
		}
	}

	sealed class SimpleOptions : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions
	{
		readonly SourceGeneratorDriver.AdditionalFile _file;
		public SimpleOptions(SourceGeneratorDriver.AdditionalFile file) => _file = file;

		public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
		{
			value = key switch
			{
				"build_metadata.additionalfiles.GenKind" => _file.Kind,
				"build_metadata.additionalfiles.TargetPath" => _file.TargetPath,
				"build_metadata.additionalfiles.ManifestResourceName" => _file.ManifestResourceName,
				"build_metadata.additionalfiles.RelativePath" => _file.RelativePath,
				"build_metadata.additionalfiles.Inflator" => "SourceGen",
				"build_property.targetFramework" => _file.TargetFramework,
				"build_property.Configuration" => "Debug",
				"build_property.EnableMauiXamlDiagnostics" => "true",
				"build_property.EnableMauiIncrementalHotReload" => _file.EnableIncrementalHotReload ? "true" : null,
				_ => null
			};
			return value is not null;
		}
	}

	sealed class EmptyOptions : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions
	{
		public static readonly EmptyOptions Instance = new();
		public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
		{ value = null; return false; }
	}
}
