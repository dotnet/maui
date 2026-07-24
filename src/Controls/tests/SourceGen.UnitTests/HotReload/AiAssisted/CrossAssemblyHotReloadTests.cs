#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

/// <summary>
/// Wave-2 cross-assembly incremental-generator probes.
///
/// These tests only exercise Roslyn's incremental driver by emitting a referenced library to PE
/// bytes, turning that image into a <see cref="MetadataReference"/>, and swapping references inside
/// one app-compilation lineage. They do not load the referenced assembly, apply runtime deltas, or
/// claim coverage for the deferred cross-assembly runtime scenarios from wave2-final-test-plan §5.
///
/// XA-03 is intentionally omitted here. Own-compilation method-body-vs-signature invalidation is
/// already covered by <c>SourceGenXamlCodeBehindTests.TestCodeBehindGenerator_ImplementationChangeDoesNotTriggerRegeneration</c>
/// and <c>SourceGenXamlCodeBehindTests.TestCodeBehindGenerator_SignatureChangeTriggersRegeneration</c>,
/// so re-authoring it in this file would duplicate existing coverage rather than add a
/// cross-assembly invariant.
/// </summary>
[Collection("XamlHotReloadTests")]
public class CrossAssemblyHotReloadTests
{
	const string AppAssemblyName = "TestCrossAssembly.App";
	const string LibraryAssemblyName = "TestCrossAssembly.Lib";

	const string PageStub = """
		namespace TestCrossAssembly;

		public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
		{
			private partial void InitializeComponent();

			public MainPage()
			{
				InitializeComponent();
			}
		}
		""";

	const string PageXaml = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:lib="clr-namespace:TestCrossAssembly.Lib;assembly=TestCrossAssembly.Lib"
		             x:Class="TestCrossAssembly.MainPage">
		    <VerticalStackLayout x:DataType="lib:PersonViewModel">
		        <lib:ProbeView />
		        <Label Text="{Binding Name}" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	const string LibrarySourceV1 = """
		namespace TestCrossAssembly.Lib;

		public class PersonViewModel
		{
			public string Name => FormatName();

			public string FormatName()
			{
				return "Ada";
			}
		}

		public class ProbeView : global::Microsoft.Maui.Controls.ContentView
		{
			public string Version => DescribeVersion();

			public string DescribeVersion()
			{
				return "v1";
			}
		}
		""";

	const string LibrarySourceV2 = """
		namespace TestCrossAssembly.Lib;

		public class PersonViewModel
		{
			public string Name => FormatName();

			public string FormatName()
			{
				return "Grace";
			}
		}

		public class ProbeView : global::Microsoft.Maui.Controls.ContentView
		{
			public string Version => DescribeVersion();

			public string DescribeVersion()
			{
				return "v2";
			}
		}
		""";

	static SourceGeneratorDriver.AdditionalFile CreateXamlFile([CallerMemberName] string scenarioName = "") =>
		new(
			SourceGeneratorDriver.ToAdditionalText($"/CrossAssembly/{scenarioName}/MainPage.xaml", PageXaml),
			"Xaml",
			"MainPage.xaml",
			$"/CrossAssembly/{scenarioName}/MainPage.xaml",
			null,
			"net11.0",
			null,
			EnableIncrementalHotReload: true);

	static CSharpCompilation CreateAppCompilation(params MetadataReference[] references) =>
		(CSharpCompilation)SourceGeneratorDriver.CreateMauiCompilation(AppAssemblyName)
			.AddSyntaxTrees(ParseSource(PageStub, "MainPage.cs"))
			.AddReferences(references);

	static CSharpCompilation CreateLibraryCompilation(string source) =>
		(CSharpCompilation)SourceGeneratorDriver.CreateMauiCompilation(LibraryAssemblyName)
			.AddSyntaxTrees(ParseSource(source, "Library.cs"));

	static PortableExecutableReference EmitLibraryReference(string source)
	{
		var compilation = CreateLibraryCompilation(source);
		using var peStream = new MemoryStream();
		EmitResult emitResult = compilation.Emit(peStream);
		Assert.True(emitResult.Success, $"Referenced library emit failed:{Environment.NewLine}{FormatDiagnostics(emitResult.Diagnostics)}");

		return MetadataReference.CreateFromImage(ImmutableArray.CreateRange(peStream.ToArray()));
	}

	static Compilation AddReference(Compilation compilation, MetadataReference reference) =>
		compilation.AddReferences(reference);

	static Compilation RemoveReference(Compilation compilation, MetadataReference reference) =>
		compilation.RemoveReferences(reference);

	static Compilation ReplaceReference(Compilation compilation, MetadataReference oldReference, MetadataReference newReference) =>
		AddReference(RemoveReference(compilation, oldReference), newReference);

	static SyntaxTree ParseSource(string source, string path) =>
		CSharpSyntaxTree.ParseText(source, path: path);

	static GeneratorRunResult GetGeneratorResult(GeneratorDriverRunResult runResult) =>
		Assert.Single(runResult.Results);

	static string? FindGeneratedSource(GeneratorRunResult result, string hintSuffix)
	{
		var generatedSource = result.GeneratedSources.SingleOrDefault(source =>
			source.HintName.EndsWith(hintSuffix, StringComparison.OrdinalIgnoreCase)
			&& !source.HintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase));
		return generatedSource.SourceText?.ToString();
	}

	static IncrementalStepRunReason GetTrackedStepReason(GeneratorRunResult result, string trackingName)
	{
		var step = Assert.Single(result.TrackedSteps[trackingName]);
		var output = Assert.Single(step.Outputs);
		return output.Reason;
	}

	static void AssertTrackedStepReasons(GeneratorRunResult result, Dictionary<string, IncrementalStepRunReason> expectedReasons)
	{
		foreach (var (trackingName, expectedReason) in expectedReasons)
		{
			var actualReason = GetTrackedStepReason(result, trackingName);
			Assert.True(
				actualReason == expectedReason,
				$"{trackingName} expected {expectedReason} but was {actualReason}.");
		}
	}

	static string FormatDiagnostics(IEnumerable<Diagnostic> diagnostics) =>
		string.Join(Environment.NewLine, diagnostics.Select(static diagnostic =>
			$"{diagnostic.Id}: {diagnostic.GetMessage()} at {diagnostic.Location}"));

	// Wave2 · CrossAssembly · cap-cross-asm-MC · XA-01
	// Provenance: wave2-final-test-plan §3.6/§4/§6/§10 | wave2-cross-assembly-patterns #30
	// Faithfulness: reaches CompilationSignaturesComparer external-reference identity invalidation and XamlSourceProviderForIC; fails-for-bug: swapped referenced PE is treated as unchanged so Lib-backed XAML state stays cached.
	// Expected: GREEN
	// Issue: https://github.com/dotnet/maui/pull/36730
	[Fact]
	public void ReferencedAssemblySwap_InvalidatesXamlPipeline()
	{
		XamlHotReloadState.Reset();

		var xamlFile = CreateXamlFile();
		var libraryReferenceV1 = EmitLibraryReference(LibrarySourceV1);
		var libraryReferenceV2 = EmitLibraryReference(LibrarySourceV2);
		var compilation = CreateAppCompilation(libraryReferenceV1);

		var (run1, run2) = SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(
			compilation,
			applyChanges: (driver, currentCompilation) => (driver, ReplaceReference(currentCompilation, libraryReferenceV1, libraryReferenceV2)),
			xamlFile);

		var result1 = GetGeneratorResult(run1);
		var result2 = GetGeneratorResult(run2);
		var initialInitializeComponent = FindGeneratedSource(result1, ".xsg.cs");
		var updatedInitializeComponent = FindGeneratedSource(result2, ".xsg.cs");

		Assert.NotNull(initialInitializeComponent);
		Assert.NotNull(updatedInitializeComponent);
		Assert.Contains("global::TestCrossAssembly.Lib.PersonViewModel", initialInitializeComponent!, StringComparison.Ordinal);
		Assert.Contains("global::TestCrossAssembly.Lib.ProbeView", initialInitializeComponent!, StringComparison.Ordinal);
		Assert.Equal(initialInitializeComponent, updatedInitializeComponent);

		AssertTrackedStepReasons(result2, new Dictionary<string, IncrementalStepRunReason>
		{
			{ TrackingNames.ProjectItemProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.CompilationProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.ReferenceTypeCacheProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.XmlnsDefinitionsProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.XamlProjectItemProviderForIC, IncrementalStepRunReason.Modified },
			{ TrackingNames.CompilationWithCodeBehindProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.XmlnsDefinitionsProviderForIC, IncrementalStepRunReason.Modified },
		});

		Assert.Contains(
			GetTrackedStepReason(result2, TrackingNames.XamlSourceProviderForIC),
			new[] { IncrementalStepRunReason.Modified, IncrementalStepRunReason.New });
	}

	// Wave2 · CrossAssembly · cap-cross-asm-MC · XA-02
	// Provenance: wave2-final-test-plan §3.6/§4/§6/§10 | wave2-cross-assembly-patterns §0 inventory
	// Faithfulness: reaches the no-op branch of CompilationSignaturesComparer and keeps the same MetadataReference instance; fails-for-bug: unchanged references still fan out through the IC pipeline.
	// Expected: GREEN
	// Issue: https://github.com/dotnet/maui/pull/36730
	[Fact]
	public void UnchangedReferences_XamlPipelineCached()
	{
		XamlHotReloadState.Reset();

		var xamlFile = CreateXamlFile();
		var libraryReference = EmitLibraryReference(LibrarySourceV1);
		var compilation = CreateAppCompilation(libraryReference);

		var (run1, run2) = SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(
			compilation,
			applyChanges: static (driver, currentCompilation) => (driver, currentCompilation),
			xamlFile);

		var result1 = GetGeneratorResult(run1);
		var result2 = GetGeneratorResult(run2);
		var initialInitializeComponent = FindGeneratedSource(result1, ".xsg.cs");
		var updatedInitializeComponent = FindGeneratedSource(result2, ".xsg.cs");

		Assert.NotNull(initialInitializeComponent);
		Assert.Equal(initialInitializeComponent, updatedInitializeComponent);

		AssertTrackedStepReasons(result2, new Dictionary<string, IncrementalStepRunReason>
		{
			{ TrackingNames.ProjectItemProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.CompilationProvider, IncrementalStepRunReason.Unchanged },
			{ TrackingNames.ReferenceTypeCacheProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XmlnsDefinitionsProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlProjectItemProviderForIC, IncrementalStepRunReason.Cached },
		});
	}
}
