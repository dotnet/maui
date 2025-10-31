using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

using static GeneratorHelpers;
using static LocationHelpers;

[Generator(LanguageNames.CSharp)]
public class CodeBehindGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext initContext)
	{
#if DEBUG
		// if (!System.Diagnostics.Debugger.IsAttached)
		// {
		// 	System.Diagnostics.Debugger.Launch();
		// }
#endif
		// Only provide a new Compilation when the references change
		var referenceCompilationProvider = initContext.CompilationProvider
			.WithComparer(new CompilationReferencesComparer())
			.WithTrackingName(TrackingNames.ReferenceCompilationProvider);

		var referenceTypeCacheProvider = referenceCompilationProvider
			.Select(GetTypeCache)
			.WithTrackingName(TrackingNames.ReferenceTypeCacheProvider);

		var xmlnsDefinitionsProvider = referenceCompilationProvider
			.Select(GetAssemblyAttributes)
			.WithTrackingName(TrackingNames.XmlnsDefinitionsProvider);

		var projectItemProvider = initContext.AdditionalTextsProvider
			.Combine(initContext.AnalyzerConfigOptionsProvider)
			.Select(ComputeProjectItem)
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
			.Combine(xmlnsDefinitionsProvider, referenceTypeCacheProvider, referenceCompilationProvider)
			.Select(GetSource)
			.WithTrackingName(TrackingNames.XamlSourceProviderForCB);

		var compilationWithCodeBehindProvider = xamlSourceProviderForCB
			.Collect()
			.Combine(referenceCompilationProvider)
			.Select(static (t, ct) =>
			{
				var sw = System.Diagnostics.Stopwatch.StartNew();
				var compilation = t.Right;
				var options = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions;
				var sourceCount = 0;
				foreach (var (source, xamlItem, diagnostics) in t.Left)
				{
					ct.ThrowIfCancellationRequested();

					if (source is not null)
					{
						var tree = CSharpSyntaxTree.ParseText(source, options: options, cancellationToken: ct);
						compilation = compilation.AddSyntaxTrees(tree);
						sourceCount++;
					}
				}
				LogToFile($"CompilationWithCodeBehindProvider added {sourceCount} syntax trees in {sw.Elapsed.TotalMilliseconds} ms");
				return compilation;
			})
			.WithTrackingName(TrackingNames.CompilationWithCodeBehindProvider);

		//this xmlnsDefinitionProvider is computed AFTER feeding the codebehind into the compilation, and allows correct assemblySymbol comparisons
		var xmlnsDefinitionsProviderForIC = compilationWithCodeBehindProvider
			.Select(GetAssemblyAttributes)
			.WithTrackingName(TrackingNames.XmlnsDefinitionsProviderForIC);

		var xamlSourceProviderForIC = xamlProjectItemProviderForIC
			.Combine(xmlnsDefinitionsProviderForIC, referenceTypeCacheProvider, compilationWithCodeBehindProvider)
			.WithTrackingName(TrackingNames.XamlSourceProviderForIC);

		// Register the XAML pipeline for CodeBehind
		initContext.RegisterSourceOutput(xamlSourceProviderForCB, static (sourceProductionContext, provider) =>
		{
			var sw = System.Diagnostics.Stopwatch.StartNew();
			var (source, xamlItem, diagnostics) = provider;
			var filePath = xamlItem?.ProjectItem?.RelativePath ?? "unknown";

			try
			{
				if (diagnostics != null)
					foreach (var diag in diagnostics)
						sourceProductionContext.ReportDiagnostic(diag);
				if (source != null)
				{
					sourceProductionContext.AddSource(GetHintName(xamlItem?.ProjectItem, "sg"), source);
					LogToFile($"RegisterSourceOutput (CodeBehind) for '{filePath}' added source in {sw.Elapsed.TotalMilliseconds} ms");
				}
				else
				{
					LogToFile($"RegisterSourceOutput (CodeBehind) for '{filePath}' skipped (no source) in {sw.Elapsed.TotalMilliseconds} ms");
				}
			}
			catch (Exception e)
			{
				var location = xamlItem?.ProjectItem?.RelativePath is not null ? Location.Create(xamlItem.ProjectItem.RelativePath, new TextSpan(), new LinePositionSpan()) : null;
				sourceProductionContext.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, e.Message));
				LogToFile($"RegisterSourceOutput (CodeBehind) for '{filePath}' threw after {sw.Elapsed.TotalMilliseconds} ms: {e.Message}");
			}
		});

		// Register the XAML pipeline for InitializeComponent
		initContext.RegisterImplementationSourceOutput(xamlSourceProviderForIC, static (sourceProductionContext, provider) =>
		{
			var sw = System.Diagnostics.Stopwatch.StartNew();
			var (xamlItem, xmlnsCache, typeCache, compilation) = provider;

			if (xamlItem?.ProjectItem?.RelativePath is not string relativePath)
			{
				throw new InvalidOperationException("Xaml item or target path is null");
			}

			if (!ShouldGenerateSourceGenInitializeComponent(xamlItem, xmlnsCache, compilation))
			{
				LogToFile($"RegisterImplementationSourceOutput (InitializeComponent) for '{relativePath}' skipped (ShouldGenerateSourceGenInitializeComponent returned false) after {sw.Elapsed.TotalMilliseconds} ms");
				return;
			}

			if (!CanSourceGenXaml(xamlItem, compilation, sourceProductionContext, xmlnsCache, typeCache))
			{
				if (xamlItem != null && xamlItem.Exception != null)
				{
					var lineInfo = xamlItem.Exception is XamlParseException xpe ? xpe.XmlInfo : new XmlLineInfo();
					var location = LocationCreate(relativePath, lineInfo, string.Empty);
					sourceProductionContext.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, xamlItem.Exception.Message));
				}
				LogToFile($"RegisterImplementationSourceOutput (InitializeComponent) for '{relativePath}' skipped (CanSourceGenXaml returned false) after {sw.Elapsed.TotalMilliseconds} ms");
				return;
			}

			try
			{
				if (!ShouldGenerateSourceGenInitializeComponent(xamlItem, xmlnsCache, compilation))
				{
					LogToFile($"RegisterImplementationSourceOutput (InitializeComponent) for '{relativePath}' skipped on second eligibility check after {sw.Elapsed.TotalMilliseconds} ms");
					return;
				}

				var icSw = System.Diagnostics.Stopwatch.StartNew();
				var code = InitializeComponentCodeWriter.GenerateInitializeComponent(xamlItem, compilation, sourceProductionContext, xmlnsCache, typeCache);
				LogToFile($"InitializeComponentCodeWriter.GenerateInitializeComponent for '{relativePath}' produced {code.Length} characters in {icSw.Elapsed.TotalMilliseconds} ms");
				sourceProductionContext.AddSource(GetHintName(xamlItem.ProjectItem, "xsg"), code);
				LogToFile($"RegisterImplementationSourceOutput (InitializeComponent) for '{relativePath}' completed in {sw.Elapsed.TotalMilliseconds} ms");
			}
			catch (Exception e)
			{
				var location = xamlItem?.ProjectItem?.RelativePath is not null ? Location.Create(xamlItem.ProjectItem.RelativePath, new TextSpan(), new LinePositionSpan()) : null;
				sourceProductionContext.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, e.Message));
				LogToFile($"RegisterImplementationSourceOutput (InitializeComponent) for '{relativePath}' threw after {sw.Elapsed.TotalMilliseconds} ms: {e.Message}");
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

#if MauiAllowImplicitXmlnsDeclaration
[assembly: global::Microsoft.Maui.Controls.Xaml.Internals.AllowImplicitXmlnsDeclaration]
#endif
"""
			, Encoding.UTF8));
		});

		// Register the global xmlns definitions. create equivalent XmlnsDefintion for the global ones, so most of the 1sr and 3rd party tooling should keep working
		initContext.RegisterSourceOutput(xmlnsDefinitionsProvider, static (sourceProductionContext, xmlnsCache) =>
		{
			var sw = System.Diagnostics.Stopwatch.StartNew();
			var source = GenerateGlobalXmlns(sourceProductionContext, xmlnsCache);
			if (!string.IsNullOrEmpty(source))
			{
				sourceProductionContext.AddSource("Global.Xmlns.cs", SourceText.From(source!, Encoding.UTF8));
				LogToFile($"RegisterSourceOutput (Global.Xmlns) emitted source in {sw.Elapsed.TotalMilliseconds} ms");
			}
			else
			{
				LogToFile($"RegisterSourceOutput (Global.Xmlns) skipped (no source) in {sw.Elapsed.TotalMilliseconds} ms");
			}
		});
	}

	private static string GetHintName(ProjectItem? projectItem, string suffix)
	{
		if (projectItem?.RelativePath is not string relativePath)
		{
			throw new InvalidOperationException("Project item or target path is null");
		}

		var prefix = Path.GetDirectoryName(relativePath).Replace(Path.DirectorySeparatorChar, '_').Replace(':', '_');
		var fileNameNoExtension = Path.GetFileNameWithoutExtension(relativePath);
		var kind = projectItem.Kind.ToLowerInvariant() ?? "unknown-kind";
		return $"{prefix}{fileNameNoExtension}.{kind}.{suffix}.cs";
	}

	private static string? GenerateGlobalXmlns(SourceProductionContext sourceProductionContext, AssemblyCaches xmlnsCache)
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		
		if (xmlnsCache.GlobalGeneratedXmlnsDefinitions.Count == 0)
		{
			LogToFile($"GenerateGlobalXmlns returned null (no definitions) after {sw.Elapsed.TotalMilliseconds} ms");
			return null;
		}

		var sb = new StringBuilder();
		sb.AppendLine(AutoGeneratedHeaderText);
		sb.AppendLine("#nullable enable");
		foreach (var xmlns in xmlnsCache.GlobalGeneratedXmlnsDefinitions)
			sb.AppendLine($"[assembly: global::Microsoft.Maui.Controls.XmlnsDefinition(\"{xmlns.XmlNamespace}\", \"{xmlns.Target}\", AssemblyName = \"{EscapeIdentifier(xmlns.AssemblyName)}\")]");

		LogToFile($"GenerateGlobalXmlns generated {xmlnsCache.GlobalGeneratedXmlnsDefinitions.Count} xmlns definitions in {sw.Elapsed.TotalMilliseconds} ms");
		return sb.ToString();
	}

	static bool ShouldGenerateSourceGenInitializeComponent(XamlProjectItemForIC xamlItem, AssemblyCaches xmlnsCache, Compilation compilation)
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		var filePath = xamlItem.ProjectItem?.RelativePath ?? "unknown";
		var text = xamlItem.ProjectItem?.AdditionalText.GetText();
		if (text == null)
		{
			LogToFile($"ShouldGenerateSourceGenInitializeComponent for '{filePath}' returned false (no text) after {sw.Elapsed.TotalMilliseconds} ms");
			return false;
		}

		XmlNode? root;
		XmlNamespaceManager nsmgr;
		try
		{
			(root, nsmgr) = LoadXmlDocument(text, xmlnsCache, CancellationToken.None);
		}
		catch (Exception ex)
		{
			LogToFile($"ShouldGenerateSourceGenInitializeComponent for '{filePath}' returned false (exception) after {sw.Elapsed.TotalMilliseconds} ms: {ex.Message}");
			return false;
		}

		if (root == null)
		{
			LogToFile($"ShouldGenerateSourceGenInitializeComponent for '{filePath}' returned false (no root) after {sw.Elapsed.TotalMilliseconds} ms");
			return false;
		}

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
			ITypeSymbol xamlResIdAttr = compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.XamlResourceIdAttribute")!;
			INamedTypeSymbol? GetTypeForResourcePath(string resourcePath, IAssemblySymbol assembly)
			{
				//XRID use paths with forward slashes. we can't change that it's used by HR
				var attr = assembly.GetAttributes(xamlResIdAttr).FirstOrDefault(attr => ((string)attr.ConstructorArguments[1].Value!).Replace('/', Path.DirectorySeparatorChar) == resourcePath);
				return attr?.ConstructorArguments[2].Value as INamedTypeSymbol;
			}

			rootType = GetTypeForResourcePath(xamlItem.ProjectItem?.RelativePath!, compilation.Assembly);
		}

		if (rootType == null)
		{
			LogToFile($"ShouldGenerateSourceGenInitializeComponent for '{filePath}' returned false (no root type) after {sw.Elapsed.TotalMilliseconds} ms");
			return false;
		}

		var xamlInflators = xamlItem.ProjectItem?.Inflator ?? (XamlInflator)0;

		if ((xamlInflators & XamlInflator.SourceGen) != XamlInflator.SourceGen)
		{
			LogToFile($"ShouldGenerateSourceGenInitializeComponent for '{filePath}' returned false (inflator not SourceGen: {xamlInflators}) after {sw.Elapsed.TotalMilliseconds} ms");
			return false;
		}

		LogToFile($"ShouldGenerateSourceGenInitializeComponent for '{filePath}' returned true after {sw.Elapsed.TotalMilliseconds} ms");
		return true;
	}

	static bool CanSourceGenXaml(XamlProjectItemForIC? xamlItem, Compilation compilation, SourceProductionContext context, AssemblyCaches xmlnsCache, IDictionary<XmlType, ITypeSymbol> typeCache)
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		var filePath = xamlItem?.ProjectItem?.RelativePath ?? "unknown";
		
		ProjectItem? projItem;
		if (xamlItem == null || (projItem = xamlItem.ProjectItem) == null)
		{
			LogToFile($"CanSourceGenXaml for '{filePath}' returned false (no xaml item or project item) after {sw.Elapsed.TotalMilliseconds} ms");
			return false;
		}
		var itemName = projItem.ManifestResourceName ?? projItem.RelativePath;
		if (itemName == null)
		{
			LogToFile($"CanSourceGenXaml for '{filePath}' returned false (no item name) after {sw.Elapsed.TotalMilliseconds} ms");
			return false;
		}
		if (xamlItem.Root == null)
		{
			LogToFile($"CanSourceGenXaml for '{filePath}' returned false (no root) after {sw.Elapsed.TotalMilliseconds} ms");
			return false;
		}
		LogToFile($"CanSourceGenXaml for '{filePath}' returned true after {sw.Elapsed.TotalMilliseconds} ms");
		return true;
	}

	// static IEnumerable<(string name, string? type, string accessModifier)> GetNamedFields(XmlNode root, XmlNamespaceManager nsmgr, Compilation compilation, AssemblyCaches xmlnsCache, IDictionary<XmlType, string> typeCache, CancellationToken cancellationToken, Action<Diagnostic> reportDiagnostic)
	// {
	// 	var xPrefix = nsmgr.LookupPrefix(XamlParser.X2006Uri) ?? nsmgr.LookupPrefix(XamlParser.X2009Uri);
	// 	if (xPrefix == null)
	// 	{
	// 		yield break;
	// 	}

	// 	XmlNodeList names =
	// 		root.SelectNodes(
	// 			"//*[@" + xPrefix + ":Name" +
	// 			"][not(ancestor:: __f__:DataTemplate) and not(ancestor:: __f__:ControlTemplate) and not(ancestor:: __f__:Style) and not(ancestor:: __f__:VisualStateManager.VisualStateGroups)" +
	// 			"and not(ancestor:: __g__:DataTemplate) and not(ancestor:: __g__:ControlTemplate) and not(ancestor:: __g__:Style) and not(ancestor:: __g__:VisualStateManager.VisualStateGroups)]", nsmgr);
	// 	foreach (XmlNode node in names)
	// 	{
	// 		cancellationToken.ThrowIfCancellationRequested();

	// 		var name = GetAttributeValue(node, "Name", XamlParser.X2006Uri, XamlParser.X2009Uri) ?? throw new Exception();
	// 		var typeArguments = GetAttributeValue(node, "TypeArguments", XamlParser.X2006Uri, XamlParser.X2009Uri);
	// 		var fieldModifier = GetAttributeValue(node, "FieldModifier", XamlParser.X2006Uri, XamlParser.X2009Uri);

	// 		var xmlType = new XmlType(node.NamespaceURI, node.LocalName,
	// 								  typeArguments != null
	// 								  ? TypeArgumentsParser.ParseExpression(typeArguments, nsmgr, null)
	// 								  : null);

	// 		var accessModifier = fieldModifier?.ToLowerInvariant().Replace("notpublic", "internal") ?? "private"; //notpublic is WPF for internal
	// 		if (!new[] { "private", "public", "internal", "protected" }.Contains(accessModifier)) //quick validation
	// 		{
	// 			accessModifier = "private";
	// 		}

	// 		yield return (name ?? "", GetTypeName(xmlType, compilation, xmlnsCache, typeCache, reportDiagnostic), accessModifier);
	// 	}
	// }

	// static string? GetTypeName(XmlType xmlType, Compilation compilation, AssemblyCaches xmlnsCache, IDictionary<XmlType, string> typeCache, Action<Diagnostic> reportDiagnostic)
	// {
	// 	if (typeCache.TryGetValue(xmlType, out string returnType))
	// 	{
	// 		return returnType;
	// 	}

	// 	var ns = GetClrNamespace(xmlType.NamespaceUri);
	// 	if (ns != null)
	// 	{
	// 		returnType = $"{ns}.{xmlType.Name}";
	// 	}
	// 	else
	// 	{
	// 		// It's an external, non-built-in namespace URL.
	// 		returnType = GetTypeNameFromCustomNamespace(xmlType, compilation, xmlnsCache, reportDiagnostic);
	// 	}

	// 	if (xmlType.TypeArguments != null)
	// 	{
	// 		returnType = $"{returnType}<{string.Join(", ", xmlType.TypeArguments.Select(typeArg => GetTypeName(typeArg, compilation, xmlnsCache, typeCache, reportDiagnostic)))}>";
	// 	}

	// 	if (returnType == null)
	// 	{
	// 		return null;
	// 	}

	// 	returnType = $"global::{returnType}";
	// 	typeCache[xmlType] = returnType;
	// 	return returnType;
	// }

	static string? GetClrNamespace(string namespaceuri)
	{
		if (namespaceuri == XamlParser.X2009Uri)
		{
			return "System";
		}

		if (namespaceuri != XamlParser.X2006Uri &&
			!namespaceuri.StartsWith("clr-namespace", StringComparison.InvariantCulture) &&
			!namespaceuri.StartsWith("using:", StringComparison.InvariantCulture))
		{
			return null;
		}

		return XmlnsHelper.ParseNamespaceFromXmlns(namespaceuri);
	}

	static string GetTypeNameFromCustomNamespace(XmlType xmlType, Compilation compilation, AssemblyCaches xmlnsCache, Action<Diagnostic> reportDiagnostic)
	{
#nullable disable
		IEnumerable<string> typeNames = xmlType.GetTypeReferences<string>(xmlnsCache.XmlnsDefinitions, null,
			(typeInfo) =>
			{
				string typeName = typeInfo.typeName.Replace('+', '/'); //Nested types
				string fullName = $"{typeInfo.clrNamespace}.{typeInfo.typeName}";
				IList<INamedTypeSymbol> types = compilation.GetTypesByMetadataName(fullName);

				if (types.Count == 0)
				{
					return null;
				}

				foreach (INamedTypeSymbol type in types)
				{
					// skip over types that are not in the correct assemblies
					if (type.ContainingAssembly.Identity.Name != typeInfo.assemblyName)
					{
						continue;
					}

					if (!IsPublicOrVisibleInternal(type, xmlnsCache.InternalsVisible))
					{
						continue;
					}

					int i = fullName.IndexOf('`');
					if (i > 0)
					{
						fullName = fullName.Substring(0, i);
					}
					return fullName;
				}

				return null;
			});

		if (typeNames.Distinct().Skip(1).Any())
		{
			reportDiagnostic(Diagnostic.Create(Descriptors.AmbiguousType, Location.None,
				new[] { xmlType.Name, xmlType.NamespaceUri }.ToArray()));

		}
		return typeNames.FirstOrDefault();
#nullable enable
	}

	static bool IsPublicOrVisibleInternal(INamedTypeSymbol type, IEnumerable<IAssemblySymbol> internalsVisible)
	{
		// return types that are public
		if (type.DeclaredAccessibility == Accessibility.Public)
		{
			return true;
		}

		// only return internal types if they are visible to us
		if (type.DeclaredAccessibility == Accessibility.Internal && internalsVisible.Contains(type.ContainingAssembly, SymbolEqualityComparer.Default))
		{
			return true;
		}

		return false;
	}

	static string? GetAttributeValue(XmlNode node, string localName, params string[] namespaceURIs)
	{
		if (node == null)
		{
			throw new ArgumentNullException(nameof(node));
		}

		if (localName == null)
		{
			throw new ArgumentNullException(nameof(localName));
		}

		if (namespaceURIs == null)
		{
			throw new ArgumentNullException(nameof(namespaceURIs));
		}

		foreach (var namespaceURI in namespaceURIs)
		{
			var attr = node.Attributes[localName, namespaceURI];
			if (attr == null)
			{
				continue;
			}

			return attr.Value;
		}
		return null;
	}

	static void GenerateCssCodeBehind(ProjectItem projItem, SourceProductionContext sourceProductionContext)
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		var filePath = projItem.RelativePath ?? "unknown";
		
		var sb = new StringBuilder();
		var hintName = $"{(string.IsNullOrEmpty(Path.GetDirectoryName(projItem.TargetPath)) ? "" : Path.GetDirectoryName(projItem.TargetPath) + Path.DirectorySeparatorChar)}{Path.GetFileNameWithoutExtension(projItem.TargetPath)}.{projItem.Kind.ToLowerInvariant()}.sg.cs".Replace(Path.DirectorySeparatorChar, '_');

		if (projItem.ManifestResourceName != null && projItem.TargetPath != null)
		{
			sb.AppendLine($"[assembly: global::Microsoft.Maui.Controls.Xaml.XamlResourceId(\"{projItem.ManifestResourceName}\", \"{projItem.TargetPath.Replace('\\', '/')}\", null)]");
		}

		sourceProductionContext.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
		LogToFile($"GenerateCssCodeBehind for '{filePath}' completed in {sw.Elapsed.TotalMilliseconds} ms");
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
