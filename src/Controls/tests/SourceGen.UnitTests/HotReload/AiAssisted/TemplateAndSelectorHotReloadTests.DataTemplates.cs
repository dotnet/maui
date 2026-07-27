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
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact]
	public void KeyedDataTemplate_EditBody_ConstructionStableAndSourceReflectsEdit()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(KeyedDataTemplateXaml("Alpha"), KeyedDataTemplateXaml("Bravo"));

		// Generated-source oracle: the new factory lands in InitializeComponent, and UpdateComponent replaces
		// the keyed entry with a bare template (the reason future realization stays deferred under #36482).
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

	// Wave-2 · Selectors · TS-02 (GREEN anchor)
	// Provenance: MAUI §hot-reload DataTemplateSelector realization; SetPropertiesVisitor.cs L245-296 (#36482)
	// Faithfulness: reaches SelectTemplate(...).CreateContent(); asserts the selector routes odd/even to
	//               distinct branch templates, existing realizations stay put across an update, and the edit
	//               reaches the generated source; fails-for-bug: a branch returns null/bare, both branches
	//               collapse to one template, or the writer drops the new odd-branch factory.
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
	// Wave-2 · Templates · TS-03 (GREEN anchor)
	// Provenance: MAUI §hot-reload compiled-binding retype; SetPropertiesVisitor.cs L245-296 + compiled-binding
	//             getter (CreateTypedBindingFrom_*, #36482).
	// Faithfulness: reaches the generated InitializeComponent source across a retype+reverse AND the V1
	//               compiled-binding realization; asserts the emitted accessor tracks the current x:DataType
	//               (retype and reverse) and construction binds correctly; fails-for-bug: a retyped x:DataType
	//               keeps emitting the stale accessor, or the compiled binding fails at construction.
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateTheory]
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
}
