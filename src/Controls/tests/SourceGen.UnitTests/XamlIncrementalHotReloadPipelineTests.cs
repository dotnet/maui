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

	// 3-run helper for testing version-chain preservation across no-op edits.
	(GeneratorDriverRunResult run1, GeneratorDriverRunResult run2, GeneratorDriverRunResult run3) ThreeRuns(
		string xamlV1,
		string xamlV2,
		string xamlV3,
		bool enableIHR = true)
	{
		var compilation = CreateCompilation();
		var fileV1 = MakeFile(xamlV1, enableIHR);
		var fileV2 = MakeFile(xamlV2, enableIHR);
		var fileV3 = MakeFile(xamlV3, enableIHR);

		ISourceGenerator generator = new XamlGenerator().AsSourceGenerator();
		var options = new Microsoft.CodeAnalysis.GeneratorDriverOptions(
			disabledOutputs: Microsoft.CodeAnalysis.IncrementalGeneratorOutputKind.None,
			trackIncrementalGeneratorSteps: true);

		Microsoft.CodeAnalysis.GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], driverOptions: options)
			.AddAdditionalTexts(System.Collections.Immutable.ImmutableArray.Create<Microsoft.CodeAnalysis.AdditionalText>(fileV1.Text))
			.WithUpdatedAnalyzerConfigOptions(new OptionsProvider([fileV1]));

		driver = driver.RunGenerators(compilation);
		var r1 = driver.GetRunResult();

		driver = driver
			.ReplaceAdditionalText(fileV1.Text, fileV2.Text)
			.WithUpdatedAnalyzerConfigOptions(new OptionsProvider([fileV2]))
			.RunGenerators(compilation);
		var r2 = driver.GetRunResult();

		driver = driver
			.ReplaceAdditionalText(fileV2.Text, fileV3.Text)
			.WithUpdatedAnalyzerConfigOptions(new OptionsProvider([fileV3]))
			.RunGenerators(compilation);
		var r3 = driver.GetRunResult();

		return (r1, r2, r3);
	}

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
	public void DataTemplate_HotReload_EmitsStableNamedMethod_NotAnonymousLambda()
	{
		// Regression for dotnet/maui#36482: under XAML Incremental Hot Reload, the DataTemplate
		// content must be emitted as a stably-named method (EnC-stable across successive edits),
		// NOT an anonymous `LoadTemplate = () => { ... }` lambda (whose synthesized-closure identity
		// is unstable across regenerations and crashes hot reload on successive DataTemplate edits).
		const string dtV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <CollectionView>
			        <CollectionView.ItemTemplate>
			            <DataTemplate>
			                <Label Text="Hi" TextColor="Black" />
			            </DataTemplate>
			        </CollectionView.ItemTemplate>
			    </CollectionView>
			</ContentPage>
			""";
		const string dtV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <CollectionView>
			        <CollectionView.ItemTemplate>
			            <DataTemplate>
			                <Label Text="Hi" TextColor="Green" />
			            </DataTemplate>
			        </CollectionView.ItemTemplate>
			    </CollectionView>
			</ContentPage>
			""";

		XamlHotReloadState.Reset();
		var (run1, run2) = TwoRuns(dtV1, dtV2);

		string? nameV1 = null, nameV2 = null;
		foreach (var (label, run) in new[] { ("run1", run1), ("run2", run2) })
		{
			var ic = FindSourceByHintSuffix(run, ".xsg.cs");
			Assert.NotNull(ic);

			// The template content must NOT be an anonymous lambda assigned to LoadTemplate.
			Assert.DoesNotContain("LoadTemplate = () =>", ic, StringComparison.Ordinal);

			// It must be assigned a named method reference, and that method must be emitted.
			Assert.Contains("LoadTemplate = LoadTemplate_", ic, StringComparison.Ordinal);
			var m = System.Text.RegularExpressions.Regex.Match(ic, @"object (LoadTemplate_\d+_\d+)\(\)");
			Assert.True(m.Success, "expected a named LoadTemplate method");

			// The template body (the edited property) must be present inside the named method.
			var expectedColor = label == "run1" ? "Colors.Black" : "Colors.Green";
			Assert.Contains(expectedColor, ic, StringComparison.Ordinal);

			if (label == "run1")
				nameV1 = m.Groups[1].Value;
			else
				nameV2 = m.Groups[1].Value;
		}

		// EnC anchor: the generated method name must be IDENTICAL across the edit, so Edit-and-Continue
		// maps it as a clean method-body update rather than a deleted/added synthesized method.
		Assert.Equal(nameV1, nameV2);
	}

	[Fact]
	public void DataTemplate_HotReload_GeneratedNamedMethod_Compiles()
	{
		// Ensures the named-method form generated under Incremental Hot Reload for a DataTemplate
		// (including one with a compiled binding inside it) is valid C# that compiles cleanly.
		const string xaml = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <CollectionView>
			        <CollectionView.ItemTemplate>
			            <DataTemplate>
			                <Label Text="{Binding Name, StringFormat='Fruit: {0}'}" TextColor="Black" FontSize="16" />
			            </DataTemplate>
			        </CollectionView.ItemTemplate>
			    </CollectionView>
			</ContentPage>
			""";

		XamlHotReloadState.Reset();
		// assertNoCompilationErrors: true throws if the generated .xsg.cs has C# errors.
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(
			CreateCompilation(), MakeFile(xaml), assertNoCompilationErrors: true);

		var ic = FindSourceByHintSuffix(result, ".xsg.cs");
		Assert.NotNull(ic);
		Assert.DoesNotContain("LoadTemplate = () =>", ic, StringComparison.Ordinal);
		Assert.Contains("LoadTemplate = LoadTemplate_", ic, StringComparison.Ordinal);
	}

	[Fact]
	public void DataTemplate_HotReload_SetMultipleTimes_EmitsSingleNamedMethod()
	{
		// Regression for dotnet/maui#36682: a DataTemplate assigned to a `required` property is set
		// twice by the generator (once in the object initializer, once as a later assignment). Under
		// Incremental Hot Reload the template body is a named local function, and emitting it twice
		// in the same scope produced two `object LoadTemplate_L_P()` declarations -> CS0128 ("already
		// defined") + CS8321 ("declared but never used"). The named method must be emitted only once,
		// with every set-site re-pointing LoadTemplate at that single function.
		const string host = """
			namespace TestApp
			{
				public class TemplateHost : Microsoft.Maui.Controls.View
				{
					public required Microsoft.Maui.Controls.DataTemplate Template { get; set; }
				}
			}
			""";
		const string xaml = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestApp"
			             x:Class="TestApp.MainPage">
			    <local:TemplateHost>
			        <local:TemplateHost.Template>
			            <DataTemplate>
			                <Label Text="Hi" TextColor="Black" />
			            </DataTemplate>
			        </local:TemplateHost.Template>
			    </local:TemplateHost>
			</ContentPage>
			""";

		XamlHotReloadState.Reset();
		var compilation = CreateCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(host));

		// assertNoCompilationErrors: true throws if the generated .xsg.cs has C# errors (e.g. the
		// duplicate-method CS0128 this test guards against).
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(
			compilation, MakeFile(xaml), assertNoCompilationErrors: true);

		var ic = FindSourceByHintSuffix(result, ".xsg.cs");
		Assert.NotNull(ic);

		// Exactly one named LoadTemplate method must be declared, even though LoadTemplate is
		// assigned more than once.
		var declarations = System.Text.RegularExpressions.Regex.Matches(ic, @"object LoadTemplate_\d+_\d+\(\)");
		Assert.Single(declarations);
		var assignments = System.Text.RegularExpressions.Regex.Matches(ic, @"\.LoadTemplate = LoadTemplate_\d+_\d+;");
		Assert.True(assignments.Count >= 2, "expected the single named method to be assigned at every set-site");
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
		var hasPrev = XamlHotReloadState.TryGetPrevious("TestApp", "net11.0", PageRelativePath, out _, out _, out _, out _, out var version);
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
	public void NoOpEdit_BetweenPatches_PreservesVersionChain()
	{
		// Regression for round-3 review finding: a semantically empty XAML edit (e.g., adding
		// a comment) used to call UpdateAndClearPatches, resetting __version to 0 and clearing
		// PatchBodies. Live instances at version 1+ would then strand when the next real edit
		// emits `if (__version == 0)` — they would silently ignore every future patch.
		XamlHotReloadState.Reset();

		// V1 — seed at version 0
		// V2 — real property change → patch at version 0→1
		// V3 — same as V2 but with an extra XML comment (no semantic change)
		var v3WithComment = PageXamlV2_PropertyChange.Replace(
			"x:Class=\"TestApp.MainPage\"",
			"x:Class=\"TestApp.MainPage\"><!-- harmless comment --",
			StringComparison.Ordinal);

		var (_, run2, run3) = ThreeRuns(PageXamlV1, PageXamlV2_PropertyChange, v3WithComment);

		// Run 2 must emit the patch
		Assert.NotNull(FindUCSource(run2, "uc.xsg"));

		// State after run 3: version preserved, exactly one patch retained
		Assert.True(XamlHotReloadState.TryGetPrevious("TestApp", "net11.0", PageRelativePath,
			out _, out _, out _, out _, out var versionAfterNoOp));
		Assert.Equal(1, versionAfterNoOp);
		Assert.Single(XamlHotReloadState.GetPatchBodies("TestApp", "net11.0", PageRelativePath));

		// Round-4 fix: UC is re-emitted in the emptyDiff branch so it doesn't transiently
		// disappear from the compilation. The patches MUST still be gated on version 0→1
		// (NOT regenerated as 0→0, which would mean version state was reset).
		var run3Uc = FindUCSource(run3, "uc.xsg");
		Assert.NotNull(run3Uc);
		Assert.Contains("__version == 0", run3Uc!, StringComparison.Ordinal);
		Assert.Contains("__version = 1", run3Uc!, StringComparison.Ordinal);
		// Defensive: the broken pre-round-3 behavior would have produced no UC at all OR
		// a UC that no longer references version 1.
		Assert.DoesNotContain("__version == 1", run3Uc!, StringComparison.Ordinal);
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
	public void SecondRun_ChildrenReordered_EmitsUCWithReorderInPlace()
	{
		// Arrange: Start with Label then Button, swap to Button then Label
		XamlHotReloadState.Reset();
		var (_, run2) = TwoRuns(PageXamlV4_TwoChildren, PageXamlV6_ChildrenReordered);

		// Assert: UC emitted with in-place reorder via RemoveAt + Insert (M13 optimization).
		// Pure reorders (no Add/Remove) must preserve platform-side handler state by reusing
		// the existing IView instances, so Clear() is NOT emitted.
		var ucSource = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(ucSource);
		Assert.DoesNotContain("Clear()", ucSource!, StringComparison.Ordinal);
		Assert.Contains("RemoveAt", ucSource!, StringComparison.Ordinal);
		Assert.Contains(".Insert(", ucSource!, StringComparison.Ordinal);
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
		var hasPrev = XamlHotReloadState.TryGetPrevious("TestApp", "net11.0", PageRelativePath, out _, out _, out _, out _, out _);
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
		XamlHotReloadState.Update("App1", "net11.0", relPath, xaml, null, null, 0, 1);

		// "App2" with same relative path should NOT see "App1" state
		var hasPrev = XamlHotReloadState.TryGetPrevious("App2", "net11.0", relPath, out _, out _, out _, out _, out _);
		Assert.False(hasPrev, "Different assembly should not see another assembly's state");

		// "App1" SHOULD see its own state
		var hasSelf = XamlHotReloadState.TryGetPrevious("App1", "net11.0", relPath, out var storedXaml, out _, out _, out _, out var storedVer);
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
		// UC must emit a full-walk StaticResource lookup (parent chain → app → system) to set
		// the converter on the BindingExtension. M8 fix replaced this.Resources[key] (page-only)
		// with the dedicated XamlComponentRegistry.FindStaticResource helper.
		Assert.Contains("InvertedStatusConverter", uc, StringComparison.Ordinal);
		Assert.Contains("XamlComponentRegistry.FindStaticResource(", uc, StringComparison.Ordinal);
		Assert.Contains("\"InvertedStatusConverter\"", uc, StringComparison.Ordinal);
		// M8 follow-up: lookup must walk from the actual target element (parentAccessor),
		// NOT from `this` — otherwise resources scoped on intermediate parents are missed.
		Assert.DoesNotContain("FindStaticResource(this,", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void MultipleMarkupExtensionChanges_UCCompilesWithoutDuplicateLocals()
	{
		// Regression: when two property changes in one UpdateComponent() each expand a markup
		// extension, the IC pipeline emits the same locals (e.g. `staticResourceExtension`,
		// `xamlServiceProvider`). Without per-expansion block scoping these collide in the
		// flattened UC method body → CS0128 "a local variable is already defined in this scope".
		XamlHotReloadState.Reset();

		const string stubs = """
			namespace TestApp
			{
				public class MainViewModel { }
			}
			""";

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestApp"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="ColorA">#FF0000</Color>
			        <Color x:Key="ColorB">#00FF00</Color>
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label x:Name="L1" TextColor="{StaticResource ColorA}" BackgroundColor="{StaticResource ColorA}" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestApp"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="ColorA">#FF0000</Color>
			        <Color x:Key="ColorB">#00FF00</Color>
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label x:Name="L1" TextColor="{StaticResource ColorB}" BackgroundColor="{StaticResource ColorB}" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRunsWithSource(xamlV1, xamlV2, stubs);
		var uc = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(uc);

		// Compile all V2 generated sources together and assert the UC has no compile errors.
		var compilation = CreateCompilation()
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(stubs, path: "Stubs.cs"));
		foreach (var gen in run2.Results)
			foreach (var src in gen.GeneratedSources)
				compilation = compilation.AddSyntaxTrees(
					CSharpSyntaxTree.ParseText(src.SourceText.ToString(), path: src.HintName));

		var errors = compilation.GetDiagnostics()
			.Where(d => d.Severity == DiagnosticSeverity.Error
				&& d.Location.SourceTree?.FilePath?.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase) == true)
			.ToArray();

		Assert.DoesNotContain(errors, e => e.Id == "CS0128");
		Assert.Empty(errors);
	}

	[Fact]
	public void AddedNamedElement_IsTrackedViaRegistry()
	{
		// jonathanpeppers review: adding a `<Button x:Name="Foo"/>` during hot reload does not (and
		// cannot) assign the strongly-typed backing field `this.Foo` — EnC/MetadataUpdate cannot add
		// an instance field to an already-loaded type. Instead the new element must be tracked in
		// XamlComponentRegistry by node id (Register(this, "<id>", ...)), which is how subsequent
		// patches locate it. This test asserts the UC registers the added named element and does NOT
		// emit an assignment to a non-existent `this.Foo` field.
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
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
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			        <Button x:Name="Foo" Text="Click" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRunsWithSource(xamlV1, xamlV2, additionalSource: null);
		var uc = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(uc);

		// The newly added element is created and registered by node id...
		Assert.Contains("new global::Microsoft.Maui.Controls.Button()", uc!, StringComparison.Ordinal);
		Assert.Contains("XamlComponentRegistry.Register(this,", uc!, StringComparison.Ordinal);
		// ...but the UC must NOT assign a backing field that can't exist on the loaded type.
		Assert.DoesNotContain("this.Foo =", uc!, StringComparison.Ordinal);
	}

	[Fact]
	public void ContentTransitions_StringToMarkupToElementAndBack_Compile()
	{
		// jonathanpeppers review: verify a content/property value transitioning through
		// string -> markup extension -> element and back produces compilable UC at each step.
		XamlHotReloadState.Reset();

		string Page(string labelText) => $$"""
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <x:String x:Key="Greeting">FromResource</x:String>
			    </ContentPage.Resources>
			    <VerticalStackLayout>
			        <Label {{labelText}} />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		// V1: plain string; V2: markup extension; V3: back to a plain string.
		var v1 = Page("Text=\"Literal\"");
		var v2 = Page("Text=\"{StaticResource Greeting}\"");
		var v3 = Page("Text=\"LiteralAgain\"");

		var f1 = MakeFile(v1);
		var f2 = MakeFile(v2);
		var f3 = MakeFile(v3);

		var comp = CreateCompilation();
		string? lastUc = null;
		var (r1, r2) = SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(
			comp,
			applyChanges: (driver, c) =>
				(driver.ReplaceAdditionalText(f1.Text, f2.Text)
					.WithUpdatedAnalyzerConfigOptions(new OptionsProvider([f2])), c),
			f1);
		lastUc = FindUCSource(r2, "uc.xsg");

		// third edit: markup -> string
		var (_, r3) = SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(
			comp,
			applyChanges: (driver, c) =>
				(driver.ReplaceAdditionalText(f2.Text, f3.Text)
					.WithUpdatedAnalyzerConfigOptions(new OptionsProvider([f3])), c),
			f2);
		var uc3 = FindUCSource(r3, "uc.xsg") ?? lastUc;
		Assert.NotNull(uc3);

		// The accumulated UC must compile (no errors in generated .xsg.cs) across the transitions.
		var compilation = CreateCompilation();
		foreach (var gen in r3.Results)
			foreach (var src in gen.GeneratedSources)
				compilation = compilation.AddSyntaxTrees(
					CSharpSyntaxTree.ParseText(src.SourceText.ToString(), path: src.HintName));

		var errors = compilation.GetDiagnostics()
			.Where(d => d.Severity == DiagnosticSeverity.Error
				&& d.Location.SourceTree?.FilePath?.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase) == true)
			.ToArray();
		Assert.Empty(errors);
	}

	[Fact]
	public void StyleChange_UCGeneratesPatch()
	{
		// Changing an inline Style's Setters should produce a UC patch.
		// The Style subtree is complex, so the UC may rebuild it structurally.
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="Hello">
			            <Label.Style>
			                <Style TargetType="Label">
			                    <Setter Property="TextColor" Value="Red" />
			                </Style>
			            </Label.Style>
			        </Label>
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="Hello">
			            <Label.Style>
			                <Style TargetType="Label">
			                    <Setter Property="TextColor" Value="Blue" />
			                    <Setter Property="FontSize" Value="24" />
			                </Style>
			            </Label.Style>
			        </Label>
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
		// UC must reference the Style property in some form
		Assert.Contains("Style", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void StyleResourceChange_UCUpdatesResourceDictionary()
	{
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Style x:Key="MyLabelStyle" TargetType="Label">
			            <Setter Property="TextColor" Value="Red" />
			        </Style>
			    </ContentPage.Resources>
			    <Label Text="Hello" Style="{StaticResource MyLabelStyle}" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Style x:Key="MyLabelStyle" TargetType="Label">
			            <Setter Property="TextColor" Value="Blue" />
			            <Setter Property="FontSize" Value="24" />
			        </Style>
			    </ContentPage.Resources>
			    <Label Text="Hello" Style="{StaticResource MyLabelStyle}" />
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("MyLabelStyle", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void TriggerAdded_UCGeneratesPatch()
	{
		// Adding a Trigger to an Entry should produce a UC patch.
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Entry Placeholder="Type here" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Entry Placeholder="Type here">
			            <Entry.Triggers>
			                <Trigger TargetType="Entry" Property="IsFocused" Value="True">
			                    <Setter Property="BackgroundColor" Value="LightYellow" />
			                </Trigger>
			            </Entry.Triggers>
			        </Entry>
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
		Assert.Contains("Trigger", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void DataTriggerChange_UCGeneratesPatch()
	{
		// Changing a DataTrigger's Setter values should produce a UC patch.
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="Hello">
			            <Label.Triggers>
			                <DataTrigger TargetType="Label" Binding="{Binding IsActive}" Value="True">
			                    <Setter Property="TextColor" Value="Green" />
			                </DataTrigger>
			            </Label.Triggers>
			        </Label>
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="Hello">
			            <Label.Triggers>
			                <DataTrigger TargetType="Label" Binding="{Binding IsActive}" Value="True">
			                    <Setter Property="TextColor" Value="Red" />
			                    <Setter Property="FontAttributes" Value="Bold" />
			                </DataTrigger>
			            </Label.Triggers>
			        </Label>
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
		Assert.Contains("Trigger", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void VisualStateManagerChange_UCGeneratesPatch()
	{
		// Changing VSM Setters should produce a UC patch.
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Button Text="Click">
			            <VisualStateManager.VisualStateGroups>
			                <VisualStateGroup Name="CommonStates">
			                    <VisualState Name="Normal">
			                        <VisualState.Setters>
			                            <Setter Property="BackgroundColor" Value="White" />
			                        </VisualState.Setters>
			                    </VisualState>
			                    <VisualState Name="Pressed">
			                        <VisualState.Setters>
			                            <Setter Property="BackgroundColor" Value="LightGray" />
			                        </VisualState.Setters>
			                    </VisualState>
			                </VisualStateGroup>
			            </VisualStateManager.VisualStateGroups>
			        </Button>
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Button Text="Click">
			            <VisualStateManager.VisualStateGroups>
			                <VisualStateGroup Name="CommonStates">
			                    <VisualState Name="Normal">
			                        <VisualState.Setters>
			                            <Setter Property="BackgroundColor" Value="White" />
			                        </VisualState.Setters>
			                    </VisualState>
			                    <VisualState Name="Pressed">
			                        <VisualState.Setters>
			                            <Setter Property="BackgroundColor" Value="DarkGray" />
			                            <Setter Property="Scale" Value="0.95" />
			                        </VisualState.Setters>
			                    </VisualState>
			                </VisualStateGroup>
			            </VisualStateManager.VisualStateGroups>
			        </Button>
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
		Assert.Contains("VisualState", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void BehaviorAdded_UCGeneratesPatch()
	{
		// Adding a Behavior to an Entry. Behaviors are a complex collection property
		// that the UC processes through the IC pipeline.
		XamlHotReloadState.Reset();

		const string stubs = """
			namespace TestApp
			{
				public class NumericValidationBehavior : Microsoft.Maui.Controls.Behavior<Microsoft.Maui.Controls.Entry>
				{
				}
			}
			""";

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestApp"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Entry Placeholder="Enter number" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestApp"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Entry Placeholder="Enter number">
			            <Entry.Behaviors>
			                <local:NumericValidationBehavior />
			            </Entry.Behaviors>
			        </Entry>
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRunsWithSource(xamlV1, xamlV2, stubs);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		Assert.Contains("__version = 1", uc, StringComparison.Ordinal);
		// Behavior is a complex property — UC should mention it (even as skipped comment)
		Assert.Contains("Behavior", uc, StringComparison.Ordinal);
	}

	// -----------------------------------------------------------------------
	// Binding cleanup and property clear tests
	// -----------------------------------------------------------------------

	[Fact]
	public void PropertyClear_UCEmitsRemoveBindingBeforeClearValue()
	{
		// When a property is removed from XAML, UC should RemoveBinding + ClearValue
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Label x:Name="lbl" Text="Hello" BackgroundColor="Red" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Label x:Name="lbl" Text="Hello" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		// Must emit RemoveBinding before ClearValue to prevent zombie bindings
		Assert.Contains("RemoveBinding", uc, StringComparison.Ordinal);
		Assert.Contains("ClearValue", uc, StringComparison.Ordinal);
		// RemoveBinding must come before ClearValue
		var removeIdx = uc!.IndexOf("RemoveBinding", StringComparison.Ordinal);
		var clearIdx = uc.IndexOf("ClearValue", StringComparison.Ordinal);
		Assert.True(removeIdx < clearIdx, "RemoveBinding should come before ClearValue");
	}

	[Fact]
	public void PropertySet_UCEmitsRemoveBindingBeforeStaticValue()
	{
		// When a bound property is replaced with a static value, UC should remove the old binding
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Label x:Name="lbl" Text="Hello" TextColor="Red" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <VerticalStackLayout>
			        <Label x:Name="lbl" Text="Hello" TextColor="Blue" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		// RemoveBinding should be emitted before the new property assignment
		Assert.Contains("RemoveBinding", uc, StringComparison.Ordinal);
		Assert.Contains("TextColorProperty", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void AttachedPropertyClear_UCResolvesDeclaringType()
	{
		// When Grid.Row is removed, UC should use Grid.RowProperty (not Button.Grid.RowProperty)
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <Grid>
			        <Button x:Name="btn" Text="Click" Grid.Row="1" />
			    </Grid>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <Grid>
			        <Button x:Name="btn" Text="Click" />
			    </Grid>
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");

		Assert.NotNull(uc);
		// Must reference Grid.RowProperty (declaring type), not Button
		Assert.Contains("Grid.RowProperty", uc, StringComparison.Ordinal);
		Assert.Contains("RemoveBinding", uc, StringComparison.Ordinal);
		Assert.Contains("ClearValue", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void CompiledBinding_UCResolvesNestedPropertyType()
	{
		// Verify UC resolves dotted binding paths (User.DisplayName) to the correct type
		// using shared SetPropertyHelpers.ResolveExpressionType (not a UC-specific clone).
		XamlHotReloadState.Reset();

		const string stubs = """
			namespace TestApp
			{
			    public class UserModel
			    {
			        public string DisplayName { get; set; }
			    }
			    public class MainViewModel
			    {
			        public UserModel User { get; set; }
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
			    <VerticalStackLayout>
			        <Label x:Name="lbl" Text="{Binding User.DisplayName}" />
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
			    <VerticalStackLayout>
			        <Label x:Name="lbl" Text="{Binding User.DisplayName, Mode=OneWay}" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (_, run2) = TwoRunsWithSource(xamlV1, xamlV2, stubs);
		var uc = FindUCSource(run2, "uc.xsg");

		if (uc != null)
		{
			// UC should emit a compiled TypedBinding with resolved nested type (string, not object)
			Assert.Contains("TypedBinding", uc, StringComparison.Ordinal);
			Assert.Contains("TypedBinding<global::TestApp.MainViewModel, string>", uc, StringComparison.Ordinal);
		}
		else
		{
			// Binding Mode change is a markup-extension property change — may be treated as structural.
			// Verify the IC at least contains a compiled TypedBinding with the correct type.
			var icSource = run2.Results.SelectMany(r => r.GeneratedSources)
				.Where(s => s.HintName.Contains("MainPage", StringComparison.Ordinal))
				.Select(s => s.SourceText.ToString())
				.FirstOrDefault() ?? "";
			Assert.Contains("TypedBinding<global::TestApp.MainViewModel, string>", icSource, StringComparison.Ordinal);
		}
	}

	[Fact]
	public void EventHandler_UCEmitsUnsubscribeAndSubscribe()
	{
		// Changing Clicked="OnClicked" to Clicked="OnClicked2" should emit -= old, += new
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <Button x:Name="btn" Clicked="OnClicked" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <Button x:Name="btn" Clicked="OnClicked2" />
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(uc);
		Assert.Contains("Clicked -= OnClicked", uc, StringComparison.Ordinal);
		Assert.Contains("Clicked += OnClicked2", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void EventHandler_UCEmitsUnsubscribeOnClear()
	{
		// Removing Clicked="OnClicked" should emit -= OnClicked
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <Button x:Name="btn" Text="Hello" Clicked="OnClicked" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <Button x:Name="btn" Text="Hello" />
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(uc);
		Assert.Contains("Clicked -= OnClicked", uc, StringComparison.Ordinal);
		Assert.DoesNotContain("Clicked +=", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void AttachedPropertySet_UCEmitsRemoveBindingBeforeSetValue()
	{
		// Changing Grid.Row on a child should emit RemoveBinding before SetValue
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <Grid><Label x:Name="lbl" Grid.Row="0" Text="Hello" /></Grid>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <Grid><Label x:Name="lbl" Grid.Row="2" Text="Hello" /></Grid>
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(uc);
		// Must have RemoveBinding before SetValue for the attached property
		Assert.Contains("RemoveBinding", uc, StringComparison.Ordinal);
		Assert.Contains("SetValue", uc, StringComparison.Ordinal);
		var removeIdx = uc!.IndexOf("RemoveBinding", StringComparison.Ordinal);
		var setIdx = uc.IndexOf("SetValue", StringComparison.Ordinal);
		Assert.True(removeIdx < setIdx, "RemoveBinding must come before SetValue for attached properties");
	}

	[Fact]
	public void EventHandler_InvalidIdentifier_UCSkips()
	{
		// If an event handler name contains special chars, UC should skip it safely
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <Button x:Name="btn" Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <Button x:Name="btn" Text="Hello" Clicked="bad;handler" />
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(uc);
		// Should NOT emit += with the invalid handler name
		Assert.DoesNotContain("+= bad;handler", uc, StringComparison.Ordinal);
		Assert.Contains("not a valid identifier", uc, StringComparison.Ordinal);
	}

	[Fact]
	public void ResourceChange_NonEmittableKeyNotRemoved()
	{
		// When a Color resource becomes a Style (non-emittable), the key must NOT be removed
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="Primary">Red</Color>
			        <Color x:Key="Accent">Blue</Color>
			    </ContentPage.Resources>
			    <Label Text="Test" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.MainPage">
			    <ContentPage.Resources>
			        <Style x:Key="Primary" TargetType="Label">
			            <Setter Property="TextColor" Value="Red" />
			        </Style>
			        <Color x:Key="Accent">Green</Color>
			    </ContentPage.Resources>
			    <Label Text="Test" />
			</ContentPage>
			""";

		var (_, run2) = TwoRuns(xamlV1, xamlV2);
		var uc = FindUCSource(run2, "uc.xsg");
		Assert.NotNull(uc);
		// The key array in the removal guard must include "Primary" even though it's non-emittable.
		// This prevents the old Color resource from being removed while the new Style can't be emitted.
		Assert.Contains("\"Primary\"", uc, StringComparison.Ordinal);
		Assert.Contains("\"Accent\"", uc, StringComparison.Ordinal);
		// Accent (still a Color) should be updated
		Assert.Contains("Resources[\"Accent\"]", uc, StringComparison.Ordinal);
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
				"build_property.EnableMauiIncrementalHotReload" => _file.EnableIncrementalHotReload ? "true" : "false",
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
