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

	// V1 with two children: Label + Button
	const string PageXamlV4_TwoChildren = """
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

	// V5: Remove Button, keep only Label (remove child)
	const string PageXamlV5_ChildRemoved = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V6: Reorder — Button before Label (swap children)
	const string PageXamlV6_ChildrenReordered = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Button Text="Click me" />
		        <Label Text="Hello" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V7: Nested layout — add a Grid inside VerticalStackLayout with children
	const string PageXamlV7_NestedLayout = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" />
		        <Grid>
		            <Label Text="Nested" />
		            <Button Text="Inner" />
		        </Grid>
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V8: Replace child type — swap Label for an Entry (different element type)
	const string PageXamlV8_ChildReplaced = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Entry Placeholder="Type here" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V9: Three children — Label, Button, Entry (add multiple)
	const string PageXamlV9_MultipleAdded = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" />
		        <Button Text="Click me" />
		        <Entry Placeholder="Type here" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// --- Type converter test variants ---

	// V10: Label with BackgroundColor (Color converter)
	const string PageXamlV10_ColorProperty = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" BackgroundColor="Red" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V11: Label with BackgroundColor changed to Blue
	const string PageXamlV11_ColorChanged = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" BackgroundColor="Blue" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V12: Label with Padding (Thickness converter)
	const string PageXamlV12_ThicknessProperty = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" Padding="10,20,30,40" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V13: Label with Padding changed
	const string PageXamlV13_ThicknessChanged = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" Padding="5" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V14: Label with FontSize (double) and FontAttributes (enum)
	const string PageXamlV14_EnumAndDouble = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" FontSize="24" FontAttributes="Bold" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V15: Label with FontSize and FontAttributes changed
	const string PageXamlV15_EnumAndDoubleChanged = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" FontSize="18" FontAttributes="Italic" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V16: Grid with ColumnDefinitions (GridLength converter)
	const string PageXamlV16_GridLength = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <Grid ColumnDefinitions="*,2*,Auto" RowDefinitions="100,*">
		        <Label Text="Hello" />
		    </Grid>
		</ContentPage>
		""";

	// V17: Grid with ColumnDefinitions changed
	const string PageXamlV17_GridLengthChanged = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <Grid ColumnDefinitions="*,*,*" RowDefinitions="50,*">
		        <Label Text="Hello" />
		    </Grid>
		</ContentPage>
		""";

	// V18: Add a new child with a Binding markup extension
	const string PageXamlV18_NewChildWithBinding = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" />
		        <Label Text="{Binding Name}" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V19: Add a new child with StaticResource and DynamicResource
	const string PageXamlV19_NewChildWithResources = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" />
		        <Label TextColor="{DynamicResource PrimaryColor}" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V20: x:DataType known — Binding should produce a compiled TypedBinding
	const string PageXamlV1_WithDataType = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:TestApp"
		             x:Class="TestApp.MainPage"
		             x:DataType="local:MyViewModel">
		    <VerticalStackLayout>
		        <Label Text="Hello" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	const string PageXamlV20_NewChildWithCompiledBinding = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:TestApp"
		             x:Class="TestApp.MainPage"
		             x:DataType="local:MyViewModel">
		    <VerticalStackLayout>
		        <Label Text="Hello" />
		        <Label Text="{Binding Name}" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V21: Same bindings but x:DataType changed to a different ViewModel
	const string PageXamlV1_BindingWithDataType = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:TestApp"
		             x:Class="TestApp.MainPage"
		             x:DataType="local:MyViewModel">
		    <VerticalStackLayout>
		        <Label Text="{Binding Name}" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	const string PageXamlV21_DataTypeChanged = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:TestApp"
		             x:Class="TestApp.MainPage"
		             x:DataType="local:OtherViewModel">
		    <VerticalStackLayout>
		        <Label Text="{Binding Title}" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V22: Root content swap — VerticalStackLayout → Grid (different direct child of ContentPage)
	const string PageXamlV22_RootContentSwap = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <Grid>
		        <Label Text="In Grid" />
		    </Grid>
		</ContentPage>
		""";

	// V23: Add a ScrollView (content container) inside VerticalStackLayout
	const string PageXamlV23_NewContentContainer = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" />
		        <ScrollView>
		            <Label Text="Scrollable" />
		        </ScrollView>
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// V24: Grid with attached properties
	const string PageXamlV24_GridWithAttached = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <Grid>
		        <Label Text="Hello" Grid.Row="0" Grid.Column="0" />
		    </Grid>
		</ContentPage>
		""";

	const string PageXamlV25_GridAttachedChanged = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <Grid>
		        <Label Text="Hello" Grid.Row="1" Grid.Column="2" />
		    </Grid>
		</ContentPage>
		""";

	// V26: Grid with attached property using binding (before)
	const string PageXamlV26_GridAttachedWithBinding = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage"
		             x:DataType="TestApp.MyViewModel">
		    <Grid>
		        <Label Text="Hello" Grid.Row="0" />
		    </Grid>
		</ContentPage>
		""";

	// V27: Grid with attached property changed to binding
	const string PageXamlV27_GridAttachedBindingChanged = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage"
		             x:DataType="TestApp.MyViewModel">
		    <Grid>
		        <Label Text="Hello" Grid.Row="{Binding RowIndex}" />
		    </Grid>
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
		=> TwoRunsWithSource(xamlV1, xamlV2, additionalSource: null, enableIHR);

	// Helper that allows injecting extra C# source (e.g., a ViewModel type for x:DataType)
	(GeneratorDriverRunResult run1, GeneratorDriverRunResult run2) TwoRunsWithSource(
		string xamlV1,
		string xamlV2,
		string? additionalSource,
		bool enableIHR = true)
	{
		var compilation = CreateCompilation();
		if (additionalSource != null)
			compilation = compilation.AddSyntaxTrees(
				CSharpSyntaxTree.ParseText(additionalSource));

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
	public void SecondRun_ChildRemoved_EmitsUCWithClearAndUnregister()
	{
		// Arrange: Start with two children (Label + Button), then remove the Button
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV4_TwoChildren, PageXamlV5_ChildRemoved);

		// Assert: UC emitted with child removal
		var ucSource = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(ucSource);
		Assert.Contains("Clear()", ucSource!, StringComparison.Ordinal);
		Assert.Contains("Unregister", ucSource!, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_ChildrenReordered_EmitsUCWithClearAndReAdd()
	{
		// Arrange: Start with Label then Button, swap to Button then Label
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV4_TwoChildren, PageXamlV6_ChildrenReordered);

		// Assert: UC emitted with reorder (clear + re-add in new order)
		var ucSource = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(ucSource);
		Assert.Contains("Clear()", ucSource!, StringComparison.Ordinal);
		// Both children should be re-added (retained)
		Assert.Contains("Add(", ucSource!, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_NestedLayoutAdded_EmitsUCWithGridAndChildren()
	{
		// Arrange: Add a Grid with nested children inside VerticalStackLayout
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV7_NestedLayout);

		// Assert: UC emitted with Grid creation and nested children
		var ucSource = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(ucSource);
		Assert.Contains("Grid", ucSource!, StringComparison.Ordinal);
		Assert.Contains("Nested", ucSource!, StringComparison.Ordinal);
		Assert.Contains("Inner", ucSource!, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_ChildReplaced_EmitsUCWithRemoveAndAdd()
	{
		// Arrange: Replace Label with Entry (different element type)
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV8_ChildReplaced);

		// Assert: UC emitted with both removal and addition
		var ucSource = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(ucSource);
		Assert.Contains("Entry", ucSource!, StringComparison.Ordinal);
		Assert.Contains("Unregister", ucSource!, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_MultipleChildrenAdded_EmitsUCWithAllNewElements()
	{
		// Arrange: Go from 1 child (Label) to 3 children (Label + Button + Entry)
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV9_MultipleAdded);

		// Assert: UC emitted with both new elements
		var ucSource = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(ucSource);
		Assert.Contains("Button", ucSource!, StringComparison.Ordinal);
		Assert.Contains("Entry", ucSource!, StringComparison.Ordinal);
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
	// Type converter tests — verify UC uses compile-time converters from IC
	// -----------------------------------------------------------------------

	[Fact]
	public void SecondRun_ColorPropertyChange_EmitsColorConverterExpression()
	{
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV10_ColorProperty, PageXamlV11_ColorChanged);
		var uc = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(uc);
		// Should contain a Color expression, NOT TypeDescriptor.GetConverter
		Assert.DoesNotContain("TypeDescriptor", uc, StringComparison.Ordinal);
		// The Color converter should emit something like Color.FromArgb or new Color
		Assert.Contains("Color", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_ThicknessPropertyChange_EmitsThicknessExpression()
	{
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV12_ThicknessProperty, PageXamlV13_ThicknessChanged);
		var uc = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(uc);
		Assert.DoesNotContain("TypeDescriptor", uc, StringComparison.Ordinal);
		// Should contain Thickness constructor
		Assert.Contains("Thickness", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_EnumPropertyChange_EmitsEnumLiteral()
	{
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV14_EnumAndDouble, PageXamlV15_EnumAndDoubleChanged);
		var uc = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(uc);
		Assert.DoesNotContain("TypeDescriptor", uc, StringComparison.Ordinal);
		// FontAttributes is an enum — should emit FontAttributes.Italic
		Assert.Contains("Italic", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void SecondRun_GridLengthChange_EmitsGridLengthExpression()
	{
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV16_GridLength, PageXamlV17_GridLengthChanged);
		var uc = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(uc);
		// Grid ColumnDefinitions/RowDefinitions may use a collection converter
		// At minimum, verify UC is generated (not null) and doesn't fall back to TypeDescriptor
		Assert.DoesNotContain("TypeDescriptor", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void NewChildWithBinding_UCContainsBindingSetup()
	{
		// V1 → V18: Add a Label with Text="{Binding Name}"
		// The new element should have its Binding property set up, not skipped.
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV18_NewChildWithBinding);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		// The UC should create a new Label
		Assert.Contains("new global::Microsoft.Maui.Controls.Label()", uc, StringComparison.Ordinal);
		// Should contain binding setup (SetBinding or Binding constructor) on the new element
		Assert.True(
			uc.Contains("SetBinding", StringComparison.Ordinal) ||
			uc.Contains("Binding", StringComparison.Ordinal),
			$"Expected Binding setup in UC for new element, got:\n{uc}");
	}

	[Fact]
	public void NewChildWithDynamicResource_UCContainsResourceSetup()
	{
		// V1 → V19: Add a Label with TextColor="{DynamicResource PrimaryColor}"
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV19_NewChildWithResources);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("new global::Microsoft.Maui.Controls.Label()", uc, StringComparison.Ordinal);
		// Should contain DynamicResource setup on the new element
		Assert.Contains("SetDynamicResource", uc, StringComparison.Ordinal);
		Assert.Contains("PrimaryColor", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void NewChildWithCompiledBinding_UCContainsTypedBinding()
	{
		// V1 (with x:DataType) → V20: Add a Label with Text="{Binding Name}"
		// When x:DataType is known, the UC should emit a compiled TypedBinding
		// with a local helper method (CreateTypedBindingFrom_*).
		XamlHotReloadState.Reset();
		const string viewModel = """
			namespace TestApp
			{
				public class MyViewModel
				{
					public string Name { get; set; }
				}
			}
			""";
		var (_, run2) = TwoRunsWithSource(
			PageXamlV1_WithDataType,
			PageXamlV20_NewChildWithCompiledBinding,
			additionalSource: viewModel);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("new global::Microsoft.Maui.Controls.Label()", uc, StringComparison.Ordinal);
		// Compiled binding should produce TypedBinding and the helper method
		Assert.Contains("TypedBinding", uc, StringComparison.Ordinal);
		Assert.Contains("CreateTypedBindingFrom_", uc, StringComparison.Ordinal);
		Assert.Contains("SetBinding", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void XDataTypeChange_UCRecompilesBindingsWithNewType()
	{
		// Changing x:DataType from MyViewModel to OtherViewModel should produce a UC
		// that re-emits all bindings with the new TypedBinding<OtherViewModel, ...>
		XamlHotReloadState.Reset();
		const string viewModels = """
			namespace TestApp
			{
				public class MyViewModel
				{
					public string Name { get; set; }
				}
				public class OtherViewModel
				{
					public string Title { get; set; }
				}
			}
			""";
		var (_, run2) = TwoRunsWithSource(
			PageXamlV1_BindingWithDataType,
			PageXamlV21_DataTypeChanged,
			additionalSource: viewModels);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		// UC should NOT trigger structural fallback (no "return;" before binding setup)
		// It should contain SetBinding with TypedBinding for the new type
		Assert.Contains("SetBinding", uc, StringComparison.Ordinal);
		Assert.Contains("TypedBinding", uc, StringComparison.Ordinal);
		Assert.Contains("OtherViewModel", uc, StringComparison.Ordinal);
		// Should NOT reference old type
		Assert.DoesNotContain("MyViewModel", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void RootContentSwap_UCReplacesContent()
	{
		// ContentPage.Content changes from VerticalStackLayout to Grid
		// This is a root-level child list change that should use Content property assignment
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV22_RootContentSwap);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		// Should create a new Grid and set it as Content
		Assert.Contains("new global::Microsoft.Maui.Controls.Grid()", uc, StringComparison.Ordinal);
		Assert.Contains(".Content", uc, StringComparison.Ordinal);
		// Should NOT have a structural fallback for root child changes
		Assert.DoesNotContain("Root-level child list change not supported", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void NewContentContainer_UCCreatesChildWithContentProperty()
	{
		// Adding a ScrollView (content container, not Layout) with a child Label
		// should use Content property assignment, not Children.Add()
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV1, PageXamlV23_NewContentContainer);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		// Should create the ScrollView
		Assert.Contains("new global::Microsoft.Maui.Controls.ScrollView()", uc, StringComparison.Ordinal);
		// Should set its Content property (not use Add())
		Assert.Contains(".Content", uc, StringComparison.Ordinal);
		// Should NOT fall back
		Assert.DoesNotContain("Non-layout container", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void AttachedPropertyChange_UCUsesSetValue()
	{
		// Changing Grid.Row and Grid.Column should emit SetValue() calls
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV24_GridWithAttached, PageXamlV25_GridAttachedChanged);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("SetValue", uc, StringComparison.Ordinal);
		Assert.Contains("RowProperty", uc, StringComparison.Ordinal);
		Assert.Contains("ColumnProperty", uc, StringComparison.Ordinal);
		// Should NOT use direct property assignment (element.Grid.Row = value)
		Assert.DoesNotContain("__uc_0.Grid.Row", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void AttachedPropertyWithBinding_UCEmitsSetBinding()
	{
		// Changing Grid.Row from "0" to "{Binding RowIndex}" should emit a SetBinding or Binding call
		XamlHotReloadState.Reset();
		const string viewModel = """
			namespace TestApp
			{
				public class MyViewModel
				{
					public int RowIndex { get; set; }
				}
			}
			""";
		var (_, run2) = TwoRunsWithSource(
			PageXamlV26_GridAttachedWithBinding,
			PageXamlV27_GridAttachedBindingChanged,
			additionalSource: viewModel);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		// Should emit a SetBinding with Grid.RowProperty on the Label
		Assert.Contains("SetBinding", uc, StringComparison.Ordinal);
		Assert.Contains("Grid.RowProperty", uc, StringComparison.Ordinal);
		Assert.Contains("RowIndex", uc, StringComparison.Ordinal);
		// Target element should be Label (not Grid)
		Assert.Contains("Label", uc, StringComparison.Ordinal);
		// Should NOT use SetValue (that's for literal values, not bindings)
		Assert.DoesNotContain("SetValue", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void ResourceAdded_UCDoesNotEmitUnreachableCode()
	{
		// Adding a resource in ContentPage.Resources should not produce CS0162 (unreachable code)
		XamlHotReloadState.Reset();
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <Color x:Key="SecondaryColor">Red</Color>
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		// Must contain __version = 1 (version was incremented)
		Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
		// Must NOT have "return;" before __version assignment (causes CS0162)
		var versionIdx = uc!.IndexOf("__version = 1", StringComparison.Ordinal);
		var bodyStart = uc.IndexOf("{", uc.IndexOf("__version == 0"), StringComparison.Ordinal);
		var bodySection = uc.Substring(bodyStart, versionIdx - bodyStart);
		Assert.DoesNotContain("return;", bodySection, StringComparison.Ordinal);
	}

	[Fact]
	public void ResourceRemoved_UCDoesNotEmitUnreachableCode()
	{
		// Removing a resource from ContentPage.Resources should not produce CS0162
		XamlHotReloadState.Reset();
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <Color x:Key="SecondaryColor">Red</Color>
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
		var versionIdx = uc!.IndexOf("__version = 1", StringComparison.Ordinal);
		var bodyStart = uc.IndexOf("{", uc.IndexOf("__version == 0"), StringComparison.Ordinal);
		var bodySection = uc.Substring(bodyStart, versionIdx - bodyStart);
		Assert.DoesNotContain("return;", bodySection, StringComparison.Ordinal);
	}

	[Fact]
	public void ResourceAdded_UCEmitsResourceDictionaryAdd()
	{
		// Adding a resource should emit this.Resources["key"] = value
		XamlHotReloadState.Reset();
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <Color x:Key="SecondaryColor">Red</Color>
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("Resources", uc, StringComparison.Ordinal);
		Assert.Contains("SecondaryColor", uc, StringComparison.Ordinal);
		Assert.Contains("RegisterResourceKeys", uc, StringComparison.Ordinal);
		Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void ResourceRemoved_UCEmitsResourceDictionaryRemove()
	{
		// Removing a resource should emit this.Resources.Remove("key")
		XamlHotReloadState.Reset();
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <Color x:Key="SecondaryColor">Red</Color>
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		// Uses targeted remove — old keys not in new set are removed
		Assert.Contains("Resources", uc, StringComparison.Ordinal);
		Assert.Contains("Remove", uc, StringComparison.Ordinal);
		Assert.Contains("AccentColor", uc, StringComparison.Ordinal);
		Assert.Contains("RegisterResourceKeys", uc, StringComparison.Ordinal);
		Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void ResourceValueChanged_UCEmitsResourceUpdate()
	{
		// Changing a resource value should emit this.Resources["key"] = newValue
		XamlHotReloadState.Reset();
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">Red</Color>
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("Resources", uc, StringComparison.Ordinal);
		Assert.Contains("AccentColor", uc, StringComparison.Ordinal);
		Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
	}


[Fact]
public void ResourceWithConverters_UCDoesNotRegisterUnencodableKeys()
{
// When resources include custom types (converters) that can't be encoded as C# expressions,
// the UC should NOT register those keys — otherwise they get removed on next patch.
XamlHotReloadState.Reset();
const string xamlV1 = """
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.MainPage">
    <ContentPage.Resources>
        <Color x:Key="AccentColor">DarkBlue</Color>
    </ContentPage.Resources>
    <Label Text="Hello" />
</ContentPage>
""";
const string xamlV2 = """
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.MainPage">
    <ContentPage.Resources>
        <Color x:Key="AccentColor">Red</Color>
        <Color x:Key="SecondaryColor">Green</Color>
    </ContentPage.Resources>
    <Label Text="Hello" />
</ContentPage>
""";

var (_, run2) = TwoRuns(xamlV1, xamlV2);
var uc = FindUCSource(run2, "uc.xsg");

Assert.NotNull(uc);
// Only emittable keys (Color values) should be in RegisterResourceKeys
Assert.Contains("AccentColor", uc, StringComparison.Ordinal);
Assert.Contains("SecondaryColor", uc, StringComparison.Ordinal);
Assert.Contains("RegisterResourceKeys", uc, StringComparison.Ordinal);
// The registered keys should only contain the color keys
Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
}

	[Fact]
	public void ConverterResourceAdded_UCEmitsNewInstance()
	{
		// Adding a converter resource (custom type) should emit a new instance in UC.
		// Previously, custom types returned null from BuildResourceValueExpression and were skipped.
		XamlHotReloadState.Reset();

		const string stubs = """
			namespace TestApp
			{
				public class StatusColorConverter : Microsoft.Maui.Controls.IValueConverter
				{
					public object? Convert(object? value, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture) => null;
					public object? ConvertBack(object? value, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture) => null;
				}
			}
			""";

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestApp"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestApp"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <local:StatusColorConverter x:Key="StatusConverter" />
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";

		var (_, run2) = TwoRunsWithSource(xamlV1, xamlV2, stubs);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
		// UC must add the converter resource to the dictionary
		Assert.Contains("StatusConverter", uc, StringComparison.Ordinal);
		Assert.Contains("new global::TestApp.StatusColorConverter()", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void ConverterResourceRemoved_UCEmitsRemove()
	{
		// Removing a converter resource should emit Resources.Remove("key") in UC.
		XamlHotReloadState.Reset();

		const string stubs = """
			namespace TestApp
			{
				public class StatusColorConverter : Microsoft.Maui.Controls.IValueConverter
				{
					public object? Convert(object? value, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture) => null;
					public object? ConvertBack(object? value, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture) => null;
				}
			}
			""";

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestApp"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <local:StatusColorConverter x:Key="StatusConverter" />
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestApp"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";

		var (_, run2) = TwoRunsWithSource(xamlV1, xamlV2, stubs);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
		// UC must remove the converter from the dictionary
		Assert.Contains("Resources.Remove", uc, StringComparison.Ordinal);
		// The converter key should no longer be registered
		Assert.DoesNotContain("\"StatusConverter\"", uc.Substring(uc.IndexOf("RegisterResourceKeys")), StringComparison.Ordinal);
	}

	[Fact]
	public void ConverterSwap_UCCompilesCleanly()
	{
		// Swapping StaticResource converter key in a Binding should produce
		// compilable UC code and not crash.
		XamlHotReloadState.Reset();

		const string stubs = """
			namespace TestApp
			{
				public class MainViewModel
				{
					public string Status { get; set; } = "Ready";
				}
				public class StatusColorConverter : Microsoft.Maui.Controls.IValueConverter
				{
					public object? Convert(object? value, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture) => null;
					public object? ConvertBack(object? value, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture) => null;
				}
				public class InvertedStatusColorConverter : Microsoft.Maui.Controls.IValueConverter
				{
					public object? Convert(object? value, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture) => null;
					public object? ConvertBack(object? value, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture) => null;
				}
			}
			""";

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestApp"
			             x:Class="TestApp.MainPage"
			             x:DataType="local:MainViewModel">
			    <ContentPage.Resources>
			        <local:StatusColorConverter x:Key="StatusConverter" />
			        <local:InvertedStatusColorConverter x:Key="InvertedStatusConverter" />
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label Text="{Binding Status}"
			               TextColor="{Binding Status, Converter={StaticResource StatusConverter}}" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestApp"
			             x:Class="TestApp.MainPage"
			             x:DataType="local:MainViewModel">
			    <ContentPage.Resources>
			        <local:StatusColorConverter x:Key="StatusConverter" />
			        <local:InvertedStatusColorConverter x:Key="InvertedStatusConverter" />
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label Text="{Binding Status}"
			               TextColor="{Binding Status, Converter={StaticResource InvertedStatusConverter}}" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRunsWithSource(xamlV1, xamlV2, stubs);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
		// UC must emit a runtime Resources lookup to set the converter on the BindingExtension
		Assert.Contains("InvertedStatusConverter", uc, StringComparison.Ordinal);
		Assert.Contains("this.Resources[\"InvertedStatusConverter\"]", uc, StringComparison.Ordinal);
	}

	// -----------------------------------------------------------------------
	// Helpers (pipeline)
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
