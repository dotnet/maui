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
	[Fact]
	public void NestedGeneratedRoots_LocalResources_EmitsGuardedResourceAddition()
	{
		using var harness = new XamlHotReloadTestHarness(
			nameof(NestedGeneratedRoots_LocalResources_EmitsGuardedResourceAddition),
			PageClass,
			PageStub,
			GeneratedProbeCardStub,
			ProbeConverterStubs);
		var generation = harness.GenerateDocuments(
			Documents(Nc04Page("A1", "B1"), Nc04ProbeCard("ProbeConverterOriginal")),
			Documents(Nc04Page("A2", "B2"), Nc04ProbeCard("ProbeConverterUpdated")));

		var update = GetProbeCardUpdateV2(generation);
		Assert.Contains("if (!this.Resources.ContainsKey(\"Fmt\"))", update, StringComparison.Ordinal);
		Assert.Contains("this.Resources.Add(\"Fmt\",", update, StringComparison.Ordinal);
		Assert.DoesNotContain("this.Resources[\"Fmt\"] =", update, StringComparison.Ordinal);
		Assert.True(harness.Compile(generation[1]).PeImage.Length > 0);
	}
}
