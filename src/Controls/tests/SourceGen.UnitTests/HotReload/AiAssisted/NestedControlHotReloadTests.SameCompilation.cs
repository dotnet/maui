#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Microsoft.Maui.Dispatching;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

public partial class NestedControlHotReloadTests
{
	// Wave2 · Nested Controls · P0-07 (V1 slice) · NC-01
	// Provenance: MAUI Wave2 plan §3.5 NC-01 (P0-07) | anonymized "reusable card with per-instance local resources" pattern
	// Faithfulness: construction-only — two same-compilation ProbeCard instances, each with its own
	//   x:Name, nested <ProbeCard.Resources> keyed converter, and an x:Reference-bound Label. No writer
	//   mutation/skip path is involved; this is the supported-construction GREEN anchor that NC-02/NC-03
	//   (below) diff against.
	// Issue: https://github.com/dotnet/maui/issues/36732   (writer-roadmap tracking; NC-01 is the green anchor, not a probe)
	[MetadataUpdateFact]
	public void NestedCustomControls_Construct_HaveIndependentIdentityAndNamescope()
	{
		var xamlV1 = Xaml("A1", "ProbeConverterOriginal", "B1", "ProbeConverterOriginal");

		using var harness = new XamlHotReloadTestHarness(
			nameof(NestedCustomControls_Construct_HaveIndependentIdentityAndNamescope), PageClass, PageStub, ProbeCardStub, ProbeConverterStubs);
		var generation = harness.Generate(xamlV1);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<global::Microsoft.Maui.Controls.ContentPage>();

			var cardA = GetNamedField<global::Microsoft.Maui.Controls.VisualElement>(page, "CardA");
			var cardB = GetNamedField<global::Microsoft.Maui.Controls.VisualElement>(page, "CardB");
			var labelA = GetNamedField<global::Microsoft.Maui.Controls.Label>(page, "CardALabel");
			var labelB = GetNamedField<global::Microsoft.Maui.Controls.Label>(page, "CardBLabel");

			// Independent identities: separate x:Name'd fields resolve to their own distinct instance.
			Assert.NotSame(cardA, cardB);

			// x:Reference behavior: each Label's binding resolved to its OWN card, via its own local converter.
			Assert.Equal("Original:A1", labelA.Text);
			Assert.Equal("Original:B1", labelB.Text);

			// Independent local resources: distinct dictionaries, distinct converter instances
			// (even though both cards use the same converter TYPE here).
			Assert.NotSame(cardA.Resources, cardB.Resources);
			var fmtA = cardA.Resources["Fmt"];
			var fmtB = cardB.Resources["Fmt"];
			Assert.NotSame(fmtA, fmtB);

			// Independent runtime state: mutating one card's Value never updates the other's Label.
			((dynamic)cardA).Value = "A9";
			Assert.Equal("Original:A9", labelA.Text);
			Assert.Equal("Original:B1", labelB.Text);
		});
	}

	[MetadataUpdateFact]
	public void NestedLocalResources_CustomConverter_UpdatesOnlyEditedDictionary()
	{
		var xamlV1 = Xaml("A1", "ProbeConverterOriginal", "B1", "ProbeConverterOriginal");
		// Only CardA's nested Resources (converter type) changes; CardB and both Values are untouched.
		var xamlV2 = Xaml("A1", "ProbeConverterUpdated", "B1", "ProbeConverterOriginal");

		using var harness = new XamlHotReloadTestHarness(
			nameof(NestedLocalResources_CustomConverter_UpdatesOnlyEditedDictionary), PageClass, PageStub, ProbeCardStub, ProbeConverterStubs);
		var generation = harness.Generate(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains("UpdateResourceDictionary(", updateComponentSource!, StringComparison.Ordinal);
		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<global::Microsoft.Maui.Controls.ContentPage>();
			var cardA = GetNamedField<global::Microsoft.Maui.Controls.VisualElement>(page, "CardA");
			var cardB = GetNamedField<global::Microsoft.Maui.Controls.VisualElement>(page, "CardB");
			var originalA = cardA.Resources["Fmt"];
			var originalB = cardB.Resources["Fmt"];
			live.ApplyUpdate<global::Microsoft.Maui.Controls.ContentPage>(1);
			Assert.NotSame(originalA, cardA.Resources["Fmt"]);
			Assert.Equal("ProbeConverterUpdated", cardA.Resources["Fmt"].GetType().Name);
			Assert.Same(originalB, cardB.Resources["Fmt"]);
		});
	}

	[MetadataUpdateFact]
	public void NestedControls_LocalResources_XReference_RebindIndependently()
	{
		// Prefixes V1/V2/V1 per-card: CardA A1→A2→A1, CardB B1→B2→B1; converter swapped and reverted alongside.
		var xamlV1 = Xaml("A1", "ProbeConverterOriginal", "B1", "ProbeConverterOriginal");
		var xamlV2 = Xaml("A2", "ProbeConverterUpdated", "B2", "ProbeConverterUpdated");
		var xamlV3 = Xaml("A1", "ProbeConverterOriginal", "B1", "ProbeConverterOriginal");

		using var harness = new XamlHotReloadTestHarness(
			nameof(NestedControls_LocalResources_XReference_RebindIndependently), PageClass, PageStub, ProbeCardStub, ProbeConverterStubs);
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		var updateComponentSourceV2 = generation[1].UpdateComponentSource;
		Assert.NotNull(updateComponentSourceV2);
		Assert.Contains("XamlComponentRegistry.TryGet", updateComponentSourceV2!, StringComparison.Ordinal);
		Assert.Contains(
			"UpdateResourceDictionary(",
			updateComponentSourceV2!,
			StringComparison.Ordinal);
		Assert.DoesNotContain("new global::TestAiAssisted.ProbeCard(", updateComponentSourceV2!, StringComparison.Ordinal);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<global::Microsoft.Maui.Controls.ContentPage>();
			var cardA = GetNamedField<global::Microsoft.Maui.Controls.VisualElement>(page, "CardA");
			var cardB = GetNamedField<global::Microsoft.Maui.Controls.VisualElement>(page, "CardB");
			var labelA = GetNamedField<global::Microsoft.Maui.Controls.Label>(page, "CardALabel");
			var labelB = GetNamedField<global::Microsoft.Maui.Controls.Label>(page, "CardBLabel");

			// Distinct converter instances per card from the start (independent local resources).
			var fmtA = cardA.Resources["Fmt"];
			var fmtB = cardB.Resources["Fmt"];
			Assert.NotSame(fmtA, fmtB);

			// V1: both cards use ProbeConverterOriginal, exactly once each — no cross-card bleed.
			Assert.Equal("Original:A1", labelA.Text);
			Assert.Equal("Original:B1", labelB.Text);
			Assert.Equal(2, GetStaticInvocationCount(cardA, "TestAiAssisted.ProbeConverterOriginal"));
			Assert.Equal(0, GetStaticInvocationCount(cardA, "TestAiAssisted.ProbeConverterUpdated"));

			live.ApplyUpdate<global::Microsoft.Maui.Controls.ContentPage>(1);

			// Retained-identity guard: same page and the SAME nested ProbeCard/Label objects after V2 —
			// nothing was reconstructed, only mutated in place.
			Assert.Same(page, live.GetInstance<global::Microsoft.Maui.Controls.ContentPage>());
			Assert.Same(cardA, GetNamedField<global::Microsoft.Maui.Controls.VisualElement>(page, "CardA"));
			Assert.Same(cardB, GetNamedField<global::Microsoft.Maui.Controls.VisualElement>(page, "CardB"));
			Assert.Same(labelA, GetNamedField<global::Microsoft.Maui.Controls.Label>(page, "CardALabel"));
			Assert.Same(labelB, GetNamedField<global::Microsoft.Maui.Controls.Label>(page, "CardBLabel"));
			Assert.NotSame(fmtA, cardA.Resources["Fmt"]);
			Assert.NotSame(fmtB, cardB.Resources["Fmt"]);
			Assert.Equal("ProbeConverterUpdated", cardA.Resources["Fmt"].GetType().Name);
			Assert.Equal("ProbeConverterUpdated", cardB.Resources["Fmt"].GetType().Name);

			// Existing StaticResource bindings retain their originally resolved converter.
			Assert.Equal("Original:A2", labelA.Text);
			Assert.Equal("Original:B2", labelB.Text);
			Assert.Equal(4, GetStaticInvocationCount(cardA, "TestAiAssisted.ProbeConverterOriginal"));
			Assert.Equal(0, GetStaticInvocationCount(cardA, "TestAiAssisted.ProbeConverterUpdated"));

			live.ApplyUpdate<global::Microsoft.Maui.Controls.ContentPage>(2);

			// Retained-identity guard: same objects again after V3 (revert) — still never reconstructed.
			Assert.Same(page, live.GetInstance<global::Microsoft.Maui.Controls.ContentPage>());
			Assert.Same(cardA, GetNamedField<global::Microsoft.Maui.Controls.VisualElement>(page, "CardA"));
			Assert.Same(cardB, GetNamedField<global::Microsoft.Maui.Controls.VisualElement>(page, "CardB"));
			Assert.Same(labelA, GetNamedField<global::Microsoft.Maui.Controls.Label>(page, "CardALabel"));
			Assert.Same(labelB, GetNamedField<global::Microsoft.Maui.Controls.Label>(page, "CardBLabel"));
			Assert.Equal("ProbeConverterOriginal", cardA.Resources["Fmt"].GetType().Name);
			Assert.Equal("ProbeConverterOriginal", cardB.Resources["Fmt"].GetType().Name);

			Assert.Equal("Original:A1", labelA.Text);
			Assert.Equal("Original:B1", labelB.Text);
			Assert.Equal(6, GetStaticInvocationCount(cardA, "TestAiAssisted.ProbeConverterOriginal"));
			Assert.Equal(0, GetStaticInvocationCount(cardA, "TestAiAssisted.ProbeConverterUpdated"));
		});
	}
}
