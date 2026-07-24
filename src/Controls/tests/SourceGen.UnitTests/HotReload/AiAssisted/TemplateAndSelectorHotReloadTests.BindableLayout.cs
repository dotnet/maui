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
	// Wave-2 · Templates · TS-05 (GREEN guard)
	// Provenance: MAUI §hot-reload complex/attached-property gap; UpdateComponentCodeWriter.cs L1319.
	// Faithfulness: reaches the writer's attached-complex branch (L1319) and pins the exact skip marker plus a
	//               successful compile of the emitted V2 UpdateComponent.
	// Expected: DOC-SKIP-GUARD
	// Issue: https://github.com/dotnet/maui/issues/36732
	[Fact]
	public void BindableLayoutTemplate_AttachedComplex_EmitsSkipMarker()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(BindableLayoutXaml("V1"), BindableLayoutXaml("V2"));
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		// Exact marker text (em-dash U+2014) from UpdateComponentCodeWriter.cs L1319, with the attached
		// property's dotted LocalName (propDiff.PropertyName.LocalName, L1132).
		Assert.Contains("Complex attached property 'BindableLayout.ItemTemplate' — skipped", updateComponentSource!, StringComparison.Ordinal);

		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Generated V2 UpdateComponent should still compile.");
	}

	// Wave-2 · Templates · TS-06 (RED-PROBE)
	// Provenance: MAUI §hot-reload complex/collection reconcile; UpdateComponentCodeWriter attached-complex skip (L1319).
	// Faithfulness: reaches the attached-complex skip and would exercise BindableLayout's controller re-templating;
	//               fails-for-bug: retype/reverse of the item template duplicates or orphans controller children.
	// Expected: RED-PROBE(#36732) — paired green anchor: BindableLayoutTemplate_AttachedComplex_EmitsSkipMarker
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "Blocked by writer complex-property gap — see #36732; green anchor: BindableLayoutTemplate_AttachedComplex_EmitsSkipMarker")]
	public void BindableLayoutTemplate_RetypeAndReverse_DoesNotDuplicateControllerChildren()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(BindableLayoutXaml("V1"), BindableLayoutXaml("V2"), BindableLayoutXaml("V3"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var host = Assert.IsType<VerticalStackLayout>(page.Content);

			var items = new ObservableCollection<string> { "a", "b" };
			BindableLayout.SetItemsSource(host, items);
			Assert.Equal(2, host.Children.Count);

			IReadOnlyList<object> ChildSnapshot() => host.Children.ToList();
			var beforeUpdate = ChildSnapshot();

			// Retype the item template; the controller must re-template WITHOUT duplicating/orphaning children.
			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Equal(2, host.Children.Count);
			Assert.DoesNotContain(host.Children, c => beforeUpdate.Contains(c)); // no stale child left parented

			// Reverse the retype.
			Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
			Assert.Equal(2, host.Children.Count);

			// A new source item after V3 must produce exactly one additional child (no double subscription).
			items.Add("c");
			Assert.Equal(3, host.Children.Count);
		});
	}
}
