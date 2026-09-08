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
}
