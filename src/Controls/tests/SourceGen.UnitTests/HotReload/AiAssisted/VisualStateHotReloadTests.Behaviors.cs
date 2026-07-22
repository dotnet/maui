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

	// Wave2 · Behavior · P0-04 · VS-04
	// Provenance: MAUI §3.4; toolkit T13/T15/T17; public-app T17
	// Faithfulness: reaches writer L1167/L1194; fails-for-bug: detach/attach lifecycle is not reconciled.
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "RED-PROBE #36732: Behavior collection lifecycle reconciliation is not implemented.")]
	public void BehaviorCollection_RemoveReAdd_DetachesAndAttachesOnce()
	{
		using var harness = CreateBehaviorHarness();
		var generation = harness.Generate(
			CreateBehaviorXaml("V1", includeBehavior: true),
			CreateBehaviorXaml("V2", includeBehavior: false),
			CreateBehaviorXaml("V3", includeBehavior: true));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var entry = Assert.IsType<Entry>(page.Content);
			var oldBehavior = Assert.Single(entry.Behaviors);

			AssertBehaviorCounts(oldBehavior, attach: 1, detach: 0, callbacks: 0);
			entry.IsEnabled = false;
			AssertBehaviorCounts(oldBehavior, attach: 1, detach: 0, callbacks: 1);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Same(entry, page.Content);
			Assert.Empty(entry.Behaviors);
			AssertBehaviorCounts(oldBehavior, attach: 1, detach: 1, callbacks: 1);
			entry.IsEnabled = true;
			AssertBehaviorCounts(oldBehavior, attach: 1, detach: 1, callbacks: 1);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
			Assert.Same(entry, page.Content);
			var newBehavior = Assert.Single(entry.Behaviors);
			Assert.NotSame(oldBehavior, newBehavior);
			AssertBehaviorCounts(newBehavior, attach: 2, detach: 1, callbacks: 1);
			entry.IsEnabled = false;
			AssertBehaviorCounts(newBehavior, attach: 2, detach: 1, callbacks: 2);
			Assert.Single(entry.Behaviors);
		});
	}
}
