#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;

internal sealed class XamlHotReloadTestHarness : IDisposable
{
	static readonly Regex GeneratedCodeAttributeRegex = new(
		@"\[global::System\.CodeDom\.Compiler\.GeneratedCodeAttribute\([^\]]+\)\]\s*\n",
		RegexOptions.CultureInvariant);

	static int s_scenarioSequence;

	readonly ImmutableArray<string> _additionalSources;
	readonly string _pageSource;
	bool _disposed;
	bool _generated;

	public XamlHotReloadTestHarness(
		string scenarioName,
		string pageClass,
		string pageSource,
		params string[] additionalSources)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(scenarioName);
		ArgumentException.ThrowIfNullOrWhiteSpace(pageClass);
		ArgumentException.ThrowIfNullOrWhiteSpace(pageSource);
		ArgumentNullException.ThrowIfNull(additionalSources);

		var sequence = Interlocked.Increment(ref s_scenarioSequence);
		ScenarioIdentity = $"{SanitizeIdentifier(scenarioName)}_{sequence}";
		AssemblyName = $"XamlHotReload_{ScenarioIdentity}";
		PageClass = pageClass;
		XamlPath = Path.Combine(Path.GetTempPath(), "MauiSourceGenHotReload", ScenarioIdentity, "MainPage.xaml");
		_pageSource = pageSource;
		_additionalSources = [.. additionalSources];

		XamlHotReloadState.Reset();
	}

	public string AssemblyName { get; }

	public string PageClass { get; }

	public string ScenarioIdentity { get; }

	public string XamlPath { get; }

	public XamlHotReloadGeneration Generate(params string[] xamlVersions)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(xamlVersions);

		if (_generated)
			throw new InvalidOperationException("A hot reload harness can generate only one scenario.");
		if (xamlVersions.Length == 0)
			throw new ArgumentException("At least one XAML version is required.", nameof(xamlVersions));

		_generated = true;

		var compilation = CreateCompilation(includeGeneratedSources: false);
		var driverOptions = new GeneratorDriverOptions(
			disabledOutputs: IncrementalGeneratorOutputKind.None,
			trackIncrementalGeneratorSteps: true);
		GeneratorDriver driver = CSharpGeneratorDriver.Create(
			[new XamlGenerator().AsSourceGenerator()],
			driverOptions: driverOptions);

		var versions = ImmutableArray.CreateBuilder<XamlHotReloadGeneratedVersion>(xamlVersions.Length);
		SourceGeneratorDriver.AdditionalFile? previousFile = null;

		for (var index = 0; index < xamlVersions.Length; index++)
		{
			var file = CreateAdditionalFile(xamlVersions[index]);
			driver = previousFile is null
				? driver.AddAdditionalTexts([file.Text])
				: driver.ReplaceAdditionalText(previousFile.Text, file.Text);
			driver = driver
				.WithUpdatedAnalyzerConfigOptions(SourceGeneratorDriver.CreateAnalyzerConfigOptionsProvider(file))
				.RunGenerators(compilation);

			var result = driver.GetRunResult();
			var initializeComponentSource = FindGeneratedSource(result, static hintName =>
				hintName.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase)
				&& !hintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase));
			Assert.NotNull(initializeComponentSource);

			versions.Add(new XamlHotReloadGeneratedVersion(
				ScenarioIdentity,
				index,
				xamlVersions[index],
				initializeComponentSource!,
				FindGeneratedSource(result, static hintName =>
					hintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase)),
				result));
			previousFile = file;
		}

		return new XamlHotReloadGeneration(ScenarioIdentity, versions.MoveToImmutable());
	}

	public XamlHotReloadCompiledVersion Compile(XamlHotReloadGeneratedVersion version)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(version);

		if (!string.Equals(version.ScenarioIdentity, ScenarioIdentity, StringComparison.Ordinal))
			throw new ArgumentException("The generated version belongs to a different harness.", nameof(version));

		var compilation = CreateCompilation(
			includeGeneratedSources: true,
			version.InitializeComponentSource,
			version.UpdateComponentSource);
		var errors = compilation.GetDiagnostics()
			.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();
		Assert.True(errors.Length == 0, $"Compilation failed:{Environment.NewLine}{FormatDiagnostics(errors)}");

		using var peStream = new MemoryStream();
		using var pdbStream = new MemoryStream();
		var emitResult = compilation.Emit(
			peStream,
			pdbStream,
			options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));
		Assert.True(emitResult.Success, $"Emit failed:{Environment.NewLine}{FormatDiagnostics(emitResult.Diagnostics)}");

		return new XamlHotReloadCompiledVersion(
			version,
			compilation,
			ImmutableArray.CreateRange(peStream.ToArray()),
			ImmutableArray.CreateRange(pdbStream.ToArray()));
	}

	public void RunLive(XamlHotReloadGeneration generation, Action<XamlHotReloadLiveSession> exercise)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(exercise);

		if (!string.Equals(generation.ScenarioIdentity, ScenarioIdentity, StringComparison.Ordinal))
			throw new ArgumentException("The generated scenario belongs to a different harness.", nameof(generation));

		using var session = new XamlHotReloadLiveSession(this, generation);
		exercise(session);
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		XamlHotReloadState.Reset();
	}

	CSharpCompilation CreateCompilation(
		bool includeGeneratedSources,
		string? initializeComponentSource = null,
		string? updateComponentSource = null)
	{
		var compilation = (CSharpCompilation)SourceGeneratorDriver.CreateMauiCompilation(AssemblyName);
		var trees = ImmutableArray.CreateBuilder<SyntaxTree>();
		trees.Add(ParseSource(_pageSource, "Page.cs"));

		for (var index = 0; index < _additionalSources.Length; index++)
			trees.Add(ParseSource(_additionalSources[index], $"Additional{index}.cs"));

		if (includeGeneratedSources)
		{
			ArgumentNullException.ThrowIfNull(initializeComponentSource);
			trees.Add(ParseSource(initializeComponentSource, "InitializeComponent.xsg.cs"));
			if (updateComponentSource is not null)
				trees.Add(ParseSource(StripGeneratedCodeAttribute(updateComponentSource), "UpdateComponent.uc.xsg.cs"));
		}

		return compilation.AddSyntaxTrees(trees);
	}

	SourceGeneratorDriver.AdditionalFile CreateAdditionalFile(string xaml) =>
		new(
			SourceGeneratorDriver.ToAdditionalText(XamlPath, xaml),
			Kind: "Xaml",
			RelativePath: "MainPage.xaml",
			TargetPath: $"{AssemblyName}/MainPage.xaml",
			ManifestResourceName: null,
			TargetFramework: "net11.0",
			NoWarn: null,
			EnableIncrementalHotReload: true);

	static SyntaxTree ParseSource(string source, string path) =>
		CSharpSyntaxTree.ParseText(source, path: path, encoding: Encoding.UTF8);

	static string? FindGeneratedSource(
		GeneratorDriverRunResult result,
		Func<string, bool> hintNamePredicate)
	{
		foreach (var generatorResult in result.Results)
		{
			foreach (var source in generatorResult.GeneratedSources)
			{
				if (hintNamePredicate(source.HintName))
					return source.SourceText.ToString();
			}
		}

		return null;
	}

	static string StripGeneratedCodeAttribute(string source) =>
		GeneratedCodeAttributeRegex.Replace(source, "");

	static string FormatDiagnostics(IEnumerable<Diagnostic> diagnostics) =>
		string.Join(Environment.NewLine, diagnostics.Select(static diagnostic =>
			$"{diagnostic.Id}: {diagnostic.GetMessage()} at {diagnostic.Location}"));

	static string SanitizeIdentifier(string value)
	{
		var characters = value
			.Select(static character => char.IsLetterOrDigit(character) ? character : '_')
			.ToArray();
		return new string(characters);
	}
}

internal sealed record XamlHotReloadGeneration(
	string ScenarioIdentity,
	ImmutableArray<XamlHotReloadGeneratedVersion> Versions)
{
	public XamlHotReloadGeneratedVersion this[int index] => Versions[index];
}

internal sealed record XamlHotReloadGeneratedVersion(
	string ScenarioIdentity,
	int Index,
	string Xaml,
	string InitializeComponentSource,
	string? UpdateComponentSource,
	GeneratorDriverRunResult GeneratorResult);

internal sealed record XamlHotReloadCompiledVersion(
	XamlHotReloadGeneratedVersion GeneratedVersion,
	CSharpCompilation Compilation,
	ImmutableArray<byte> PeImage,
	ImmutableArray<byte> PdbImage);

internal sealed class XamlHotReloadLiveSession : IDisposable
{
	readonly XamlHotReloadTestHarness _harness;
	readonly XamlHotReloadGeneration _generation;
	Assembly? _assembly;
	EmitBaseline? _baseline;
	XamlHotReloadCompiledVersion? _compiledVersion;
	object? _instance;
	AssemblyLoadContext? _loadContext;
	ModuleMetadata? _moduleMetadata;
	Type? _pageType;
	bool _disposed;
	int _versionIndex;

	public XamlHotReloadLiveSession(
		XamlHotReloadTestHarness harness,
		XamlHotReloadGeneration generation)
	{
		_harness = harness;
		_generation = generation;
		_compiledVersion = harness.Compile(generation[0]);
		_moduleMetadata = ModuleMetadata.CreateFromImage(_compiledVersion.PeImage);
		_baseline = EmitBaseline.CreateInitialBaseline(
			_compiledVersion.Compilation,
			_moduleMetadata,
			debugInformationProvider: static _ => default,
			localSignatureProvider: static _ => default,
			hasPortableDebugInformation: true);

		_loadContext = new AssemblyLoadContext(
			$"XamlHotReload_{harness.ScenarioIdentity}",
			isCollectible: true);
		Assert.True(_loadContext.IsCollectible);

		using var peStream = new MemoryStream(_compiledVersion.PeImage.ToArray(), writable: false);
		using var pdbStream = new MemoryStream(_compiledVersion.PdbImage.ToArray(), writable: false);
		_assembly = _loadContext.LoadFromStream(peStream, pdbStream);
		_pageType = _assembly.GetType(harness.PageClass);
		Assert.NotNull(_pageType);
		_instance = Activator.CreateInstance(_pageType!);
		Assert.NotNull(_instance);
	}

	public object Instance =>
		_instance ?? throw new ObjectDisposedException(nameof(XamlHotReloadLiveSession));

	public T GetInstance<T>() where T : class =>
		Assert.IsAssignableFrom<T>(Instance);

	public T ApplyUpdate<T>(int versionIndex) where T : class
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		if (versionIndex != _versionIndex + 1)
			throw new ArgumentOutOfRangeException(nameof(versionIndex), "Updates must be applied sequentially.");

		var instance = Instance;
		var nextVersion = _harness.Compile(_generation[versionIndex]);
		var edits = CreateSemanticEdits(
			_compiledVersion!.Compilation,
			nextVersion.Compilation,
			_harness.PageClass);
		var insertsUpdateComponent = _compiledVersion.Compilation
			.GetTypeByMetadataName(_harness.PageClass)!
			.GetMembers("UpdateComponent")
			.Length == 0;

		using var metadataDelta = new MemoryStream();
		using var ilDelta = new MemoryStream();
		using var pdbDelta = new MemoryStream();
		var difference = nextVersion.Compilation.EmitDifference(
			_baseline!,
			edits,
			isAddedSymbol: symbol => insertsUpdateComponent && symbol.Name == "UpdateComponent",
			metadataDelta,
			ilDelta,
			pdbDelta,
			CancellationToken.None);
		Assert.True(
			difference.Success,
			$"EmitDifference failed:{Environment.NewLine}{string.Join(Environment.NewLine, difference.Diagnostics)}");

		MetadataUpdater.ApplyUpdate(
			_assembly!,
			metadataDelta.ToArray(),
			ilDelta.ToArray(),
			pdbDelta.ToArray());

		var updateMethod = _pageType!.GetMethod(
			"UpdateComponent",
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		Assert.NotNull(updateMethod);
		updateMethod!.Invoke(instance, null);
		Assert.Same(instance, Instance);

		_baseline = difference.Baseline;
		_compiledVersion = nextVersion;
		_versionIndex = versionIndex;
		return Assert.IsAssignableFrom<T>(Instance);
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		if (_instance is not null)
			global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.Unregister(_instance);
		_instance = null;
		_pageType = null;
		_assembly = null;
		_compiledVersion = null;
		_baseline = null;
		_moduleMetadata?.Dispose();
		_moduleMetadata = null;

		var loadContext = _loadContext;
		_loadContext = null;
		loadContext?.Unload();
	}

	static ImmutableArray<SemanticEdit> CreateSemanticEdits(
		CSharpCompilation previousCompilation,
		CSharpCompilation nextCompilation,
		string pageClass)
	{
		var previousPageType = previousCompilation.GetTypeByMetadataName(pageClass);
		var nextPageType = nextCompilation.GetTypeByMetadataName(pageClass);
		Assert.NotNull(previousPageType);
		Assert.NotNull(nextPageType);

		var previousInitializeComponent = GetPartialImplementation(previousPageType!, "InitializeComponent");
		var nextInitializeComponent = GetPartialImplementation(nextPageType!, "InitializeComponent");
		var edits = ImmutableArray.CreateBuilder<SemanticEdit>();
		edits.Add(new SemanticEdit(
			SemanticEditKind.Update,
			previousInitializeComponent,
			nextInitializeComponent));

		var previousUpdateComponent = previousPageType!
			.GetMembers("UpdateComponent")
			.OfType<IMethodSymbol>()
			.SingleOrDefault();
		var nextUpdateComponent = nextPageType!
			.GetMembers("UpdateComponent")
			.OfType<IMethodSymbol>()
			.SingleOrDefault();
		Assert.NotNull(nextUpdateComponent);
		edits.Add(previousUpdateComponent is null
			? new SemanticEdit(SemanticEditKind.Insert, null, nextUpdateComponent)
			: new SemanticEdit(SemanticEditKind.Update, previousUpdateComponent, nextUpdateComponent));

		return edits.ToImmutable();
	}

	static IMethodSymbol GetPartialImplementation(INamedTypeSymbol type, string methodName)
	{
		var method = type.GetMembers(methodName).OfType<IMethodSymbol>().First();
		return method.PartialImplementationPart ?? method;
	}
}
