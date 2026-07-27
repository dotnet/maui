#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

public partial class ResourceAndThemeHotReloadTests
{
	// Wave2 · Resources · P0-09 · RT-02
	// Provenance: MAUI §1.1; public-app T1/T3
	// Faithfulness: reaches the generic complex-property skip path (UpdateComponentCodeWriter
	// ~L1194) for a non-root element's Resources property; fails (i.e. reclassify to GREEN) if the
	// writer ever starts emitting real merged-dictionary patch code for this shape.
	// Expected: DOC-SKIP-GUARD
	[Fact]
	public void MergedDictionaries_ComplexProperty_EmitsSkipMarker()
	{
		// Root ContentPage.Resources is special-cased by the writer (it always resolves via
		// TryEmitResourceDictionaryChange and never falls through to the generic marker below),
		// so this guard targets a non-root element's Resources — the shape that is empirically
		// declined via the generic complex-property path.
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <VerticalStackLayout>
			    <VerticalStackLayout.Resources>
			      <ResourceDictionary>
			        <ResourceDictionary.MergedDictionaries>
			          <ResourceDictionary>
			            <Color x:Key="Accent">Red</Color>
			          </ResourceDictionary>
			          <ResourceDictionary>
			            <Color x:Key="Accent">Blue</Color>
			          </ResourceDictionary>
			        </ResourceDictionary.MergedDictionaries>
			      </ResourceDictionary>
			    </VerticalStackLayout.Resources>
			    <Label TextColor="{DynamicResource Accent}" />
			  </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <VerticalStackLayout>
			    <VerticalStackLayout.Resources>
			      <ResourceDictionary>
			        <ResourceDictionary.MergedDictionaries>
			          <ResourceDictionary>
			            <Color x:Key="Accent">Blue</Color>
			          </ResourceDictionary>
			          <ResourceDictionary>
			            <Color x:Key="Accent">Red</Color>
			          </ResourceDictionary>
			        </ResourceDictionary.MergedDictionaries>
			      </ResourceDictionary>
			    </VerticalStackLayout.Resources>
			    <Label TextColor="{DynamicResource Accent}" />
			  </VerticalStackLayout>
			</ContentPage>
			""";

		using var harness = CreateHarness(nameof(MergedDictionaries_ComplexProperty_EmitsSkipMarker));
		var generation = harness.Generate(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains("// Complex property 'Resources' (ElementNode) \u2014 skipped (not yet supported)", updateComponentSource!, StringComparison.Ordinal);
	}

	// Wave2 · Resources · P1-01 · RT-10 (generator/compile-atomic anchor)
	// Provenance: MAUI §3.1 (Source= merged dictionaries); public-app T13
	// Faithfulness: strongest faithful level the harness supports for Source= merged dictionaries.
	// It tracks the reorder then removal of the Source= sibling documents and recompiles the page in
	// every version (mirrors HarnessCapabilityTests.MultiDocument_DictionaryOnlyEdit_TracksAllDocuments
	// AndCompilesPage). The live runtime cannot reload a Source= dictionary into a retained page in this
	// in-memory generator/ALC harness, so this PR intentionally stops at document-tracking/compile coverage.
	[Fact]
	public void SourceMergedDictionaries_ReorderThenRemove_TracksDocumentsAndCompiles()
	{
		string Page(string mergedBody) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <ResourceDictionary>
			      <ResourceDictionary.MergedDictionaries>
			{{mergedBody}}
			      </ResourceDictionary.MergedDictionaries>
			    </ResourceDictionary>
			  </ContentPage.Resources>
			  <Label TextColor="{DynamicResource Accent}" />
			</ContentPage>
			""";

		const string sourceA = "        <ResourceDictionary Source=\"A.xaml\" />";
		const string sourceB = "        <ResourceDictionary Source=\"B.xaml\" />";
		const string dictA = """
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			  <Color x:Key="Accent">Red</Color>
			</ResourceDictionary>
			""";
		const string dictB = """
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			  <Color x:Key="Accent">Blue</Color>
			</ResourceDictionary>
			""";

		IReadOnlyDictionary<string, XamlHotReloadDocument> Snapshot(string mergedBody, bool includeA)
		{
			var snapshot = new Dictionary<string, XamlHotReloadDocument>(StringComparer.Ordinal)
			{
				["MainPage.xaml"] = new XamlHotReloadDocument(Page(mergedBody)),
				["B.xaml"] = new XamlHotReloadDocument(dictB),
			};
			if (includeA)
				snapshot["A.xaml"] = new XamlHotReloadDocument(dictA);
			return snapshot;
		}

		using var harness = CreateHarness(nameof(SourceMergedDictionaries_ReorderThenRemove_TracksDocumentsAndCompiles));
		var generation = harness.GenerateDocuments(
			Snapshot(sourceA + "\n" + sourceB, includeA: true),   // V1: A;B
			Snapshot(sourceB + "\n" + sourceA, includeA: true),   // V2: B;A (reorder)
			Snapshot(sourceB, includeA: false));                  // V3: B only (A removed)

		// Reorder and removal of the Source= sibling documents are visible to the harness.
		Assert.True(generation[0].Documents.ContainsKey("A.xaml"));
		Assert.True(generation[1].Documents.ContainsKey("A.xaml"));
		Assert.False(generation[2].Documents.ContainsKey("A.xaml"));

		// The page owns exactly one generated root and compiles in every version.
		for (var index = 0; index < 3; index++)
		{
			Assert.Single(generation[index].GeneratedRoots);
			Assert.Equal(PageClass, generation[index].GeneratedRoots[0].TypeName);
			Assert.True(harness.Compile(generation[index]).PeImage.Length > 0);
		}
	}

	// Wave2 · Resources · P1-05 · RT-11
	// Provenance: MAUI §3.1/§3.5 (multi-document malformed→repair); public-app T14
	// Faithfulness: multi-document malformed→repair atomicity at the strongest faithful level the
	// harness supports — GENERATOR-ATOMIC, NOT live-resource atomic. The batch carries two
	// AdditionalTexts (MainPage.xaml + a sibling Theme.xaml resource dictionary). Source= cannot be
	// used to wire the page to the dictionary in this in-memory generator/ALC harness (it emits an
	// unconditional MAUIG1001 "not a valid resource path" — the same boundary proven by RT-10), so the
	// malformed edit is applied to the generator-parsed document (MainPage.xaml) while the sibling
	// dictionary is carried unchanged through the whole batch. The atomicity invariant asserted here:
	// a malformed intermediate version surfaces a parser error and does NOT phantom-advance to a valid
	// compiled page, and the repaired version fully recovers (generates the repaired content and
	// compiles), with the sibling document tracked across every version. The LIVE retained-object
	// atomic reload of a Source= dictionary is out of harness scope, hence generator-atomic labeling.
	// Fails if a malformed batch silently produces a compilable page, if the repaired batch does not
	// generate/compile, or if the sibling document is dropped.
	[Fact]
	public void MultiDocumentMalformedThenRepair_IsAtomicAcrossPageAndDictionary()
	{
		const string pageValidBefore = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="Before" />
			</ContentPage>
			""";
		// V2: the <Label> element is never closed — a parser error in the page document.
		const string pageMalformed = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="AfterRepair"
			</ContentPage>
			""";
		const string pageRepaired = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="AfterRepair" />
			</ContentPage>
			""";

		// The sibling dictionary is a valid resource document carried unchanged across every version.
		const string theme = """
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			  <Color x:Key="Accent">Blue</Color>
			</ResourceDictionary>
			""";

		IReadOnlyDictionary<string, XamlHotReloadDocument> Snapshot(string page) =>
			new Dictionary<string, XamlHotReloadDocument>(StringComparer.Ordinal)
			{
				["MainPage.xaml"] = new XamlHotReloadDocument(page),
				["Theme.xaml"] = new XamlHotReloadDocument(theme),
			};

		using var harness = CreateHarness(nameof(MultiDocumentMalformedThenRepair_IsAtomicAcrossPageAndDictionary));
		var generation = harness.GenerateDocumentsAllowingDiagnostics(
			Snapshot(pageValidBefore),
			Snapshot(pageMalformed),
			Snapshot(pageRepaired));

		// The sibling dictionary document is tracked through the malformed→repair batch, unchanged.
		Assert.True(generation[0].Documents.ContainsKey("Theme.xaml"));
		Assert.True(generation[1].Documents.ContainsKey("Theme.xaml"));
		Assert.True(generation[2].Documents.ContainsKey("Theme.xaml"));
		Assert.Equal(theme, generation[2].Documents["Theme.xaml"].Text);

		// V1: both documents valid — the page generates and compiles.
		Assert.DoesNotContain(
			generation[0].GeneratorResult.Diagnostics,
			diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
		Assert.NotNull(generation[0].InitializeComponentSource);
		Assert.True(harness.Compile(generation[0]).PeImage.Length > 0);

		// V2: the malformed page surfaces at least one parser error diagnostic and does NOT
		// phantom-advance to a valid compiled page.
		Assert.Contains(
			generation[1].GeneratorResult.Diagnostics,
			diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
		Assert.Null(generation[1].InitializeComponentSource);

		// V3: the page is repaired — it generates the repaired content and compiles cleanly.
		Assert.DoesNotContain(
			generation[2].GeneratorResult.Diagnostics,
			diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
		Assert.NotNull(generation[2].InitializeComponentSource);
		Assert.Contains("AfterRepair", generation[2].InitializeComponentSource!, StringComparison.Ordinal);
		Assert.True(harness.Compile(generation[2]).PeImage.Length > 0);
	}
}
