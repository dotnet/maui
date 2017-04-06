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
		const string XAML2006 = "http://schemas.microsoft.com/winfx/2006/xaml";
		const string XAML2009 = "http://schemas.microsoft.com/winfx/2009/xaml";

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
			out IDictionary<string, CodeTypeReference> namesAndTypes)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(xaml);

			var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
			nsmgr.AddNamespace("__f__", "http://xamarin.com/schemas/2014/forms");

			var root = xmlDoc.SelectSingleNode("/*", nsmgr);

			if (root == null)
			{
				Console.Error.WriteLine("{0}: No root node found");
				rootType = null;
				rootNs = null;
				baseType = null;
				namesAndTypes = null;
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

			var rootClass = root.Attributes["Class", XAML2006]
						 ?? root.Attributes["Class", XAML2009];
			if (rootClass == null)
			{
				rootType = null;
				rootNs = null;
				baseType = null;
				namesAndTypes = null;
				return;
			}

			string rootAsm, targetPlatform;
			XmlnsHelper.ParseXmlns(rootClass.Value, out rootType, out rootNs, out rootAsm, out targetPlatform);
			namesAndTypes = GetNamesAndTypes(root, nsmgr);

			var typeArgsAttr = root.Attributes["TypeArguments", XAML2006]
							?? root.Attributes["TypeArguments", XAML2009];

			var xmlType = new XmlType(root.NamespaceURI, root.LocalName, typeArgsAttr != null ? TypeArgumentsParser.ParseExpression(typeArgsAttr.Value, nsmgr, null): null);
			baseType = GetType(xmlType, root.GetNamespaceOfPrefix);
		}

		static CodeAttributeDeclaration GeneratedCodeAttrDecl =>
			new CodeAttributeDeclaration(new CodeTypeReference($"global::{typeof(GeneratedCodeAttribute).FullName}"),
						new CodeAttributeArgument(new CodePrimitiveExpression("Xamarin.Forms.Build.Tasks.XamlG")),
						new CodeAttributeArgument(new CodePrimitiveExpression("0.0.0.0")));

		internal static void GenerateCode(string rootType, string rootNs, CodeTypeReference baseType,
			IDictionary<string, CodeTypeReference> namesAndTypes, string xamlFile, string outFile)
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

			foreach (var entry in namesAndTypes)
			{
				string name = entry.Key;
				var type = entry.Value;

				var field = new CodeMemberField
				{
					Name = name,
					Type = type,
					CustomAttributes = { GeneratedCodeAttrDecl }
				};

				declType.Members.Add(field);

				var find_invoke = new CodeMethodInvokeExpression(
					new CodeMethodReferenceExpression(
						new CodeTypeReferenceExpression(new CodeTypeReference($"global::{typeof(NameScopeExtensions).FullName}")),
						"FindByName", type),
					new CodeThisReferenceExpression(), new CodePrimitiveExpression(name));

				CodeAssignStatement assign = new CodeAssignStatement(
					new CodeVariableReferenceExpression(name), find_invoke);

				initcomp.Statements.Add(assign);
			}

			using (var writer = new StreamWriter(outFile))
				Provider.GenerateCodeFromCompileUnit(ccu, writer, new CodeGeneratorOptions());
		}

		internal static void GenerateFile(string xamlFile, string outFile)
		{
			string rootType, rootNs;
			CodeTypeReference baseType;
			IDictionary<string, CodeTypeReference> namesAndTypes;

			using (StreamReader reader = File.OpenText(xamlFile))
				ParseXaml(reader, out rootType, out rootNs, out baseType, out namesAndTypes);

			GenerateCode(rootType, rootNs, baseType, namesAndTypes, Path.GetFullPath(xamlFile), outFile);
		}

		static Dictionary<string, CodeTypeReference> GetNamesAndTypes(XmlNode root, XmlNamespaceManager nsmgr)
		{
			var res = new Dictionary<string, CodeTypeReference>();

			var xPrefix = nsmgr.LookupPrefix(XAML2006) ?? nsmgr.LookupPrefix(XAML2009);
			if (xPrefix == null)
				return null;

			XmlNodeList names =
				root.SelectNodes(
				"//*[@" + xPrefix + ":Name" +
					"][not(ancestor:: __f__:DataTemplate) and not(ancestor:: __f__:ControlTemplate) and not(ancestor:: __f__:Style)]", nsmgr);
			foreach (XmlNode node in names)
			{
				// Don't take the root canvas
				if (node == root)
					continue;

				XmlAttribute attr = node.Attributes["Name", XAML2006]
								 ?? node.Attributes["Name", XAML2009];
				XmlAttribute typeArgsAttr = node.Attributes["TypeArguments", XAML2006]
										 ?? node.Attributes["TypeArguments", XAML2009];
				var xmlType = new XmlType(node.NamespaceURI, node.LocalName,
										  typeArgsAttr != null
										  ? TypeArgumentsParser.ParseExpression(typeArgsAttr.Value, nsmgr, null)
										  : null);

				res[attr.Value] = GetType(xmlType, node.GetNamespaceOfPrefix);
			}

			return res;
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
			if (namespaceuri == "http://xamarin.com/schemas/2014/forms")
				return "Xamarin.Forms";
			if (namespaceuri == XAML2009)
				return "System";
			if (namespaceuri != XAML2006 && !namespaceuri.Contains("clr-namespace"))
				throw new Exception($"Can't load types from xmlns {namespaceuri}");
			return XmlnsHelper.ParseNamespaceFromXmlns(namespaceuri);
		}
	}
}