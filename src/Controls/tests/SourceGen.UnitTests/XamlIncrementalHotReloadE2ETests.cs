// End-to-end tests for XAML Incremental Hot Reload.
//
// These tests exercise the full pipeline, from XAML → SourceGen → Compile → Load → ApplyUpdate:
//   1. Run the XamlGenerator on XAML V1 → get generated InitializeComponent C# source.
//   2. Compile that C# into an in-memory assembly, load it, create an instance.
//   3. Run the XamlGenerator on XAML V2 → get generated UpdateComponent C# source.
//   4. Compile V2, EmitDifference from V1→V2, MetadataUpdater.ApplyUpdate.
//   5. Call UpdateComponent() on the live instance via reflection.
//   6. Assert that properties changed on the live MAUI object tree.
//
// Follows the pattern from: https://gist.github.com/StephaneDelcroix/2ed08a8ff3632ce7341c3c2c16d338b2

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// True end-to-end tests: XAML → SourceGen → Compile → Load → Hot Reload → Verify.
/// Uses <see cref="MetadataUpdater.ApplyUpdate"/> to apply deltas to a live assembly.
/// </summary>
[Collection("XamlHotReloadTests")]
public class XamlIncrementalHotReloadE2ETests : IDisposable
{
	public void Dispose() => XamlHotReloadState.Reset();

	const string PageRelativePath = "MainPage.xaml";
	const string PageClass = "TestE2EApp.MainPage";
	const string AssemblyName = "TestE2EApp";

	static SourceGeneratorDriver.AdditionalFile MakeFile(string xaml) =>
		new(
			SourceGeneratorDriver.ToAdditionalText(PageRelativePath, xaml),
			Kind: "Xaml",
			RelativePath: PageRelativePath,
			TargetPath: "TestE2EApp/MainPage.xaml",
			ManifestResourceName: null,
			TargetFramework: "net11.0",
			NoWarn: null,
			EnableIncrementalHotReload: true);

	/// <summary>
	/// Creates a Roslyn compilation with MAUI references, suitable for compiling
	/// source-generated code that uses MAUI controls, XamlComponentRegistry, etc.
	/// </summary>
	static CSharpCompilation CreateMauiCompilation(params SyntaxTree[] trees)
	{
		var refs = GetCompilationReferences();
		return CSharpCompilation.Create(
			AssemblyName,
			trees,
			refs,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	}

	/// <summary>
	/// Gets metadata references for MAUI runtime assemblies + BCL.
	/// </summary>
	static MetadataReference[] GetCompilationReferences()
	{
		string dotNetAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
		return
		[
			MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.InternalsVisibleToAttribute).Assembly.Location),
			MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "mscorlib.dll")),
			MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.dll")),
			MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.Core.dll")),
			MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.Private.CoreLib.dll")),
			MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.Runtime.dll")),
			MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.ObjectModel.dll")),
			MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Microsoft.Maui.Graphics.Color).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Button).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(BindingExtension).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Thickness).Assembly.Location),
			MetadataReference.CreateFromFile(Assembly.Load("System.Private.Xml").Location),
			MetadataReference.CreateFromFile(Assembly.Load("System.Xml.ReaderWriter").Location),
			MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(System.ComponentModel.TypeConverter).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(System.ComponentModel.TypeDescriptor).Assembly.Location),
		];
	}

	/// <summary>
	/// Runs the XamlGenerator source generator to produce IC/UC code from XAML.
	/// Returns the generated source file contents keyed by hint name suffix.
	/// </summary>
	(string? icSource, string? ucSource) RunSourceGen(string xaml)
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation(AssemblyName);
		var file = MakeFile(xaml);

		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(
			compilation, file, assertNoCompilationErrors: false);

		string? icSource = null, ucSource = null;
		foreach (var gen in result.Results)
		{
			foreach (var src in gen.GeneratedSources)
			{
				if (src.HintName.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase)
					&& !src.HintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase))
					icSource = src.SourceText.ToString();
				if (src.HintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase))
					ucSource = src.SourceText.ToString();
			}
		}
		return (icSource, ucSource);
	}

	/// <summary>
	/// Runs the source generator for V1, then for V2 (with V1 cached in state),
	/// returning both IC sources and the UC source from V2.
	/// </summary>
	(string icV1, string icV2, string? ucV2) RunSourceGenTwoPhase(string xamlV1, string xamlV2)
	{
		// Phase 1: V1 seeds state
		var (icV1, _) = RunSourceGen(xamlV1);
		Assert.NotNull(icV1);

		// Phase 2: V2 produces UC (state now has V1 cached)
		var compilation = SourceGeneratorDriver.CreateMauiCompilation(AssemblyName);
		var fileV1 = MakeFile(xamlV1);
		var fileV2 = MakeFile(xamlV2);

		var (run1, run2) = SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(
			compilation,
			applyChanges: (driver, comp) =>
			{
				var updatedDriver = driver
					.ReplaceAdditionalText(fileV1.Text, fileV2.Text)
					.WithUpdatedAnalyzerConfigOptions(new OptionsProvider([fileV2]));
				return (updatedDriver, comp);
			},
			fileV1);

		// Get IC from run2 (V2's InitializeComponent)
		string? icV2 = null, ucV2 = null;
		foreach (var gen in run2.Results)
		{
			foreach (var src in gen.GeneratedSources)
			{
				if (src.HintName.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase)
					&& !src.HintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase))
					icV2 = src.SourceText.ToString();
				if (src.HintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase))
					ucV2 = src.SourceText.ToString();
			}
		}

		Assert.NotNull(icV2);
		return (icV1!, icV2!, ucV2);
	}

	/// <summary>
	/// Compiles C# sources into a PE+PDB byte pair, asserting compilation success.
	/// </summary>
	/// <summary>
	/// Strips the <c>[GeneratedCode]</c> attribute from UC source to avoid CS0579 duplicate
	/// when compiling IC + UC together (both emit it on the same partial class).
	/// </summary>
	static string StripGeneratedCodeAttribute(string source) =>
		System.Text.RegularExpressions.Regex.Replace(source,
			@"\[global::System\.CodeDom\.Compiler\.GeneratedCodeAttribute\([^\]]+\)\]\s*\n",
			"");

	static (byte[] pe, byte[] pdb, CSharpCompilation compilation) CompileSources(params string[] sources)
	{
		var trees = sources.Select((s, i) =>
			CSharpSyntaxTree.ParseText(s, path: $"Source{i}.cs", encoding: System.Text.Encoding.UTF8)).ToArray();

		var compilation = CreateMauiCompilation(trees);

		using var peStream = new MemoryStream();
		using var pdbStream = new MemoryStream();
		var emitResult = compilation.Emit(peStream, pdbStream,
			options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));

		if (!emitResult.Success)
		{
			var errors = string.Join("\n", emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
			Assert.Fail($"Compilation failed:\n{errors}");
		}

		return (peStream.ToArray(), pdbStream.ToArray(), compilation);
	}

	/// <summary>
	/// Minimal partial class stub with InitializeComponentRuntime() to satisfy the generated code.
	/// </summary>
	const string PageStub = $$"""
		namespace TestE2EApp;

		public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
		{
			// Declaring partial method (the source generator provides the implementing declaration)
			private partial void InitializeComponent();

			// Runtime fallback used by UC fallback path
			public void InitializeComponentRuntime() { }

			// Constructor calls IC
			public MainPage()
			{
				InitializeComponent();
			}
		}
		""";

	[MetadataUpdateFact]
	public void PropertyChange_AppliedViaHotReload()
	{
		XamlHotReloadState.Reset();

		// --- XAML V1: Label says "Hello" ---
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		// --- XAML V2: Label says "World" ---
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="World" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		// Step 1: Source-gen both versions
		var (icV1, icV2, ucV2) = RunSourceGenTwoPhase(xamlV1, xamlV2);
		Assert.NotNull(ucV2); // UC must be generated for a property change

		// Step 2: Compile V1 (IC only, no UC)
		var (peV1, pdbV1, compilationV1) = CompileSources(PageStub, icV1);

		// Step 3: Load V1, create instance
		var alc = new AssemblyLoadContext("E2EHotReloadTest", isCollectible: true);
		try
		{
			var assembly = alc.LoadFromStream(new MemoryStream(peV1), new MemoryStream(pdbV1));
			var pageType = assembly.GetType(PageClass)!;
			var instance = Activator.CreateInstance(pageType)!;

			// Verify V1: find the Label in the Content tree
			var page = (ContentPage)instance;
			var layout = page.Content as Layout;
			Assert.NotNull(layout);
			var label = layout!.Children.OfType<Label>().FirstOrDefault();
			Assert.NotNull(label);
			Assert.Equal("Hello", label!.Text);

			// Step 4: Compile V2 (IC + UC)
			var treesV2 = new[]
			{
				CSharpSyntaxTree.ParseText(PageStub, path: "PageStub.cs", encoding: System.Text.Encoding.UTF8),
				CSharpSyntaxTree.ParseText(icV2, path: "IC.cs", encoding: System.Text.Encoding.UTF8),
				CSharpSyntaxTree.ParseText(StripGeneratedCodeAttribute(ucV2), path: "UC.cs", encoding: System.Text.Encoding.UTF8),
			};
			var compilationV2 = CreateMauiCompilation(treesV2);

			var v2Errors = compilationV2.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
			if (v2Errors.Length > 0)
				Assert.Fail($"V2 compilation failed:\n{string.Join("\n", v2Errors.Select(e => $"{e.Id}: {e.GetMessage()}"))}");

			// Step 5: Create baseline from V1
			var moduleMetadata = ModuleMetadata.CreateFromImage(peV1);
			var baseline = EmitBaseline.CreateInitialBaseline(
				compilationV1,
				moduleMetadata,
				debugInformationProvider: handle => default,
				localSignatureProvider: handle => default,
				hasPortableDebugInformation: true);

			// Step 6: Compute semantic edits
			// The UpdateComponent method was added in V2 — it's a new member
			var oldPageType = compilationV1.GetTypeByMetadataName(PageClass)!;
			var newPageType = compilationV2.GetTypeByMetadataName(PageClass)!;

			var edits = new List<SemanticEdit>();

			// IC was updated (version bump)
			// InitializeComponent is a partial method — EnC needs the implementing declaration
			var oldICDef = oldPageType.GetMembers("InitializeComponent").OfType<IMethodSymbol>().First();
			var newICDef = newPageType.GetMembers("InitializeComponent").OfType<IMethodSymbol>().First();
			// PartialImplementationPart gives us the implementation; use it if available
			var oldIC = oldICDef.PartialImplementationPart ?? oldICDef;
			var newIC = newICDef.PartialImplementationPart ?? newICDef;
			edits.Add(new SemanticEdit(SemanticEditKind.Update, oldIC, newIC));

			// UC is new in V2
			var newUC = newPageType.GetMembers("UpdateComponent").Single();
			edits.Add(new SemanticEdit(SemanticEditKind.Insert, null, newUC));

			// Step 7: Emit delta
			using var mdDelta = new MemoryStream();
			using var ilDelta = new MemoryStream();
			using var pdbDelta = new MemoryStream();

			var diffResult = compilationV2.EmitDifference(
				baseline, edits,
				isAddedSymbol: s => s.Name == "UpdateComponent",
				mdDelta, ilDelta, pdbDelta,
				System.Threading.CancellationToken.None);

			Assert.True(diffResult.Success,
				$"EmitDifference failed:\n{string.Join("\n", diffResult.Diagnostics)}");

			// Step 8: Apply the delta
			MetadataUpdater.ApplyUpdate(assembly,
				mdDelta.ToArray(), ilDelta.ToArray(), pdbDelta.ToArray());

			// Step 9: Call UpdateComponent() on the SAME instance
			var updateMethod = pageType.GetMethod("UpdateComponent",
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			Assert.NotNull(updateMethod);
			updateMethod!.Invoke(instance, null);

			// Step 10: Verify the Label's Text changed to "World"
			var updatedLabel = ((Layout)page.Content!).Children.OfType<Label>().FirstOrDefault();
			Assert.NotNull(updatedLabel);
			Assert.Equal("World", updatedLabel!.Text);
		}
		finally
		{
			alc.Unload();
		}
	}

	[MetadataUpdateFact]
	public void MultiplePropertyChanges_ChainedPatches()
	{
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage"
			             Title="V1">
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage"
			             Title="V2">
			    <Label Text="World" />
			</ContentPage>
			""";

		// Run source gen: V1 seeds, V2 produces UC with the single V1->V2 patch (no version chain)
		var (icV1, icV2, ucV2) = RunSourceGenTwoPhase(xamlV1, xamlV2);
		Assert.NotNull(ucV2);

		// New design: a single unconditional patch — no accumulated `if (__version == N)` version guard.
		Assert.DoesNotContain("if (__version ==", ucV2!, StringComparison.Ordinal);

		// Verify UC contains the new property values
		Assert.Contains("\"World\"", ucV2!, StringComparison.Ordinal);
		Assert.Contains("\"V2\"", ucV2!, StringComparison.Ordinal);
	}

	/// <summary>
	/// Runs the generator over three successive XAML versions against the same compilation, so
	/// <see cref="XamlHotReloadState"/> accumulates exactly as it would across live edits, and
	/// returns the final (V3) IC + UC sources.
	/// </summary>
	(string icV3, string? ucV3) RunSourceGenThreePhase(string xamlV1, string xamlV2, string xamlV3)
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation(AssemblyName);
		var fileV1 = MakeFile(xamlV1);
		var fileV2 = MakeFile(xamlV2);
		var fileV3 = MakeFile(xamlV3);

		Microsoft.CodeAnalysis.ISourceGenerator generator = new XamlGenerator().AsSourceGenerator();
		var options = new Microsoft.CodeAnalysis.GeneratorDriverOptions(
			disabledOutputs: Microsoft.CodeAnalysis.IncrementalGeneratorOutputKind.None,
			trackIncrementalGeneratorSteps: true);

		Microsoft.CodeAnalysis.GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], driverOptions: options)
			.AddAdditionalTexts(System.Collections.Immutable.ImmutableArray.Create<Microsoft.CodeAnalysis.AdditionalText>(fileV1.Text))
			.WithUpdatedAnalyzerConfigOptions(new OptionsProvider([fileV1]));

		driver = driver.RunGenerators(compilation);
		driver = driver
			.ReplaceAdditionalText(fileV1.Text, fileV2.Text)
			.WithUpdatedAnalyzerConfigOptions(new OptionsProvider([fileV2]))
			.RunGenerators(compilation);
		driver = driver
			.ReplaceAdditionalText(fileV2.Text, fileV3.Text)
			.WithUpdatedAnalyzerConfigOptions(new OptionsProvider([fileV3]))
			.RunGenerators(compilation);

		var run3 = driver.GetRunResult();
		string? icV3 = null, ucV3 = null;
		foreach (var gen in run3.Results)
		{
			foreach (var src in gen.GeneratedSources)
			{
				if (src.HintName.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase)
					&& !src.HintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase))
					icV3 = src.SourceText.ToString();
				if (src.HintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase))
					ucV3 = src.SourceText.ToString();
			}
		}

		Assert.NotNull(icV3);
		return (icV3!, ucV3);
	}

	/// <summary>
	/// Regression for the XIHR versioning determinism bug (Tomas Matousek). Editing a property to an
	/// INVALID value and then reverting it must leave the generator in a state where the generated
	/// output for the (now identical to baseline) XAML compiles cleanly and does not retain the
	/// invalid intermediate value. Today's monotonic __version chain accumulates every patch, so the
	/// invalid "Level22" block lingers in UpdateComponent() and the output fails to compile.
	/// </summary>
	[Fact]
	public void RevertToOriginal_ProducesCompilableOutput_WithoutStalePatch()
	{
		XamlHotReloadState.Reset();

		string Page(string headingLevel) => $$"""
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="Hi" SemanticProperties.HeadingLevel="{{headingLevel}}" />
			</ContentPage>
			""";

		// V1 (valid) -> V2 (invalid enum member 'Level22') -> V3 (revert, identical to V1).
		var (icV3, ucV3) = RunSourceGenThreePhase(Page("Level2"), Page("Level22"), Page("Level2"));

		// The invalid intermediate value must NOT survive into the reverted generation.
		if (ucV3 is not null)
			Assert.DoesNotContain("Level22", ucV3, StringComparison.Ordinal);
		Assert.DoesNotContain("Level22", icV3, StringComparison.Ordinal);

		// And the final generated code (IC + UC) must compile — the whole point of reverting.
		var sources = new List<string> { PageStub, icV3 };
		if (ucV3 is not null)
			sources.Add(StripGeneratedCodeAttribute(ucV3));

		var trees = sources.Select((s, i) =>
			CSharpSyntaxTree.ParseText(s, path: $"Source{i}.cs", encoding: System.Text.Encoding.UTF8)).ToArray();
		var comp = CreateMauiCompilation(trees);
		var errors = comp.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
		Assert.True(errors.Length == 0,
			$"Reverted generation should compile, but got:\n{string.Join("\n", errors.Select(e => $"{e.Id}: {e.GetMessage()}"))}");
	}

	/// <summary>
	/// Runs the generator over three successive XAML versions against the same compilation and
	/// returns the IC + UC source for EVERY phase, so a test can reason about how the generated
	/// content identity (<c>__version</c>) and patches evolve across edits — including reverts.
	/// </summary>
	(string icV1, string? ucV1, string icV2, string? ucV2, string icV3, string? ucV3)
		RunSourceGenAllPhases(string xamlV1, string xamlV2, string xamlV3)
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation(AssemblyName);
		var fileV1 = MakeFile(xamlV1);
		var fileV2 = MakeFile(xamlV2);
		var fileV3 = MakeFile(xamlV3);

		Microsoft.CodeAnalysis.ISourceGenerator generator = new XamlGenerator().AsSourceGenerator();
		var options = new Microsoft.CodeAnalysis.GeneratorDriverOptions(
			disabledOutputs: Microsoft.CodeAnalysis.IncrementalGeneratorOutputKind.None,
			trackIncrementalGeneratorSteps: true);

		Microsoft.CodeAnalysis.GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], driverOptions: options)
			.AddAdditionalTexts(System.Collections.Immutable.ImmutableArray.Create<Microsoft.CodeAnalysis.AdditionalText>(fileV1.Text))
			.WithUpdatedAnalyzerConfigOptions(new OptionsProvider([fileV1]));

		driver = driver.RunGenerators(compilation);
		var (icV1, ucV1) = ExtractICUC(driver.GetRunResult());

		driver = driver
			.ReplaceAdditionalText(fileV1.Text, fileV2.Text)
			.WithUpdatedAnalyzerConfigOptions(new OptionsProvider([fileV2]))
			.RunGenerators(compilation);
		var (icV2, ucV2) = ExtractICUC(driver.GetRunResult());

		driver = driver
			.ReplaceAdditionalText(fileV2.Text, fileV3.Text)
			.WithUpdatedAnalyzerConfigOptions(new OptionsProvider([fileV3]))
			.RunGenerators(compilation);
		var (icV3, ucV3) = ExtractICUC(driver.GetRunResult());

		return (icV1, ucV1, icV2, ucV2, icV3, ucV3);
	}

	static (string ic, string? uc) ExtractICUC(Microsoft.CodeAnalysis.GeneratorDriverRunResult run)
	{
		string? ic = null, uc = null;
		foreach (var gen in run.Results)
		{
			foreach (var src in gen.GeneratedSources)
			{
				if (src.HintName.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase)
					&& !src.HintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase))
					ic = src.SourceText.ToString();
				if (src.HintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase))
					uc = src.SourceText.ToString();
			}
		}
		Assert.NotNull(ic);
		return (ic!, uc);
	}

	/// <summary>
	/// Reverse-transition (Kirill's revert requirement): editing a property forward then reverting must
	/// produce a <c>UpdateComponent()</c> that restores the baseline value on live instances, without
	/// retaining the intermediate value. (Determinism of the generated output is covered separately and
	/// exhaustively by <see cref="RevertedGeneration_InitializeComponent_IsByteIdentical_ToInitialGeneration"/>.)
	/// </summary>
	[Fact]
	public void ReverseEdit_UC_RestoresBaselineValue_WithoutRetainingIntermediate()
	{
		XamlHotReloadState.Reset();

		string Page(string text) => $$"""
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="{{text}}" />
			</ContentPage>
			""";

		var v1 = Page("Hello");
		var v2 = Page("World");

		// V1 -> V2 -> V1 (revert).
		var (_, _, _, ucV2, _, ucV3) = RunSourceGenAllPhases(v1, v2, v1);

		// Forward edit's UC applies the new value (its non-empty body is also the XAML-change signal)...
		Assert.NotNull(ucV2);
		Assert.Contains("\"World\"", ucV2!, StringComparison.Ordinal);

		// ...and the reverse edit's UC brings it back to the V1 value (reverse transition), without
		// retaining the intermediate "World".
		Assert.NotNull(ucV3);
		Assert.Contains("\"Hello\"", ucV3!, StringComparison.Ordinal);
		Assert.DoesNotContain("\"World\"", ucV3!, StringComparison.Ordinal);
	}

	/// <summary>
	/// The strongest determinism guarantee (Tomas Matousek's purity principle): the generated
	/// <c>InitializeComponent</c> for a given XAML must be BYTE-IDENTICAL regardless of how that
	/// content was reached. Both sources here come from the SAME generator driver/options, so edit
	/// history is the only variable: phase 1 (<c>icV1</c>) generates V1 from a clean state; phase 3
	/// (<c>icV3</c>) reaches byte-identical V1 content by reverting an edit (V1→V2→V1). If any embedded
	/// value (registry node IDs, the <c>__version</c> content hash, etc.) depended on edit history, the
	/// two would differ; they must not. Covers a property-only edit AND a structural edit (added child).
	/// </summary>
	[Theory]
	[InlineData("<Label Text=\"Hi\" />", "<Label Text=\"Bye\" />")]          // property-only edit
	[InlineData("<Label Text=\"Hi\" />", "<Label Text=\"Hi\" /><Button Text=\"New\" />")] // structural edit (added child)
	public void RevertedGeneration_InitializeComponent_IsByteIdentical_ToInitialGeneration(string bodyV1, string bodyV2)
	{
		string Page(string body) => $$"""
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="Anchor" />
			        {{body}}
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var v1 = Page(bodyV1);
		var v2 = Page(bodyV2);

		XamlHotReloadState.Reset();
		var (icV1, _, _, _, icV3, _) = RunSourceGenAllPhases(v1, v2, v1);

		// InitializeComponent for identical content must be identical regardless of edit history —
		// every value it embeds is a pure function of the current content, not of the path taken.
		Assert.Equal(icV1, icV3);
	}

	/// <summary>
	/// Regression for the inherited-XAML build break introduced by always-emitting UpdateComponent():
	/// a XAML class that derives from another class exposing an <c>internal UpdateComponent()</c> emits
	/// its own <c>UpdateComponent()</c>, which hides the base's (CS0108). Since UpdateComponent() is now
	/// emitted on every generation — not just on edit — this must compile cleanly. The generated UC file
	/// suppresses CS0108 (the hiding is intentional: each level patches its own XAML tree).
	/// </summary>
	[Fact]
	public void UpdateComponent_OnInheritedXamlClass_CompilesWithoutHidingWarning()
	{
		XamlHotReloadState.Reset();

		const string xaml = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="Hi" />
			</ContentPage>
			""";

		var (ic, uc) = RunSourceGen(xaml);
		Assert.NotNull(ic);
		Assert.NotNull(uc); // always-emit: UC present even without an edit

		// Code-behind: MainPage derives from a base that ALSO declares an internal UpdateComponent().
		const string stub = """
			namespace TestE2EApp;

			public partial class BaseXamlPage : global::Microsoft.Maui.Controls.ContentPage
			{
				internal void UpdateComponent() { }
			}

			public partial class MainPage : BaseXamlPage
			{
				private partial void InitializeComponent();
				public void InitializeComponentRuntime() { }
				public MainPage() { InitializeComponent(); }
			}
			""";

		var trees = new[]
		{
			CSharpSyntaxTree.ParseText(stub, path: "Stub.cs", encoding: System.Text.Encoding.UTF8),
			CSharpSyntaxTree.ParseText(ic!, path: "IC.cs", encoding: System.Text.Encoding.UTF8),
			CSharpSyntaxTree.ParseText(StripGeneratedCodeAttribute(uc!), path: "UC.cs", encoding: System.Text.Encoding.UTF8),
		};
		var comp = CreateMauiCompilation(trees);

		// CS0108 (member hides inherited member) must NOT appear at ANY severity — the pragma in the
		// generated UC file must suppress it. (Checked independently of general errors so the test
		// would fail if the pragma were missing, regardless of warnings-as-errors configuration.)
		var cs0108 = comp.GetDiagnostics().Where(d => d.Id == "CS0108").ToArray();
		Assert.Empty(cs0108);

		var errors = comp.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
		Assert.True(errors.Length == 0,
			$"Inherited-XAML generation should compile, but got:\n{string.Join("\n", errors.Select(e => $"{e.Id}: {e.GetMessage()}"))}");
	}

	/// <summary>
	/// Diagnostics classification (Kirill Ovchinnikov's requirement), the "empty UpdateComponent" design.
	/// <c>UpdateComponent()</c> is always present (member stability / no EnC churn), but its BODY is empty
	/// when a generation carries no XAML change and non-empty (a patch) when the XAML changed. The runtime
	/// classifies a delta as a XAML change — SYNCHRONOUSLY, pre-dispatch, on
	/// <c>HotReloadRequestedEventArgs.HandledTypes</c>, exactly where XamlTools reads it today — by
	/// inspecting whether <c>UpdateComponent()</c>'s body is non-empty. An empty UC (a page whose XAML did
	/// not change, e.g. a pure C#/code-behind edit) is correctly NOT reported as a XAML change. No live
	/// instance or main-thread dispatcher is needed: classification is a property of the type's UC body.
	/// </summary>
	[MetadataUpdateFact]
	public void UpdateRequested_ReportsXamlChange_WhenUpdateComponentBodyIsNonEmpty()
	{
		XamlHotReloadState.Reset();

		const string switchName = "Microsoft.Maui.RuntimeFeature.IsIncrementalHotReloadEnabled";
		AppContext.TryGetSwitch(switchName, out var previousSwitch);
		AppContext.SetSwitch(switchName, true);

		string Page(string text) => $$"""
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="{{text}}" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var xamlV1 = Page("Hello");
		var xamlV2 = Page("World");

		var (icV1, ucV1, icV2, ucV2, _, _) = RunSourceGenAllPhases(xamlV1, xamlV2, xamlV2);
		Assert.NotNull(ucV1); // v1: UpdateComponent() present but EMPTY (first generation, no XAML change)
		Assert.NotNull(ucV2); // v2: UpdateComponent() carries a patch (non-empty)

		var (peV1, pdbV1, compilationV1) = CompileSources(PageStub, icV1, StripGeneratedCodeAttribute(ucV1!));

		var alc = new AssemblyLoadContext("E2EClassifyTest", isCollectible: true);
		IReadOnlyList<Type>? lastHandled = null;
		EventHandler<HotReloadRequestedEventArgs> capture = (_, e) => lastHandled = e.HandledTypes;
		HotReloadDiagnostics.UpdateRequested += capture;
		try
		{
			var assembly = alc.LoadFromStream(new MemoryStream(peV1), new MemoryStream(pdbV1));
			var pageType = assembly.GetType(PageClass)!;

			// The loaded type's UpdateComponent() is EMPTY → the update is NOT a XAML change.
			XamlIncrementalHotReloadHandler.UpdateApplication(new[] { pageType });
			Assert.NotNull(lastHandled);
			Assert.DoesNotContain(pageType, lastHandled!);

			// Apply the V1→V2 delta: UpdateComponent()'s body becomes non-empty. The same notification now
			// classifies the type as a XAML change.
			var compilationV2 = CreateMauiCompilation(new[]
			{
				CSharpSyntaxTree.ParseText(PageStub, path: "PageStub.cs", encoding: System.Text.Encoding.UTF8),
				CSharpSyntaxTree.ParseText(icV2, path: "IC.cs", encoding: System.Text.Encoding.UTF8),
				CSharpSyntaxTree.ParseText(StripGeneratedCodeAttribute(ucV2!), path: "UC.cs", encoding: System.Text.Encoding.UTF8),
			});
			AssertNoCompileErrors(compilationV2, "V2");

			var baseline = EmitBaseline.CreateInitialBaseline(
				compilationV1, ModuleMetadata.CreateFromImage(peV1),
				debugInformationProvider: handle => default,
				localSignatureProvider: handle => default,
				hasPortableDebugInformation: true);

			ApplyMethodBodyDelta(assembly, compilationV1, compilationV2, baseline);
			XamlIncrementalHotReloadHandler.UpdateApplication(new[] { pageType });
			Assert.NotNull(lastHandled);
			Assert.Contains(pageType, lastHandled!);
		}
		finally
		{
			HotReloadDiagnostics.UpdateRequested -= capture;
			alc.Unload();
			AppContext.SetSwitch(switchName, previousSwitch);
		}
	}

	static void AssertNoCompileErrors(CSharpCompilation comp, string label)
	{
		var errors = comp.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
		if (errors.Length > 0)
			Assert.Fail($"{label} compilation failed:\n{string.Join("\n", errors.Select(e => $"{e.Id}: {e.GetMessage()}"))}");
	}

	/// <summary>
	/// Emits an EnC delta updating both <c>InitializeComponent</c> and <c>UpdateComponent</c> from
	/// <paramref name="oldComp"/> to <paramref name="newComp"/>, applies it to the live
	/// <paramref name="assembly"/>, and returns the updated baseline for the next delta. Every edit is
	/// an UPDATE (never Insert/Delete) because UpdateComponent() exists from the first generation.
	/// </summary>
	static EmitBaseline ApplyMethodBodyDelta(
		Assembly assembly, CSharpCompilation oldComp, CSharpCompilation newComp, EmitBaseline baseline)
	{
		var oldType = oldComp.GetTypeByMetadataName(PageClass)!;
		var newType = newComp.GetTypeByMetadataName(PageClass)!;

		var oldICDef = oldType.GetMembers("InitializeComponent").OfType<IMethodSymbol>().First();
		var newICDef = newType.GetMembers("InitializeComponent").OfType<IMethodSymbol>().First();

		var edits = new List<SemanticEdit>
		{
			new SemanticEdit(SemanticEditKind.Update,
				oldICDef.PartialImplementationPart ?? oldICDef,
				newICDef.PartialImplementationPart ?? newICDef),
			new SemanticEdit(SemanticEditKind.Update,
				oldType.GetMembers("UpdateComponent").Single(),
				newType.GetMembers("UpdateComponent").Single()),
		};

		using var mdDelta = new MemoryStream();
		using var ilDelta = new MemoryStream();
		using var pdbDelta = new MemoryStream();
		var result = newComp.EmitDifference(
			baseline, edits,
			isAddedSymbol: _ => false,
			mdDelta, ilDelta, pdbDelta,
			System.Threading.CancellationToken.None);
		Assert.True(result.Success, $"EmitDifference failed:\n{string.Join("\n", result.Diagnostics)}");

		MetadataUpdater.ApplyUpdate(assembly, mdDelta.ToArray(), ilDelta.ToArray(), pdbDelta.ToArray());
		return result.Baseline!;
	}

	/// <summary>
	/// The crown-jewel runtime proof of reverse version transitions (Kirill's requirement): a live
	/// instance created at V1 (Text="Hello"), hot-reloaded forward to V2 (Text="World"), then
	/// hot-reloaded BACKWARD to V1 (Text="Hello") — the live object's property must return to the
	/// baseline value. Because UpdateComponent() exists from the first generation, every transition is
	/// a method-body UPDATE (no member churn), and because patches are absolute the reverse patch
	/// deterministically restores the earlier value.
	/// </summary>
	[MetadataUpdateFact]
	public void PropertyRevert_AppliedViaHotReload_ReturnsToBaseline()
	{
		XamlHotReloadState.Reset();

		string Page(string text) => $$"""
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="{{text}}" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var xamlV1 = Page("Hello");
		var xamlV2 = Page("World");
		var xamlV3 = Page("Hello"); // revert — byte-identical to V1

		var (icV1, ucV1, icV2, ucV2, icV3, ucV3) = RunSourceGenAllPhases(xamlV1, xamlV2, xamlV3);
		Assert.NotNull(ucV1); // always-emit: UpdateComponent() is present from the first generation
		Assert.NotNull(ucV2);
		Assert.NotNull(ucV3);

		// Baseline V1 = IC + (empty) UC. Compiling UC into the baseline means every transition below
		// is an UPDATE of UpdateComponent(), never an Insert — the member never churns.
		var (peV1, pdbV1, compilationV1) = CompileSources(PageStub, icV1, StripGeneratedCodeAttribute(ucV1!));

		var alc = new AssemblyLoadContext("E2ERevertTest", isCollectible: true);
		try
		{
			var assembly = alc.LoadFromStream(new MemoryStream(peV1), new MemoryStream(pdbV1));
			var pageType = assembly.GetType(PageClass)!;
			var instance = Activator.CreateInstance(pageType)!;
			var page = (ContentPage)instance;

			string CurrentText() => ((Layout)page.Content!).Children.OfType<Label>().First().Text;
			Assert.Equal("Hello", CurrentText());

			var updateMethod = pageType.GetMethod("UpdateComponent",
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;

			CSharpCompilation Compile(string ic, string uc) => CreateMauiCompilation(new[]
			{
				CSharpSyntaxTree.ParseText(PageStub, path: "PageStub.cs", encoding: System.Text.Encoding.UTF8),
				CSharpSyntaxTree.ParseText(ic, path: "IC.cs", encoding: System.Text.Encoding.UTF8),
				CSharpSyntaxTree.ParseText(StripGeneratedCodeAttribute(uc), path: "UC.cs", encoding: System.Text.Encoding.UTF8),
			});

			var compilationV2 = Compile(icV2, ucV2!);
			var compilationV3 = Compile(icV3, ucV3!);
			AssertNoCompileErrors(compilationV2, "V2");
			AssertNoCompileErrors(compilationV3, "V3");

			var baseline = EmitBaseline.CreateInitialBaseline(
				compilationV1, ModuleMetadata.CreateFromImage(peV1),
				debugInformationProvider: handle => default,
				localSignatureProvider: handle => default,
				hasPortableDebugInformation: true);

			// --- Forward: V1 -> V2 (Hello -> World) ---
			baseline = ApplyMethodBodyDelta(assembly, compilationV1, compilationV2, baseline);
			updateMethod.Invoke(instance, null);
			Assert.Equal("World", CurrentText());

			// --- Reverse: V2 -> V3 (World -> Hello). The live instance must return to the baseline. ---
			baseline = ApplyMethodBodyDelta(assembly, compilationV2, compilationV3, baseline);
			updateMethod.Invoke(instance, null);
			Assert.Equal("Hello", CurrentText());
		}
		finally
		{
			alc.Unload();
		}
	}

	[Fact]
	public void IdenticalXaml_EmitsEmptyUC()
	{
		XamlHotReloadState.Reset();

		const string xaml = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="Hello" />
			</ContentPage>
			""";

		var (_, _, ucV2) = RunSourceGenTwoPhase(xaml, xaml);
		// UC is always emitted (present-but-empty for unchanged XAML) so the method never disappears
		// across generations. No content change → no patch statements in the body.
		Assert.NotNull(ucV2);
		Assert.Contains("internal void UpdateComponent()", ucV2!, StringComparison.Ordinal);
		Assert.DoesNotContain("XamlComponentRegistry", ucV2!, StringComparison.Ordinal);
	}

	[Fact]
	public void GeneratedIC_CompilesCleanly()
	{
		XamlHotReloadState.Reset();

		const string xaml = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			        <Button Text="Click me" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var (ic, _) = RunSourceGen(xaml);
		Assert.NotNull(ic);

		// This should compile without errors
		var (pe, _, _) = CompileSources(PageStub, ic!);
		Assert.True(pe.Length > 0, "Compiled assembly should not be empty");
	}

	[Fact]
	public void GeneratedUC_CompilesCleanly()
	{
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="World" />
			</ContentPage>
			""";

		var (_, icV2, ucV2) = RunSourceGenTwoPhase(xamlV1, xamlV2);
		Assert.NotNull(ucV2);

		// IC + UC + stub should compile together without errors
		var (pe, _, _) = CompileSources(PageStub, icV2, StripGeneratedCodeAttribute(ucV2!));
		Assert.True(pe.Length > 0, "Compiled assembly should not be empty");
	}

	// Regression test for https://github.com/dotnet/maui/issues/36256:
	// replacing the page's root content with a different control made UpdateComponent emit
	// `this.Content = (IView)__na_0;`, which fails to compile (CS0266) because ContentPage.Content
	// is typed View, not IView. The generated content-property assignment must not cast to IView.
	[Fact]
	public void RootContentReplaced_CompilesCleanly()
	{
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <CollectionView />
			</ContentPage>
			""";

		var (_, icV2, ucV2) = RunSourceGenTwoPhase(xamlV1, xamlV2);
		Assert.NotNull(ucV2);

		// The generated content assignment must be type-correct; CompileSources fails on CS0266.
		Assert.DoesNotContain(".Content = (global::Microsoft.Maui.IView)", ucV2!, StringComparison.Ordinal);
		var (pe, _, _) = CompileSources(PageStub, icV2, StripGeneratedCodeAttribute(ucV2!));
		Assert.True(pe.Length > 0, "Compiled assembly should not be empty");
	}

	[Fact]
	public void ResourceAdded_CompilesCleanly()
	{
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <Color x:Key="SecondaryColor">Red</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";

		var (_, icV2, ucV2) = RunSourceGenTwoPhase(xamlV1, xamlV2);
		Assert.NotNull(ucV2);

		// IC + UC + stub should compile together without errors
		var (pe, _, _) = CompileSources(PageStub, icV2, StripGeneratedCodeAttribute(ucV2!));
		Assert.True(pe.Length > 0, "Compiled assembly should not be empty");
	}

	[Fact]
	public void ResourceRemoved_CompilesCleanly()
	{
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <Color x:Key="SecondaryColor">Red</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";

		var (_, icV2, ucV2) = RunSourceGenTwoPhase(xamlV1, xamlV2);
		Assert.NotNull(ucV2);

		var (pe, _, _) = CompileSources(PageStub, icV2, StripGeneratedCodeAttribute(ucV2!));
		Assert.True(pe.Length > 0, "Compiled assembly should not be empty");
	}

	[MetadataUpdateFact]
	public void ResourceAdded_AppliedViaHotReload()
	{
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <Color x:Key="SecondaryColor">Red</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";

		var (icV1, icV2, ucV2) = RunSourceGenTwoPhase(xamlV1, xamlV2);
		Assert.NotNull(ucV2);

		var (peV1, pdbV1, compilationV1) = CompileSources(PageStub, icV1);

		var alc = new AssemblyLoadContext("E2EResourceAdd", isCollectible: true);
		try
		{
			var assembly = alc.LoadFromStream(new MemoryStream(peV1), new MemoryStream(pdbV1));
			var pageType = assembly.GetType(PageClass)!;
			var instance = Activator.CreateInstance(pageType)!;
			var page = (ContentPage)instance;

			// V1: only AccentColor
			Assert.True(page.Resources.ContainsKey("AccentColor"));
			Assert.False(page.Resources.ContainsKey("SecondaryColor"));

			// Compile V2 and apply delta
			var treesV2 = new[]
			{
				CSharpSyntaxTree.ParseText(PageStub, path: "PageStub.cs", encoding: System.Text.Encoding.UTF8),
				CSharpSyntaxTree.ParseText(icV2, path: "IC.cs", encoding: System.Text.Encoding.UTF8),
				CSharpSyntaxTree.ParseText(StripGeneratedCodeAttribute(ucV2), path: "UC.cs", encoding: System.Text.Encoding.UTF8),
			};
			var compilationV2 = CreateMauiCompilation(treesV2);
			var v2Errors = compilationV2.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
			if (v2Errors.Length > 0)
				Assert.Fail($"V2 compilation failed:\n{string.Join("\n", v2Errors.Select(e => $"{e.Id}: {e.GetMessage()}"))}");

			var baseline = EmitBaseline.CreateInitialBaseline(
				compilationV1,
				ModuleMetadata.CreateFromImage(peV1),
				debugInformationProvider: handle => default,
				localSignatureProvider: handle => default,
				hasPortableDebugInformation: true);

			var oldPageType = compilationV1.GetTypeByMetadataName(PageClass)!;
			var newPageType = compilationV2.GetTypeByMetadataName(PageClass)!;
			var edits = new List<SemanticEdit>();
			var oldIC = oldPageType.GetMembers("InitializeComponent").OfType<IMethodSymbol>().First();
			var newIC = newPageType.GetMembers("InitializeComponent").OfType<IMethodSymbol>().First();
			edits.Add(new SemanticEdit(SemanticEditKind.Update, oldIC.PartialImplementationPart ?? oldIC, newIC.PartialImplementationPart ?? newIC));
			edits.Add(new SemanticEdit(SemanticEditKind.Insert, null, newPageType.GetMembers("UpdateComponent").Single()));

			using var mdDelta = new MemoryStream();
			using var ilDelta = new MemoryStream();
			using var pdbDelta = new MemoryStream();
			var diffResult = compilationV2.EmitDifference(baseline, edits,
				isAddedSymbol: s => s.Name == "UpdateComponent",
				mdDelta, ilDelta, pdbDelta, System.Threading.CancellationToken.None);
			Assert.True(diffResult.Success, $"EmitDifference failed:\n{string.Join("\n", diffResult.Diagnostics)}");

			MetadataUpdater.ApplyUpdate(assembly, mdDelta.ToArray(), ilDelta.ToArray(), pdbDelta.ToArray());

			var updateMethod = pageType.GetMethod("UpdateComponent", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			Assert.NotNull(updateMethod);
			updateMethod!.Invoke(instance, null);

			// V2: both resources exist
			Assert.True(page.Resources.ContainsKey("AccentColor"), "AccentColor should still exist");
			Assert.True(page.Resources.ContainsKey("SecondaryColor"), "SecondaryColor should be added");
			Assert.Equal(Microsoft.Maui.Graphics.Colors.Red, page.Resources["SecondaryColor"]);
		}
		finally
		{
			alc.Unload();
		}
	}

	[MetadataUpdateFact]
	public void ResourceRemoved_AppliedViaHotReload()
	{
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <Color x:Key="SecondaryColor">Red</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";

		var (icV1, icV2, ucV2) = RunSourceGenTwoPhase(xamlV1, xamlV2);
		Assert.NotNull(ucV2);

		var (peV1, pdbV1, compilationV1) = CompileSources(PageStub, icV1);

		var alc = new AssemblyLoadContext("E2EResourceRemove", isCollectible: true);
		try
		{
			var assembly = alc.LoadFromStream(new MemoryStream(peV1), new MemoryStream(pdbV1));
			var pageType = assembly.GetType(PageClass)!;
			var instance = Activator.CreateInstance(pageType)!;
			var page = (ContentPage)instance;

			// V1: both resources
			Assert.True(page.Resources.ContainsKey("AccentColor"));
			Assert.True(page.Resources.ContainsKey("SecondaryColor"));

			// Compile V2 and apply delta
			var treesV2 = new[]
			{
				CSharpSyntaxTree.ParseText(PageStub, path: "PageStub.cs", encoding: System.Text.Encoding.UTF8),
				CSharpSyntaxTree.ParseText(icV2, path: "IC.cs", encoding: System.Text.Encoding.UTF8),
				CSharpSyntaxTree.ParseText(StripGeneratedCodeAttribute(ucV2), path: "UC.cs", encoding: System.Text.Encoding.UTF8),
			};
			var compilationV2 = CreateMauiCompilation(treesV2);
			var v2Errors = compilationV2.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
			if (v2Errors.Length > 0)
				Assert.Fail($"V2 compilation failed:\n{string.Join("\n", v2Errors.Select(e => $"{e.Id}: {e.GetMessage()}"))}");

			var baseline = EmitBaseline.CreateInitialBaseline(
				compilationV1,
				ModuleMetadata.CreateFromImage(peV1),
				debugInformationProvider: handle => default,
				localSignatureProvider: handle => default,
				hasPortableDebugInformation: true);

			var oldPageType = compilationV1.GetTypeByMetadataName(PageClass)!;
			var newPageType = compilationV2.GetTypeByMetadataName(PageClass)!;
			var edits = new List<SemanticEdit>();
			var oldIC = oldPageType.GetMembers("InitializeComponent").OfType<IMethodSymbol>().First();
			var newIC = newPageType.GetMembers("InitializeComponent").OfType<IMethodSymbol>().First();
			edits.Add(new SemanticEdit(SemanticEditKind.Update, oldIC.PartialImplementationPart ?? oldIC, newIC.PartialImplementationPart ?? newIC));
			edits.Add(new SemanticEdit(SemanticEditKind.Insert, null, newPageType.GetMembers("UpdateComponent").Single()));

			using var mdDelta = new MemoryStream();
			using var ilDelta = new MemoryStream();
			using var pdbDelta = new MemoryStream();
			var diffResult = compilationV2.EmitDifference(baseline, edits,
				isAddedSymbol: s => s.Name == "UpdateComponent",
				mdDelta, ilDelta, pdbDelta, System.Threading.CancellationToken.None);
			Assert.True(diffResult.Success, $"EmitDifference failed:\n{string.Join("\n", diffResult.Diagnostics)}");

			MetadataUpdater.ApplyUpdate(assembly, mdDelta.ToArray(), ilDelta.ToArray(), pdbDelta.ToArray());

			var updateMethod = pageType.GetMethod("UpdateComponent", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			Assert.NotNull(updateMethod);
			updateMethod!.Invoke(instance, null);

			// V2: SecondaryColor removed, AccentColor remains
			Assert.True(page.Resources.ContainsKey("AccentColor"), "AccentColor should still exist");
			Assert.False(page.Resources.ContainsKey("SecondaryColor"), "SecondaryColor should be removed");
		}
		finally
		{
			alc.Unload();
		}
	}

	[MetadataUpdateFact]
	public void ResourceValueChanged_AppliedViaHotReload()
	{
		XamlHotReloadState.Reset();

		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">Red</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";

		var (icV1, icV2, ucV2) = RunSourceGenTwoPhase(xamlV1, xamlV2);
		Assert.NotNull(ucV2);

		var (peV1, pdbV1, compilationV1) = CompileSources(PageStub, icV1);

		var alc = new AssemblyLoadContext("E2EResourceChange", isCollectible: true);
		try
		{
			var assembly = alc.LoadFromStream(new MemoryStream(peV1), new MemoryStream(pdbV1));
			var pageType = assembly.GetType(PageClass)!;
			var instance = Activator.CreateInstance(pageType)!;
			var page = (ContentPage)instance;

			// V1: AccentColor is DarkBlue
			Assert.Equal(Microsoft.Maui.Graphics.Colors.DarkBlue, page.Resources["AccentColor"]);

			// Compile V2 and apply delta
			var treesV2 = new[]
			{
				CSharpSyntaxTree.ParseText(PageStub, path: "PageStub.cs", encoding: System.Text.Encoding.UTF8),
				CSharpSyntaxTree.ParseText(icV2, path: "IC.cs", encoding: System.Text.Encoding.UTF8),
				CSharpSyntaxTree.ParseText(StripGeneratedCodeAttribute(ucV2), path: "UC.cs", encoding: System.Text.Encoding.UTF8),
			};
			var compilationV2 = CreateMauiCompilation(treesV2);
			var v2Errors = compilationV2.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
			if (v2Errors.Length > 0)
				Assert.Fail($"V2 compilation failed:\n{string.Join("\n", v2Errors.Select(e => $"{e.Id}: {e.GetMessage()}"))}");

			var baseline = EmitBaseline.CreateInitialBaseline(
				compilationV1,
				ModuleMetadata.CreateFromImage(peV1),
				debugInformationProvider: handle => default,
				localSignatureProvider: handle => default,
				hasPortableDebugInformation: true);

			var oldPageType = compilationV1.GetTypeByMetadataName(PageClass)!;
			var newPageType = compilationV2.GetTypeByMetadataName(PageClass)!;
			var edits = new List<SemanticEdit>();
			var oldIC = oldPageType.GetMembers("InitializeComponent").OfType<IMethodSymbol>().First();
			var newIC = newPageType.GetMembers("InitializeComponent").OfType<IMethodSymbol>().First();
			edits.Add(new SemanticEdit(SemanticEditKind.Update, oldIC.PartialImplementationPart ?? oldIC, newIC.PartialImplementationPart ?? newIC));
			edits.Add(new SemanticEdit(SemanticEditKind.Insert, null, newPageType.GetMembers("UpdateComponent").Single()));

			using var mdDelta = new MemoryStream();
			using var ilDelta = new MemoryStream();
			using var pdbDelta = new MemoryStream();
			var diffResult = compilationV2.EmitDifference(baseline, edits,
				isAddedSymbol: s => s.Name == "UpdateComponent",
				mdDelta, ilDelta, pdbDelta, System.Threading.CancellationToken.None);
			Assert.True(diffResult.Success, $"EmitDifference failed:\n{string.Join("\n", diffResult.Diagnostics)}");

			MetadataUpdater.ApplyUpdate(assembly, mdDelta.ToArray(), ilDelta.ToArray(), pdbDelta.ToArray());

			var updateMethod = pageType.GetMethod("UpdateComponent", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			Assert.NotNull(updateMethod);
			updateMethod!.Invoke(instance, null);

			// V2: AccentColor changed to Red
			Assert.Equal(Microsoft.Maui.Graphics.Colors.Red, page.Resources["AccentColor"]);
		}
		finally
		{
			alc.Unload();
		}
	}

	sealed class OptionsProvider : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider
	{
		readonly SourceGeneratorDriver.AdditionalFile[] _files;
		public OptionsProvider(SourceGeneratorDriver.AdditionalFile[] files) => _files = files;

		public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GlobalOptions =>
			throw new NotImplementedException();

		public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(SyntaxTree tree) =>
			throw new NotImplementedException();

		public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(AdditionalText textFile)
		{
			foreach (var f in _files)
			{
				if (f.Text.Path == textFile.Path)
					return new SimpleOptions(f);
			}
			return EmptyOptions.Instance;
		}
	}

	sealed class SimpleOptions : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions
	{
		readonly SourceGeneratorDriver.AdditionalFile _file;
		public SimpleOptions(SourceGeneratorDriver.AdditionalFile file) => _file = file;

		public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
		{
			value = key switch
			{
				"build_metadata.additionalfiles.GenKind" => _file.Kind,
				"build_metadata.additionalfiles.TargetPath" => _file.TargetPath,
				"build_metadata.additionalfiles.ManifestResourceName" => _file.ManifestResourceName,
				"build_metadata.additionalfiles.RelativePath" => _file.RelativePath,
				"build_metadata.additionalfiles.Inflator" => "SourceGen",
				"build_property.targetFramework" => _file.TargetFramework,
				"build_property.Configuration" => "Debug",
				"build_property.EnableMauiXamlDiagnostics" => "true",
				"build_property.EnableMauiIncrementalHotReload" => _file.EnableIncrementalHotReload ? "true" : "false",
				_ => null
			};
			return value is not null;
		}
	}

	sealed class EmptyOptions : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions
	{
		public static readonly EmptyOptions Instance = new();
		public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
		{ value = null; return false; }
	}
}
