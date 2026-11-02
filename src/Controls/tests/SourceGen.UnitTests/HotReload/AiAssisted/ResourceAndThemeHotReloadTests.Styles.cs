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
	[MetadataUpdateFact]
	public void NestedImplicitStyleUpdate_ResolvesAncestorResources()
	{
		static string Xaml(string text) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <Color x:Key="Accent">Red</Color>
			  </ContentPage.Resources>
			  <VerticalStackLayout>
			    <VerticalStackLayout.Resources>
			      <Style TargetType="Label">
			        <Setter Property="Text" Value="{{text}}" />
			        <Setter Property="TextColor" Value="{StaticResource Accent}" />
			      </Style>
			    </VerticalStackLayout.Resources>
			    <Label />
			  </VerticalStackLayout>
			</ContentPage>
			""";

		using var harness = CreateHarness(nameof(NestedImplicitStyleUpdate_ResolvesAncestorResources));
		var generation = harness.Generate(Xaml("Before"), Xaml("After"));
		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var layout = Assert.IsType<VerticalStackLayout>(page.Content);
			var label = Assert.IsType<Label>(Assert.Single(layout.Children));
			Assert.Equal("Before", label.Text);
			Assert.Equal(Colors.Red, label.TextColor);
			live.ApplyUpdate<ContentPage>(1);
			Assert.Same(label, Assert.Single(layout.Children));
			Assert.Equal("After", label.Text);
			Assert.Equal(Colors.Red, label.TextColor);
		});
	}

	[MetadataUpdateFact]
	public void ResourceUpdate_PreservesNonSharedFactories()
	{
		static string Xaml(string text) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <Label x:Key="NonShared" x:Shared="False" Text="{{text}}" />
			  </ContentPage.Resources>
			</ContentPage>
			""";

		using var harness = CreateHarness(nameof(ResourceUpdate_PreservesNonSharedFactories));
		var generation = harness.Generate(Xaml("Before"), Xaml("After"));
		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var original = Assert.IsType<Label>(page.Resources["NonShared"]);
			Assert.Equal("Before", original.Text);
			Assert.NotSame(original, page.Resources["NonShared"]);
			live.ApplyUpdate<ContentPage>(1);
			var updated = Assert.IsType<Label>(page.Resources["NonShared"]);
			Assert.Equal("After", updated.Text);
			Assert.NotSame(updated, page.Resources["NonShared"]);
		});
	}

	[MetadataUpdateFact]
	public void BasedOnStyle_ResourceUpdate_UsesUpdatedBaseStyle()
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

		using var harness = CreateHarness(nameof(BasedOnStyle_ResourceUpdate_UsesUpdatedBaseStyle));
		var generation = harness.Generate(xamlV1, xamlV2);
		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);
			Assert.Equal(Colors.Red, label.TextColor);
			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Same(label, page.Content);
			var derived = Assert.IsType<Style>(page.Resources["Derived"]);
			Assert.Same(page.Resources["BaseB"], derived.BasedOn);
			var newLabel = new Label { Style = derived };
			Assert.Equal(Colors.Blue, newLabel.TextColor);
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
}
