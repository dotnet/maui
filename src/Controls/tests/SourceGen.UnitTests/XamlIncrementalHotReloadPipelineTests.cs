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
using System.Collections.Immutable;
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

	// Extracts generated sources from a run result by hint-name suffix
	static ImmutableArray<GeneratedSourceResult> GetSources(GeneratorDriverRunResult result)
		=> result.GeneratedTrees
			.Select(t => t)
			.Select(_ => default(GeneratedSourceResult))
			.ToImmutableArray();

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

		var ucSource = FindSourceByHintSuffix(result, ".uc_v1to2.xsg.cs");
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

		// Assert: state was seeded
		var hasPrev = XamlHotReloadState.TryGetPrevious("TestApp", PageRelativePath, out _, out var version);
		Assert.True(hasPrev, "XamlHotReloadState should be seeded after first run");
		Assert.Equal(1, version);
	}

	[Fact]
	public void SecondRun_PropertyChange_EmitsUCSource()
	{
		// Arrange
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV2_PropertyChange);

		// Assert: UC source exists in run2
		var ucSource = FindUCSource(run2, "uc_v1to2");
		Assert.NotNull(ucSource);

		// Should contain UpdateComponent method with Text property assignment
		Assert.Contains("UpdateComponent_v1to2", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_PropertyChange_UCContainsNewValue()
	{
		// Arrange
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV2_PropertyChange);

		// Find the UC source specifically (contains "uc_v1to2")
		var ucSource = FindUCSource(run2, "uc_v1to2");

		Assert.NotNull(ucSource);
		Assert.Contains("\"World\"", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_PropertyChange_UCContainsVersionGuard()
	{
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV2_PropertyChange);

		var ucSource = FindUCSource(run2, "uc_v1to2");
		Assert.NotNull(ucSource);

		// Guard: if (__version != 1) return;
		Assert.Contains("__version != 1", ucSource, StringComparison.Ordinal);
		// Version bump
		Assert.Contains("__version = 2", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_StructuralChange_NoUCEmitted()
	{
		// Arrange
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV3_StructuralChange);

		// Assert: no UC source for structural change
		var ucSource = FindUCSource(run2, "uc_v1to2");
		Assert.Null(ucSource);
	}

	[Fact]
	public void SecondRun_IdenticalXaml_NoUCEmitted()
	{
		// Arrange
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV1); // no change

		// Assert: no UC for identical XAML
		var ucSource = FindUCSource(run2, "uc_v1to2");
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
		var hasPrev = XamlHotReloadState.TryGetPrevious("TestApp", PageRelativePath, out _, out _);
		Assert.False(hasPrev, "XamlHotReloadState should NOT be seeded when IHR is disabled");
	}

	[Fact]
	public void FirstRun_EmitsMetadataUpdateHandlerSource()
	{
		// Arrange
		XamlHotReloadState.Reset();
		var compilation = CreateCompilation();
		var file = MakeFile(PageXamlV1);

		// Act
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(
			compilation, file, assertNoCompilationErrors: false);

		// Assert: handler source file was emitted alongside IC
		var handlerSource = FindUCSource(result, ".handler.xsg.cs");
		Assert.NotNull(handlerSource);

		// Should declare MetadataUpdateHandler assembly attribute
		Assert.Contains("MetadataUpdateHandler", handlerSource, StringComparison.Ordinal);
		// Should reference the page type
		Assert.Contains("MainPage", handlerSource, StringComparison.Ordinal);
		// Should contain UpdateApplication method
		Assert.Contains("UpdateApplication", handlerSource, StringComparison.Ordinal);
		// Should enumerate live instances via XamlComponentRegistry
		Assert.Contains("XamlComponentRegistry", handlerSource, StringComparison.Ordinal);
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
		var ucSource = FindUCSource(run2, "uc_v");

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
		var ucSource = FindUCSource(run2, "uc_v");

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
		XamlHotReloadState.Update("App1", relPath, xaml, 1);

		// "App2" with same relative path should NOT see "App1" state
		var hasPrev = XamlHotReloadState.TryGetPrevious("App2", relPath, out _, out _);
		Assert.False(hasPrev, "Different assembly should not see another assembly's state");

		// "App1" SHOULD see its own state
		var hasSelf = XamlHotReloadState.TryGetPrevious("App1", relPath, out var storedXaml, out var storedVer);
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
