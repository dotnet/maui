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
}
