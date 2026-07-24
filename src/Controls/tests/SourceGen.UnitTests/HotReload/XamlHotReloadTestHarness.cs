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
	readonly XamlHotReloadHostFixture? _applicationHost;
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

	public XamlHotReloadTestHarness(
		string scenarioName,
		string pageClass,
		string pageSource,
		XamlHotReloadApplicationOptions applicationOptions,
		params string[] additionalSources)
		: this(scenarioName, pageClass, pageSource, additionalSources)
	{
		_applicationHost = new XamlHotReloadHostFixture(applicationOptions);
	}

	public string AssemblyName { get; }

	public XamlHotReloadHostFixture? ApplicationHost => _applicationHost;

	public string PageClass { get; }

	public string ScenarioIdentity { get; }

	public string XamlPath { get; }

	public XamlHotReloadGeneration Generate(params string[] xamlVersions)
		=> Generate(CreateMainPageSnapshots(xamlVersions), allowDiagnostics: false);

	public XamlHotReloadGeneration GenerateAllowingDiagnostics(params string[] xamlVersions)
		=> Generate(CreateMainPageSnapshots(xamlVersions), allowDiagnostics: true);

	public XamlHotReloadGeneration GenerateDocuments(
		params IReadOnlyDictionary<string, XamlHotReloadDocument>[] documentVersions)
		=> Generate(documentVersions, allowDiagnostics: false);

	public XamlHotReloadGeneration GenerateDocumentsAllowingDiagnostics(
		params IReadOnlyDictionary<string, XamlHotReloadDocument>[] documentVersions)
		=> Generate(documentVersions, allowDiagnostics: true);

	XamlHotReloadGeneration Generate(
		IReadOnlyDictionary<string, XamlHotReloadDocument>[] documentVersions,
		bool allowDiagnostics)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(documentVersions);

		if (_generated)
			throw new InvalidOperationException("A hot reload harness can generate only one scenario.");
		if (documentVersions.Length == 0)
			throw new ArgumentException("At least one document version is required.", nameof(documentVersions));

		_generated = true;

		var compilation = CreateCompilation(includeGeneratedSources: false);
		var driverOptions = new GeneratorDriverOptions(
			disabledOutputs: IncrementalGeneratorOutputKind.None,
			trackIncrementalGeneratorSteps: true);
		GeneratorDriver driver = CSharpGeneratorDriver.Create(
			[new XamlGenerator().AsSourceGenerator()],
			driverOptions: driverOptions);

		var versions = ImmutableArray.CreateBuilder<XamlHotReloadGeneratedVersion>(documentVersions.Length);
		var previousFiles = ImmutableDictionary<string, XamlHotReloadAdditionalFile>.Empty;

		for (var index = 0; index < documentVersions.Length; index++)
		{
			var files = CreateAdditionalFiles(documentVersions[index]);
			driver = UpdateAdditionalTexts(driver, previousFiles, files);
			driver = driver
				.WithUpdatedAnalyzerConfigOptions(SourceGeneratorDriver.CreateAnalyzerConfigOptionsProvider(
					[.. files.Values.Select(static file => file.File)]))
				.RunGenerators(compilation);
			var stateVersion = XamlHotReloadState.GetVersion(AssemblyName, "net11.0", XamlPath);

			var result = driver.GetRunResult();
			var generatedRoots = FindGeneratedRoots(result);
			var initializeComponentSource = generatedRoots
				.FirstOrDefault(root => string.Equals(root.TypeName, PageClass, StringComparison.Ordinal))
				?.InitializeComponentSource;
			if (!allowDiagnostics)
				Assert.NotNull(initializeComponentSource);

			versions.Add(new XamlHotReloadGeneratedVersion(
				ScenarioIdentity,
				index,
				files["MainPage.xaml"].Document.Text,
				initializeComponentSource,
				generatedRoots
					.FirstOrDefault(root => string.Equals(root.TypeName, PageClass, StringComparison.Ordinal))
					?.UpdateComponentSource,
				stateVersion,
				result,
				generatedRoots,
				files.ToImmutableDictionary(
					static pair => pair.Key,
					static pair => pair.Value.Document)));
			previousFiles = files;
		}

		return new XamlHotReloadGeneration(ScenarioIdentity, versions.MoveToImmutable());
	}

	public XamlHotReloadCompiledVersion Compile(XamlHotReloadGeneratedVersion version)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(version);

		if (!string.Equals(version.ScenarioIdentity, ScenarioIdentity, StringComparison.Ordinal))
			throw new ArgumentException("The generated version belongs to a different harness.", nameof(version));

		Assert.True(
			version.InitializeComponentSource is not null,
			$"Cannot compile generated version {version.Index} because it has no InitializeComponent source.");

		var compilation = CreateCompilation(includeGeneratedSources: true, version);
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
		try
		{
			XamlHotReloadState.Reset();
		}
		finally
		{
			_applicationHost?.Dispose();
		}
	}

	CSharpCompilation CreateCompilation(
		bool includeGeneratedSources,
		XamlHotReloadGeneratedVersion? generatedVersion = null)
	{
		var compilation = (CSharpCompilation)SourceGeneratorDriver.CreateMauiCompilation(AssemblyName);
		var trees = ImmutableArray.CreateBuilder<SyntaxTree>();
		trees.Add(ParseSource(_pageSource, "Page.cs"));

		for (var index = 0; index < _additionalSources.Length; index++)
			trees.Add(ParseSource(_additionalSources[index], $"Additional{index}.cs"));

		if (includeGeneratedSources)
		{
			ArgumentNullException.ThrowIfNull(generatedVersion);
			var sourceCompilation = compilation.AddSyntaxTrees(trees);

			foreach (var root in generatedVersion.GeneratedRoots)
			{
				if (sourceCompilation.GetTypeByMetadataName(root.TypeName) is null)
					continue;

				if (root.InitializeComponentSource is not null)
					trees.Add(ParseSource(root.InitializeComponentSource, $"{root.TypeName}.InitializeComponent.xsg.cs"));
				if (root.UpdateComponentSource is not null)
					trees.Add(ParseSource(StripGeneratedCodeAttribute(root.UpdateComponentSource), $"{root.TypeName}.UpdateComponent.uc.xsg.cs"));
			}
		}

		return compilation.AddSyntaxTrees(trees);
	}

	IReadOnlyDictionary<string, XamlHotReloadDocument>[] CreateMainPageSnapshots(string[] xamlVersions)
	{
		ArgumentNullException.ThrowIfNull(xamlVersions);
		return xamlVersions
			.Select(static xaml => (IReadOnlyDictionary<string, XamlHotReloadDocument>)
				new Dictionary<string, XamlHotReloadDocument>(StringComparer.Ordinal)
				{
					["MainPage.xaml"] = new XamlHotReloadDocument(xaml),
				})
			.ToArray();
	}

	ImmutableDictionary<string, XamlHotReloadAdditionalFile> CreateAdditionalFiles(
		IReadOnlyDictionary<string, XamlHotReloadDocument> documents)
	{
		ArgumentNullException.ThrowIfNull(documents);
		if (!documents.ContainsKey("MainPage.xaml"))
			throw new ArgumentException("Each document snapshot must include MainPage.xaml.", nameof(documents));

		var files = ImmutableDictionary.CreateBuilder<string, XamlHotReloadAdditionalFile>(StringComparer.Ordinal);
		foreach (var document in documents)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(document.Key);
			ArgumentNullException.ThrowIfNull(document.Value);
			ArgumentNullException.ThrowIfNull(document.Value.Text);

			var relativePath = document.Key.Replace('\\', '/');
			if (Path.IsPathRooted(relativePath) || relativePath.Split('/').Any(static segment => segment == ".."))
				throw new ArgumentException("Document paths must be relative to the test project.", nameof(documents));

			var xamlPath = Path.Combine(Path.GetDirectoryName(XamlPath)!, relativePath);
			var options = document.Value;
			var file = new SourceGeneratorDriver.AdditionalFile(
				SourceGeneratorDriver.ToAdditionalText(xamlPath, options.Text),
				options.Kind,
				relativePath,
				options.TargetPath ?? $"{AssemblyName}/{relativePath}",
				options.ManifestResourceName,
				options.TargetFramework,
				options.NoWarn,
				EnableIncrementalHotReload: true);
			files.Add(document.Key, new XamlHotReloadAdditionalFile(options, file));
		}

		return files.ToImmutable();
	}

	static GeneratorDriver UpdateAdditionalTexts(
		GeneratorDriver driver,
		ImmutableDictionary<string, XamlHotReloadAdditionalFile> previousFiles,
		ImmutableDictionary<string, XamlHotReloadAdditionalFile> currentFiles)
	{
		foreach (var previousFile in previousFiles)
		{
			if (!currentFiles.ContainsKey(previousFile.Key))
				driver = driver.RemoveAdditionalTexts([previousFile.Value.File.Text]);
		}

		foreach (var currentFile in currentFiles)
		{
			if (!previousFiles.TryGetValue(currentFile.Key, out var previousFile))
				driver = driver.AddAdditionalTexts([currentFile.Value.File.Text]);
			else if (previousFile.Document != currentFile.Value.Document)
				driver = driver.ReplaceAdditionalText(previousFile.File.Text, currentFile.Value.File.Text);
		}

		return driver;
	}

	static SyntaxTree ParseSource(string source, string path) =>
		CSharpSyntaxTree.ParseText(source, path: path, encoding: Encoding.UTF8);

	static ImmutableArray<XamlHotReloadGeneratedRoot> FindGeneratedRoots(GeneratorDriverRunResult result)
	{
		var roots = new Dictionary<string, XamlHotReloadGeneratedRootBuilder>(StringComparer.Ordinal);
		foreach (var generatorResult in result.Results)
		{
			foreach (var source in generatorResult.GeneratedSources)
			{
				var hintName = source.HintName;
				if (!hintName.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase))
					continue;

				var sourceText = source.SourceText.ToString();
				var typeName = FindGeneratedRootTypeName(sourceText);
				if (typeName is null)
					continue;

				if (!roots.TryGetValue(typeName, out var root))
				{
					root = new XamlHotReloadGeneratedRootBuilder(typeName);
					roots.Add(typeName, root);
				}

				if (hintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase))
					root.UpdateComponentSource = sourceText;
				else
					root.InitializeComponentSource = sourceText;
			}
		}

		return [.. roots.Values
			.OrderBy(static root => root.TypeName, StringComparer.Ordinal)
			.Select(static root => new XamlHotReloadGeneratedRoot(
				root.TypeName,
				root.InitializeComponentSource,
				root.UpdateComponentSource))];
	}

	static string? FindGeneratedRootTypeName(string source)
	{
		var root = CSharpSyntaxTree.ParseText(source).GetRoot();
		var type = root.DescendantNodes()
			.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax>()
			.FirstOrDefault(static candidate => candidate.Members
				.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
				.Any(static method => method.Identifier.ValueText is "InitializeComponent" or "UpdateComponent"));
		if (type is null)
			return null;

		var namespaces = new Stack<string>();
		var types = new Stack<string>();
		for (SyntaxNode? current = type; current is not null; current = current.Parent)
		{
			switch (current)
			{
				case Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax @namespace:
					namespaces.Push(@namespace.Name.ToString());
					break;
				case Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax nestedType:
					types.Push(nestedType.Identifier.ValueText);
					break;
			}
		}

		return string.Join(".", namespaces.Concat(types));
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

internal sealed record XamlHotReloadDocument(
	string Text,
	string Kind = "Xaml",
	string? TargetPath = null,
	string? ManifestResourceName = null,
	string? TargetFramework = "net11.0",
	string? NoWarn = null);

internal sealed record XamlHotReloadGeneratedVersion(
	string ScenarioIdentity,
	int Index,
	string Xaml,
	string? InitializeComponentSource,
	string? UpdateComponentSource,
	int StateVersion,
	GeneratorDriverRunResult GeneratorResult,
	ImmutableArray<XamlHotReloadGeneratedRoot> GeneratedRoots,
	ImmutableDictionary<string, XamlHotReloadDocument> Documents);

internal sealed record XamlHotReloadGeneratedRoot(
	string TypeName,
	string? InitializeComponentSource,
	string? UpdateComponentSource);

internal sealed record XamlHotReloadCompiledVersion(
	XamlHotReloadGeneratedVersion GeneratedVersion,
	CSharpCompilation Compilation,
	ImmutableArray<byte> PeImage,
	ImmutableArray<byte> PdbImage);

sealed record XamlHotReloadAdditionalFile(
	XamlHotReloadDocument Document,
	SourceGeneratorDriver.AdditionalFile File);

sealed class XamlHotReloadGeneratedRootBuilder(string typeName)
{
	public string TypeName { get; } = typeName;
	public string? InitializeComponentSource { get; set; }
	public string? UpdateComponentSource { get; set; }
}

internal sealed class XamlHotReloadLiveSession : IDisposable
{
	readonly XamlHotReloadTestHarness _harness;
	readonly XamlHotReloadGeneration _generation;
	Assembly? _assembly;
	EmitBaseline? _baseline;
	XamlHotReloadCompiledVersion? _compiledVersion;
	readonly List<object> _instances = [];
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
		CreateInstance();
	}

	public object Instance =>
		_instances.FirstOrDefault() ?? throw new ObjectDisposedException(nameof(XamlHotReloadLiveSession));

	public T GetInstance<T>() where T : class =>
		Assert.IsAssignableFrom<T>(Instance);

	public T CreateInstance<T>() where T : class =>
		Assert.IsAssignableFrom<T>(CreateInstance());

	public T ApplyUpdate<T>(int versionIndex) where T : class
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		if (versionIndex != _versionIndex + 1)
			throw new ArgumentOutOfRangeException(nameof(versionIndex), "Updates must be applied sequentially.");

		var nextVersion = _harness.Compile(_generation[versionIndex]);
		var semanticEdits = CreateSemanticEdits(
			_compiledVersion!.Compilation,
			nextVersion.Compilation,
			_compiledVersion.GeneratedVersion,
			nextVersion.GeneratedVersion);

		_harness.ApplicationHost?.Dispatch(() => ApplyUpdate(nextVersion, semanticEdits));
		if (_harness.ApplicationHost is null)
			ApplyUpdate(nextVersion, semanticEdits);

		return Assert.IsAssignableFrom<T>(Instance);
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		foreach (var instance in _instances)
		{
			if (instance is global::Microsoft.Maui.Controls.Page page)
				_harness.ApplicationHost?.Detach(page);
			global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.Unregister(instance);
		}
		_instances.Clear();
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

	object CreateInstance()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		var instance = Activator.CreateInstance(_pageType!);
		Assert.NotNull(instance);
		if (_instances.Count == 0 && instance is global::Microsoft.Maui.Controls.Page page)
			_harness.ApplicationHost?.Attach(page);
		_instances.Add(instance!);
		return instance;
	}

	void ApplyUpdate(
		XamlHotReloadCompiledVersion nextVersion,
		XamlHotReloadSemanticEdits semanticEdits)
	{
		Assert.NotEmpty(semanticEdits.Edits);

		using var metadataDelta = new MemoryStream();
		using var ilDelta = new MemoryStream();
		using var pdbDelta = new MemoryStream();
		var difference = nextVersion.Compilation.EmitDifference(
			_baseline!,
			semanticEdits.Edits,
			isAddedSymbol: symbol => semanticEdits.InsertedSymbols.Contains(symbol, SymbolEqualityComparer.Default),
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

		foreach (var instance in _instances)
		{
			if (!semanticEdits.AffectedTypes.Contains(instance.GetType().FullName!, StringComparer.Ordinal))
				continue;

			var updateMethod = instance.GetType().GetMethod(
				"UpdateComponent",
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			Assert.NotNull(updateMethod);
			updateMethod!.Invoke(instance, null);
		}

		_baseline = difference.Baseline;
		_compiledVersion = nextVersion;
		_versionIndex = nextVersion.GeneratedVersion.Index;
	}

	static XamlHotReloadSemanticEdits CreateSemanticEdits(
		CSharpCompilation previousCompilation,
		CSharpCompilation nextCompilation,
		XamlHotReloadGeneratedVersion previousVersion,
		XamlHotReloadGeneratedVersion nextVersion)
	{
		var edits = ImmutableArray.CreateBuilder<SemanticEdit>();
		var insertedSymbols = ImmutableArray.CreateBuilder<ISymbol>();
		var affectedTypes = ImmutableArray.CreateBuilder<string>();
		foreach (var nextRoot in nextVersion.GeneratedRoots)
		{
			if (nextRoot.UpdateComponentSource is null)
				continue;

			var previousType = previousCompilation.GetTypeByMetadataName(nextRoot.TypeName);
			var nextType = nextCompilation.GetTypeByMetadataName(nextRoot.TypeName);
			if (previousType is null || nextType is null)
				continue;

			var previousInitializeComponent = GetPartialImplementation(previousType, "InitializeComponent");
			var nextInitializeComponent = GetPartialImplementation(nextType, "InitializeComponent");
			edits.Add(new SemanticEdit(
				SemanticEditKind.Update,
				previousInitializeComponent,
				nextInitializeComponent));

			var previousUpdateComponent = previousType
				.GetMembers("UpdateComponent")
				.OfType<IMethodSymbol>()
				.SingleOrDefault();
			var nextUpdateComponent = nextType
				.GetMembers("UpdateComponent")
				.OfType<IMethodSymbol>()
				.SingleOrDefault();
			Assert.NotNull(nextUpdateComponent);
			if (previousUpdateComponent is null)
			{
				edits.Add(new SemanticEdit(SemanticEditKind.Insert, null, nextUpdateComponent));
				insertedSymbols.Add(nextUpdateComponent!);
			}
			else
			{
				edits.Add(new SemanticEdit(SemanticEditKind.Update, previousUpdateComponent, nextUpdateComponent));
			}

			affectedTypes.Add(nextRoot.TypeName);
		}

		return new XamlHotReloadSemanticEdits(
			edits.ToImmutable(),
			insertedSymbols.ToImmutable(),
			affectedTypes.ToImmutable());
	}

	static IMethodSymbol GetPartialImplementation(INamedTypeSymbol type, string methodName)
	{
		var method = type.GetMembers(methodName).OfType<IMethodSymbol>().First();
		return method.PartialImplementationPart ?? method;
	}
}

sealed record XamlHotReloadSemanticEdits(
	ImmutableArray<SemanticEdit> Edits,
	ImmutableArray<ISymbol> InsertedSymbols,
	ImmutableArray<string> AffectedTypes);
