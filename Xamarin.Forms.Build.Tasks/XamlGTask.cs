using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CSharp;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	public class XamlGTask : Task
	{
		internal static CodeDomProvider Provider = new CSharpCodeProvider();

		[Required]
		public string Source { get; set; }

		public string Language { get; set; }

		public string AssemblyName { get; set; }

		[Output]
		public string OutputFile { get; set; }

		public override bool Execute()
		{
			if (Source == null || OutputFile == null)
			{
				Log.LogMessage("Skipping XamlG");
				return true;
			}

			Log.LogMessage("Source: {0}", Source);
			Log.LogMessage("Language: {0}", Language);
			Log.LogMessage("AssemblyName: {0}", AssemblyName);
			Log.LogMessage("OutputFile {0}", OutputFile);

			try
			{
				GenerateFile(Source, OutputFile);
				return true;
			}
			catch (XmlException xe)
			{
				Log.LogError(null, null, null, Source, xe.LineNumber, xe.LinePosition, 0, 0, xe.Message, xe.HelpLink, xe.Source);

				return false;
			}
			catch (Exception e)
			{
				Log.LogError(null, null, null, Source, 0, 0, 0, 0, e.Message, e.HelpLink, e.Source);
				return false;
			}
		}

		internal static void ParseXaml(TextReader xaml, out string rootType, out string rootNs, out CodeTypeReference baseType,
			out IEnumerable<CodeMemberField> namedFields)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(xaml);

			var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
			nsmgr.AddNamespace("__f__", XamlParser.XFUri);

			var root = xmlDoc.SelectSingleNode("/*", nsmgr);

			if (root == null)
			{
				Console.Error.WriteLine("{0}: No root node found");
				rootType = null;
				rootNs = null;
				baseType = null;
				namedFields = null;
				return;
			}

			foreach (XmlAttribute attr in root.Attributes) {
				if (attr.Name == "xmlns") {
					nsmgr.AddNamespace("", attr.Value); //Add default xmlns
				}
				if (attr.Prefix != "xmlns")
					continue;
				nsmgr.AddNamespace(attr.LocalName, attr.Value);
			}

			var rootClass = root.Attributes["Class", XamlParser.X2006Uri]
			                    ?? root.Attributes["Class", XamlParser.X2009Uri];
			if (rootClass == null)
			{
				rootType = null;
				rootNs = null;
				baseType = null;
				namedFields = null;
				return;
			}

			string rootAsm, targetPlatform;
			XmlnsHelper.ParseXmlns(rootClass.Value, out rootType, out rootNs, out rootAsm, out targetPlatform);
			namedFields = GetCodeMemberFields(root, nsmgr);

			var typeArguments = GetAttributeValue(root, "TypeArguments", XamlParser.X2006Uri, XamlParser.X2009Uri);
			var xmlType = new XmlType(root.NamespaceURI, root.LocalName, typeArguments != null ? TypeArgumentsParser.ParseExpression(typeArguments, nsmgr, null): null);
			baseType = GetType(xmlType, root.GetNamespaceOfPrefix);
		}

		static CodeAttributeDeclaration GeneratedCodeAttrDecl =>
			new CodeAttributeDeclaration(new CodeTypeReference($"global::{typeof(GeneratedCodeAttribute).FullName}"),
						new CodeAttributeArgument(new CodePrimitiveExpression("Xamarin.Forms.Build.Tasks.XamlG")),
						new CodeAttributeArgument(new CodePrimitiveExpression("0.0.0.0")));

		internal static void GenerateCode(string rootType, string rootNs, CodeTypeReference baseType,
		                                  IEnumerable<CodeMemberField> namedFields, string xamlFile, string outFile)
		{
			if (rootType == null)
			{
				File.WriteAllText(outFile, "");
				return;
			}

			var ccu = new CodeCompileUnit();
			var declNs = new CodeNamespace(rootNs);
			ccu.Namespaces.Add(declNs);

			var declType = new CodeTypeDeclaration(rootType) {
				IsPartial = true,
				CustomAttributes = {
					new CodeAttributeDeclaration(new CodeTypeReference($"global::{typeof(XamlFilePathAttribute).FullName}"),
						 new CodeAttributeArgument(new CodePrimitiveExpression(xamlFile)))
				}
			};
			declType.BaseTypes.Add(baseType);

			declNs.Types.Add(declType);

			var initcomp = new CodeMemberMethod {
				Name = "InitializeComponent",
				CustomAttributes = { GeneratedCodeAttrDecl }
			};

			declType.Members.Add(initcomp);

			initcomp.Statements.Add(new CodeMethodInvokeExpression(
				new CodeTypeReferenceExpression(new CodeTypeReference($"global::{typeof(Extensions).FullName}")),
				"LoadFromXaml", new CodeThisReferenceExpression(), new CodeTypeOfExpression(declType.Name)));

			foreach (var namedField in namedFields)
			{
				declType.Members.Add(namedField);

				var find_invoke = new CodeMethodInvokeExpression(
					new CodeMethodReferenceExpression(
						new CodeTypeReferenceExpression(new CodeTypeReference($"global::{typeof(NameScopeExtensions).FullName}")),
						"FindByName", namedField.Type),
					new CodeThisReferenceExpression(), new CodePrimitiveExpression(namedField.Name));

				CodeAssignStatement assign = new CodeAssignStatement(
					new CodeVariableReferenceExpression(namedField.Name), find_invoke);

				initcomp.Statements.Add(assign);
			}

			using (var writer = new StreamWriter(outFile))
				Provider.GenerateCodeFromCompileUnit(ccu, writer, new CodeGeneratorOptions());
		}

		internal static void GenerateFile(string xamlFile, string outFile)
		{
			string rootType, rootNs;
			CodeTypeReference baseType;
			IEnumerable<CodeMemberField> namedFields;

			using (StreamReader reader = File.OpenText(xamlFile))
				ParseXaml(reader, out rootType, out rootNs, out baseType, out namedFields);

			GenerateCode(rootType, rootNs, baseType, namedFields, Path.GetFullPath(xamlFile), outFile);
		}

		static IEnumerable<CodeMemberField> GetCodeMemberFields(XmlNode root, XmlNamespaceManager nsmgr)
		{
			var xPrefix = nsmgr.LookupPrefix(XamlParser.X2006Uri) ?? nsmgr.LookupPrefix(XamlParser.X2009Uri);
			if (xPrefix == null)
				yield break;

			XmlNodeList names =
				root.SelectNodes(
				"//*[@" + xPrefix + ":Name" +
					"][not(ancestor:: __f__:DataTemplate) and not(ancestor:: __f__:ControlTemplate) and not(ancestor:: __f__:Style)]", nsmgr);
			foreach (XmlNode node in names)
			{
				// Don't take the root canvas
				if (node == root)
					continue;
				var name = GetAttributeValue(node, "Name", XamlParser.X2006Uri, XamlParser.X2009Uri);
				var typeArguments = GetAttributeValue(node, "TypeArguments", XamlParser.X2006Uri, XamlParser.X2009Uri);
				var fieldModifier = GetAttributeValue(node, "FieldModifier", XamlParser.X2006Uri, XamlParser.X2009Uri);

				var xmlType = new XmlType(node.NamespaceURI, node.LocalName,
										  typeArguments != null
										  ? TypeArgumentsParser.ParseExpression(typeArguments, nsmgr, null)
										  : null);

				var access = MemberAttributes.Private;
				if (fieldModifier!=null) {
					switch (fieldModifier.ToLowerInvariant()){
					default:
					case "private":
						access = MemberAttributes.Private;
						break;
					case "public":
						access = MemberAttributes.Public;
						break;
					case "protected":
						access = MemberAttributes.Family;
						break;
					case "internal":
					case "notpublic": //WPF syntax
						access = MemberAttributes.Assembly;
						break;
					}
				}

				yield return new CodeMemberField {
					Name = name,
					Type = GetType(xmlType, node.GetNamespaceOfPrefix),
					Attributes = access,
					CustomAttributes = { GeneratedCodeAttrDecl }
				};
			}
		}

		static CodeTypeReference GetType(XmlType xmlType,
			Func<string, string> getNamespaceOfPrefix = null)
		{
			var type = xmlType.Name;
			var ns = GetClrNamespace(xmlType.NamespaceUri);
			if (ns != null)
				type = $"{ns}.{type}";

			if (xmlType.TypeArguments != null)
				type = $"{type}`{xmlType.TypeArguments.Count}";

			var returnType = new CodeTypeReference(type);
			if (ns != null)
				returnType.Options |= CodeTypeReferenceOptions.GlobalReference;

			if (xmlType.TypeArguments != null)
				foreach (var typeArg in xmlType.TypeArguments)
					returnType.TypeArguments.Add(GetType(typeArg, getNamespaceOfPrefix));

			return returnType;
		}

		static string GetClrNamespace(string namespaceuri)
		{
			if (namespaceuri == XamlParser.XFUri)
				return "Xamarin.Forms";
			if (namespaceuri == XamlParser.X2009Uri)
				return "System";
			if (namespaceuri != XamlParser.X2006Uri && !namespaceuri.Contains("clr-namespace"))
				throw new Exception($"Can't load types from xmlns {namespaceuri}");
			return XmlnsHelper.ParseNamespaceFromXmlns(namespaceuri);
		}

		static string GetAttributeValue(XmlNode node, string localName, params string[] namespaceURIs)
		{
			if (node == null)
				throw new ArgumentNullException(nameof(node));
			if (localName == null)
				throw new ArgumentNullException(nameof(localName));
			if (namespaceURIs == null)
				throw new ArgumentNullException(nameof(namespaceURIs));
			foreach (var namespaceURI in namespaceURIs) {
				var attr = node.Attributes[localName, namespaceURI];
				if (attr == null)
					continue;
				return attr.Value;
			}
			return null;
		}
	}
}