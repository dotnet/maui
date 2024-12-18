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

static class CodeBehindCodeWriter
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
#nullable enable
";
	internal static readonly string[] accessModifiers = ["private", "public", "internal", "protected"];

	public static void GenerateXamlCodeBehind(XamlProjectItemForCB? xamlItem, Compilation compilation, SourceProductionContext context, AssemblyCaches xmlnsCache, IDictionary<XmlType, ITypeSymbol> typeCache)
	{
		var projItem = xamlItem?.ProjectItem;

		// Get a unique string for this xaml project item
		var itemName = projItem?.ManifestResourceName ?? projItem?.RelativePath;
		if (itemName == null)		
			return;

		if (xamlItem!.Root == null)
		{
			if (xamlItem.Exception != null)
			{
				var location = projItem!.RelativePath is not null ? Location.Create(projItem.RelativePath, new TextSpan(), new LinePositionSpan()) : null;
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, xamlItem.Exception.Message));
			}
			return;
		}

		var uid = Crc64.ComputeHashString($"{compilation.AssemblyName}.{itemName}");
		if (!TryParseXaml(xamlItem, uid, compilation, xmlnsCache, typeCache, context.CancellationToken, context, out var accessModifier, out var rootType, out var rootClrNamespace, out var generateDefaultCtor, out var addXamlCompilationAttribute, out var hideFromIntellisense, out var XamlResourceIdOnly, out var baseType, out var namedFields))
		{
			return;
		}

		var sb = new StringBuilder();
		sb.AppendLine(AutoGeneratedHeaderText);

		var hintName = $"{(string.IsNullOrEmpty(Path.GetDirectoryName(projItem!.TargetPath)) ? "" : Path.GetDirectoryName(projItem.TargetPath) + Path.DirectorySeparatorChar)}{Path.GetFileNameWithoutExtension(projItem.TargetPath)}.{projItem.Kind.ToLowerInvariant()}.sg.cs".Replace(Path.DirectorySeparatorChar, '_');

		if (projItem.ManifestResourceName != null && projItem.TargetPath != null)
		{
			sb.AppendLine($"[assembly: global::Microsoft.Maui.Controls.Xaml.XamlResourceId(\"{projItem.ManifestResourceName}\", \"{projItem.TargetPath.Replace('\\', '/')}\", {(rootType == null ? "null" : "typeof(global::" + rootClrNamespace + "." + rootType + ")")})]");
		}

		if (XamlResourceIdOnly)
		{
			context.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
			return;
		}

		if (rootType == null)
		{
			throw new Exception("Something went wrong");
		}

		var rootSymbol = compilation.GetTypeByMetadataName($"{rootClrNamespace}.{rootType}");

		(var generateInflatorSwitch, var xamlInflators, _) = rootSymbol?.GetXamlProcessing() ?? (false, XamlInflator.Default, false);

		sb.AppendLine($"namespace {rootClrNamespace}");
		sb.AppendLine("{");
		sb.AppendLine($"\t[global::Microsoft.Maui.Controls.Xaml.XamlFilePath(\"{projItem.RelativePath?.Replace("\\", "\\\\")}\")]");
		if (addXamlCompilationAttribute)
		{
			sb.AppendLine($"\t[global::Microsoft.Maui.Controls.Xaml.XamlCompilation(global::Microsoft.Maui.Controls.Xaml.XamlCompilationOptions.Compile)]");
		}

		if (hideFromIntellisense)
		{
			sb.AppendLine($"\t[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
		}

		sb.AppendLine($"\t{accessModifier} partial class {rootType} : {baseType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}");
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
		{
			foreach ((var fname, var ftype, var faccess) in namedFields)
			{
				sb.AppendLine($"\t\t[global::System.CodeDom.Compiler.GeneratedCode(\"Microsoft.Maui.Controls.SourceGen\", \"1.0.0.0\")]");

				sb.AppendLine($"\t\t{faccess} {ftype} {EscapeIdentifier(fname)};");
				sb.AppendLine();
			}
		}

		var initCompRuntimeName = "InitializeComponent";
		if (generateInflatorSwitch)
			initCompRuntimeName = "InitializeComponentRuntime";

		//initializeComponent
		sb.AppendLine($"\t\t[global::System.CodeDom.Compiler.GeneratedCode(\"Microsoft.Maui.Controls.SourceGen\", \"1.0.0.0\")]");

		// add MemberNotNull attributes
		if (namedFields != null && namedFields.Any())
		{
			sb.AppendLine($"#if NET5_0_OR_GREATER");
			foreach ((var fname, _, _) in namedFields)
			{

				sb.AppendLine($"\t\t[global::System.Diagnostics.CodeAnalysis.MemberNotNullAttribute(nameof({EscapeIdentifier(fname)}))]");
			}

			sb.AppendLine($"#endif");
		}

		sb.AppendLine($"\t\tprivate void {initCompRuntimeName}()");
		sb.AppendLine("\t\t{");
		sb.AppendLine("#pragma warning disable IL2026, IL3050 // The body of InitializeComponent will be replaced by XamlC so LoadFromXaml will never be called in production builds");
		sb.AppendLine($"\t\t\tglobal::Microsoft.Maui.Controls.Xaml.Extensions.LoadFromXaml(this, typeof({rootType}));");

		if (namedFields != null)
		{
			foreach ((var fname, var ftype, var faccess) in namedFields)
			{
				sb.AppendLine($"\t\t\t{EscapeIdentifier(fname)} = global::Microsoft.Maui.Controls.NameScopeExtensions.FindByName<{ftype}>(this, \"{fname}\");");
			}
		}
		sb.AppendLine("#pragma warning restore IL2026, IL3050");

		sb.AppendLine("\t\t}");


		if (generateInflatorSwitch)
		{
			sb.AppendLine();

			// add MemberNotNull attributes
			if (namedFields != null && namedFields.Any())
			{
				sb.AppendLine($"#if NET5_0_OR_GREATER");
				foreach ((var fname, _, _) in namedFields)
				{
					sb.AppendLine($"\t\t[global::System.Diagnostics.CodeAnalysis.MemberNotNullAttribute(nameof({EscapeIdentifier(fname)}))]");
				}

				sb.AppendLine($"#endif");
			}
			sb.AppendLine($"\t\tprivate void InitializeComponent() => InitializeComponentRuntime();");
			sb.AppendLine();

			if(xamlInflators == XamlInflator.Default || (xamlInflators & XamlInflator.SourceGen) == XamlInflator.SourceGen)
			{
				if (namedFields != null)
				{
					sb.AppendLine($"#if NET5_0_OR_GREATER");
					foreach ((var fname, _, _) in namedFields)
					{
						sb.AppendLine($"\t\t[global::System.Diagnostics.CodeAnalysis.MemberNotNullAttribute(nameof({EscapeIdentifier(fname)}))]");
					}

					sb.AppendLine($"#endif");
				}
				sb.AppendLine($"\t\tprivate partial void InitializeComponentSourceGen();");
				sb.AppendLine();
			}

			sb.AppendLine($"\t\t[global::System.CodeDom.Compiler.GeneratedCode(\"Microsoft.Maui.Controls.SourceGen\", \"1.0.0.0\")]");

			sb.AppendLine($"\t\tpublic {rootType}(global::Microsoft.Maui.Controls.Xaml.XamlInflator inflator)");
			sb.AppendLine("\t\t{");
			sb.AppendLine("\t\t\tswitch (inflator)");
			sb.AppendLine("\t\t\t{");
			if(xamlInflators == XamlInflator.Default || (xamlInflators & XamlInflator.Runtime) == XamlInflator.Runtime)
			{
				sb.AppendLine("\t\t\t\tcase global::Microsoft.Maui.Controls.Xaml.XamlInflator.Runtime:");
				sb.AppendLine("\t\t\t\t\tInitializeComponentRuntime();");
				sb.AppendLine("\t\t\t\t\tbreak;");
			}
			if(xamlInflators == XamlInflator.Default || (xamlInflators & XamlInflator.XamlC) == XamlInflator.XamlC)
			{
				sb.AppendLine("\t\t\t\tcase global::Microsoft.Maui.Controls.Xaml.XamlInflator.XamlC:");
				sb.AppendLine("\t\t\t\t\tInitializeComponent();");
				sb.AppendLine("\t\t\t\t\tbreak;");
			}
			if(xamlInflators == XamlInflator.Default || (xamlInflators & XamlInflator.SourceGen) == XamlInflator.SourceGen)
			{
				sb.AppendLine("\t\t\t\tcase global::Microsoft.Maui.Controls.Xaml.XamlInflator.SourceGen:");
				sb.AppendLine("\t\t\t\t\tInitializeComponentSourceGen();");
				sb.AppendLine("\t\t\t\t\tbreak;");
			}
			sb.AppendLine("\t\t\t\tcase global::Microsoft.Maui.Controls.Xaml.XamlInflator.Default:");
			sb.AppendLine("\t\t\t\t\tInitializeComponent();");
			sb.AppendLine("\t\t\t\t\tbreak;");
			sb.AppendLine("\t\t\t\tdefault:");
			sb.AppendLine("\t\t\t\t\tthrow new global::System.NotSupportedException($\"no code for {inflator} generated. check the [XamlProcessing] attribute.\");");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t}");
		}
		sb.AppendLine("\t}");
		sb.AppendLine("}");
		
		context.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
	}

	public static bool TryParseXaml(XamlProjectItemForCB parseResult, string uid, Compilation compilation, AssemblyCaches xmlnsCache, IDictionary<XmlType, ITypeSymbol> typeCache, CancellationToken cancellationToken, SourceProductionContext context, out string? accessModifier, out string? rootType, out string? rootClrNamespace, out bool generateDefaultCtor, out bool addXamlCompilationAttribute, out bool hideFromIntellisense, out bool xamlResourceIdOnly, out ITypeSymbol? baseType, out IEnumerable<(string, string, string)>? namedFields)
	{
		accessModifier = null;
		rootType = null;
		rootClrNamespace = null;
		generateDefaultCtor = false;
		addXamlCompilationAttribute = false;
		hideFromIntellisense = false;
		xamlResourceIdOnly = false;
		namedFields = null;
		baseType = null;

		cancellationToken.ThrowIfCancellationRequested();

		var root = parseResult.Root;
		var nsmgr = parseResult.Nsmgr;

		if (root == null || nsmgr == null)
		{
			return false;
		}

		// if the following xml processing instruction is present
		//
		// <?xaml-comp compile="true" ?>
		//
		// we will generate a xaml.g.cs file with the default ctor calling InitializeComponent, and a XamlCompilation attribute
		var hasXamlCompilationProcessingInstruction = GetXamlCompilationProcessingInstruction(root.OwnerDocument);

		var rootClass = root.Attributes["Class", XamlParser.X2006Uri]
					 ?? root.Attributes["Class", XamlParser.X2009Uri];

		if (rootClass != null)
		{
			XmlnsHelper.ParseXmlns(rootClass.Value, out rootType, out rootClrNamespace, out _, out _);
		}
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

		namedFields = GetNamedFields(root, nsmgr, compilation, xmlnsCache, typeCache, cancellationToken, context);
		var typeArguments = GetAttributeValue(root, "TypeArguments", XamlParser.X2006Uri, XamlParser.X2009Uri);
		baseType = new XmlType(root.NamespaceURI, root.LocalName, typeArguments != null ? TypeArgumentsParser.ParseExpression(typeArguments, nsmgr, null) : null).GetTypeSymbol(context.ReportDiagnostic, compilation, xmlnsCache);

		// x:ClassModifier attribute
		var classModifier = GetAttributeValue(root, "ClassModifier", XamlParser.X2006Uri, XamlParser.X2009Uri);
		accessModifier = classModifier?.ToLowerInvariant().Replace("notpublic", "internal") ?? "public"; // notpublic is WPF for internal

		return true;
	}

	static bool GetXamlCompilationProcessingInstruction(XmlDocument xmlDoc)
	{
		var instruction = xmlDoc.SelectSingleNode("processing-instruction('xaml-comp')") as XmlProcessingInstruction;
		if (instruction == null)
		{
			return true;
		}

		var parts = instruction.Data.Split(' ', '=');
		var indexOfCompile = Array.IndexOf(parts, "compile");
		if (indexOfCompile != -1)
		{
			return !parts[indexOfCompile + 1].Trim('"', '\'').Equals("false", StringComparison.OrdinalIgnoreCase);
		}

		return true;
	}

		public static string EscapeIdentifier(string identifier)
	{
		var kind = SyntaxFacts.GetKeywordKind(identifier);
		return kind == SyntaxKind.None
			? identifier
			: $"@{identifier}";
	}

	static IEnumerable<(string name, string type, string accessModifier)> GetNamedFields(XmlNode root, XmlNamespaceManager nsmgr, Compilation compilation, AssemblyCaches xmlnsCache, IDictionary<XmlType, ITypeSymbol> typeCache, CancellationToken cancellationToken, SourceProductionContext context)
	{
		var xPrefix = nsmgr.LookupPrefix(XamlParser.X2006Uri) ?? nsmgr.LookupPrefix(XamlParser.X2009Uri);
		if (xPrefix == null)
		{
			yield break;
		}

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
			if (!accessModifiers.Contains(accessModifier)) //quick validation
				accessModifier = "private";

			yield return (name ?? "", xmlType.GetTypeSymbol(context.ReportDiagnostic, compilation, xmlnsCache)?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "", accessModifier);
		}
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
}