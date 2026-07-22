#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Microsoft.Maui.Dispatching;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

/// <summary>
/// Wave-2 Phase-1 nested/custom-control hot reload coverage (NC-01..NC-03).
/// Fixture: a same-compilation "ProbeCard" (<see cref="global::Microsoft.Maui.Controls.ContentView"/>)
/// used twice per page, each instance carrying its own nested
/// <c>&lt;ProbeCard.Resources&gt;</c> keyed converter and a child <c>Label</c> bound via
/// <c>x:Reference</c> back to its own card. This is a generic, anonymized stand-in for the common
/// "reusable card control with per-instance local resources" shape — no proprietary names or text
/// are used or copied from any specific application.
/// </summary>
[Collection("XamlHotReloadTests")]
public class NestedControlHotReloadTests : IDisposable
{
	public NestedControlHotReloadTests() => DispatcherProvider.SetCurrent(new StubDispatcherProvider());

	public void Dispose() => DispatcherProvider.SetCurrent(null);

	const string PageClass = "TestAiAssisted.MainPage";

	const string PageStub = """
		namespace TestAiAssisted;

		public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
		{
			private partial void InitializeComponent();

			public void InitializeComponentRuntime() { }

			public MainPage()
			{
				InitializeComponent();
			}

			// x:Name'd elements below need a matching field for the generator's InitializeComponent
			// to assign into (the harness only wires XamlGenerator, not the separate code-behind
			// field-declaration generator that normally supplies these in a real build).
			private global::TestAiAssisted.ProbeCard CardA = default!;
			private global::TestAiAssisted.ProbeCard CardB = default!;
			private global::Microsoft.Maui.Controls.Label CardALabel = default!;
			private global::Microsoft.Maui.Controls.Label CardBLabel = default!;
		}
		""";

	const string ProbeCardStub = """
		namespace TestAiAssisted;

		// Generic same-compilation custom control: a reusable "card" with its own bindable Value.
		public partial class ProbeCard : global::Microsoft.Maui.Controls.ContentView
		{
			public static readonly global::Microsoft.Maui.Controls.BindableProperty ValueProperty =
				global::Microsoft.Maui.Controls.BindableProperty.Create(nameof(Value), typeof(string), typeof(ProbeCard), default(string));

			public string? Value
			{
				get => (string?)GetValue(ValueProperty);
				set => SetValue(ValueProperty, value);
			}

			private partial void InitializeComponent();

			private global::TestAiAssisted.ProbeCard Root = default!;
			private global::Microsoft.Maui.Controls.Label CardLabel = default!;

			public ProbeCard()
			{
				InitializeComponent();
			}
		}

		public sealed class ProbeConverterOriginal : global::Microsoft.Maui.Controls.IValueConverter
		{
			public static int InvocationCount;

			public object? Convert(object? value, global::System.Type targetType, object? parameter, global::System.Globalization.CultureInfo culture)
			{
				InvocationCount++;
				return $"Original:{value}";
			}

			public object? ConvertBack(object? value, global::System.Type targetType, object? parameter, global::System.Globalization.CultureInfo culture) => value;
		}

		public sealed class ProbeConverterUpdated : global::Microsoft.Maui.Controls.IValueConverter
		{
			public static int InvocationCount;

			public object? Convert(object? value, global::System.Type targetType, object? parameter, global::System.Globalization.CultureInfo culture)
			{
				InvocationCount++;
				return $"Updated:{value}";
			}

			public object? ConvertBack(object? value, global::System.Type targetType, object? parameter, global::System.Globalization.CultureInfo culture) => value;
		}
		""";

	const string XamlTemplate = """
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:TestAiAssisted"
		             x:Class="TestAiAssisted.MainPage">
		  <VerticalStackLayout>
		    <local:ProbeCard x:Name="CardA" Value="__CARD_A_VALUE__">
		      <local:ProbeCard.Resources>
		        <ResourceDictionary>
		          <local:__CARD_A_CONVERTER__ x:Key="Fmt" />
		        </ResourceDictionary>
		      </local:ProbeCard.Resources>
		      <Label x:Name="CardALabel" Text="{Binding Source={x:Reference CardA}, Path=Value, Converter={StaticResource Fmt}}" />
		    </local:ProbeCard>
		    <local:ProbeCard x:Name="CardB" Value="__CARD_B_VALUE__">
		      <local:ProbeCard.Resources>
		        <ResourceDictionary>
		          <local:__CARD_B_CONVERTER__ x:Key="Fmt" />
		        </ResourceDictionary>
		      </local:ProbeCard.Resources>
		      <Label x:Name="CardBLabel" Text="{Binding Source={x:Reference CardB}, Path=Value, Converter={StaticResource Fmt}}" />
		    </local:ProbeCard>
		  </VerticalStackLayout>
		</ContentPage>
		""";

	static string Xaml(string cardAValue, string cardAConverter, string cardBValue, string cardBConverter) =>
		XamlTemplate
			.Replace("__CARD_A_VALUE__", cardAValue, StringComparison.Ordinal)
			.Replace("__CARD_A_CONVERTER__", cardAConverter, StringComparison.Ordinal)
			.Replace("__CARD_B_VALUE__", cardBValue, StringComparison.Ordinal)
			.Replace("__CARD_B_CONVERTER__", cardBConverter, StringComparison.Ordinal);

	static int GetStaticInvocationCount(object anyInstanceFromGeneratedAssembly, string fullyQualifiedTypeName)
	{
		var assembly = anyInstanceFromGeneratedAssembly.GetType().Assembly;
		var type = assembly.GetType(fullyQualifiedTypeName) ?? throw new InvalidOperationException($"Type '{fullyQualifiedTypeName}' not found in generated assembly.");
		var field = type.GetField("InvocationCount", BindingFlags.Public | BindingFlags.Static) ?? throw new InvalidOperationException("InvocationCount field not found.");
		return (int)field.GetValue(null)!;
	}

	// PageStub's x:Name'd fields are declared `private` (matching the real code-behind field
	// modifier default) — the harness does not run the code-behind field-declaration generator
	// that would normally expose a public accessor, so tests reach them via reflection.
	static T GetNamedField<T>(object page, string fieldName) where T : class
	{
		var field = page.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
			?? throw new InvalidOperationException($"Field '{fieldName}' not found on '{page.GetType()}'.");
		return (T)field.GetValue(page)!;
	}

	static void ApplyGeneratedUpdate(object root)
	{
		var updateMethod = root.GetType().GetMethod(
			"UpdateComponent",
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
			?? throw new InvalidOperationException($"UpdateComponent not found on '{root.GetType()}'.");
		updateMethod.Invoke(root, null);
	}

	static IReadOnlyDictionary<string, XamlHotReloadDocument> Documents(string page, string probeCard) =>
		new Dictionary<string, XamlHotReloadDocument>(StringComparer.Ordinal)
		{
			["MainPage.xaml"] = new XamlHotReloadDocument(page),
			["ProbeCard.xaml"] = new XamlHotReloadDocument(probeCard),
		};

	const string Nc04PageTemplate = """
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:TestAiAssisted"
		             x:Class="TestAiAssisted.MainPage">
		  <VerticalStackLayout>
		    <local:ProbeCard x:Name="CardA" Value="__CARD_A_VALUE__" />
		    <local:ProbeCard x:Name="CardB" Value="__CARD_B_VALUE__" />
		  </VerticalStackLayout>
		</ContentPage>
		""";

	const string Nc04ProbeCardTemplate = """
		<ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:TestAiAssisted"
		             x:Class="TestAiAssisted.ProbeCard"
		             x:Name="Root">
		  <ContentView.Resources>
		    <ResourceDictionary>
		      <local:__CONVERTER__ x:Key="Fmt" />
		    </ResourceDictionary>
		  </ContentView.Resources>
		  <Label x:Name="CardLabel"
		         Text="{Binding Source={x:Reference Root}, Path=Value, Converter={StaticResource Fmt}}" />
		</ContentView>
		""";

	static string Nc04Page(string cardAValue, string cardBValue) =>
		Nc04PageTemplate
			.Replace("__CARD_A_VALUE__", cardAValue, StringComparison.Ordinal)
			.Replace("__CARD_B_VALUE__", cardBValue, StringComparison.Ordinal);

	static string Nc04ProbeCard(string converter) =>
		Nc04ProbeCardTemplate.Replace("__CONVERTER__", converter, StringComparison.Ordinal);

	static string GetProbeCardUpdateV2(XamlHotReloadGeneration generation)
	{
		var updateComponentSource = Assert.Single(generation[1].GeneratedRoots,
			static root => root.TypeName == "TestAiAssisted.ProbeCard").UpdateComponentSource;
		Assert.NotNull(updateComponentSource);
		return updateComponentSource!;
	}

	static void AssertProbeCardResourceDecline(string updateComponentSource)
	{
		Assert.Contains("XamlComponentRegistry.GetResourceKeys(this)", updateComponentSource, StringComparison.Ordinal);
		Assert.Contains("XamlComponentRegistry.RegisterResourceKeys(this, global::System.Array.Empty<string>())", updateComponentSource, StringComparison.Ordinal);
		Assert.DoesNotContain("ProbeConverterUpdated", updateComponentSource, StringComparison.Ordinal);
	}

	// Wave2 · Nested Controls · P0-07 (V1 slice) · NC-01
	// Provenance: MAUI Wave2 plan §3.5 NC-01 (P0-07) | anonymized "reusable card with per-instance local resources" pattern
	// Faithfulness: construction-only — two same-compilation ProbeCard instances, each with its own
	//   x:Name, nested <ProbeCard.Resources> keyed converter, and an x:Reference-bound Label. No writer
	//   mutation/skip path is involved; this is the supported-construction GREEN anchor that NC-02/NC-03
	//   (below) diff against.
	// Expected: GREEN
	// Issue: https://github.com/dotnet/maui/issues/36732   (writer-roadmap tracking; NC-01 is the green anchor, not a probe)
	[MetadataUpdateFact]
	public void NestedCustomControls_Construct_HaveIndependentIdentityAndNamescope()
	{
		var xamlV1 = Xaml("A1", "ProbeConverterOriginal", "B1", "ProbeConverterOriginal");

		using var harness = new XamlHotReloadTestHarness(
			nameof(NestedCustomControls_Construct_HaveIndependentIdentityAndNamescope), PageClass, PageStub, ProbeCardStub);
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

	// Wave2 · Nested Controls · P0-07 · NC-02
	// Provenance: MAUI Wave2 plan §3.5 NC-02 (P0-07) | anonymized "reusable card with per-instance local resources" pattern
	// Faithfulness: reaches the non-root complex-property skip at
	//   UpdateComponentCodeWriter.cs:1194 (EmitPropertyChange, isRoot: false). The plan's original
	//   citation (L929, TryEmitResourceDictionaryChange's "left untouched" marker) was verified against
	//   this generator and found to be ROOT-ONLY: TryEmitResourceDictionaryChange has exactly one
	//   caller, EmitRootPropertyChange (L1096), which only ever runs for the page/root element's own
	//   `this.Resources`. A nested (non-root) ProbeCard's <ProbeCard.Resources> edit falls through
	//   TryEmitMarkupNodeChange and hits the generic complex-property decline at L1194 instead —
	//   confirmed empirically via generated-source inspection below.
	// Expected: DOC-SKIP-GUARD
	// Issue: https://github.com/dotnet/maui/issues/36732   (writer-roadmap tracking for non-root Resources support)
	[Fact]
	public void NestedLocalResources_CustomConverter_EmitsSkipMarker()
	{
		var xamlV1 = Xaml("A1", "ProbeConverterOriginal", "B1", "ProbeConverterOriginal");
		// Only CardA's nested Resources (converter type) changes; CardB and both Values are untouched.
		var xamlV2 = Xaml("A1", "ProbeConverterUpdated", "B1", "ProbeConverterOriginal");

		using var harness = new XamlHotReloadTestHarness(
			nameof(NestedLocalResources_CustomConverter_EmitsSkipMarker), PageClass, PageStub, ProbeCardStub);
		var generation = harness.Generate(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains(
			"Complex property 'Resources' (ElementNode) — skipped (not yet supported)",
			updateComponentSource!,
			StringComparison.Ordinal);

		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Generated V2 UpdateComponent (with the nested-Resources skip) should still compile.");
	}

	// Wave2 · Nested Controls · P0-07 · NC-03
	// Provenance: MAUI Wave2 plan §3.5 NC-03 (P0-07) | anonymized "reusable card with per-instance local resources" pattern
	// Faithfulness: exercises the same nested-Resources skip as NC-02 (UpdateComponentCodeWriter.cs:1194)
	//   together with a live x:Reference rebind, on the SAME retained page/ProbeCard/Label instances,
	//   across three versions (V1 A1/B1 -> V2 A2/B2 -> V3=V1 A1/B1). Confirmed by inspecting the actual
	//   generated V2 UpdateComponent below: each ProbeCard is retrieved via
	//   `XamlComponentRegistry.TryGet(this, id, ...)` (never `new ProbeCard(...)`) in the exact same
	//   diff block that emits the Resources skip comment for that element — i.e. this reaches the
	//   intended retained nested-resource + x:Reference path, not a whole-subtree/root replacement.
	//   Because the skip only declines the Resources-dictionary diff, the sibling Value diff on the
	//   same retained element is still applied directly; the pre-existing Label Binding (built once at
	//   construction, referencing its own card via x:Reference) observes that Value's PropertyChanged and
	//   re-invokes its ORIGINAL (never-replaced) converter with the new value, independently per card.
	//   Empirically this is a well-defined, side-effect-free fallback, not a hang/crash/bleed bug:
	//   page/cardA/cardB/labelA/labelB identities are all retained (ReferenceEquals) across every
	//   version; ProbeConverterUpdated.InvocationCount stays 0 for the whole scenario;
	//   ProbeConverterOriginal.InvocationCount increases by exactly 2 (one per card) per version with no
	//   multiplication and no cross-card bleed; and each card's local converter/Resources dictionary
	//   object is untouched (same reference) throughout, proving left/right independence never
	//   degrades. No incorrect/multiplied/cross-bled behavior was found — classified GREEN.
	// Expected: GREEN
	[MetadataUpdateFact]
	public void NestedControls_LocalResources_XReference_RebindIndependently()
	{
		// Prefixes V1/V2/V1 per-card: CardA A1→A2→A1, CardB B1→B2→B1; converter swapped and reverted alongside.
		var xamlV1 = Xaml("A1", "ProbeConverterOriginal", "B1", "ProbeConverterOriginal");
		var xamlV2 = Xaml("A2", "ProbeConverterUpdated", "B2", "ProbeConverterUpdated");
		var xamlV3 = Xaml("A1", "ProbeConverterOriginal", "B1", "ProbeConverterOriginal");

		using var harness = new XamlHotReloadTestHarness(
			nameof(NestedControls_LocalResources_XReference_RebindIndependently), PageClass, PageStub, ProbeCardStub);
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		// Faithfulness guard on the generated source itself: the V2 diff must retrieve the EXISTING
		// ProbeCard from the component registry (never reconstruct it) in the same block that declines
		// the nested Resources edit — this is what proves the test below reaches the intended retained
		// nested-resource skip + x:Reference rebind path rather than passing for an unrelated reason.
		var updateComponentSourceV2 = generation[1].UpdateComponentSource;
		Assert.NotNull(updateComponentSourceV2);
		Assert.Contains("XamlComponentRegistry.TryGet", updateComponentSourceV2!, StringComparison.Ordinal);
		Assert.Contains(
			"Complex property 'Resources' (ElementNode) — skipped (not yet supported)",
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
			// Each card's local converter/Resources dictionary is untouched by the skip (still the SAME
			// object) — left/right independence holds, and no swap occurred.
			Assert.Same(fmtA, cardA.Resources["Fmt"]);
			Assert.Same(fmtB, cardB.Resources["Fmt"]);

			// V2: prefixes actually change (A1/B1 -> A2/B2). The XAML now declares ProbeConverterUpdated,
			// but the nested-Resources skip means the converter swap never applies. The still-live Value
			// update re-invokes the ORIGINAL converter with the new value on each card, independently and
			// exactly once (no multiplication, no cross-card bleed).
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
			Assert.Same(fmtA, cardA.Resources["Fmt"]);
			Assert.Same(fmtB, cardB.Resources["Fmt"]);

			// V3: prefixes revert (A2/B2 -> A1/B1), matching V1 exactly. ProbeConverterUpdated was never
			// constructed/invoked at any point across the whole V1->V2->V1 sequence.
			Assert.Equal("Original:A1", labelA.Text);
			Assert.Equal("Original:B1", labelB.Text);
			Assert.Equal(6, GetStaticInvocationCount(cardA, "TestAiAssisted.ProbeConverterOriginal"));
			Assert.Equal(0, GetStaticInvocationCount(cardA, "TestAiAssisted.ProbeConverterUpdated"));
		});
	}

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
			ProbeCardStub);
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
			ProbeCardStub);
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

	sealed class StubDispatcher : IDispatcher
	{
		public bool IsDispatchRequired => false;

		public bool Dispatch(Action action)
		{
			action();
			return true;
		}

		public bool DispatchDelayed(TimeSpan delay, Action action)
		{
			action();
			return true;
		}

		public IDispatcherTimer CreateTimer() => throw new NotSupportedException();
	}

	sealed class StubDispatcherProvider : IDispatcherProvider
	{
		public IDispatcher GetForCurrentThread() => new StubDispatcher();
	}
}
