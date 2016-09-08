using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			out IDictionary<string, CodeTypeReference> namesAndTypes)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(xaml);

			var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
			nsmgr.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");
			nsmgr.AddNamespace("x2009", "http://schemas.microsoft.com/winfx/2009/xaml");
			nsmgr.AddNamespace("f", "http://xamarin.com/schemas/2014/forms");

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

			var rootClass = root.Attributes["Class", "http://schemas.microsoft.com/winfx/2006/xaml"] ??
			                root.Attributes["Class", "http://schemas.microsoft.com/winfx/2009/xaml"];
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

			var typeArguments = root.Attributes["TypeArguments", "http://schemas.microsoft.com/winfx/2009/xaml"];

			baseType = GetType(root.NamespaceURI, root.LocalName, typeArguments == null ? null : typeArguments.Value.Split(','),
				root.GetNamespaceOfPrefix);
		}

		internal static void GenerateCode(string rootType, string rootNs, CodeTypeReference baseType,
			IDictionary<string, CodeTypeReference> namesAndTypes, string outFile)
		{
			if (rootType == null)
			{
				File.WriteAllText(outFile, "");
				return;
			}

			var ccu = new CodeCompileUnit();
			var declNs = new CodeNamespace(rootNs);
			ccu.Namespaces.Add(declNs);

			declNs.Imports.Add(new CodeNamespaceImport("System"));
			declNs.Imports.Add(new CodeNamespaceImport("Xamarin.Forms"));
			declNs.Imports.Add(new CodeNamespaceImport("Xamarin.Forms.Xaml"));

			var declType = new CodeTypeDeclaration(rootType);
			declType.IsPartial = true;
			declType.BaseTypes.Add(baseType);

			declNs.Types.Add(declType);

			var initcomp = new CodeMemberMethod
			{
				Name = "InitializeComponent",
				CustomAttributes =
				{
					new CodeAttributeDeclaration(new CodeTypeReference(typeof (GeneratedCodeAttribute)),
						new CodeAttributeArgument(new CodePrimitiveExpression("Xamarin.Forms.Build.Tasks.XamlG")),
						new CodeAttributeArgument(new CodePrimitiveExpression("0.0.0.0")))
				}
			};
			declType.Members.Add(initcomp);

			initcomp.Statements.Add(new CodeMethodInvokeExpression(
				new CodeThisReferenceExpression(),
				"LoadFromXaml", new CodeTypeOfExpression(declType.Name)));

			foreach (var entry in namesAndTypes)
			{
				string name = entry.Key;
				var type = entry.Value;

				var field = new CodeMemberField
				{
					Name = name,
					Type = type,
					CustomAttributes =
					{
						new CodeAttributeDeclaration(new CodeTypeReference(typeof (GeneratedCodeAttribute)),
							new CodeAttributeArgument(new CodePrimitiveExpression("Xamarin.Forms.Build.Tasks.XamlG")),
							new CodeAttributeArgument(new CodePrimitiveExpression("0.0.0.0")))
					}
				};

				declType.Members.Add(field);

				var find_invoke = new CodeMethodInvokeExpression(
					new CodeMethodReferenceExpression(
						new CodeThisReferenceExpression(),
						"FindByName", type), new CodePrimitiveExpression(name));

				//CodeCastExpression cast = new CodeCastExpression (type, find_invoke);

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
			GenerateCode(rootType, rootNs, baseType, namesAndTypes, outFile);
		}

		static Dictionary<string, CodeTypeReference> GetNamesAndTypes(XmlNode root, XmlNamespaceManager nsmgr)
		{
			var res = new Dictionary<string, CodeTypeReference>();

			foreach (string attrib in new[] { "x:Name", "x2009:Name" })
			{
				XmlNodeList names =
					root.SelectNodes(
						"//*[@" + attrib +
						"][not(ancestor:: f:DataTemplate) and not(ancestor:: f:ControlTemplate) and not(ancestor:: f:Style)]", nsmgr);
				foreach (XmlNode node in names)
				{
					// Don't take the root canvas
					if (node == root)
						continue;

					XmlAttribute attr = node.Attributes["Name", "http://schemas.microsoft.com/winfx/2006/xaml"] ??
					                    node.Attributes["Name", "http://schemas.microsoft.com/winfx/2009/xaml"];
					XmlAttribute typeArgsAttr = node.Attributes["x:TypeArguments"];
					var typeArgsList = typeArgsAttr == null ? null : typeArgsAttr.Value.Split(',').ToList();
					string name = attr.Value;

					res[name] = GetType(node.NamespaceURI, node.LocalName, typeArgsList, node.GetNamespaceOfPrefix);
				}
			}

			return res;
		}

		static CodeTypeReference GetType(string nsuri, string type, IList<string> typeArguments = null,
			Func<string, string> getNamespaceOfPrefix = null)
		{
			var ns = GetNamespace(nsuri);
			if (ns != null)
				type = String.Concat(ns, ".", type);

			if (typeArguments != null)
				type = String.Concat(type, "`", typeArguments.Count);

			var returnType = new CodeTypeReference(type);
			if (ns != null)
				returnType.Options |= CodeTypeReferenceOptions.GlobalReference;

			if (typeArguments != null)
			{
				foreach (var typeArg in typeArguments)
				{
					var ns_uri = "";
					var _type = typeArg;
					if (typeArg.Contains(":"))
					{
						var prefix = typeArg.Split(':')[0].Trim();
						ns_uri = getNamespaceOfPrefix(prefix);
						_type = typeArg.Split(':')[1].Trim();
					}
					returnType.TypeArguments.Add(GetType(ns_uri, _type, null, getNamespaceOfPrefix));
				}
			}

			return returnType;
		}

		static string GetNamespace(string namespaceuri)
		{
			if (!XmlnsHelper.IsCustom(namespaceuri))
				return "Xamarin.Forms";
			if (namespaceuri == "http://schemas.microsoft.com/winfx/2009/xaml")
				return "System";
			if (namespaceuri != "http://schemas.microsoft.com/winfx/2006/xaml" && !namespaceuri.Contains("clr-namespace"))
				throw new Exception(String.Format("Can't load types from xmlns {0}", namespaceuri));
			return XmlnsHelper.ParseNamespaceFromXmlns(namespaceuri);
		}
	}
}