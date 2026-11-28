using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Microsoft.Maui.Controls.BindingSourceGen;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

using static GeneratorHelpers;

[Generator(LanguageNames.CSharp)]
public class XamlGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext initContext)
	{
		// The pipeline closure keeps this token alive for exactly the lifetime of its generator driver.
		var hotReloadSessionToken = new HotReloadSessionToken();
#if DEBUG
		// if (!System.Diagnostics.Debugger.IsAttached)
		// {
		// 	System.Diagnostics.Debugger.Launch();
		// }
#endif
		var globalUsingNamespacesProvider = initContext.SyntaxProvider
			.CreateSyntaxProvider(
				static (node, _) => node is UsingDirectiveSyntax directive
					&& directive.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword)
					&& directive.Alias is null
					&& !directive.StaticKeyword.IsKind(SyntaxKind.StaticKeyword),
				static (context, ct) => GetGlobalUsingNamespace(context, ct))
			.Where(static ns => ns is not null)
			.Select(static (ns, _) => ns!)
			.Collect()
			.Select(static (namespaces, _) => new EquatableArray<string>(
				namespaces.Distinct(StringComparer.Ordinal).OrderBy(static ns => ns, StringComparer.Ordinal).ToArray()));

		// Keep the current compilation when a global using changes, even if its type signatures do not.
		var referenceCompilationProvider = initContext.CompilationProvider
			.Combine(globalUsingNamespacesProvider)
			.WithComparer(new CompilationAndGlobalUsingsComparer())
			.WithTrackingName(TrackingNames.CompilationProvider)
			.Select(static (pair, _) => pair.Item1);

		var referenceTypeCacheProvider = referenceCompilationProvider
			.Select(GetTypeCache)
			.WithTrackingName(TrackingNames.ReferenceTypeCacheProvider);

		var xmlnsDefinitionsProvider = referenceCompilationProvider
			.Combine(globalUsingNamespacesProvider)
			.Select(static (pair, ct) => GetAssemblyAttributes(pair.Left, pair.Right.ToArray(), ct))
			.WithTrackingName(TrackingNames.XmlnsDefinitionsProvider);

		var projectItemProvider = initContext.AdditionalTextsProvider
			.Combine(initContext.AnalyzerConfigOptionsProvider)
			.Select((tuple, ct) => ComputeProjectItem(tuple, ct, hotReloadSessionToken))
			.WithTrackingName(TrackingNames.ProjectItemProvider);

		var xamlProjectItemProviderForCB = projectItemProvider
			.Where(static p => p?.Kind == "Xaml")
			.Combine(xmlnsDefinitionsProvider)
			.Select(ComputeXamlProjectItemForCB)
			.WithTrackingName(TrackingNames.XamlProjectItemProviderForCB);

		var xamlProjectItemProviderForIC = projectItemProvider
			.Where(static p => p?.Kind == "Xaml")
			.Combine(xmlnsDefinitionsProvider)
			.Select(ComputeXamlProjectItemForIC)
			.WithTrackingName(TrackingNames.XamlProjectItemProviderForIC);

		var cssProjectItemProvider = projectItemProvider
			.Where(static p => p?.Kind == "Css")
			.WithTrackingName(TrackingNames.CssProjectItemProvider);

		var xamlSourceProviderForCB = xamlProjectItemProviderForCB
			.Combine(xmlnsDefinitionsProvider, referenceTypeCacheProvider, initContext.CompilationProvider)
			.Select(GetSource)
			.WithTrackingName(TrackingNames.XamlSourceProviderForCB);

		var markedAttributeNamesInReferencesProvider = referenceCompilationProvider
			.Select(static (compilation, ct) => GeneratedBindablePropertyDeclarations.GetMarkedAttributeNamesInReferences(compilation, ct))
			.WithTrackingName(TrackingNames.MarkedAttributeNamesInReferencesProvider);

		var generatedBindablePropertyStubsProvider = initContext.CompilationProvider
			.Combine(markedAttributeNamesInReferencesProvider)
			.Select(static (pair, ct) => GeneratedBindablePropertyDeclarations.GetStubDeclarations(pair.Left, pair.Right, ct))
			.WithTrackingName(TrackingNames.GeneratedBindablePropertyStubsProvider);

		initContext.RegisterSourceOutput(generatedBindablePropertyStubsProvider, static (context, declarations) =>
		{
			foreach (var problem in declarations.Problems)
			{
				var location = Location.Create(problem.FilePath, problem.Span, problem.LineSpan);
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.UnrecognizedGeneratedMemberAccessibility, location, problem.AttributeName, problem.ArgumentName, problem.Value));
			}
		});

		var compilationWithCodeBehindProvider = xamlSourceProviderForCB
			.Collect()
			.Combine(initContext.CompilationProvider)
			.Combine(generatedBindablePropertyStubsProvider)
			.Select(static (tuple, ct) =>
			{
				var t = tuple.Left;
				var compilation = GeneratedBindablePropertyDeclarations.AddStubs(t.Right, tuple.Right, ct);
				var options = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions;
				foreach (var (source, xamlItem, diagnostics) in t.Left)
				{
					ct.ThrowIfCancellationRequested();

					if (source is not null)
					{
						var tree = CSharpSyntaxTree.ParseText(source, options: options, cancellationToken: ct);
						compilation = compilation.AddSyntaxTrees(tree);
					}
				}
				return compilation;
			})
			.WithTrackingName(TrackingNames.CompilationWithCodeBehindProvider);

		//this xmlnsDefinitionProvider is computed AFTER feeding the codebehind into the compilation, and allows correct assemblySymbol comparisons
		var xmlnsDefinitionsProviderForIC = compilationWithCodeBehindProvider
			.Combine(globalUsingNamespacesProvider)
			.Select(static (pair, ct) => GetAssemblyAttributes(pair.Left, pair.Right.ToArray(), ct))
			.WithTrackingName(TrackingNames.XmlnsDefinitionsProviderForIC);

		// Select first: WithComparer clones the upstream transform, which would re-parse all code-behind.
		var conflictCompilationProvider = compilationWithCodeBehindProvider
			.Select(static (compilation, _) => compilation)
			.WithComparer(new CompilationSignaturesComparer(includePrivateMembers: true));
		var conflictXmlnsProvider = conflictCompilationProvider.Select(GetAssemblyAttributes);
		var conflictDocumentProvider = xamlProjectItemProviderForCB
			.Combine(conflictXmlnsProvider)
			.Select(XamlTypeConflictAnalyzer.ParseDocument);
		var conflictDiagnosticsProvider = conflictDocumentProvider
			.Combine(conflictXmlnsProvider, conflictCompilationProvider)
			.Select(XamlTypeConflictAnalyzer.Analyze)
			.WithTrackingName("XamlTypeConflictDiagnostics");

		var xamlSourceProviderForIC = xamlProjectItemProviderForIC
			.Combine(xmlnsDefinitionsProviderForIC, compilationWithCodeBehindProvider.Select(GetTypeCache), compilationWithCodeBehindProvider)
			.WithTrackingName(TrackingNames.XamlSourceProviderForIC);

		// Register the XAML pipeline for CodeBehind
		initContext.RegisterSourceOutput(xamlSourceProviderForCB, static (sourceProductionContext, provider) =>
		{
			var (source, xamlItem, diagnostics) = provider;

			try
			{
				if (diagnostics != null)
					foreach (var diag in diagnostics)
						sourceProductionContext.ReportDiagnostic(diag);
				if (source != null)
					sourceProductionContext.AddSource(GetHintName(xamlItem?.ProjectItem, "sg"), source);
			}
			catch (Exception e)
			{
				sourceProductionContext.ReportDiagnostic(XamlDiagnosticHelpers.CreateXamlParserDiagnostic(xamlItem?.ProjectItem, xamlItem?.SourceText, e));
			}
		});

		initContext.RegisterSourceOutput(conflictDiagnosticsProvider, static (context, diagnostics) =>
		{
			foreach (var diagnostic in diagnostics)
				context.ReportDiagnostic(diagnostic);
		});

		// Register the XAML pipeline for InitializeComponent
		initContext.RegisterImplementationSourceOutput(xamlSourceProviderForIC, static (sourceProductionContext, provider) =>
		{
			var (xamlItem, xmlnsCache, typeCache, compilation) = provider;

			if (xamlItem?.ProjectItem?.RelativePath is null)
			{
				throw new InvalidOperationException("Xaml item or target path is null");
			}

			if (!ShouldGenerateSourceGenInitializeComponent(xamlItem, xmlnsCache, compilation))
				return;

			if (!CanSourceGenXaml(xamlItem, compilation, sourceProductionContext, xmlnsCache, typeCache))
			{
				if (xamlItem != null && xamlItem.Exception != null)
				{
					sourceProductionContext.ReportDiagnostic(XamlDiagnosticHelpers.CreateXamlParserDiagnostic(xamlItem.ProjectItem, xamlItem.SourceText, xamlItem.Exception));
				}
				return;
			}

			// The state before this XAML edit, restored if the edit turns out not to compile.
			XamlHotReloadState.CacheEntry? lastApplicableState = null;
			try
			{
				if (!ShouldGenerateSourceGenInitializeComponent(xamlItem, xmlnsCache, compilation))
					return;

				// Incremental Hot Reload: compute the diff and update state BEFORE generating IC.
				string? ucCode = null;
				// Resolve the root type once. UpdateComponent() must be present on EVERY generation
				// (first compile, no-op edits, unchanged rebuilds), so a XIHR type never gains or loses
				// the method across generations — that member churn is what crashes Roslyn's EnC tracking.
				INamedTypeSymbol? ucRootType = null;
				string ucAccessModifier = "public";
				bool canEmitUC = xamlItem.ProjectItem.EnableIncrementalHotReload
					&& xamlItem.Xaml is not null
					&& InitializeComponentCodeWriter.TryGetRootType(xamlItem, compilation, xmlnsCache, out ucRootType, out ucAccessModifier)
					&& ucRootType != null;
				bool parseErrorOccurred = false;
				var assemblyName = compilation.AssemblyName ?? string.Empty;
				var targetFramework = xamlItem.ProjectItem.TargetFramework ?? string.Empty;
				// Key the incremental-HR patch-chain state on the XAML file's absolute path (not its
				// project-relative path) via ProjectItem.HotReloadStateKey. Roslyn source generators
				// live in a long-lived VBCSCompiler process shared across many project builds; keying on
				// (AssemblyName, TFM, RelativePath) lets a patch chain leak between two projects that
				// merely share an assembly name and a file name. A leaked patch can even reference a type
				// the other project doesn't reference (e.g. a BlazorWebView from a Blazor app bleeding
				// into a plain app), producing uncompilable generated code.
				var stateKey = xamlItem.ProjectItem.HotReloadStateKey;
				string? previousXaml = null;
				SGRootNode? previousRoot = null;
				Dictionary<ElementNode, string>? previousNodeIds = null;
				int previousNextId = 0;
				int previousVersion = 0;
				var hadPreviousEntry = xamlItem.ProjectItem.EnableIncrementalHotReload
					&& xamlItem.Xaml is not null
					&& XamlHotReloadState.TryGetPrevious(assemblyName, targetFramework, stateKey, xamlItem.ProjectItem.HotReloadSessionToken, out previousXaml, out previousRoot, out previousNodeIds, out previousNextId, out previousVersion);
				if (hadPreviousEntry
					&& previousXaml != xamlItem.Xaml
					&& canEmitUC)
				{
					var rootType = ucRootType!;
					var accessModifier = ucAccessModifier;
					lastApplicableState = XamlHotReloadState.Capture(assemblyName, targetFramework, stateKey, xamlItem.ProjectItem.HotReloadSessionToken);
					// The exact generation this diff starts from (copied before the diff can touch the IDs), and
					// the generations its own patch could be applied from.
					var previousGeneration = XamlHotReloadState.GetCurrentGeneration(assemblyName, targetFramework, stateKey, xamlItem.ProjectItem.HotReloadSessionToken);
					var previousBaselines = XamlHotReloadState.GetBaselines(assemblyName, targetFramework, stateKey, xamlItem.ProjectItem.HotReloadSessionToken);
					var generatedFields = XamlHotReloadState.GetGeneratedFields(assemblyName, targetFramework, stateKey, xamlItem.ProjectItem.HotReloadSessionToken);
					var patchBody = InitializeComponentCodeWriter.TryGeneratePatchBody(
						previousRoot,
						previousNodeIds,
						previousNextId,
						previousXaml!,
						xamlItem.Xaml!,
						fromVersion: previousVersion,
						toVersion: previousVersion + 1,
						rootType,
						compilation,
						xmlnsCache,
						typeCache,
						sourceProductionContext,
						xamlItem.ProjectItem,
						generatedFields,
						out var parsedNewRoot,
						out var effectiveNewIds,
						out var newNextNodeId,
						out var parseError,
						out var emptyDiff);

					if (parseError)
					{
						// New XAML is invalid — keep last-good state untouched. IC generation below
						// will re-attempt parsing the same broken XAML, throw, and the outer catch
						// will emit the parse-error diagnostic. No UC update for this iteration.
						parseErrorOccurred = true;
					}
					else if (patchBody != null || emptyDiff)
					{
						// Patch property-sets are absolute (they assign target values, not relative deltas). Do NOT
						// accumulate a version chain: accumulation retained stale/invalid intermediate patches and
						// produced non-deterministic, sometimes-uncompilable output when an edit was reverted (the
						// invalid value lingered forever). See the XIHR versioning fix.
						// Instead, keep the last few generations, newest first: a throwaway run with different XAML
						// advances the cache to a generation the app never got, so UpdateComponent() patches each live
						// instance from the generation it is at (https://github.com/dotnet/maui/issues/36937).
						// An empty diff (a revert to the previous state, or a formatting/comment-only edit) needs
						// no patch for instances at the previous generation, but still does for older ones.
						var newKey = parsedNewRoot is null || effectiveNewIds is null ? null : NodeIdHelper.ComputeGenerationKey(xamlItem.Xaml!, parsedNewRoot, effectiveNewIds);
						var baselines = new List<XamlHotReloadState.GenerationSnapshot>();
						foreach (var baseline in previousGeneration is null ? previousBaselines : new[] { previousGeneration }.Concat(previousBaselines))
						{
							if (baselines.Count == XamlHotReloadState.MaxBaselines)
								break;
							if (baseline.Key is not null
								&& baseline.Key != newKey
								&& !baselines.Any(retained => retained.Key == baseline.Key))
								baselines.Add(baseline);
						}
						XamlHotReloadState.Update(assemblyName, targetFramework, stateKey, xamlItem.Xaml!, parsedNewRoot, effectiveNewIds, newNextNodeId, patchBody != null ? previousVersion + 1 : previousVersion, xamlItem.ProjectItem.HotReloadSessionToken, baselines);
						ucCode = GenerateUpdateComponentFromBaselines(xamlItem, rootType, accessModifier, assemblyName, targetFramework, stateKey, compilation, xmlnsCache, typeCache, sourceProductionContext);
					}
					else
					{
						// Structural change: reset state to the new XAML with fresh IDs. Fresh instances get
						// the new InitializeComponent; live instances can't be patched incrementally across a
						// structural change, so UpdateComponent() is emitted empty (but still present).
						Dictionary<ElementNode, string>? freshIds = null;
						int freshNextId = 0;
						if (parsedNewRoot != null)
						{
							freshIds = NodeIdHelper.AssignIds(parsedNewRoot, 0, out freshNextId);
						}
						XamlHotReloadState.Update(assemblyName, targetFramework, stateKey, xamlItem.Xaml!, parsedNewRoot, freshIds, freshNextId, 0, xamlItem.ProjectItem.HotReloadSessionToken);
						ucCode = UpdateComponentCodeWriter.GenerateUpdateComponent(rootType, accessModifier, (string?)null);
					}
				}
				else if (hadPreviousEntry
					&& previousXaml == xamlItem.Xaml
					&& canEmitUC
					&& XamlHotReloadState.GetBaselines(assemblyName, targetFramework, stateKey, xamlItem.ProjectItem.HotReloadSessionToken).Count > 0)
				{
					// Same XAML as the generation that produced the pending patch: re-derive it from its
					// baselines instead of emitting an empty UpdateComponent() and silently dropping the edit.
					// This covers re-evaluations of the same inputs (driver forks, speculative runs) as well as
					// generations with different C# sources (a throwaway compilation that evaluated the edit
					// first, then the real one, or a later C#-only edit). The patch is unchanged in the last
					// case, so the companion signal type is unchanged and the runtime doesn't re-apply it
					// (https://github.com/dotnet/maui/issues/36937). The baselines are left untouched, so the
					// output only depends on the inputs, never on evaluation order.
					ucCode = GenerateUpdateComponentFromBaselines(xamlItem, ucRootType!, ucAccessModifier, assemblyName, targetFramework, stateKey, compilation, xmlnsCache, typeCache, sourceProductionContext);
				}
				else if (!hadPreviousEntry && xamlItem.ProjectItem.EnableIncrementalHotReload && xamlItem.Xaml is not null)
				{
					// First run for this file (no cache entry): seed the cache with parsed tree and fresh IDs.
					// IMPORTANT: this branch must NOT fire when the cache already exists with the same XAML —
					// doing so would reset the cached state and re-diff the next edit against the wrong baseline.
					SGRootNode? seedRoot = null;
					Dictionary<ElementNode, string>? seedIds = null;
					int seedNextId = 0;
					try
					{
						seedRoot = GeneratorHelpers.ParseXaml(xamlItem.Xaml, xmlnsCache);
						if (seedRoot != null)
						{
							seedIds = NodeIdHelper.AssignIds(seedRoot, 0, out seedNextId);
						}
					}
					catch (Exception)
					{
						// Best-effort seed only: if the initial XAML can't be parsed, seed with a null
						// tree so state still exists at version 0. IC generation below re-parses the same
						// XAML and the outer catch surfaces the real parse-error diagnostic — swallowing
						// here just avoids reporting it twice.
					}
					XamlHotReloadState.Update(assemblyName, targetFramework, stateKey, xamlItem.Xaml, seedRoot, seedIds, seedNextId, 0, xamlItem.ProjectItem.HotReloadSessionToken);
				}
				// else: cache exists and XAML unchanged (or rootType lookup failed). Leave state untouched.

				// Always-emit: if a UC was not produced above (first compile, no-op edit, or unchanged
				// rebuild) but this is a XIHR-enabled type whose root resolved, emit an EMPTY
				// UpdateComponent() so the method is present from the very first generation and never
				// appears/disappears across generations (Roslyn EnC member-stability requirement).
				if (ucCode == null && canEmitUC && !parseErrorOccurred)
				{
					ucCode = UpdateComponentCodeWriter.GenerateUpdateComponent(ucRootType!, ucAccessModifier, (string?)null);
				}

				// Generate IC — reads latest version from XamlHotReloadState
				var reportedError = false;
				var code = InitializeComponentCodeWriter.GenerateInitializeComponent(xamlItem, compilation, sourceProductionContext, xmlnsCache, typeCache,
					diagnostic => reportedError |= FailsBuild(diagnostic, compilation, sourceProductionContext.CancellationToken));
				sourceProductionContext.AddSource(GetHintName(xamlItem.ProjectItem, "xsg"), code);

				// A rude edit (https://github.com/dotnet/maui/issues/36156) fails the build, so it never
				// reaches the running app. Roll the state back, so the next edit is diffed against the last
				// generation that could have been applied and its patch carries this edit's changes too.
				if (lastApplicableState is not null
					&& (reportedError
						|| HasSyntaxErrors(code, compilation, sourceProductionContext.CancellationToken)
						|| HasSyntaxErrors(ucCode, compilation, sourceProductionContext.CancellationToken)
						|| HasSemanticErrors(code, ucCode, compilation, sourceProductionContext.CancellationToken)))
				{
					XamlHotReloadState.Restore(assemblyName, targetFramework, stateKey, lastApplicableState, xamlItem.ProjectItem.HotReloadSessionToken);
				}

				// Emit UC source if a diff was computed
				if (ucCode != null)
				{
					sourceProductionContext.AddSource(GetHintName(xamlItem.ProjectItem, "uc.xsg"), ucCode);
				}
			}
			catch (Exception e)
			{
				if (lastApplicableState is not null)
				{
					XamlHotReloadState.Restore(compilation.AssemblyName ?? string.Empty, xamlItem.ProjectItem.TargetFramework ?? string.Empty, xamlItem.ProjectItem.HotReloadStateKey, lastApplicableState, xamlItem.ProjectItem.HotReloadSessionToken);
				}
				sourceProductionContext.ReportDiagnostic(XamlDiagnosticHelpers.CreateXamlParserDiagnostic(xamlItem?.ProjectItem, xamlItem?.SourceText, e));
			}
		});

		// Register the CSS pipeline
		initContext.RegisterImplementationSourceOutput(cssProjectItemProvider, static (sourceProductionContext, cssItem) =>
		{
			if (cssItem == null)
				return;

			GenerateCssCodeBehind(cssItem, sourceProductionContext);
		});


		// This could have been in the template, but having it here ensure it's never removed
		initContext.RegisterPostInitializationOutput(static context =>
		{
			context.AddSource("GlobalXmlns.g.cs", SourceText.From(
$"""
{AutoGeneratedHeaderText}
#nullable enable

[assembly: global::Microsoft.Maui.Controls.XmlnsDefinition("{XamlParser.MauiGlobalUri}", "{XamlParser.MauiUri}")]
[assembly: global::Microsoft.Maui.Controls.XmlnsPrefix("{XamlParser.MauiGlobalUri}", "global")]
"""
			, Encoding.UTF8));
		});

		initContext.RegisterSourceOutput(globalUsingNamespacesProvider, static (context, namespaces) =>
		{
			if (namespaces.Length == 0)
				return;

			var sb = new StringBuilder();
			sb.AppendLine(AutoGeneratedHeaderText);
			sb.AppendLine("#nullable enable");
			foreach (var ns in namespaces)
				sb.AppendLine($"[assembly: global::Microsoft.Maui.Controls.XmlnsDefinition({Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(XamlParser.MauiGlobalUri, true)}, {Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(ns, true)})]");
			context.AddSource("Global.Usings.Xmlns.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
		});

		// Register the global xmlns definitions. create equivalent XmlnsDefintion for the global ones, so most of the 1sr and 3rd party tooling should keep working
		initContext.RegisterSourceOutput(xmlnsDefinitionsProvider, static (sourceProductionContext, xmlnsCache) =>
		{
			var source = GenerateGlobalXmlns(sourceProductionContext, xmlnsCache);
			if (!string.IsNullOrEmpty(source))
				sourceProductionContext.AddSource("Global.Xmlns.cs", SourceText.From(source!, Encoding.UTF8));
		});
	}

	private static string? GetGlobalUsingNamespace(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		var name = ((UsingDirectiveSyntax)context.Node).Name;
		if (name is null || name.DescendantNodesAndSelf().OfType<AliasQualifiedNameSyntax>()
			.Any(static alias => alias.Alias.Identifier.Text != "global"))
			return null;

		if (context.SemanticModel.GetSymbolInfo(name, cancellationToken).Symbol is not INamespaceSymbol ns)
			return null;

		var parts = new Stack<string>();
		for (var current = ns; !current.IsGlobalNamespace; current = current.ContainingNamespace)
			parts.Push(current.Name);
		return parts.Count == 0 ? null : string.Join(".", parts);
	}

	private sealed class CompilationAndGlobalUsingsComparer : IEqualityComparer<(Compilation Compilation, EquatableArray<string> GlobalUsings)>
	{
		private readonly CompilationSignaturesComparer _compilationComparer = new();

		public bool Equals((Compilation Compilation, EquatableArray<string> GlobalUsings) x, (Compilation Compilation, EquatableArray<string> GlobalUsings) y)
			=> x.GlobalUsings.Equals(y.GlobalUsings) && _compilationComparer.Equals(x.Compilation, y.Compilation);

		public int GetHashCode((Compilation Compilation, EquatableArray<string> GlobalUsings) value)
			=> value.GlobalUsings.GetHashCode();
	}

	private static string GetHintName(ProjectItem? projectItem, string suffix)
	{
		if (projectItem?.RelativePath is not string relativePath)
		{
			throw new InvalidOperationException("Project item or target path is null");
		}

		var prefix = Path.GetDirectoryName(relativePath).Replace(Path.DirectorySeparatorChar, '_').Replace(':', '_');
		if (!string.IsNullOrEmpty(prefix))
			prefix += "_";
		var fileNameNoExtension = Path.GetFileNameWithoutExtension(relativePath);
		var kind = projectItem.Kind.ToLowerInvariant() ?? "unknown-kind";
		return $"{prefix}{fileNameNoExtension}.{kind}.{suffix}.cs";
	}

	private static string? GenerateGlobalXmlns(SourceProductionContext sourceProductionContext, AssemblyAttributes xmlnsCache)
	{
		if (xmlnsCache.GlobalGeneratedXmlnsDefinitions.Count == 0)
			return null;

		var sb = new StringBuilder();
		sb.AppendLine(AutoGeneratedHeaderText);
		sb.AppendLine("#nullable enable");
		foreach (var xmlns in xmlnsCache.GlobalGeneratedXmlnsDefinitions
			.OrderBy(static xmlns => xmlns.XmlNamespace, StringComparer.Ordinal)
			.ThenBy(static xmlns => xmlns.Target, StringComparer.Ordinal)
			.ThenBy(static xmlns => xmlns.AssemblyName, StringComparer.Ordinal))
			sb.AppendLine($"[assembly: global::Microsoft.Maui.Controls.XmlnsDefinition(\"{xmlns.XmlNamespace}\", \"{xmlns.Target}\", AssemblyName = \"{EscapeIdentifier(xmlns.AssemblyName)}\")]");

		return sb.ToString();
	}

	static bool ShouldGenerateSourceGenInitializeComponent(XamlProjectItemForIC xamlItem, AssemblyAttributes xmlnsCache, Compilation compilation)
	{
		var text = xamlItem.ProjectItem.AdditionalText.GetText();
		if (text == null)
			return false;

		XmlNode? root;
		XmlNamespaceManager nsmgr;
		try
		{
			(root, nsmgr) = LoadXmlDocument(text, xmlnsCache, CancellationToken.None);
		}
		catch (Exception)
		{
			return false;
		}

		if (root == null)
			return false;

		var rootClass = root.Attributes["Class", XamlParser.X2006Uri]
					 ?? root.Attributes["Class", XamlParser.X2009Uri];
		INamedTypeSymbol? rootType;

		if (rootClass != null)
		{
			XmlnsHelper.ParseXmlns(rootClass.Value, out var rootTypeName, out var rootClrNamespace, out _, out _);
			rootType = compilation.GetTypeByMetadataName($"{rootClrNamespace}.{rootTypeName}");
		}
		else
		{ //no x:Class, but it can be an autogenerated type (starting with __Type, and with a XamlResourceId attribute)
			rootType = InitializeComponentCodeWriter.GetGeneratedRootTypeForResourcePath(xamlItem.ProjectItem.RelativePath!, compilation);
		}

		if (rootType == null)
			return false;

		var xamlInflators = xamlItem.ProjectItem.Inflator;

		if ((xamlInflators & XamlInflator.SourceGen) != XamlInflator.SourceGen)
			return false;

		return true;
	}

	static bool FailsBuild(Diagnostic diagnostic, Compilation compilation, CancellationToken cancellationToken)
	{
		if (diagnostic.Severity != DiagnosticSeverity.Error || diagnostic.IsSuppressed)
			return false;

		// The user may have downgraded the error (NoWarn, WarningsNotAsErrors, .editorconfig/.globalconfig).
		// Roslyn applies that after the generator reports it, so look it up here.
		if (compilation.Options.SpecificDiagnosticOptions.TryGetValue(diagnostic.Id, out var configured)
			|| compilation.Options.SyntaxTreeOptionsProvider?.TryGetGlobalDiagnosticValue(diagnostic.Id, cancellationToken, out configured) == true)
			return configured is ReportDiagnostic.Default or ReportDiagnostic.Error;

		return true;
	}

	/// <summary>
	/// Generates the <c>UpdateComponent()</c> for the current generation of a file from the generations it
	/// keeps as baselines (see <see cref="XamlHotReloadState"/>). The first baseline, the previous generation,
	/// gives the patch for live instances at an unknown generation. An older baseline gets its own branch when
	/// its patch leaves the components registry exactly as the current <c>InitializeComponent()</c> builds it;
	/// instances at any other generation get the default patch, as before.
	/// </summary>
	static string? GenerateUpdateComponentFromBaselines(
		XamlProjectItemForIC xamlItem,
		INamedTypeSymbol rootType,
		string accessModifier,
		string assemblyName,
		string targetFramework,
		string stateKey,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		SourceProductionContext sourceProductionContext)
	{
		var sessionToken = xamlItem.ProjectItem!.HotReloadSessionToken;
		var current = XamlHotReloadState.GetCurrentGeneration(assemblyName, targetFramework, stateKey, sessionToken);
		var baselines = XamlHotReloadState.GetBaselines(assemblyName, targetFramework, stateKey, sessionToken);
		if (current is null || baselines.Count == 0)
			return null;

		var generatedFields = XamlHotReloadState.GetGeneratedFields(assemblyName, targetFramework, stateKey, sessionToken);
		string? defaultPatchBody = null;
		var baselinePatches = new List<UpdateComponentCodeWriter.BaselinePatch>();
		for (int i = 0; i < baselines.Count; i++)
		{
			var baseline = baselines[i];
			var patchBody = InitializeComponentCodeWriter.TryGeneratePatchBody(
				baseline.ParsedRoot,
				baseline.NodeIds is null ? null : new Dictionary<ElementNode, string>(baseline.NodeIds),
				baseline.NextNodeId,
				baseline.XamlText,
				current.XamlText,
				fromVersion: baseline.Version,
				toVersion: baseline.Version + 1,
				rootType,
				compilation,
				xmlnsCache,
				typeCache,
				sourceProductionContext,
				xamlItem.ProjectItem,
				generatedFields,
				// The previous generation's diff is the one that gave the current IDs.
				currentRoot: i == 0 ? null : current.ParsedRoot,
				currentIds: i == 0 ? null : current.NodeIds,
				out _,
				out _,
				out _,
				out _,
				out var emptyDiff,
				out var idsMatchCurrent);
			if (i == 0)
				defaultPatchBody = patchBody;
			else if (idsMatchCurrent && (patchBody != null || emptyDiff) && baseline.Key is not null)
				baselinePatches.Add(new UpdateComponentCodeWriter.BaselinePatch(baseline.Key, patchBody));
		}

		if (current.Key is null)
			return defaultPatchBody is null ? null : UpdateComponentCodeWriter.GenerateUpdateComponent(rootType, accessModifier, defaultPatchBody);
		if (defaultPatchBody is null && baselinePatches.Count == 0)
			return null;
		return UpdateComponentCodeWriter.GenerateUpdateComponent(rootType, accessModifier, current.Key, defaultPatchBody, baselinePatches);
	}

	static bool HasSyntaxErrors(string? generatedCode, Compilation compilation, CancellationToken cancellationToken)
	{
		if (generatedCode is null)
			return false;

		var options = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions;
		var tree = CSharpSyntaxTree.ParseText(generatedCode, options, cancellationToken: cancellationToken);
		return tree.GetDiagnostics(cancellationToken).Any(d => d.Severity == DiagnosticSeverity.Error);
	}

	/// <summary>
	/// Whether the generated IC/UC code fails type checking, against the user code and the generated
	/// code-behind. Only errors located in that generated code count, and errors that another source
	/// generator could make go away are ignored (see <see cref="MayBeFixedByAnotherGenerator"/>): a false
	/// positive would keep diffing against a generation the app already moved past.
	/// </summary>
	static bool HasSemanticErrors(string code, string? ucCode, Compilation compilation, CancellationToken cancellationToken)
	{
		var options = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions;
		var trees = new List<SyntaxTree>(2) { CSharpSyntaxTree.ParseText(code, options, cancellationToken: cancellationToken) };
		if (ucCode is not null)
			trees.Add(CSharpSyntaxTree.ParseText(ucCode, options, cancellationToken: cancellationToken));
		var withGeneratedCode = compilation.AddSyntaxTrees(trees);

		foreach (var tree in trees)
		{
			var model = withGeneratedCode.GetSemanticModel(tree);
			foreach (var diagnostic in model.GetDiagnostics(cancellationToken: cancellationToken))
			{
				if (diagnostic.Severity == DiagnosticSeverity.Error
					&& !diagnostic.IsSuppressed
					&& !MayBeFixedByAnotherGenerator(diagnostic, model, cancellationToken))
					return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Other generators run on the same input compilation, so the XAML generator doesn't see what they add.
	/// They can declare new types and namespaces, and add members to partial types (e.g. CommunityToolkit.Mvvm
	/// <c>[ObservableProperty]</c> / <c>[RelayCommand]</c>). Returns <see langword="true"/> for errors that may
	/// come from such a missing declaration: unresolved names, and failed member lookups or overload
	/// resolution on a partial type declared in source.
	/// </summary>
	static bool MayBeFixedByAnotherGenerator(Diagnostic diagnostic, SemanticModel model, CancellationToken cancellationToken)
	{
		switch (diagnostic.Id)
		{
			case "CS0103": // The name does not exist in the current context
			case "CS0234": // The type or namespace name does not exist in the namespace
			case "CS0246": // The type or namespace name could not be found
			case "CS0426": // The type name does not exist in the type
				return true;
		}

		var node = model.SyntaxTree.GetRoot(cancellationToken).FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
		for (SyntaxNode? current = node; current is not null; current = current.Parent)
		{
			switch (current)
			{
				case MemberAccessExpressionSyntax memberAccess when memberAccess.Name.Span.Contains(diagnostic.Location.SourceSpan):
					return IsPartialSourceType(GetReceiverType(memberAccess.Expression, model, cancellationToken), cancellationToken);
				case MemberBindingExpressionSyntax memberBinding:
					return memberBinding.Ancestors().OfType<ConditionalAccessExpressionSyntax>().FirstOrDefault() is { } conditionalAccess
						&& IsPartialSourceType(GetReceiverType(conditionalAccess.Expression, model, cancellationToken), cancellationToken);
				case InvocationExpressionSyntax invocation:
					return invocation.Expression switch
					{
						MemberAccessExpressionSyntax memberAccess => IsPartialSourceType(GetReceiverType(memberAccess.Expression, model, cancellationToken), cancellationToken),
						SimpleNameSyntax => IsPartialSourceType(model.GetEnclosingSymbol(invocation.SpanStart, cancellationToken)?.ContainingType, cancellationToken),
						_ => false,
					};
				case BaseObjectCreationExpressionSyntax creation:
					return IsPartialSourceType(model.GetTypeInfo(creation, cancellationToken).Type, cancellationToken);
				// An error on an argument comes from the invocation's overload resolution; an error anywhere
				// else inside an argument isn't about the invocation.
				case ArgumentSyntax argument when argument != node && argument.Expression != node:
					return false;
				case StatementSyntax:
				case AnonymousFunctionExpressionSyntax:
				case MemberDeclarationSyntax:
					return false;
			}
		}
		return false;

		static ITypeSymbol? GetReceiverType(ExpressionSyntax receiver, SemanticModel model, CancellationToken cancellationToken)
			=> model.GetTypeInfo(receiver, cancellationToken).Type is { TypeKind: not TypeKind.Error } type
				? type
				: model.GetSymbolInfo(receiver, cancellationToken).Symbol as ITypeSymbol;

		static bool IsPartialSourceType(ITypeSymbol? type, CancellationToken cancellationToken)
			=> type?.OriginalDefinition.DeclaringSyntaxReferences.Any(reference =>
				reference.GetSyntax(cancellationToken) is TypeDeclarationSyntax declaration
				&& declaration.Modifiers.Any(SyntaxKind.PartialKeyword)) == true;
	}

	static bool CanSourceGenXaml(XamlProjectItemForIC? xamlItem, Compilation compilation, SourceProductionContext context, AssemblyAttributes xmlnsCache, IDictionary<XmlType, INamedTypeSymbol> typeCache)
	{
		ProjectItem? projItem;
		if (xamlItem == null || (projItem = xamlItem.ProjectItem) == null)
			return false;
		var itemName = projItem.ManifestResourceName ?? projItem.RelativePath;
		if (itemName == null)
			return false;
		if (xamlItem.Xaml == null)
			return false;
		return true;
	}

	static void GenerateCssCodeBehind(ProjectItem projItem, SourceProductionContext sourceProductionContext)
	{
		var sb = new StringBuilder();
		var hintName = $"{(string.IsNullOrEmpty(Path.GetDirectoryName(projItem.TargetPath)) ? "" : Path.GetDirectoryName(projItem.TargetPath) + Path.DirectorySeparatorChar)}{Path.GetFileNameWithoutExtension(projItem.TargetPath)}.{projItem.Kind.ToLowerInvariant()}.sg.cs".Replace(Path.DirectorySeparatorChar, '_');

		if (projItem.ManifestResourceName != null && projItem.TargetPath != null)
		{
			sb.AppendLine($"[assembly: global::Microsoft.Maui.Controls.Xaml.XamlResourceId(\"{projItem.ManifestResourceName}\", \"{projItem.TargetPath.Replace('\\', '/')}\", null)]");
		}

		sourceProductionContext.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
	}

	static void ApplyTransforms(XmlNode node, string? targetFramework, XmlNamespaceManager nsmgr)
	{
		SimplifyOnPlatform(node, targetFramework, nsmgr);
	}

	static void SimplifyOnPlatform(XmlNode node, string? targetFramework, XmlNamespaceManager nsmgr)
	{
		//remove OnPlatform nodes if the platform doesn't match, so we don't generate field for x:Name of elements being removed
		if (targetFramework == null)
		{
			return;
		}

		string? target = null;
		targetFramework = targetFramework.Trim();
		if (targetFramework.IndexOf("-android", StringComparison.OrdinalIgnoreCase) != -1)
		{
			target = "Android";
		}

		if (targetFramework.IndexOf("-ios", StringComparison.OrdinalIgnoreCase) != -1)
		{
			target = "iOS";
		}

		if (targetFramework.IndexOf("-macos", StringComparison.OrdinalIgnoreCase) != -1)
		{
			target = "macOS";
		}

		if (targetFramework.IndexOf("-maccatalyst", StringComparison.OrdinalIgnoreCase) != -1)
		{
			target = "MacCatalyst";
		}

		if (target == null)
		{
			return;
		}

		//no need to handle {OnPlatform} markup extension, as you can't x:Name there
		var onPlatformNodes = node.SelectNodes("//__f__:OnPlatform", nsmgr);
		foreach (XmlNode onPlatformNode in onPlatformNodes)
		{
			var onNodes = onPlatformNode.SelectNodes("__f__:On", nsmgr);
			foreach (XmlNode onNode in onNodes)
			{
				var platforms = onNode.SelectSingleNode("@Platform");
				var plats = platforms.Value.Split(',');
				var match = false;

				foreach (var plat in plats)
				{
					if (string.IsNullOrWhiteSpace(plat))
					{
						continue;
					}

					if (plat.Trim() == target)
					{
						match = true;
						break;
					}
				}
				if (!match)
				{
					onNode.ParentNode.RemoveChild(onNode);
				}
			}
		}
	}

}
