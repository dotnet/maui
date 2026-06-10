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

	// -----------------------------------------------------------------------
	// Helpers
	// -----------------------------------------------------------------------

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

	static void AssertHotReloadSupported()
	{
		Assert.True(MetadataUpdater.IsSupported,
			"MetadataUpdater.IsSupported is false. " +
			"Ensure <MetadataUpdaterSupport>true</MetadataUpdaterSupport> is set in the project, " +
			"and for Mono, set DOTNET_MODIFIABLE_ASSEMBLIES=debug.");
	}

	// -----------------------------------------------------------------------
	// Stub class that the generated partial class extends
	// -----------------------------------------------------------------------

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

	// -----------------------------------------------------------------------
	// Tests
	// -----------------------------------------------------------------------

	[Fact]
	public void PropertyChange_AppliedViaHotReload()
	{
		AssertHotReloadSupported();
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

			// __version field syntax changed (initializer may differ) — but fields cannot be updated via EnC.
			// The __version field was already emitted in V1 as `private int __version = 0;`.
			// In V2, it's still `private int __version = 0;` in the generated code.

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

	[Fact]
	public void MultiplePropertyChanges_ChainedPatches()
	{
		AssertHotReloadSupported();
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

		// Run source gen: V1 seeds, V2 produces UC with patch v0→v1
		var (icV1, icV2, ucV2) = RunSourceGenTwoPhase(xamlV1, xamlV2);
		Assert.NotNull(ucV2);

		// Verify UC contains the expected version guard
		Assert.Contains("__version == 0", ucV2!, StringComparison.Ordinal);
		Assert.Contains("__version = 1", ucV2!, StringComparison.Ordinal);

		// Verify UC contains the new property value
		Assert.Contains("\"World\"", ucV2!, StringComparison.Ordinal);
		Assert.Contains("\"V2\"", ucV2!, StringComparison.Ordinal);
	}

	[Fact]
	public void IdenticalXaml_NoUCGenerated()
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
		Assert.Null(ucV2); // No change → no UC
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

	[Fact]
	public void ResourceAdded_AppliedViaHotReload()
	{
		AssertHotReloadSupported();
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

	[Fact]
	public void ResourceRemoved_AppliedViaHotReload()
	{
		AssertHotReloadSupported();
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

	[Fact]
	public void ResourceValueChanged_AppliedViaHotReload()
	{
		AssertHotReloadSupported();
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

	// -----------------------------------------------------------------------
	// OptionsProvider (same as in pipeline tests)
	// -----------------------------------------------------------------------

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
				"build_property.EnableMauiIncrementalHotReload" => _file.EnableIncrementalHotReload ? "true" : null,
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
