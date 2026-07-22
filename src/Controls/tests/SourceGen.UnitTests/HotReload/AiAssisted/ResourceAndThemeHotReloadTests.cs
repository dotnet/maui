#nullable enable

using System;
using System.Globalization;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

// Wave2 scope note: this file owns ONLY RT-01..RT-07 (resource dictionaries, keyed styles,
// BasedOn styles and triggered styles). RT-08..RT-11 (AppThemeBinding/App.xaml/multi-document
// scenarios) are prereq-gated per the Wave-2 test plan and are intentionally not implemented here.
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
}
