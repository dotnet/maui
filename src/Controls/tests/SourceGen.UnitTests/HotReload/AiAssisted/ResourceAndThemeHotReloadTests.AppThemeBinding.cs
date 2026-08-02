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
	// Dark branch is left untouched. Live re-provision of the edited AppThemeBinding remains deferred
	// under #36732 because the current UpdateComponent path supplies an IProvideValueTarget whose
	// TargetProperty is null, so product code cannot run it faithfully in this harness.
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
}
