#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

// Wave2 scope note: this file owns RT-01..RT-11 for the resources/theme portfolio.
// RT-01..RT-07 cover resource dictionaries, keyed styles, BasedOn styles and triggered styles.
// RT-08..RT-11 (Phase-2) cover AppThemeBinding branch edits, application-scope DynamicResource
// fanout, and multi-document (Source= merged dictionary / malformed->repair) scenarios; they rely
// on the Phase-0/1 harness capabilities cap-app-host, cap-theme-flip, cap-multi-instance and
// cap-multi-doc. Where the harness cannot faithfully reach a live invariant (Source= dictionaries
// have no compiled resource payload in this in-memory generator/ALC harness), coverage is provided
// at the strongest faithful level (generator/compile-atomic) with a paired live RED-PROBE that
// documents the precise boundary — never an always-skipped empty body.
[Collection("XamlHotReloadTests")]
public class ResourceAndThemeHotReloadTests
{
	const string PageClass = "TestAiAssisted.MainPage";

	const string PageStub = """
		namespace TestAiAssisted;

		public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
		{
			private partial void InitializeComponent();

			public MainPage()
			{
				InitializeComponent();
			}
		}
		""";

	static XamlHotReloadTestHarness CreateHarness(string scenarioName) =>
		new(scenarioName, PageClass, PageStub);

	static XamlHotReloadTestHarness CreateHostedHarness(
		XamlHotReloadApplicationOptions options, string scenarioName) =>
		new(scenarioName, PageClass, PageStub, options);

	// Wave2 · Resources · P0-10 · RT-01
	// Provenance: MAUI §1.4; public-app T6
	// Faithfulness: reaches DynamicResource markup path (UpdateComponentCodeWriter ~L1548,
	// TryEmitExpandedMarkupExtension) + keyed-scalar resource patch; fails if the value is not
	// re-resolved against the renamed key on live update.
	// Expected: GREEN
	[Theory]
	[InlineData("TextColor", "Color", "Color", "Red", "Blue")]
	[InlineData("FontSize", "x:Double", "Double", "20", "30")]
	[InlineData("Text", "x:String", "String", "Hello", "World")]
	public void DynamicResourceKey_RenameAndReverse_UpdatesVisibleValue(
		string propertyName, string resourceTag, string clrKind, string valueA, string valueB)
	{
		string Xaml(string resourceKey, string resourceValue, string consumerKey) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <{{resourceTag}} x:Key="{{resourceKey}}">{{resourceValue}}</{{resourceTag}}>
			  </ContentPage.Resources>
			  <Label {{propertyName}}="{DynamicResource {{consumerKey}}}" />
			</ContentPage>
			""";

		// V1: K1 = A, consumer binds to K1.
		var xamlV1 = Xaml("K1", valueA, "K1");
		// V2: resource renamed to K2 = B, consumer follows the rename.
		var xamlV2 = Xaml("K2", valueB, "K2");
		// V3: exact revert of V1 (K1 = A again).
		var xamlV3 = Xaml("K1", valueA, "K1");

		object Parse(string raw) => clrKind switch
		{
			"Color" => Color.Parse(raw),
			"Double" => double.Parse(raw, CultureInfo.InvariantCulture),
			"String" => raw,
			_ => throw new NotSupportedException(clrKind),
		};

		object? GetPropertyValue(Label label) =>
			typeof(Label).GetProperty(propertyName)!.GetValue(label);

		using var harness = CreateHarness(nameof(DynamicResourceKey_RenameAndReverse_UpdatesVisibleValue) + "_" + propertyName);
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);

			Assert.Equal(Parse(valueA), GetPropertyValue(label));
			Assert.True(page.Resources.ContainsKey("K1"));
			Assert.False(page.Resources.ContainsKey("K2"));

			live.ApplyUpdate<ContentPage>(1);
			Assert.Equal(Parse(valueB), GetPropertyValue(label));
			Assert.False(page.Resources.ContainsKey("K1"));
			Assert.True(page.Resources.ContainsKey("K2"));

			live.ApplyUpdate<ContentPage>(2);
			Assert.Equal(Parse(valueA), GetPropertyValue(label));
			Assert.True(page.Resources.ContainsKey("K1"));
			Assert.False(page.Resources.ContainsKey("K2"));
		});
	}

	// Wave2 · Resources · P0-09 · RT-02
	// Provenance: MAUI §1.1; public-app T1/T3
	// Faithfulness: reaches the generic complex-property skip path (UpdateComponentCodeWriter
	// ~L1194) for a non-root element's Resources property; fails (i.e. reclassify to GREEN) if the
	// writer ever starts emitting real merged-dictionary patch code for this shape.
	// Expected: DOC-SKIP-GUARD
	[Fact]
	public void MergedDictionaries_ComplexProperty_EmitsSkipMarker()
	{
		// Root ContentPage.Resources is special-cased by the writer (it always resolves via
		// TryEmitResourceDictionaryChange and never falls through to the generic marker below),
		// so this guard targets a non-root element's Resources — the shape that is empirically
		// declined via the generic complex-property path.
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <VerticalStackLayout>
			    <VerticalStackLayout.Resources>
			      <ResourceDictionary>
			        <ResourceDictionary.MergedDictionaries>
			          <ResourceDictionary>
			            <Color x:Key="Accent">Red</Color>
			          </ResourceDictionary>
			          <ResourceDictionary>
			            <Color x:Key="Accent">Blue</Color>
			          </ResourceDictionary>
			        </ResourceDictionary.MergedDictionaries>
			      </ResourceDictionary>
			    </VerticalStackLayout.Resources>
			    <Label TextColor="{DynamicResource Accent}" />
			  </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <VerticalStackLayout>
			    <VerticalStackLayout.Resources>
			      <ResourceDictionary>
			        <ResourceDictionary.MergedDictionaries>
			          <ResourceDictionary>
			            <Color x:Key="Accent">Blue</Color>
			          </ResourceDictionary>
			          <ResourceDictionary>
			            <Color x:Key="Accent">Red</Color>
			          </ResourceDictionary>
			        </ResourceDictionary.MergedDictionaries>
			      </ResourceDictionary>
			    </VerticalStackLayout.Resources>
			    <Label TextColor="{DynamicResource Accent}" />
			  </VerticalStackLayout>
			</ContentPage>
			""";

		using var harness = CreateHarness(nameof(MergedDictionaries_ComplexProperty_EmitsSkipMarker));
		var generation = harness.Generate(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains("// Complex property 'Resources' (ElementNode) \u2014 skipped (not yet supported)", updateComponentSource!, StringComparison.Ordinal);
	}

	// Wave2 · Resources · P0-09 · RT-03
	// Provenance: MAUI §1.1; public-app T1/T3
	// Faithfulness: reaches the same non-root merged-dictionary complex-property path as RT-02;
	// fails-for-bug: merged-dictionary "last wins" recomputation across reorder/removal is not
	// re-evaluated live because the property update is skipped entirely (paired guard: RT-02).
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "Blocked by complex property/collection reconciliation; tracked by #36732")]
	public void InlineMergedDictionaries_ReorderThenRemove_RecomputesWinner()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <VerticalStackLayout>
			    <VerticalStackLayout.Resources>
			      <ResourceDictionary>
			        <ResourceDictionary.MergedDictionaries>
			          <ResourceDictionary>
			            <Color x:Key="Accent">Red</Color>
			          </ResourceDictionary>
			          <ResourceDictionary>
			            <Color x:Key="Accent">Blue</Color>
			          </ResourceDictionary>
			        </ResourceDictionary.MergedDictionaries>
			      </ResourceDictionary>
			    </VerticalStackLayout.Resources>
			    <Label TextColor="{DynamicResource Accent}" />
			  </VerticalStackLayout>
			</ContentPage>
			""";
		// V2: same two dictionaries, order reversed — "last wins" should flip to Red.
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <VerticalStackLayout>
			    <VerticalStackLayout.Resources>
			      <ResourceDictionary>
			        <ResourceDictionary.MergedDictionaries>
			          <ResourceDictionary>
			            <Color x:Key="Accent">Blue</Color>
			          </ResourceDictionary>
			          <ResourceDictionary>
			            <Color x:Key="Accent">Red</Color>
			          </ResourceDictionary>
			        </ResourceDictionary.MergedDictionaries>
			      </ResourceDictionary>
			    </VerticalStackLayout.Resources>
			    <Label TextColor="{DynamicResource Accent}" />
			  </VerticalStackLayout>
			</ContentPage>
			""";
		// V3: the Red dictionary is removed entirely, leaving only Blue.
		const string xamlV3 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <VerticalStackLayout>
			    <VerticalStackLayout.Resources>
			      <ResourceDictionary>
			        <ResourceDictionary.MergedDictionaries>
			          <ResourceDictionary>
			            <Color x:Key="Accent">Blue</Color>
			          </ResourceDictionary>
			        </ResourceDictionary.MergedDictionaries>
			      </ResourceDictionary>
			    </VerticalStackLayout.Resources>
			    <Label TextColor="{DynamicResource Accent}" />
			  </VerticalStackLayout>
			</ContentPage>
			""";

		using var harness = CreateHarness(nameof(InlineMergedDictionaries_ReorderThenRemove_RecomputesWinner));
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var stack = Assert.IsType<VerticalStackLayout>(page.Content);
			var label = Assert.IsType<Label>(stack.Children[0]);

			Assert.Equal(Colors.Blue, label.TextColor);
			Assert.Equal(Colors.Blue, stack.Resources["Accent"]);

			live.ApplyUpdate<ContentPage>(1);
			Assert.Equal(Colors.Red, label.TextColor);
			Assert.Equal(Colors.Red, stack.Resources["Accent"]);

			live.ApplyUpdate<ContentPage>(2);
			Assert.Equal(Colors.Blue, label.TextColor);
			Assert.Equal(Colors.Blue, stack.Resources["Accent"]);

			// The removed Red dictionary must no longer influence the live winner after V3.
			stack.Resources["Accent"] = Colors.Red;
			Assert.Equal(Colors.Blue, label.TextColor);
		});
	}

	// Wave2 · Resources · P0-13 · RT-04
	// Provenance: toolkit T31/T32; public-app T10/T11
	// Faithfulness: reaches the resource-dictionary keyed-encode path (UpdateComponentCodeWriter
	// ~L929, BuildResourceValueExpression); Style has no public parameterless constructor so it
	// always fails encoding and is left untouched. Fails (i.e. reclassify to GREEN) if the writer
	// ever gains a Style-aware resource-value builder.
	// Expected: DOC-SKIP-GUARD
	[Fact]
	public void BasedOnStyle_ComplexResource_EmitsSkipMarker()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			      <Style x:Key="BaseA" TargetType="Label">
			        <Setter Property="TextColor" Value="Red" />
			      </Style>
			      <Style x:Key="BaseB" TargetType="Label">
			        <Setter Property="TextColor" Value="Blue" />
			      </Style>
			      <Style x:Key="Derived" TargetType="Label" BasedOn="{StaticResource BaseA}" />
			  </ContentPage.Resources>
			  <Label Style="{StaticResource Derived}" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			      <Style x:Key="BaseA" TargetType="Label">
			        <Setter Property="TextColor" Value="Red" />
			      </Style>
			      <Style x:Key="BaseB" TargetType="Label">
			        <Setter Property="TextColor" Value="Blue" />
			      </Style>
			      <Style x:Key="Derived" TargetType="Label" BasedOn="{StaticResource BaseB}" />
			  </ContentPage.Resources>
			  <Label Style="{StaticResource Derived}" />
			</ContentPage>
			""";

		using var harness = CreateHarness(nameof(BasedOnStyle_ComplexResource_EmitsSkipMarker));
		var generation = harness.Generate(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains("// Cannot encode resource 'Derived' \u2014 left untouched", updateComponentSource!, StringComparison.Ordinal);
	}

	// Wave2 · Resources · P0-13 · RT-05
	// Provenance: toolkit T31/T32; public-app T10/T11
	// Faithfulness: reaches the same style-resource keyed-encode path as RT-04; fails-for-bug:
	// re-targeting a BasedOn style's base and reapplying it to an already-live target requires
	// subtree reconstruction that the writer does not perform once the resource is left untouched
	// (paired guard: RT-04).
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "Blocked by complex property/collection reconciliation; tracked by #36732")]
	public void BasedOnStyle_BaseSwapAndReverse_ReappliesLiveTarget()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			      <Style x:Key="BaseA" TargetType="Label">
			        <Setter Property="TextColor" Value="Red" />
			      </Style>
			      <Style x:Key="BaseB" TargetType="Label">
			        <Setter Property="TextColor" Value="Blue" />
			      </Style>
			      <Style x:Key="Derived" TargetType="Label" BasedOn="{StaticResource BaseA}" />
			  </ContentPage.Resources>
			  <Label Style="{StaticResource Derived}" />
			</ContentPage>
			""";
		// V2: Derived's BasedOn switches from BaseA to BaseB; the bases themselves are unchanged.
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			      <Style x:Key="BaseA" TargetType="Label">
			        <Setter Property="TextColor" Value="Red" />
			      </Style>
			      <Style x:Key="BaseB" TargetType="Label">
			        <Setter Property="TextColor" Value="Blue" />
			      </Style>
			      <Style x:Key="Derived" TargetType="Label" BasedOn="{StaticResource BaseB}" />
			  </ContentPage.Resources>
			  <Label Style="{StaticResource Derived}" />
			</ContentPage>
			""";
		// V3: exact revert of V1.
		const string xamlV3 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			      <Style x:Key="BaseA" TargetType="Label">
			        <Setter Property="TextColor" Value="Red" />
			      </Style>
			      <Style x:Key="BaseB" TargetType="Label">
			        <Setter Property="TextColor" Value="Blue" />
			      </Style>
			      <Style x:Key="Derived" TargetType="Label" BasedOn="{StaticResource BaseA}" />
			  </ContentPage.Resources>
			  <Label Style="{StaticResource Derived}" />
			</ContentPage>
			""";

		using var harness = CreateHarness(nameof(BasedOnStyle_BaseSwapAndReverse_ReappliesLiveTarget));
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);

			Assert.Equal(Colors.Red, label.TextColor);

			live.ApplyUpdate<ContentPage>(1);
			Assert.Equal(Colors.Blue, label.TextColor);

			live.ApplyUpdate<ContentPage>(2);
			Assert.Equal(Colors.Red, label.TextColor);

			// After V3, BaseA (the active base) must affect the live target; BaseB (the
			// no-longer-referenced base) must not.
			Assert.Equal(Colors.Red, ((Style)page.Resources["BaseA"]).Setters[0].Value);
			((Style)page.Resources["BaseB"]).Setters[0].Value = Colors.Lime;
			Assert.Equal(Colors.Red, label.TextColor);
		});
	}

	// Wave2 · Resources · P0-05 · RT-06
	// Provenance: MAUI §3.2/3.3; public-app T15
	// Faithfulness: reaches the generic complex-property skip path (UpdateComponentCodeWriter
	// ~L1194) for an inline Label.Style replacement; fails (i.e. reclassify to GREEN) if the
	// writer ever starts emitting real inline-Style patch code.
	// Expected: DOC-SKIP-GUARD
	[Fact]
	public void TriggeredStyle_ComplexProperty_EmitsSkipMarker()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="Fallback" IsEnabled="False">
			    <Label.Style>
			      <Style TargetType="Label">
			        <Style.Triggers>
			          <Trigger TargetType="Label" Property="IsEnabled" Value="False">
			            <Setter Property="Text" Value="V1Active" />
			          </Trigger>
			        </Style.Triggers>
			      </Style>
			    </Label.Style>
			  </Label>
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="Fallback" IsEnabled="False">
			    <Label.Style>
			      <Style TargetType="Label">
			        <Style.Triggers>
			          <Trigger TargetType="Label" Property="IsEnabled" Value="False">
			            <Setter Property="Text" Value="V3Active" />
			          </Trigger>
			        </Style.Triggers>
			      </Style>
			    </Label.Style>
			  </Label>
			</ContentPage>
			""";

		using var harness = CreateHarness(nameof(TriggeredStyle_ComplexProperty_EmitsSkipMarker));
		var generation = harness.Generate(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains("// Complex property 'Style' (ElementNode) \u2014 skipped (not yet supported)", updateComponentSource!, StringComparison.Ordinal);
	}

	// Wave2 · Resources · P0-05 · RT-07
	// Provenance: MAUI §3.2/3.3; public-app T15
	// Faithfulness: reaches the same inline-Style complex-property path as RT-06; fails-for-bug:
	// removing then re-adding a triggered style must fully unapply the trigger's setters before
	// reattaching, which requires subtree reconciliation the writer does not perform once the
	// property update is skipped (paired guard: RT-06).
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "Blocked by complex property/collection reconciliation; tracked by #36732")]
	public void ActiveTriggerStyle_RemoveReAdd_UnappliesBeforeReattach()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="Fallback" IsEnabled="False">
			    <Label.Style>
			      <Style TargetType="Label">
			        <Style.Triggers>
			          <Trigger TargetType="Label" Property="IsEnabled" Value="False">
			            <Setter Property="Text" Value="V1Active" />
			          </Trigger>
			        </Style.Triggers>
			      </Style>
			    </Label.Style>
			  </Label>
			</ContentPage>
			""";
		// V2: the Style is removed entirely; the trigger's setter must be unapplied.
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="Fallback" IsEnabled="False" />
			</ContentPage>
			""";
		// V3: the same trigger graph is re-added, with a new setter value.
		const string xamlV3 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="Fallback" IsEnabled="False">
			    <Label.Style>
			      <Style TargetType="Label">
			        <Style.Triggers>
			          <Trigger TargetType="Label" Property="IsEnabled" Value="False">
			            <Setter Property="Text" Value="V3Active" />
			          </Trigger>
			        </Style.Triggers>
			      </Style>
			    </Label.Style>
			  </Label>
			</ContentPage>
			""";

		using var harness = CreateHarness(nameof(ActiveTriggerStyle_RemoveReAdd_UnappliesBeforeReattach));
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);

			Assert.Equal("V1Active", label.Text);

			live.ApplyUpdate<ContentPage>(1);
			Assert.Null(label.Style);
			Assert.Equal("Fallback", label.Text);

			live.ApplyUpdate<ContentPage>(2);
			Assert.Equal("V3Active", label.Text);

			// Toggling IsEnabled twice after the style is reattached should produce exactly one
			// apply/unapply cycle of the trigger's setter, not a stacked or duplicated effect.
			label.IsEnabled = true;
			Assert.Equal("Fallback", label.Text);
			label.IsEnabled = false;
			Assert.Equal("V3Active", label.Text);
		});
	}

	// Wave2 · Resources · family-4 · RT-08 (generation-atomic anchor)
	// Provenance: MAUI §3.4 (AppThemeBinding); public-app T18
	// Faithfulness: strongest faithful level the harness supports for an AppThemeBinding branch edit.
	// It proves the edit and its exact revert are captured in the generated component and that every
	// version compiles: the Light branch literal moves Light1→Light2→Light1 across versions while the
	// Dark branch is left untouched. The LIVE re-provide of the edited AppThemeBinding through
	// UpdateComponent is the paired RED-PROBE below (AppThemeBinding_BranchEdit_UpdatesSelectedBranch
	// Live) — the generator currently emits an UpdateComponent that re-provides the AppThemeBinding
	// through an IProvideValueTarget whose TargetProperty is null, so product code cannot run it.
	// Expected: GREEN
	[Fact]
	public void AppThemeBinding_BranchEdit_IsCapturedInGeneratedComponent()
	{
		string Xaml(string lightBranch) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="{AppThemeBinding Light={{lightBranch}}, Dark=DarkText}" />
			</ContentPage>
			""";

		using var harness = CreateHarness(nameof(AppThemeBinding_BranchEdit_IsCapturedInGeneratedComponent));
		// V1: Light=Light1. V2: only the Light branch is edited to Light2. V3: exact revert of V1.
		var generation = harness.Generate(Xaml("Light1"), Xaml("Light2"), Xaml("Light1"));

		for (var index = 0; index < 3; index++)
		{
			Assert.NotNull(generation[index].InitializeComponentSource);
			Assert.Contains("\"DarkText\"", generation[index].InitializeComponentSource!, StringComparison.Ordinal);
			Assert.True(harness.Compile(generation[index]).PeImage.Length > 0);
		}

		// The edited Light branch is captured in V2 and exactly reverted in V3; Dark stays untouched.
		Assert.Contains("Light = \"Light1\"", generation[0].InitializeComponentSource!, StringComparison.Ordinal);
		Assert.Contains("Light = \"Light2\"", generation[1].InitializeComponentSource!, StringComparison.Ordinal);
		Assert.Contains("Light = \"Light1\"", generation[2].InitializeComponentSource!, StringComparison.Ordinal);
		Assert.DoesNotContain("Light = \"Light2\"", generation[2].InitializeComponentSource!, StringComparison.Ordinal);
	}

	// Wave2 · Resources · family-4 · RT-08 (live probe)
	// Provenance: MAUI §3.4 (AppThemeBinding); public-app T18
	// Faithfulness: the intended LIVE invariant — under an Application host, editing the Light branch
	// must re-apply on each live update (Light1→Light2→Light1) on the SAME retained consumer, and that
	// single retained proxy must then honor a synchronous Light/Dark theme flip in both directions
	// (no stale or duplicated AppThemeBinding proxy). This cannot run in the current harness: the
	// generated UpdateComponent re-provides the AppThemeBinding through a SimpleValueTargetProvider
	// whose TargetProperty is null, so AppThemeBindingExtension.ProvideValue throws
	// InvalidOperationException "Cannot determine property to provide the value for."
	// (src/Controls/src/Xaml/MarkupExtensions/AppThemeBindingExtension.cs). The body below is the real
	// assertion that would run once UpdateComponent supplies the target BindableProperty.
	// Paired green anchor: AppThemeBinding_BranchEdit_IsCapturedInGeneratedComponent.
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "Blocked by the generated UpdateComponent for an AppThemeBinding: it re-provides the markup extension through an IProvideValueTarget whose TargetProperty is null, so AppThemeBindingExtension.ProvideValue throws 'Cannot determine property to provide the value for'; the branch edit is still proven at generation level by AppThemeBinding_BranchEdit_IsCapturedInGeneratedComponent; tracked by #36732")]
	public void AppThemeBinding_BranchEdit_UpdatesSelectedBranchLive()
	{
		string Xaml(string lightBranch) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="{AppThemeBinding Light={{lightBranch}}, Dark=DarkText}" />
			</ContentPage>
			""";

		// V1: Light branch = Light1. V2: only the Light branch is edited (Dark stays DarkText).
		// V3: exact revert of V1.
		var xamlV1 = Xaml("Light1");
		var xamlV2 = Xaml("Light2");
		var xamlV3 = Xaml("Light1");

		using var harness = CreateHostedHarness(
			new XamlHotReloadApplicationOptions(InitialTheme: AppTheme.Light),
			nameof(AppThemeBinding_BranchEdit_UpdatesSelectedBranchLive));
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);

			// Under the Light theme the selected branch is edited across versions.
			Assert.Equal("Light1", label.Text);

			live.ApplyUpdate<ContentPage>(1);
			Assert.Equal("Light2", label.Text);

			live.ApplyUpdate<ContentPage>(2);
			Assert.Equal("Light1", label.Text);

			// The current, single retained proxy must react to a live theme flip in both directions.
			harness.ApplicationHost!.SetAppTheme(AppTheme.Dark);
			Assert.Equal("DarkText", label.Text);   // Dark branch of the current binding

			harness.ApplicationHost!.SetAppTheme(AppTheme.Light);
			Assert.Equal("Light1", label.Text);      // back to Light1 — no stacked/stale proxy
		});
	}

	// Wave2 · Resources · P1-04 · RT-09
	// Provenance: MAUI §3.1 (App.xaml DynamicResource); public-app T16
	// Faithfulness: application-scope DynamicResource fanout across two retained roots plus one fresh
	// post-update root, under a live Application host (cap-app-host + cap-multi-instance). The harness
	// regenerates the page (not App.xaml), so the visible Red→Blue→Red transition is realized by hot-
	// reloading the consumer's DynamicResource *key* across two application-scoped keys (AccentA=Red,
	// AccentB=Blue). This exercises app-scope resolution (ResourcesExtensions.TryGetResource, including
	// the Application.Current fallback for not-yet-attached roots), multi-instance UpdateComponent
	// fanout to every retained root, and fresh-root-starts-latest without replay. The application
	// ResourceDictionary object identity is asserted stable across the page updates. Fails if a
	// retained root is not re-resolved, a fresh root replays history, or the app dictionary is swapped.
	// Expected: GREEN
	[MetadataUpdateFact]
	public void ApplicationDynamicResource_FansOutAndFreshRootsStartLatest()
	{
		string Xaml(string key) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label TextColor="{DynamicResource {{key}}}" />
			</ContentPage>
			""";

		var xamlV1 = Xaml("AccentA"); // resolves app AccentA = Red
		var xamlV2 = Xaml("AccentB"); // resolves app AccentB = Blue
		var xamlV3 = Xaml("AccentA"); // reverts to Red

		var options = new XamlHotReloadApplicationOptions(new Dictionary<string, object>
		{
			["AccentA"] = Colors.Red,
			["AccentB"] = Colors.Blue,
		});

		using var harness = CreateHostedHarness(
			options, nameof(ApplicationDynamicResource_FansOutAndFreshRootsStartLatest));
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		harness.RunLive(generation, live =>
		{
			var appResources = harness.ApplicationHost!.Application.Resources;

			var first = live.GetInstance<ContentPage>();       // attached as the single MainPage
			var second = live.CreateInstance<ContentPage>();   // retained, not attached
			var firstLabel = Assert.IsType<Label>(first.Content);
			var secondLabel = Assert.IsType<Label>(second.Content);
			Assert.Equal(Colors.Red, firstLabel.TextColor);
			Assert.Equal(Colors.Red, secondLabel.TextColor);

			// V2: both retained roots re-resolve to the application AccentB (Blue) value, once each.
			live.ApplyUpdate<ContentPage>(1);
			Assert.Equal(Colors.Blue, firstLabel.TextColor);
			Assert.Equal(Colors.Blue, secondLabel.TextColor);

			// A fresh root created after V2 starts at the latest version (Blue), without replaying V1.
			var third = live.CreateInstance<ContentPage>();
			Assert.Equal(Colors.Blue, Assert.IsType<Label>(third.Content).TextColor);

			// V3: reverse; every retained root — including the fresh one — tracks back to Red.
			live.ApplyUpdate<ContentPage>(2);
			Assert.Equal(Colors.Red, firstLabel.TextColor);
			Assert.Equal(Colors.Red, secondLabel.TextColor);
			Assert.Equal(Colors.Red, Assert.IsType<Label>(third.Content).TextColor);

			// The application dictionary was never mutated or swapped by the page updates.
			Assert.Same(appResources, harness.ApplicationHost!.Application.Resources);
			Assert.Equal(Colors.Red, (Color)appResources["AccentA"]);
			Assert.Equal(Colors.Blue, (Color)appResources["AccentB"]);
		});
	}

	// Wave2 · Resources · P1-01 · RT-10 (generator/compile-atomic anchor)
	// Provenance: MAUI §3.1 (Source= merged dictionaries); public-app T13
	// Faithfulness: strongest faithful level the harness supports for Source= merged dictionaries.
	// It TRACKS the reorder then removal of the Source= sibling documents and RECOMPILES the page in
	// every version (mirrors HarnessCapabilityTests.MultiDocument_DictionaryOnlyEdit_TracksAllDocuments
	// AndCompilesPage). The live runtime cannot reload a Source= dictionary into a retained page (no
	// compiled resource payload in this in-memory generator/ALC harness); that live invariant is the
	// paired RED-PROBE below (SourceMergedDictionaries_ReorderThenRemove_UsesRuntimeFallback).
	// Expected: GREEN
	[Fact]
	public void SourceMergedDictionaries_ReorderThenRemove_TracksDocumentsAndCompiles()
	{
		string Page(string mergedBody) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <ResourceDictionary>
			      <ResourceDictionary.MergedDictionaries>
			{{mergedBody}}
			      </ResourceDictionary.MergedDictionaries>
			    </ResourceDictionary>
			  </ContentPage.Resources>
			  <Label TextColor="{DynamicResource Accent}" />
			</ContentPage>
			""";

		const string sourceA = "        <ResourceDictionary Source=\"A.xaml\" />";
		const string sourceB = "        <ResourceDictionary Source=\"B.xaml\" />";
		const string dictA = """
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			  <Color x:Key="Accent">Red</Color>
			</ResourceDictionary>
			""";
		const string dictB = """
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			  <Color x:Key="Accent">Blue</Color>
			</ResourceDictionary>
			""";

		IReadOnlyDictionary<string, XamlHotReloadDocument> Snapshot(string mergedBody, bool includeA)
		{
			var snapshot = new Dictionary<string, XamlHotReloadDocument>(StringComparer.Ordinal)
			{
				["MainPage.xaml"] = new XamlHotReloadDocument(Page(mergedBody)),
				["B.xaml"] = new XamlHotReloadDocument(dictB),
			};
			if (includeA)
				snapshot["A.xaml"] = new XamlHotReloadDocument(dictA);
			return snapshot;
		}

		using var harness = CreateHarness(nameof(SourceMergedDictionaries_ReorderThenRemove_TracksDocumentsAndCompiles));
		var generation = harness.GenerateDocuments(
			Snapshot(sourceA + "\n" + sourceB, includeA: true),   // V1: A;B
			Snapshot(sourceB + "\n" + sourceA, includeA: true),   // V2: B;A (reorder)
			Snapshot(sourceB, includeA: false));                  // V3: B only (A removed)

		// Reorder and removal of the Source= sibling documents are visible to the harness.
		Assert.True(generation[0].Documents.ContainsKey("A.xaml"));
		Assert.True(generation[1].Documents.ContainsKey("A.xaml"));
		Assert.False(generation[2].Documents.ContainsKey("A.xaml"));

		// The page owns exactly one generated root and compiles in every version.
		for (var index = 0; index < 3; index++)
		{
			Assert.Single(generation[index].GeneratedRoots);
			Assert.Equal(PageClass, generation[index].GeneratedRoots[0].TypeName);
			Assert.True(harness.Compile(generation[index]).PeImage.Length > 0);
		}
	}

	// Wave2 · Resources · P1-01 · RT-10 (live probe)
	// Provenance: MAUI §3.1 (Source= merged dictionaries); public-app T13
	// Faithfulness: the intended LIVE invariant — reordering then removing Source= merged dictionaries
	// must recompute the winning {DynamicResource Accent} on the retained page (Blue→Red→Blue), and a
	// removed dictionary must stop contributing. This cannot run in the current harness: a
	// ResourceDictionary loaded through Source= has no compiled resource payload in the in-memory
	// generator/ALC harness, so the runtime cannot reload it into a retained page without faking
	// ResourceLoader behavior (see HarnessCapabilityTests.MultiDocument_DictionaryOnlyEdit_RetainsPage
	// AndLabelIdentity, which is skipped for the same reason). The body below is the real assertion
	// that would run once the harness can load compiled Source= payloads.
	// Paired green anchor: SourceMergedDictionaries_ReorderThenRemove_TracksDocumentsAndCompiles.
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "Blocked by the harness Source= resource-loader boundary: a ResourceDictionary loaded through Source= has no compiled resource payload in this in-memory generator/ALC harness, so the runtime cannot reload it into a retained page without faking ResourceLoader; green anchor: SourceMergedDictionaries_ReorderThenRemove_TracksDocumentsAndCompiles; tracked by #36732")]
	public void SourceMergedDictionaries_ReorderThenRemove_UsesRuntimeFallback()
	{
		string Page(string mergedBody) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <ResourceDictionary>
			      <ResourceDictionary.MergedDictionaries>
			{{mergedBody}}
			      </ResourceDictionary.MergedDictionaries>
			    </ResourceDictionary>
			  </ContentPage.Resources>
			  <Label TextColor="{DynamicResource Accent}" />
			</ContentPage>
			""";

		const string sourceA = "        <ResourceDictionary Source=\"A.xaml\" />";
		const string sourceB = "        <ResourceDictionary Source=\"B.xaml\" />";
		const string dictA = """
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			  <Color x:Key="Accent">Red</Color>
			</ResourceDictionary>
			""";
		const string dictB = """
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			  <Color x:Key="Accent">Blue</Color>
			</ResourceDictionary>
			""";

		IReadOnlyDictionary<string, XamlHotReloadDocument> Snapshot(string mergedBody, bool includeA)
		{
			var snapshot = new Dictionary<string, XamlHotReloadDocument>(StringComparer.Ordinal)
			{
				["MainPage.xaml"] = new XamlHotReloadDocument(Page(mergedBody)),
				["B.xaml"] = new XamlHotReloadDocument(dictB),
			};
			if (includeA)
				snapshot["A.xaml"] = new XamlHotReloadDocument(dictA);
			return snapshot;
		}

		using var harness = CreateHarness(nameof(SourceMergedDictionaries_ReorderThenRemove_UsesRuntimeFallback));
		var generation = harness.GenerateDocuments(
			Snapshot(sourceA + "\n" + sourceB, includeA: true),   // V1: A;B → later B wins → Blue
			Snapshot(sourceB + "\n" + sourceA, includeA: true),   // V2: B;A → later A wins → Red
			Snapshot(sourceB, includeA: false));                  // V3: B only → Blue

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);

			// MergedDictionaries: the last-added dictionary wins for a duplicate key.
			Assert.Equal(Colors.Blue, label.TextColor);   // A;B → B wins

			live.ApplyUpdate<ContentPage>(1);
			Assert.Equal(Colors.Red, label.TextColor);    // B;A → A wins

			live.ApplyUpdate<ContentPage>(2);
			Assert.Equal(Colors.Blue, label.TextColor);   // B only

			// The removed A dictionary must no longer contribute: mutating a fresh copy of its value
			// after removal must not affect the retained label.
			page.Resources.MergedDictionaries.First()["Accent"] = Colors.Fuchsia;
			Assert.Equal(Colors.Blue, label.TextColor);
		});
	}

	// Wave2 · Resources · P1-05 · RT-11
	// Provenance: MAUI §3.1/§3.5 (multi-document malformed→repair); public-app T14
	// Faithfulness: multi-document malformed→repair atomicity at the strongest faithful level the
	// harness supports — GENERATOR-ATOMIC, NOT live-resource atomic. The batch carries two
	// AdditionalTexts (MainPage.xaml + a sibling Theme.xaml resource dictionary). Source= cannot be
	// used to wire the page to the dictionary in this in-memory generator/ALC harness (it emits an
	// unconditional MAUIG1001 "not a valid resource path" — the same boundary proven by RT-10), so the
	// malformed edit is applied to the generator-parsed document (MainPage.xaml) while the sibling
	// dictionary is carried unchanged through the whole batch. The atomicity invariant asserted here:
	// a malformed intermediate version surfaces a parser error and does NOT phantom-advance to a valid
	// compiled page, and the repaired version fully recovers (generates the repaired content and
	// compiles), with the sibling document tracked across every version. The LIVE retained-object
	// atomic reload of a Source= dictionary is out of harness scope, hence generator-atomic labeling.
	// Fails if a malformed batch silently produces a compilable page, if the repaired batch does not
	// generate/compile, or if the sibling document is dropped.
	// Expected: GREEN (generator-atomic)
	[Fact]
	public void MultiDocumentMalformedThenRepair_IsAtomicAcrossPageAndDictionary()
	{
		const string pageValidBefore = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="Before" />
			</ContentPage>
			""";
		// V2: the <Label> element is never closed — a parser error in the page document.
		const string pageMalformed = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="AfterRepair"
			</ContentPage>
			""";
		const string pageRepaired = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="AfterRepair" />
			</ContentPage>
			""";

		// The sibling dictionary is a valid resource document carried unchanged across every version.
		const string theme = """
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			  <Color x:Key="Accent">Blue</Color>
			</ResourceDictionary>
			""";

		IReadOnlyDictionary<string, XamlHotReloadDocument> Snapshot(string page) =>
			new Dictionary<string, XamlHotReloadDocument>(StringComparer.Ordinal)
			{
				["MainPage.xaml"] = new XamlHotReloadDocument(page),
				["Theme.xaml"] = new XamlHotReloadDocument(theme),
			};

		using var harness = CreateHarness(nameof(MultiDocumentMalformedThenRepair_IsAtomicAcrossPageAndDictionary));
		var generation = harness.GenerateDocumentsAllowingDiagnostics(
			Snapshot(pageValidBefore),
			Snapshot(pageMalformed),
			Snapshot(pageRepaired));

		// The sibling dictionary document is tracked through the malformed→repair batch, unchanged.
		Assert.True(generation[0].Documents.ContainsKey("Theme.xaml"));
		Assert.True(generation[1].Documents.ContainsKey("Theme.xaml"));
		Assert.True(generation[2].Documents.ContainsKey("Theme.xaml"));
		Assert.Equal(theme, generation[2].Documents["Theme.xaml"].Text);

		// V1: both documents valid — the page generates and compiles.
		Assert.DoesNotContain(
			generation[0].GeneratorResult.Diagnostics,
			diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
		Assert.NotNull(generation[0].InitializeComponentSource);
		Assert.True(harness.Compile(generation[0]).PeImage.Length > 0);

		// V2: the malformed page surfaces at least one parser error diagnostic and does NOT
		// phantom-advance to a valid compiled page.
		Assert.Contains(
			generation[1].GeneratorResult.Diagnostics,
			diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
		Assert.Null(generation[1].InitializeComponentSource);

		// V3: the page is repaired — it generates the repaired content and compiles cleanly.
		Assert.DoesNotContain(
			generation[2].GeneratorResult.Diagnostics,
			diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
		Assert.NotNull(generation[2].InitializeComponentSource);
		Assert.Contains("AfterRepair", generation[2].InitializeComponentSource!, StringComparison.Ordinal);
		Assert.True(harness.Compile(generation[2]).PeImage.Length > 0);
	}
}
