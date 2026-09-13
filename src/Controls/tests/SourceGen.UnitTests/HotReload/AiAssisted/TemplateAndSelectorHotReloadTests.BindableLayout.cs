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
}
