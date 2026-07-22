#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

public partial class TemplateAndSelectorHotReloadTests
{
	// Wave-2 · Templates · TS-01 (GREEN anchor)
	// Provenance: MAUI §hot-reload DataTemplate realization; SetPropertiesVisitor.cs L245-296 (#36482);
	//             UpdateComponent resource-replacement verified by dumping the generated V2 source.
	// Faithfulness: reaches CreateContent (construction) + the live update; asserts construction correctness,
	//               already-realized stability, and that the generated source carries the edit; fails-for-bug:
	//               a body edit corrupts an already-realized subtree, or the writer stops emitting the new
	//               factory / stops replacing the keyed entry.
	// Expected: GREEN
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact]
	public void KeyedDataTemplate_EditBody_ConstructionStableAndSourceReflectsEdit()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(KeyedDataTemplateXaml("Alpha"), KeyedDataTemplateXaml("Bravo"));

		// Generated-source oracle: the new factory lands in InitializeComponent, and UpdateComponent replaces
		// the keyed entry with a bare template (the reason future realization is reclassified below).
		Assert.NotNull(generation[1].InitializeComponentSource);
		Assert.Contains("Bravo", generation[1].InitializeComponentSource!, StringComparison.Ordinal);
		Assert.NotNull(generation[1].UpdateComponentSource);
		Assert.Contains(
			"this.Resources[\"Row\"] = new global::Microsoft.Maui.Controls.DataTemplate();",
			generation[1].UpdateComponentSource!,
			StringComparison.Ordinal);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var template = Assert.IsType<DataTemplate>(page.Resources["Row"]);

			// Construction realization is correct.
			var r1 = Assert.IsType<Label>(template.CreateContent());
			Assert.Equal("Alpha", r1.Text);

			// The update applies cleanly and the already-realized subtree is left intact.
			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Equal("Alpha", r1.Text);
		});
	}

	// Wave-2 · Templates · TS-01 (RED-PROBE — reclassified future realization)
	// Provenance: keyed template resources are replaced by a factory-less `new DataTemplate()` in
	//             UpdateComponent (dumped), so a held template's next CreateContent() serves the stale factory.
	// Faithfulness: reaches the post-update CreateContent(); fails-for-bug: future realization does not reflect
	//               the edited body (observed "Alpha" instead of "Bravo").
	// Expected: RED-PROBE(#36482) — green anchor: KeyedDataTemplate_EditBody_ConstructionStableAndSourceReflectsEdit
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact(Skip = "Keyed DataTemplate future realization does not reflect the edit under IHR (writer replaces the entry with a factory-less new DataTemplate) — see #36482; green anchor: KeyedDataTemplate_EditBody_ConstructionStableAndSourceReflectsEdit")]
	public void KeyedDataTemplate_EditBody_FutureRealizationReflectsNewFactory()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(KeyedDataTemplateXaml("Alpha"), KeyedDataTemplateXaml("Bravo"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var template = Assert.IsType<DataTemplate>(page.Resources["Row"]);

			var r1 = Assert.IsType<Label>(template.CreateContent());
			Assert.Equal("Alpha", r1.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Equal("Alpha", r1.Text); // already-realized subtree stays on V1 (correct)

			// The intended-but-not-yet-delivered behavior: the next realization should reflect the edit.
			var r2 = Assert.IsType<Label>(template.CreateContent());
			Assert.Equal("Bravo", r2.Text);
			Assert.NotSame(r1, r2);
		});
	}

	// Wave-2 · Selectors · TS-02 (GREEN anchor)
	// Provenance: MAUI §hot-reload DataTemplateSelector realization; SetPropertiesVisitor.cs L245-296 (#36482)
	// Faithfulness: reaches SelectTemplate(...).CreateContent(); asserts the selector routes odd/even to
	//               distinct branch templates, existing realizations stay put across an update, and the edit
	//               reaches the generated source; fails-for-bug: a branch returns null/bare, both branches
	//               collapse to one template, or the writer drops the new odd-branch factory.
	// Expected: GREEN
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact]
	public void Selector_ConstructionDistinguishesBranchesAndIsStableAcrossUpdate()
	{
		using var harness = CreateHarness(additionalSources: SelectorSource);
		var generation = harness.Generate(SelectorXaml("OddB1"), SelectorXaml("OddB2"));

		Assert.NotNull(generation[1].InitializeComponentSource);
		Assert.Contains("OddB2", generation[1].InitializeComponentSource!, StringComparison.Ordinal);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var selector = Assert.IsAssignableFrom<DataTemplateSelector>(page.Resources["Sel"]);

			// Odd vs even resolve to DISTINCT branch templates, each producing its own leaf.
			var oddTemplate = selector.SelectTemplate(1, page);
			var evenTemplate = selector.SelectTemplate(2, page);
			Assert.NotNull(oddTemplate);
			Assert.NotNull(evenTemplate);
			Assert.NotSame(oddTemplate, evenTemplate);

			var odd = Assert.IsType<Label>(oddTemplate!.CreateContent());
			var even = Assert.IsType<Label>(evenTemplate!.CreateContent());
			Assert.Equal("OddB1", odd.Text);
			Assert.Equal("EvenA", even.Text);

			// The update applies cleanly; the already-realized odd subtree is unchanged.
			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Equal("OddB1", odd.Text);
		});
	}

	// Wave-2 · Selectors · TS-02 (RED-PROBE — reclassified future realization)
	// Provenance: the selector's keyed branch templates keep their construction-time factories after an edit
	//             (same keyed-resource replacement gap as TS-01).
	// Faithfulness: reaches the post-update SelectTemplate(...).CreateContent(); fails-for-bug: a future odd
	//               selection still yields the old leaf ("OddB1" instead of "OddB2").
	// Expected: RED-PROBE(#36482) — green anchor: Selector_ConstructionDistinguishesBranchesAndIsStableAcrossUpdate
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact(Skip = "DataTemplateSelector future realization does not reflect the edited odd-branch factory under IHR — see #36482; green anchor: Selector_ConstructionDistinguishesBranchesAndIsStableAcrossUpdate")]
	public void Selector_FutureRealizationReflectsNewFactory()
	{
		using var harness = CreateHarness(additionalSources: SelectorSource);
		var generation = harness.Generate(SelectorXaml("OddB1"), SelectorXaml("OddB2"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var selector = Assert.IsAssignableFrom<DataTemplateSelector>(page.Resources["Sel"]);

			var r1 = Assert.IsType<Label>(selector.SelectTemplate(1, page)!.CreateContent());
			Assert.Equal("OddB1", r1.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Equal("OddB1", r1.Text);

			var r2 = Assert.IsType<Label>(selector.SelectTemplate(1, page)!.CreateContent());
			Assert.Equal("OddB2", r2.Text); // future odd realization should be V2
		});
	}

	// Wave-2 · Templates · TS-03 (GREEN anchor)
	// Provenance: MAUI §hot-reload compiled-binding retype; SetPropertiesVisitor.cs L245-296 + compiled-binding
	//             getter (CreateTypedBindingFrom_*, #36482).
	// Faithfulness: reaches the generated InitializeComponent source across a retype+reverse AND the V1
	//               compiled-binding realization; asserts the emitted accessor tracks the current x:DataType
	//               (retype and reverse) and construction binds correctly; fails-for-bug: a retyped x:DataType
	//               keeps emitting the stale accessor, or the compiled binding fails at construction.
	// Expected: GREEN
	// Issue: https://github.com/dotnet/maui/issues/36482
	[Theory]
	[InlineData("local:ItemA", "local:ItemB")]
	[InlineData("{x:Type local:ItemA}", "{x:Type local:ItemB}")]
	public void CompiledTemplate_RetypeAndReverse_GeneratedSourceTracksCurrentType(string dataTypeA, string dataTypeB)
	{
		var xamlV1 = CompiledTemplateXaml(dataTypeA, "Caption");
		var xamlV2 = CompiledTemplateXaml(dataTypeB, "Heading");
		var xamlV3 = CompiledTemplateXaml(dataTypeA, "Caption");

		using var harness = CreateHarness(additionalSources: ItemTypesSource);
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		// V2 emits the new type's accessor and not the stale one; V3 reverses it back.
		var v2 = generation[1].InitializeComponentSource;
		var v3 = generation[2].InitializeComponentSource;
		Assert.NotNull(v2);
		Assert.NotNull(v3);
		Assert.Contains("Heading", v2!, StringComparison.Ordinal);
		Assert.Contains("ItemB", v2!, StringComparison.Ordinal);
		Assert.DoesNotContain("Caption", v2!, StringComparison.Ordinal);
		Assert.Contains("Caption", v3!, StringComparison.Ordinal);
		Assert.Contains("ItemA", v3!, StringComparison.Ordinal);
		Assert.DoesNotContain("Heading", v3!, StringComparison.Ordinal);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var template = Assert.IsType<DataTemplate>(page.Resources["Row"]);

			// V1 compiled binding realizes and binds against ItemA.Caption.
			var r1 = Assert.IsType<Label>(template.CreateContent());
			r1.BindingContext = MakeLiveItem(page, "TestTemplates.ItemA", "Caption", "alpha");
			Assert.Equal("alpha", r1.Text);

			// The update applies cleanly; the already-realized binding is unchanged.
			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Equal("alpha", r1.Text);
		});
	}

	// Wave-2 · Templates · TS-03 (RED-PROBE — reclassified future realization)
	// Provenance: the retyped compiled-binding getter lives in a keyed template whose entry is replaced by a
	//             factory-less `new DataTemplate()` on update (dumped), so a held template keeps the V1 getter.
	// Faithfulness: reaches the post-update CreateContent() + a live ItemB BindingContext; fails-for-bug: the
	//               future realization still binds the old type, yielding null instead of "bravo".
	// Expected: RED-PROBE(#36482) — green anchor: CompiledTemplate_RetypeAndReverse_GeneratedSourceTracksCurrentType
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact(Skip = "Compiled-binding retype does not reach future realizations of a keyed DataTemplate under IHR — see #36482; green anchor: CompiledTemplate_RetypeAndReverse_GeneratedSourceTracksCurrentType")]
	public void CompiledTemplate_Retype_FutureRealizationBindsNewType()
	{
		using var harness = CreateHarness(additionalSources: ItemTypesSource);
		var generation = harness.Generate(
			CompiledTemplateXaml("local:ItemA", "Caption"),
			CompiledTemplateXaml("local:ItemB", "Heading"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var template = Assert.IsType<DataTemplate>(page.Resources["Row"]);

			var r1 = Assert.IsType<Label>(template.CreateContent());
			r1.BindingContext = MakeLiveItem(page, "TestTemplates.ItemA", "Caption", "alpha");
			Assert.Equal("alpha", r1.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));

			var r2 = Assert.IsType<Label>(template.CreateContent());
			r2.BindingContext = MakeLiveItem(page, "TestTemplates.ItemB", "Heading", "bravo");
			Assert.Equal("bravo", r2.Text); // future realization should bind ItemB.Heading
		});
	}
}
