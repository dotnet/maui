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
	// Wave2 · Nested Controls · P0-07 · NC-04
	// Provenance: MAUI Wave2 plan §3.5 NC-04 (P0-07)
	// Expected: GREEN
	[Fact]
	public void NestedGeneratedRoots_LocalResources_EmitsDocumentedResourceDecline()
	{
		using var harness = new XamlHotReloadTestHarness(
			nameof(NestedGeneratedRoots_LocalResources_EmitsDocumentedResourceDecline),
			PageClass,
			PageStub,
			GeneratedProbeCardStub,
			ProbeConverterStubs);
		var generation = harness.GenerateDocuments(
			Documents(Nc04Page("A1", "B1"), Nc04ProbeCard("ProbeConverterOriginal")),
			Documents(Nc04Page("A2", "B2"), Nc04ProbeCard("ProbeConverterUpdated")));

		AssertProbeCardResourceDecline(GetProbeCardUpdateV2(generation));
		Assert.True(harness.Compile(generation[1]).PeImage.Length > 0);
	}

	// Faithfulness: ProbeCard is a separately generated XAML root, not a same-assembly C# stand-in.
	// The page creates two cards, and the live session creates two retained pages, so each update
	// must reach four independent ProbeCard roots. The harness applies deltas to page roots; the
	// test explicitly dispatches the generated ProbeCard UpdateComponent once per retained card,
	// matching the registry-root dispatch that a host must perform for separately generated roots.
	// Expected: DOC-SKIP-GUARD
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "Issue #36732: generated root Resources updates remove registered keys but do not reconstruct the V2 dictionary, so retained x:Reference bindings keep the V1 converter.")]
	public void NestedGeneratedRoots_LocalResources_XReference_RetainedInstancesAndFreshInstanceStayIndependent()
	{
		using var harness = new XamlHotReloadTestHarness(
			nameof(NestedGeneratedRoots_LocalResources_XReference_RetainedInstancesAndFreshInstanceStayIndependent),
			PageClass,
			PageStub,
			GeneratedProbeCardStub,
			ProbeConverterStubs);
		var generation = harness.GenerateDocuments(
			Documents(Nc04Page("A1", "B1"), Nc04ProbeCard("ProbeConverterOriginal")),
			Documents(Nc04Page("A2", "B2"), Nc04ProbeCard("ProbeConverterUpdated")),
			Documents(Nc04Page("A1", "B1"), Nc04ProbeCard("ProbeConverterOriginal")));

		// Temporarily unskipped and confirmed: the root-Resources path removes the V1 registered
		// key then registers an empty key set without constructing ProbeConverterUpdated. That leaves
		// retained StaticResource bindings attached to their V1 converter; it is the #36732 decline.
		AssertProbeCardResourceDecline(GetProbeCardUpdateV2(generation));

		harness.RunLive(generation, live =>
		{
			var firstPage = live.GetInstance<global::Microsoft.Maui.Controls.ContentPage>();
			var secondPage = live.CreateInstance<global::Microsoft.Maui.Controls.ContentPage>();
			var firstCardA = GetNamedField<global::Microsoft.Maui.Controls.ContentView>(firstPage, "CardA");
			var firstCardB = GetNamedField<global::Microsoft.Maui.Controls.ContentView>(firstPage, "CardB");
			var secondCardA = GetNamedField<global::Microsoft.Maui.Controls.ContentView>(secondPage, "CardA");
			var secondCardB = GetNamedField<global::Microsoft.Maui.Controls.ContentView>(secondPage, "CardB");
			var cards = new[] { firstCardA, firstCardB, secondCardA, secondCardB };
			var labels = Array.ConvertAll(cards, static card => Assert.IsType<global::Microsoft.Maui.Controls.Label>(card.Content));

			Assert.NotSame(firstCardA, secondCardA);
			Assert.NotSame(firstCardB, secondCardB);
			Assert.NotSame(firstCardA.Resources, secondCardA.Resources);
			Assert.NotSame(firstCardB.Resources, secondCardB.Resources);
			Assert.NotSame(firstCardA.Resources["Fmt"], secondCardA.Resources["Fmt"]);
			Assert.NotSame(firstCardB.Resources["Fmt"], secondCardB.Resources["Fmt"]);
			Assert.Equal(new[] { "Original:A1", "Original:B1", "Original:A1", "Original:B1" },
				Array.ConvertAll(labels, static label => label.Text));
			// The separate generated root observes its default Value and then the page's assigned
			// Value during construction, yielding two initial conversions per retained card.
			Assert.Equal(8, GetStaticInvocationCount(firstPage, "TestAiAssisted.ProbeConverterOriginal"));
			Assert.Equal(0, GetStaticInvocationCount(firstPage, "TestAiAssisted.ProbeConverterUpdated"));

			live.ApplyUpdate<global::Microsoft.Maui.Controls.ContentPage>(1);
			foreach (var card in cards)
				ApplyGeneratedUpdate(card);

			Assert.Same(firstPage, live.GetInstance<global::Microsoft.Maui.Controls.ContentPage>());
			Assert.Same(firstCardA, GetNamedField<global::Microsoft.Maui.Controls.ContentView>(firstPage, "CardA"));
			Assert.Same(secondCardB, GetNamedField<global::Microsoft.Maui.Controls.ContentView>(secondPage, "CardB"));
			Assert.Equal(new[] { "Updated:A2", "Updated:B2", "Updated:A2", "Updated:B2" },
				Array.ConvertAll(labels, static label => label.Text));
			Assert.Equal(12, GetStaticInvocationCount(firstPage, "TestAiAssisted.ProbeConverterOriginal"));
			Assert.Equal(4, GetStaticInvocationCount(firstPage, "TestAiAssisted.ProbeConverterUpdated"));

			var freshPage = live.CreateInstance<global::Microsoft.Maui.Controls.ContentPage>();
			var freshCardA = GetNamedField<global::Microsoft.Maui.Controls.ContentView>(freshPage, "CardA");
			var freshCardB = GetNamedField<global::Microsoft.Maui.Controls.ContentView>(freshPage, "CardB");
			Assert.Equal("Updated:A2", Assert.IsType<global::Microsoft.Maui.Controls.Label>(freshCardA.Content).Text);
			Assert.Equal("Updated:B2", Assert.IsType<global::Microsoft.Maui.Controls.Label>(freshCardB.Content).Text);
			Assert.Equal(8, GetStaticInvocationCount(firstPage, "TestAiAssisted.ProbeConverterUpdated"));

			live.ApplyUpdate<global::Microsoft.Maui.Controls.ContentPage>(2);
			foreach (var card in cards)
				ApplyGeneratedUpdate(card);

			Assert.Equal(new[] { "Original:A1", "Original:B1", "Original:A1", "Original:B1" },
				Array.ConvertAll(labels, static label => label.Text));
			Assert.Equal(16, GetStaticInvocationCount(firstPage, "TestAiAssisted.ProbeConverterOriginal"));
			Assert.Equal(14, GetStaticInvocationCount(firstPage, "TestAiAssisted.ProbeConverterUpdated"));
		});
	}
}
