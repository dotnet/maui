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
	// Wave2 · Behavior · P0-04 · VS-03
	// Provenance: MAUI §3.4; toolkit T13/T15/T17; public-app T17
	// Faithfulness: reaches writer L1167/L1194; fails-for-bug: Behavior re-add is explicitly declined.
	// Expected: DOC-SKIP-GUARD
	// Issue: https://github.com/dotnet/maui/issues/36732
	[Fact]
	public void Behavior_ClearAndComplexProperty_EmitsSkipMarker()
	{
		using var harness = CreateBehaviorHarness();
		var generation = harness.Generate(
			CreateBehaviorXaml("V1", includeBehavior: true),
			CreateBehaviorXaml("V2", includeBehavior: false),
			CreateBehaviorXaml("V3", includeBehavior: true));
		var removalSource = generation[1].UpdateComponentSource;
		var reAddSource = generation[2].UpdateComponentSource;

		Assert.NotNull(removalSource);
		Assert.Contains(
			"RemoveBinding(global::Microsoft.Maui.Controls.Entry.BehaviorsProperty)",
			removalSource!,
			StringComparison.Ordinal);
		Assert.Contains(
			"ClearValue(global::Microsoft.Maui.Controls.Entry.BehaviorsProperty)",
			removalSource!,
			StringComparison.Ordinal);
		Assert.NotNull(reAddSource);
		Assert.Contains(
			"Complex property 'Behaviors' (ElementNode) — skipped (not yet supported)",
			reAddSource!,
			StringComparison.Ordinal);

		Assert.True(harness.Compile(generation[1]).PeImage.Length > 0, "Generated Behavior clear path should compile.");
		Assert.True(harness.Compile(generation[2]).PeImage.Length > 0, "Generated Behavior re-add skip path should compile.");
	}
}
