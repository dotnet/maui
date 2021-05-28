using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen
{
	[Generator]
	public class CodeBehindGenerator : ISourceGenerator
	{
		static string[] _CSharpKeywords = new[] {
			"abstract", "as",
			"base", "bool", "break", "byte",
			"case", "catch", "char", "checked", "class", "const", "continue",
			"decimal", "default", "delegate", "do", "double",
			"else", "enum", "event", "explicit", "extern",
			"false", "finally", "fixed", "float", "for", "foreach",
			"goto",
			"if", "implicit", "in", "int", "interface", "internal", "is",
			"lock", "long", "namespace", "new", "null",
			"object", "operator", "out", "override",
			"params", "private", "protected", "public",
			"readonly", "ref", "return",
			"sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
			"this", "throw", "true", "try", "typeof",
			"uint", "ulong", "unchecked", "unsafe", "ushort", "using",
			"virtual", "void", "volatile",
			"while",
		};
		public void Execute(GeneratorExecutionContext context)
		{
			IList<XmlnsDefinitionAttribute> xmlnsDefinitionCache = GetXmlnsDefinitionAttribules(context);

			foreach (var file in context.AdditionalFiles)
			{
				string? code;
				if (!context.AnalyzerConfigOptions.GetOptions(file).TryGetValue("build_metadata.additionalfiles.GenKind", out string? kind) || kind == null)
					continue;

				switch (kind)
				{
					case "Xaml":
						if (!TryGenerateXamlCodeBehind(file, context, xmlnsDefinitionCache, out code))
							continue;
						break;
					case "Css":
						if (!TryGenerateCssCodeBehind(file, context, out code))
							continue;
						break;
					default:
						continue; //throw ??
				}
				if (code == null)
					continue;
				if (!context.AnalyzerConfigOptions.GetOptions(file).TryGetValue("build_metadata.additionalfiles.TargetPath", out string? name))
					name = file.Path;
				name = $"{(string.IsNullOrEmpty(Path.GetDirectoryName(name)) ? "" : Path.GetDirectoryName(name) + Path.DirectorySeparatorChar)}{Path.GetFileNameWithoutExtension(name)}.{kind.ToLowerInvariant()}.sg.cs".Replace(Path.DirectorySeparatorChar, '_');
				context.AddSource(name, SourceText.From(code, Encoding.UTF8));
			}
		}

		IList<XmlnsDefinitionAttribute> GetXmlnsDefinitionAttribules(GeneratorExecutionContext context)
		{
			var cache = new List<XmlnsDefinitionAttribute>();
			INamedTypeSymbol? xmlnsDefinitonAttribute = context.Compilation.GetTypeByMetadataName(typeof(XmlnsDefinitionAttribute).FullName);
			if (xmlnsDefinitonAttribute == null)
				return cache;
			foreach (var reference in context.Compilation.References)
			{
				var symbol = context.Compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
				if (symbol == null)
					continue;
				foreach (var attr in symbol.GetAttributes())
				{
					if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, xmlnsDefinitonAttribute))
						continue;
					var xmlnsDef = new XmlnsDefinitionAttribute(attr.ConstructorArguments[0].Value as string, attr.ConstructorArguments[1].Value as string);
					if (attr.NamedArguments.Length == 1 && attr.NamedArguments[0].Key == nameof(XmlnsDefinitionAttribute.AssemblyName))
						xmlnsDef.AssemblyName = attr.NamedArguments[0].Value.Value as string;
					else
						xmlnsDef.AssemblyName = symbol.Name;
					cache.Add(xmlnsDef);
				}
			}

			return cache;
		}

		bool TryGenerateXamlCodeBehind(AdditionalText file, GeneratorExecutionContext context, IList<XmlnsDefinitionAttribute> xmlnsDefinitionCache, out string? code)
		{
			code = null;
			using (var reader = File.OpenText(file.Path))
			{
				if (!TryParseXaml(reader, context, xmlnsDefinitionCache, out var rootType, out var rootClrNamespace, out var generateDefaultCtor, out var addXamlCompilationAttribute, out var hideFromIntellisense, out var XamlResourceIdOnly, out var baseType, out var namedFields))
					return false;

				var sb = new StringBuilder();
				var opt = context.AnalyzerConfigOptions.GetOptions(file);
				if (context.AnalyzerConfigOptions.GetOptions(file).TryGetValue("build_metadata.additionalfiles.ManifestResourceName", out string? manifestResourceName)
					&& context.AnalyzerConfigOptions.GetOptions(file).TryGetValue("build_metadata.additionalfiles.TargetPath", out string? targetPath))
					sb.AppendLine($"[assembly: global::Microsoft.Maui.Controls.Xaml.XamlResourceId(\"{manifestResourceName}\", \"{targetPath.Replace('\\', '/')}\", {(rootType == null ? "null" : "typeof(global::" + rootClrNamespace + "." + rootType + ")")})]");
				if (XamlResourceIdOnly)
				{
					code = sb.ToString();
					return true;
				}
				if (rootType == null)
					throw new Exception("Something went wrong");

				sb.AppendLine($"namespace {rootClrNamespace}");
				sb.AppendLine("{");

				if (context.AnalyzerConfigOptions.GetOptions(file).TryGetValue("build_metadata.additionalfiles.ItemSpec", out string? itemSpec))
					sb.AppendLine($"\t[global::Microsoft.Maui.Controls.Xaml.XamlFilePath(\"{itemSpec.Replace("\\", "\\\\")}\")]");
				if (addXamlCompilationAttribute)
					sb.AppendLine($"\t[global::Microsoft.Maui.Controls.Xaml.XamlCompilation(global::Microsoft.Maui.Controls.Xaml.XamlCompilationOptions.Compile)]");
				if (hideFromIntellisense)
					sb.AppendLine($"\t[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");

				sb.AppendLine($"\tpublic partial class {rootType} : {baseType}");
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

						sb.AppendLine($"\t\t{faccess} {ftype} {(_CSharpKeywords.Contains(fname) ? "@" + fname : fname)};");
						sb.AppendLine();
					}

				//initializeComponent
				sb.AppendLine($"\t\t[global::System.CodeDom.Compiler.GeneratedCode(\"Microsoft.Maui.Controls.SourceGen\", \"1.0.0.0\")]");
				sb.AppendLine("\t\tprivate void InitializeComponent()");
				sb.AppendLine("\t\t{");
				sb.AppendLine($"\t\t\tglobal::Microsoft.Maui.Controls.Xaml.Extensions.LoadFromXaml(this, typeof({rootType}));");
				if (namedFields != null)
					foreach ((var fname, var ftype, var faccess) in namedFields)
						sb.AppendLine($"\t\t\t{(_CSharpKeywords.Contains(fname) ? "@" + fname : fname)} = global::Microsoft.Maui.Controls.NameScopeExtensions.FindByName<{ftype}>(this, \"{fname}\");");

				sb.AppendLine("\t\t}");
				sb.AppendLine("\t}");
				sb.AppendLine("}");

				code = sb.ToString();
				return true;
			}
		}

		bool TryParseXaml(TextReader xaml, GeneratorExecutionContext context, IList<XmlnsDefinitionAttribute> xmlnsDefinitionCache, out string? rootType, out string? rootClrNamespace, out bool generateDefaultCtor, out bool addXamlCompilationAttribute, out bool hideFromIntellisense, out bool xamlResourceIdOnly, out string? baseType, out IEnumerable<(string, string, string)>? namedFields)
		{
			rootType = null;
			rootClrNamespace = null;
			generateDefaultCtor = false;
			addXamlCompilationAttribute = false;
			hideFromIntellisense = false;
			xamlResourceIdOnly = false;
			namedFields = null;
			baseType = null;

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(xaml);

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

			foreach (XmlAttribute attr in root.Attributes)
			{
				if (attr.Name == "xmlns")
					nsmgr.AddNamespace("", attr.Value); //Add default xmlns
				if (attr.Prefix != "xmlns")
					continue;
				nsmgr.AddNamespace(attr.LocalName, attr.Value);
			}

			var rootClass = root.Attributes["Class", XamlParser.X2006Uri]
						 ?? root.Attributes["Class", XamlParser.X2009Uri];

			if (rootClass != null)
				XmlnsHelper.ParseXmlns(rootClass.Value, out rootType, out rootClrNamespace, out var rootAsm, out var targetPlatform);
			else if (hasXamlCompilationProcessingInstruction)
			{
				rootClrNamespace = "__XamlGeneratedCode__";
				rootType = $"__Type{Guid.NewGuid():N}";
				generateDefaultCtor = true;
				addXamlCompilationAttribute = true;
				hideFromIntellisense = true;
			}
			else
			{ // rootClass == null && !hasXamlCompilationProcessingInstruction) {
				xamlResourceIdOnly = true; //only generate the XamlResourceId assembly attribute
				return true;
			}

			namedFields = GetNamedFields(root, nsmgr, context, xmlnsDefinitionCache);
			var typeArguments = GetAttributeValue(root, "TypeArguments", XamlParser.X2006Uri, XamlParser.X2009Uri);
			baseType = GetTypeName(new XmlType(root.NamespaceURI, root.LocalName, typeArguments != null ? TypeArgumentsParser.ParseExpression(typeArguments, nsmgr, null) : null), context, xmlnsDefinitionCache);

			return true;
		}


		static bool GetXamlCompilationProcessingInstruction(XmlDocument xmlDoc)
		{
			var instruction = xmlDoc.SelectSingleNode("processing-instruction('xaml-comp')") as XmlProcessingInstruction;
			if (instruction == null)
				return false;

			var parts = instruction.Data.Split(' ', '=');
			var indexOfCompile = Array.IndexOf(parts, "compile");
			if (indexOfCompile != -1)
				return parts[indexOfCompile + 1].Trim('"', '\'').Equals("true", StringComparison.InvariantCultureIgnoreCase);
			return false;
		}

		IEnumerable<(string name, string type, string accessModifier)> GetNamedFields(XmlNode root, XmlNamespaceManager nsmgr, GeneratorExecutionContext context, IList<XmlnsDefinitionAttribute> xmlnsDefinitionCache)
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
				yield return (name ?? "", GetTypeName(xmlType, context, xmlnsDefinitionCache), accessModifier);
			}
		}

		string GetTypeName(XmlType xmlType, GeneratorExecutionContext context, IList<XmlnsDefinitionAttribute> xmlnsDefinitionCache)
		{
			string returnType;
			var ns = GetClrNamespace(xmlType.NamespaceUri);
			if (ns != null)
				returnType = $"{ns}.{xmlType.Name}";
			else
			{
				// It's an external, non-built-in namespace URL.
				returnType = GetTypeNameFromCustomNamespace(xmlType, context, xmlnsDefinitionCache);
			}

			if (xmlType.TypeArguments != null)
				returnType = $"{returnType}<{string.Join(", ", xmlType.TypeArguments.Select(typeArg => GetTypeName(typeArg, context, xmlnsDefinitionCache)))}>";

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

		string GetTypeNameFromCustomNamespace(XmlType xmlType, GeneratorExecutionContext context, IList<XmlnsDefinitionAttribute> xmlnsDefinitionCache)
		{
#nullable disable
			string typeName = xmlType.GetTypeReference<string>(xmlnsDefinitionCache, null,
				(typeInfo) =>
				{
					string typeName = typeInfo.TypeName.Replace('+', '/'); //Nested types
					string fullName = $"{typeInfo.ClrNamespace}.{typeInfo.TypeName}";

					if (context.Compilation.GetTypeByMetadataName(fullName) != null)
						return fullName;
					return null;
				}, out _);

			return typeName;
#nullable enable
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

		bool TryGenerateCssCodeBehind(AdditionalText file, GeneratorExecutionContext context, out string? code)
		{
			var sb = new StringBuilder();
			var opt = context.AnalyzerConfigOptions.GetOptions(file);
			if (context.AnalyzerConfigOptions.GetOptions(file).TryGetValue("build_metadata.additionalfiles.ManifestResourceName", out string? manifestResourceName)
				&& context.AnalyzerConfigOptions.GetOptions(file).TryGetValue("build_metadata.additionalfiles.TargetPath", out string? targetPath))
				sb.AppendLine($"[assembly: global::Microsoft.Maui.Controls.Xaml.XamlResourceId(\"{manifestResourceName}\", \"{targetPath.Replace('\\', '/')}\", null)]");

			code = sb.ToString();
			return true;
		}

		public void Initialize(GeneratorInitializationContext context)
		{
			//#if DEBUG
			//			if (!Debugger.IsAttached)
			//				Debugger.Launch();
			//#endif
		}
	}
}
