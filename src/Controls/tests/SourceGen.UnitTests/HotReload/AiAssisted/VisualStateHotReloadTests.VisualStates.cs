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

	static string[] CreateVs05StateGraphVersions() =>
	[
		CreateStateGraphXaml(includeActive: true, activeColor: "Red"),
		CreateStateGraphXaml(includeActive: false, activeColor: null),
		CreateStateGraphXaml(includeActive: true, activeColor: "Green"),
	];

	// Wave2 · Visual State · family 2 core · VS-05
	// Provenance: MAUI §2.1; public-app T13/T14; minimal add/remove/re-add extension
	// Faithfulness: reaches writer L1319 with the exact state-graph versions used by the live probe.
	// Expected: DOC-SKIP-GUARD
	// Issue: https://github.com/dotnet/maui/issues/36732
	[Fact]
	public void VsmState_AddRemoveReAdd_ComplexAttachedProperty_EmitsSkipMarker()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(CreateVs05StateGraphVersions());

		for (var index = 0; index < generation.Versions.Length; index++)
		{
			var compilation = harness.Compile(generation[index]);
			Assert.True(compilation.PeImage.Length > 0, $"Generated VS-05 version {index} should compile.");

			if (index == 0)
				continue;

			var updateComponentSource = generation[index].UpdateComponentSource;
			Assert.NotNull(updateComponentSource);
			Assert.Contains(
				"Complex attached property 'VisualStateManager.VisualStateGroups' — skipped",
				updateComponentSource!,
				StringComparison.Ordinal);
		}
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
}
