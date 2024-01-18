﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen
{
	[Generator(LanguageNames.CSharp)]
	public class CodeBehindGenerator : IIncrementalGenerator
	{
		const string AutoGeneratedHeaderText = @"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a .NET MAUI source generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
";

		public void Initialize(IncrementalGeneratorInitializationContext initContext)
		{
#if DEBUG
			//if (!System.Diagnostics.Debugger.IsAttached)
			//	System.Diagnostics.Debugger.Launch();
#endif
			var projectItemProvider = initContext.AdditionalTextsProvider
				.Combine(initContext.AnalyzerConfigOptionsProvider)
				.Select(ComputeProjectItem);

			var xmlnsDefinitionsProvider = initContext.CompilationProvider
				.Select(GetAssemblyAttributes);

			var sourceProvider = projectItemProvider
				.Combine(xmlnsDefinitionsProvider)
				.Combine(initContext.CompilationProvider)
				.Select(static (t, _) => (t.Left.Left, t.Left.Right, t.Right));

			initContext.RegisterSourceOutput(sourceProvider, static (sourceProductionContext, provider) =>
			{
				var (projectItem, caches, compilation) = provider;
				if (projectItem == null)
					return;
				switch (projectItem.Kind)
				{
					case "Xaml":
						GenerateXamlCodeBehind(projectItem, compilation, sourceProductionContext, caches);
						break;
					case "Css":
						GenerateCssCodeBehind(projectItem, sourceProductionContext);
						break;
				}
			});
		}

		static string EscapeIdentifier(string identifier)
		{
			var kind = SyntaxFacts.GetKeywordKind(identifier);
			return kind == SyntaxKind.None
				? identifier
				: $"@{identifier}";
		}

		static ProjectItem? ComputeProjectItem((AdditionalText, AnalyzerConfigOptionsProvider) tuple, CancellationToken cancellationToken)
		{
			var (additionalText, optionsProvider) = tuple;
			var fileOptions = optionsProvider.GetOptions(additionalText);
			var globalOptions = optionsProvider.GlobalOptions;
			if (!fileOptions.TryGetValue("build_metadata.additionalfiles.GenKind", out string? kind) || kind is null)
				return null;
			fileOptions.TryGetValue("build_metadata.additionalfiles.TargetPath", out var targetPath);
			fileOptions.TryGetValue("build_metadata.additionalfiles.ManifestResourceName", out var manifestResourceName);
			fileOptions.TryGetValue("build_metadata.additionalfiles.RelativePath", out var relativePath);
			fileOptions.TryGetValue("build_property.targetframework", out var targetFramework);
			return new ProjectItem(additionalText, targetPath: targetPath, relativePath: relativePath, manifestResourceName: manifestResourceName, kind: kind, targetFramework: targetFramework);
		}

		static AssemblyCaches GetAssemblyAttributes(Compilation compilation, CancellationToken cancellationToken)
		{
			// [assembly: XmlnsDefinition]
			INamedTypeSymbol? xmlnsDefinitonAttribute = compilation.GetTypesByMetadataName(typeof(XmlnsDefinitionAttribute).FullName)
				.SingleOrDefault(t => t.ContainingAssembly.Identity.Name == "Microsoft.Maui.Controls");

			// [assembly: InternalsVisibleTo]
			INamedTypeSymbol? internalsVisibleToAttribute = compilation.GetTypeByMetadataName(typeof(InternalsVisibleToAttribute).FullName);

			if (xmlnsDefinitonAttribute is null || internalsVisibleToAttribute is null)
				return AssemblyCaches.Empty;

			var xmlnsDefinitions = new List<XmlnsDefinitionAttribute>();
			var internalsVisible = new List<IAssemblySymbol>();

			internalsVisible.Add(compilation.Assembly);

			// load from references
			foreach (var reference in compilation.References)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol symbol)
					continue;
				foreach (var attr in symbol.GetAttributes())
				{
					if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, xmlnsDefinitonAttribute))
					{
						// [assembly: XmlnsDefinition]
						var xmlnsDef = new XmlnsDefinitionAttribute(attr.ConstructorArguments[0].Value as string, attr.ConstructorArguments[1].Value as string);
						if (attr.NamedArguments.Length == 1 && attr.NamedArguments[0].Key == nameof(XmlnsDefinitionAttribute.AssemblyName))
							xmlnsDef.AssemblyName = attr.NamedArguments[0].Value.Value as string;
						else
							xmlnsDef.AssemblyName = symbol.Name;
						xmlnsDefinitions.Add(xmlnsDef);
					}
					else if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, internalsVisibleToAttribute))
					{
						// [assembly: InternalsVisibleTo]
						if (attr.ConstructorArguments[0].Value is string assemblyName && new AssemblyName(assemblyName).Name == compilation.Assembly.Identity.Name)
						{
							internalsVisible.Add(symbol);
						}
					}
				}
			}
			return new AssemblyCaches(xmlnsDefinitions, internalsVisible);
		}

		static void GenerateXamlCodeBehind(ProjectItem projItem, Compilation compilation, SourceProductionContext context, AssemblyCaches caches)
		{
			var text = projItem.AdditionalText.GetText(context.CancellationToken);
			if (text == null)
				return;

			// Get a unique string for this xaml project item
			var itemName = projItem.ManifestResourceName ?? projItem.RelativePath;
			if (itemName == null)
				return;
			var uid = Crc64.ComputeHashString($"{compilation.AssemblyName}.{itemName}");

			if (!TryParseXaml(text, uid, compilation, caches, context.CancellationToken, projItem.TargetFramework, out var accessModifier, out var rootType, out var rootClrNamespace, out var generateDefaultCtor, out var addXamlCompilationAttribute, out var hideFromIntellisense, out var XamlResourceIdOnly, out var baseType, out var namedFields, out var parseException))
			{
				if (parseException != null)
				{
					var location = projItem.RelativePath is not null ? Location.Create(projItem.RelativePath, new TextSpan(), new LinePositionSpan()) : null;
					context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, parseException.Message));
				}
				return;
			}
			var sb = new StringBuilder();
			sb.AppendLine(AutoGeneratedHeaderText);

			var hintName = $"{(string.IsNullOrEmpty(Path.GetDirectoryName(projItem.TargetPath)) ? "" : Path.GetDirectoryName(projItem.TargetPath) + Path.DirectorySeparatorChar)}{Path.GetFileNameWithoutExtension(projItem.TargetPath)}.{projItem.Kind.ToLowerInvariant()}.sg.cs".Replace(Path.DirectorySeparatorChar, '_');

			if (projItem.ManifestResourceName != null && projItem.TargetPath != null)
				sb.AppendLine($"[assembly: global::Microsoft.Maui.Controls.Xaml.XamlResourceId(\"{projItem.ManifestResourceName}\", \"{projItem.TargetPath.Replace('\\', '/')}\", {(rootType == null ? "null" : "typeof(global::" + rootClrNamespace + "." + rootType + ")")})]");

			if (XamlResourceIdOnly)
			{
				context.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
				return;
			}

			if (rootType == null)
				throw new Exception("Something went wrong");

			sb.AppendLine($"namespace {rootClrNamespace}");
			sb.AppendLine("{");
			sb.AppendLine($"\t[global::Microsoft.Maui.Controls.Xaml.XamlFilePath(\"{projItem.RelativePath?.Replace("\\", "\\\\")}\")]");
			if (addXamlCompilationAttribute)
				sb.AppendLine($"\t[global::Microsoft.Maui.Controls.Xaml.XamlCompilation(global::Microsoft.Maui.Controls.Xaml.XamlCompilationOptions.Compile)]");
			if (hideFromIntellisense)
				sb.AppendLine($"\t[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");

			sb.AppendLine($"\t{accessModifier} partial class {rootType} : {baseType}");
			sb.AppendLine("\t{");

			//optional default ctor
			if (generateDefaultCtor)
			{
				sb.AppendLine($"\t\t[global::System.CodeDom.Compiler.GeneratedCode(\"Microsoft.Maui.Controls.SourceGen\", \"1.0.0.0\")]");
				sb.AppendLine($"\t\tpublic {rootType}()");
				sb.AppendLine("\t\t{");
				sb.AppendLine("\t\t\tInitializeComponent();");
				sb.AppendLine("\t\t}");
				sb.AppendLine();
			}

			//create fields
			if (namedFields != null)
				foreach ((var fname, var ftype, var faccess) in namedFields)
				{
					sb.AppendLine($"\t\t[global::System.CodeDom.Compiler.GeneratedCode(\"Microsoft.Maui.Controls.SourceGen\", \"1.0.0.0\")]");

					sb.AppendLine($"\t\t{faccess} {ftype} {EscapeIdentifier(fname)};");
					sb.AppendLine();
				}

			//initializeComponent
			sb.AppendLine($"\t\t[global::System.CodeDom.Compiler.GeneratedCode(\"Microsoft.Maui.Controls.SourceGen\", \"1.0.0.0\")]");

			// add MemberNotNull attributes
			if (namedFields != null)
			{
				sb.AppendLine($"#if NET5_0_OR_GREATER");
				foreach ((var fname, _, _) in namedFields)
				{

					sb.AppendLine($"\t\t[global::System.Diagnostics.CodeAnalysis.MemberNotNullAttribute(nameof({EscapeIdentifier(fname)}))]");
				}

				sb.AppendLine($"#endif");
			}

			sb.AppendLine("\t\tprivate void InitializeComponent()");
			sb.AppendLine("\t\t{");
			sb.AppendLine($"\t\t\tglobal::Microsoft.Maui.Controls.Xaml.Extensions.LoadFromXaml(this, typeof({rootType}));");
			if (namedFields != null)
				foreach ((var fname, var ftype, var faccess) in namedFields)
					sb.AppendLine($"\t\t\t{EscapeIdentifier(fname)} = global::Microsoft.Maui.Controls.NameScopeExtensions.FindByName<{ftype}>(this, \"{fname}\");");

			sb.AppendLine("\t\t}");
			sb.AppendLine("\t}");
			sb.AppendLine("}");

			context.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
		}

		static bool TryParseXaml(SourceText text, string uid, Compilation compilation, AssemblyCaches caches, CancellationToken cancellationToken, string? targetFramework, out string? accessModifier, out string? rootType, out string? rootClrNamespace, out bool generateDefaultCtor, out bool addXamlCompilationAttribute, out bool hideFromIntellisense, out bool xamlResourceIdOnly, out string? baseType, out IEnumerable<(string, string, string)>? namedFields, out Exception? exception)
		{
			cancellationToken.ThrowIfCancellationRequested();

			accessModifier = null;
			rootType = null;
			rootClrNamespace = null;
			generateDefaultCtor = false;
			addXamlCompilationAttribute = false;
			hideFromIntellisense = false;
			xamlResourceIdOnly = false;
			namedFields = null;
			baseType = null;
			exception = null;

			var xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.LoadXml(text.ToString());
			}
			catch (XmlException xe)
			{
				exception = xe;
				return false;
			}

#pragma warning disable CS0618 // Type or member is obsolete
			if (xmlDoc.DocumentElement.NamespaceURI == XamlParser.FormsUri)
			{
				exception = new Exception($"{XamlParser.FormsUri} is not a valid namespace. Use {XamlParser.MauiUri} instead");
				return false;
			}
#pragma warning restore CS0618 // Type or member is obsolete

			cancellationToken.ThrowIfCancellationRequested();

			// if the following xml processing instruction is present
			//
			// <?xaml-comp compile="true" ?>
			//
			// we will generate a xaml.g.cs file with the default ctor calling InitializeComponent, and a XamlCompilation attribute
			var hasXamlCompilationProcessingInstruction = GetXamlCompilationProcessingInstruction(xmlDoc);

			var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
			nsmgr.AddNamespace("__f__", XamlParser.MauiUri);

			var root = xmlDoc.SelectSingleNode("/*", nsmgr);
			if (root == null)
				return false;

			ApplyTransforms(root, targetFramework, nsmgr);
			cancellationToken.ThrowIfCancellationRequested();

			foreach (XmlAttribute attr in root.Attributes)
			{
				if (attr.Name == "xmlns")
					nsmgr.AddNamespace("", attr.Value); //Add default xmlns
				if (attr.Prefix != "xmlns")
					continue;
				nsmgr.AddNamespace(attr.LocalName, attr.Value);
			}

			cancellationToken.ThrowIfCancellationRequested();

			var rootClass = root.Attributes["Class", XamlParser.X2006Uri]
						 ?? root.Attributes["Class", XamlParser.X2009Uri];

			if (rootClass != null)
				XmlnsHelper.ParseXmlns(rootClass.Value, out rootType, out rootClrNamespace, out var rootAsm, out var targetPlatform);
			else if (hasXamlCompilationProcessingInstruction && root.NamespaceURI == XamlParser.MauiUri)
			{
				rootClrNamespace = "__XamlGeneratedCode__";
				rootType = $"__Type{uid}";
				generateDefaultCtor = true;
				addXamlCompilationAttribute = true;
				hideFromIntellisense = true;
			}
			else
			{ // rootClass == null && !hasXamlCompilationProcessingInstruction) {
				xamlResourceIdOnly = true; //only generate the XamlResourceId assembly attribute
				return true;
			}

			namedFields = GetNamedFields(root, nsmgr, compilation, caches, cancellationToken);
			var typeArguments = GetAttributeValue(root, "TypeArguments", XamlParser.X2006Uri, XamlParser.X2009Uri);
			baseType = GetTypeName(new XmlType(root.NamespaceURI, root.LocalName, typeArguments != null ? TypeArgumentsParser.ParseExpression(typeArguments, nsmgr, null) : null), compilation, caches);

			// x:ClassModifier attribute
			var classModifier = GetAttributeValue(root, "ClassModifier", XamlParser.X2006Uri, XamlParser.X2009Uri);
			accessModifier = classModifier?.ToLowerInvariant().Replace("notpublic", "internal") ?? "public"; // notpublic is WPF for internal

			return true;
		}


		static bool GetXamlCompilationProcessingInstruction(XmlDocument xmlDoc)
		{
			var instruction = xmlDoc.SelectSingleNode("processing-instruction('xaml-comp')") as XmlProcessingInstruction;
			if (instruction == null)
				return true;

			var parts = instruction.Data.Split(' ', '=');
			var indexOfCompile = Array.IndexOf(parts, "compile");
			if (indexOfCompile != -1)
				return !parts[indexOfCompile + 1].Trim('"', '\'').Equals("false", StringComparison.OrdinalIgnoreCase);
			return true;
		}

		static IEnumerable<(string name, string type, string accessModifier)> GetNamedFields(XmlNode root, XmlNamespaceManager nsmgr, Compilation compilation, AssemblyCaches caches, CancellationToken cancellationToken)
		{
			var xPrefix = nsmgr.LookupPrefix(XamlParser.X2006Uri) ?? nsmgr.LookupPrefix(XamlParser.X2009Uri);
			if (xPrefix == null)
				yield break;

			XmlNodeList names =
				root.SelectNodes(
					"//*[@" + xPrefix + ":Name" +
					"][not(ancestor:: __f__:DataTemplate) and not(ancestor:: __f__:ControlTemplate) and not(ancestor:: __f__:Style) and not(ancestor:: __f__:VisualStateManager.VisualStateGroups)]", nsmgr);
			foreach (XmlNode node in names)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var name = GetAttributeValue(node, "Name", XamlParser.X2006Uri, XamlParser.X2009Uri) ?? throw new Exception();
				var typeArguments = GetAttributeValue(node, "TypeArguments", XamlParser.X2006Uri, XamlParser.X2009Uri);
				var fieldModifier = GetAttributeValue(node, "FieldModifier", XamlParser.X2006Uri, XamlParser.X2009Uri);

				var xmlType = new XmlType(node.NamespaceURI, node.LocalName,
										  typeArguments != null
										  ? TypeArgumentsParser.ParseExpression(typeArguments, nsmgr, null)
										  : null);

				var accessModifier = fieldModifier?.ToLowerInvariant().Replace("notpublic", "internal") ?? "private"; //notpublic is WPF for internal
				if (!new[] { "private", "public", "internal", "protected" }.Contains(accessModifier)) //quick validation
					accessModifier = "private";
				yield return (name ?? "", GetTypeName(xmlType, compilation, caches), accessModifier);
			}
		}

		static string GetTypeName(XmlType xmlType, Compilation compilation, AssemblyCaches caches)
		{
			string returnType;
			var ns = GetClrNamespace(xmlType.NamespaceUri);
			if (ns != null)
				returnType = $"{ns}.{xmlType.Name}";
			else
			{
				// It's an external, non-built-in namespace URL.
				returnType = GetTypeNameFromCustomNamespace(xmlType, compilation, caches);
			}

			if (xmlType.TypeArguments != null)
				returnType = $"{returnType}<{string.Join(", ", xmlType.TypeArguments.Select(typeArg => GetTypeName(typeArg, compilation, caches)))}>";

			return $"global::{returnType}";
		}

		static string? GetClrNamespace(string namespaceuri)
		{
			if (namespaceuri == XamlParser.X2009Uri)
				return "System";
			if (namespaceuri != XamlParser.X2006Uri &&
				!namespaceuri.StartsWith("clr-namespace", StringComparison.InvariantCulture) &&
				!namespaceuri.StartsWith("using:", StringComparison.InvariantCulture))
				return null;
			return XmlnsHelper.ParseNamespaceFromXmlns(namespaceuri);
		}

		static string GetTypeNameFromCustomNamespace(XmlType xmlType, Compilation compilation, AssemblyCaches caches)
		{
#nullable disable
			string typeName = xmlType.GetTypeReference<string>(caches.XmlnsDefinitions, null,
				(typeInfo) =>
				{
					string typeName = typeInfo.typeName.Replace('+', '/'); //Nested types
					string fullName = $"{typeInfo.clrNamespace}.{typeInfo.typeName}";
					IList<INamedTypeSymbol> types = compilation.GetTypesByMetadataName(fullName);

					if (types.Count == 0)
						return null;

					foreach (INamedTypeSymbol type in types)
					{
						// skip over types that are not in the correct assemblies
						if (type.ContainingAssembly.Identity.Name != typeInfo.assemblyName)
							continue;

						if (!IsPublicOrVisibleInternal(type, caches.InternalsVisible))
							continue;

						int i = fullName.IndexOf('`');
						if (i > 0)
						{
							fullName = fullName.Substring(0, i);
						}
						return fullName;
					}

					return null;
				});

			return typeName;
#nullable enable
		}

		static bool IsPublicOrVisibleInternal(INamedTypeSymbol type, IEnumerable<IAssemblySymbol> internalsVisible)
		{
			// return types that are public
			if (type.DeclaredAccessibility == Accessibility.Public)
				return true;

			// only return internal types if they are visible to us
			if (type.DeclaredAccessibility == Accessibility.Internal && internalsVisible.Contains(type.ContainingAssembly, SymbolEqualityComparer.Default))
				return true;

			return false;
		}

		static string? GetAttributeValue(XmlNode node, string localName, params string[] namespaceURIs)
		{
			if (node == null)
				throw new ArgumentNullException(nameof(node));
			if (localName == null)
				throw new ArgumentNullException(nameof(localName));
			if (namespaceURIs == null)
				throw new ArgumentNullException(nameof(namespaceURIs));
			foreach (var namespaceURI in namespaceURIs)
			{
				var attr = node.Attributes[localName, namespaceURI];
				if (attr == null)
					continue;
				return attr.Value;
			}
			return null;
		}

		static void GenerateCssCodeBehind(ProjectItem projItem, SourceProductionContext sourceProductionContext)
		{
			var sb = new StringBuilder();
			var hintName = $"{(string.IsNullOrEmpty(Path.GetDirectoryName(projItem.TargetPath)) ? "" : Path.GetDirectoryName(projItem.TargetPath) + Path.DirectorySeparatorChar)}{Path.GetFileNameWithoutExtension(projItem.TargetPath)}.{projItem.Kind.ToLowerInvariant()}.sg.cs".Replace(Path.DirectorySeparatorChar, '_');

			if (projItem.ManifestResourceName != null && projItem.TargetPath != null)
				sb.AppendLine($"[assembly: global::Microsoft.Maui.Controls.Xaml.XamlResourceId(\"{projItem.ManifestResourceName}\", \"{projItem.TargetPath.Replace('\\', '/')}\", null)]");

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
				return;

			string? target = null;
			targetFramework = targetFramework.Trim();
			if (targetFramework.IndexOf("-android", StringComparison.OrdinalIgnoreCase) != -1)
				target = "Android";
			if (targetFramework.IndexOf("-ios", StringComparison.OrdinalIgnoreCase) != -1)
				target = "iOS";
			if (targetFramework.IndexOf("-macos", StringComparison.OrdinalIgnoreCase) != -1)
				target = "macOS";
			if (targetFramework.IndexOf("-maccatalyst", StringComparison.OrdinalIgnoreCase) != -1)
				target = "MacCatalyst";
			if (target == null)
				return;

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
							continue;
						if (plat.Trim() == target)
						{
							match = true;
							break;
						}
					}
					if (!match)
						onNode.ParentNode.RemoveChild(onNode);
				}
			}
		}

		class ProjectItem
		{
			public ProjectItem(AdditionalText additionalText, string? targetPath, string? relativePath, string? manifestResourceName, string kind, string? targetFramework)
			{
				AdditionalText = additionalText;
				TargetPath = targetPath ?? additionalText.Path;
				RelativePath = relativePath;
				ManifestResourceName = manifestResourceName;
				Kind = kind;
				TargetFramework = targetFramework;
			}

			public AdditionalText AdditionalText { get; }
			public string? TargetPath { get; }
			public string? RelativePath { get; }
			public string? ManifestResourceName { get; }
			public string Kind { get; }
			public string? TargetFramework { get; }
		}

		class AssemblyCaches
		{
			public static readonly AssemblyCaches Empty = new(Array.Empty<XmlnsDefinitionAttribute>(), Array.Empty<IAssemblySymbol>());

			public AssemblyCaches(IReadOnlyList<XmlnsDefinitionAttribute> xmlnsDefinitions, IReadOnlyList<IAssemblySymbol> internalsVisible)
			{
				XmlnsDefinitions = xmlnsDefinitions;
				InternalsVisible = internalsVisible;
			}

			public IReadOnlyList<XmlnsDefinitionAttribute> XmlnsDefinitions { get; }

			public IReadOnlyList<IAssemblySymbol> InternalsVisible { get; }
		}
	}
}
