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

public partial class ResourceAndThemeHotReloadTests
{
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
