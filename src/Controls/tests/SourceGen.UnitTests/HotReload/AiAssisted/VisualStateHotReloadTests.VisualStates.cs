#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

public partial class VisualStateHotReloadTests
{
	// Wave2 · Visual State · P0-03 · VS-01
	// Provenance: MAUI §2.1; public-app T13/T14; PoolMath CAT-02
	// Faithfulness: reaches writer L1319; fails-for-bug: active VSM setter edits are explicitly declined.
	// Expected: DOC-SKIP-GUARD
	// Issue: https://github.com/dotnet/maui/issues/36732
	[Fact]
	public void ActiveVsmSetter_ComplexAttachedProperty_EmitsSkipMarker()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(
			CreateActiveStateXaml("BackgroundColor", "Red"),
			CreateActiveStateXaml("BackgroundColor", "Blue"));
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains(
			"Complex attached property 'VisualStateManager.VisualStateGroups' — skipped",
			updateComponentSource!,
			StringComparison.Ordinal);

		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Generated VSM skip path should compile.");
	}

	// Wave2 · Visual State · P0-03 · VS-02
	// Provenance: MAUI §2.1; public-app T13/T14; PoolMath CAT-02
	// Faithfulness: reaches writer L1319; fails-for-bug: the active state retains stale setter values.
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[Theory(Skip = "RED-PROBE #36732: active VSM setter reconciliation is not implemented.")]
	[InlineData("BackgroundColor", "Red", "Blue")]
	[InlineData("Opacity", ".25", ".75")]
	[InlineData("Scale", "1", "2")]
	public void ActiveVsmSetter_EditAndReverse_ReappliesImmediately(
		string propertyName,
		string valueV1,
		string valueV2)
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(
			CreateActiveStateXaml(propertyName, valueV1),
			CreateActiveStateXaml(propertyName, valueV2),
			CreateActiveStateXaml(propertyName, valueV1));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);

			Assert.True(VisualStateManager.GoToState(label, "Active"));
			AssertActiveState(label);
			AssertVisualProperty(label, propertyName, valueV1);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Same(label, page.Content);
			AssertActiveState(label);
			AssertVisualProperty(label, propertyName, valueV2);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
			Assert.Same(label, page.Content);
			AssertActiveState(label);
			AssertVisualProperty(label, propertyName, valueV1);
		});
	}

	// Wave2 · Visual State · family 2 core · VS-05
	// Provenance: MAUI §2.1; public-app T13/T14; minimal add/remove/re-add extension
	// Faithfulness: reaches writer L1319; fails-for-bug: state removal cannot apply fallback or re-add a live state.
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "RED-PROBE #36732: VSM state graph reconciliation and fallback are not implemented.")]
	public void VsmState_AddRemoveReAdd_And_FallbackReversion()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(
			CreateStateGraphXaml(includeActive: true, activeColor: "Red"),
			CreateStateGraphXaml(includeActive: false, activeColor: null),
			CreateStateGraphXaml(includeActive: true, activeColor: "Green"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);
			var initialGroup = Assert.Single(VisualStateManager.GetVisualStateGroups(label));
			var oldActiveState = Assert.Single(initialGroup.States, static state => state.Name == "Active");

			Assert.True(VisualStateManager.GoToState(label, "Active"));
			Assert.Equal("Active", initialGroup.CurrentState.Name);
			Assert.Equal(Colors.Red, label.BackgroundColor);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Same(label, page.Content);
			var fallbackGroup = Assert.Single(VisualStateManager.GetVisualStateGroups(label));
			Assert.DoesNotContain(fallbackGroup.States, static state => state.Name == "Active");
			Assert.Equal("Normal", fallbackGroup.CurrentState.Name);
			Assert.Equal(Colors.Blue, label.BackgroundColor);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
			Assert.Same(label, page.Content);
			var reAddedGroup = Assert.Single(VisualStateManager.GetVisualStateGroups(label));
			var newActiveState = Assert.Single(reAddedGroup.States, static state => state.Name == "Active");
			Assert.NotSame(oldActiveState, newActiveState);
			Assert.Equal("Normal", reAddedGroup.CurrentState.Name);
			Assert.Equal(Colors.Blue, label.BackgroundColor);

			Assert.True(VisualStateManager.GoToState(label, "Active"));
			Assert.Equal("Active", reAddedGroup.CurrentState.Name);
			Assert.Equal(Colors.Green, label.BackgroundColor);
		});
	}

	// Wave2 · Visual State · family 2 combined · VS-06
	// Provenance: MAUI §2.1; public-app T13/T14; cap-app-host/theme extension
	// Faithfulness: reaches writer L1319 with nested AppThemeBinding/StaticResource values;
	// fails-for-bug: generated VisualStateGroups replacement is explicitly declined.
	// Expected: DOC-SKIP-GUARD
	// Issue: https://github.com/dotnet/maui/issues/36732
	[Fact]
	public void ActiveVsmThemeResourceSetter_ComplexAttachedProperty_EmitsSkipMarker()
	{
		using var harness = CreateHostedHarness(CreateThemeOptions());
		var generation = harness.Generate(
			CreateThemeResourceStateXaml("V1"),
			CreateThemeResourceStateXaml("V2"));
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains(
			"Complex attached property 'VisualStateManager.VisualStateGroups' — skipped",
			updateComponentSource!,
			StringComparison.Ordinal);
		Assert.True(
			harness.Compile(generation[1]).PeImage.Length > 0,
			"Generated VSM theme/resource skip path should compile.");
	}

	// Wave2 · Visual State · family 2 combined · VS-06
	// Provenance: MAUI §2.1; public-app T13/T14; cap-app-host/theme extension
	// Faithfulness: combines an active VSM setter with AppThemeBinding and app-level
	// StaticResource lookup; fails-for-bug: the active setter remains on its V1 resource keys.
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "RED-PROBE #36732: active VSM theme/resource setter reconciliation is not implemented.")]
	public void ActiveVsmThemeResourceSetter_EditAndReverse_PreservesStateAndThemeSemantics()
	{
		var previousApplication = Application.Current;

		using (var harness = CreateHostedHarness(CreateThemeOptions()))
		{
			var generation = harness.Generate(
				CreateThemeResourceStateXaml("V1"),
				CreateThemeResourceStateXaml("V2"),
				CreateThemeResourceStateXaml("V1"));

			harness.RunLive(generation, live =>
			{
				var page = live.GetInstance<ContentPage>();
				var label = Assert.IsType<Label>(page.Content);

				Assert.True(VisualStateManager.GoToState(label, "Active"));
				AssertActiveThemeState(label, harness, AppTheme.Light, Colors.Red);

				harness.ApplicationHost!.SetAppTheme(AppTheme.Dark);
				AssertActiveThemeState(label, harness, AppTheme.Dark, Colors.Maroon);

				Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
				Assert.Same(label, page.Content);
				AssertActiveThemeState(label, harness, AppTheme.Dark, Colors.Blue);

				harness.ApplicationHost.SetAppTheme(AppTheme.Light);
				AssertActiveThemeState(label, harness, AppTheme.Light, Colors.Green);

				Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
				Assert.Same(label, page.Content);
				AssertActiveThemeState(label, harness, AppTheme.Light, Colors.Red);

				harness.ApplicationHost.SetAppTheme(AppTheme.Dark);
				AssertActiveThemeState(label, harness, AppTheme.Dark, Colors.Maroon);
			});
		}

		Assert.Same(previousApplication, Application.Current);
	}
}
