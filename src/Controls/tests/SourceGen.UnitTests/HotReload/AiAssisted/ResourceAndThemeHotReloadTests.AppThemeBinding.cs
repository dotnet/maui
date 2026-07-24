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
}
