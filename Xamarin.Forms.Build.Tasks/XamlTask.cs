using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Mono.Cecil;

using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	public abstract class XamlTask : AppDomainIsolatedTask
	{
		[Required]
		public string Assembly { get; set; }
		public string DependencyPaths { get; set; }
		public string ReferencePath { get; set; }
		public int Verbosity { get; set; }
		public bool DebugSymbols { get; set; }

		internal XamlTask()
		{
		}

		protected Logger Logger { get; set; }

		public override bool Execute()
		{
			Logger = new Logger(Log, Verbosity);
			return Execute(null);
		}

		public abstract bool Execute(IList<Exception> thrownExceptions);

		protected static MethodDefinition DuplicateMethodDef(TypeDefinition typeDef, MethodDefinition methodDef, string newName)
		{
			var dup = new MethodDefinition(newName, methodDef.Attributes, methodDef.ReturnType);
			dup.Body = methodDef.Body;
			typeDef.Methods.Add(dup);
			return dup;
		}

		internal static ILRootNode ParseXaml(Stream stream, TypeReference typeReference)
		{
			ILRootNode rootnode = null;
			using (var reader = XmlReader.Create(stream)) {
				while (reader.Read()) {
					//Skip until element
					if (reader.NodeType == XmlNodeType.Whitespace)
						continue;
					if (reader.NodeType != XmlNodeType.Element) {
						Debug.WriteLine("Unhandled node {0} {1} {2}", reader.NodeType, reader.Name, reader.Value);
						continue;
					}

					XamlParser.ParseXaml(
						rootnode = new ILRootNode(new XmlType(reader.NamespaceURI, reader.Name, null), typeReference, reader as IXmlNamespaceResolver), reader);
					break;
				}
			}
			return rootnode;
		}
	}

	static class CecilExtensions
	{
		public static bool IsXaml(this EmbeddedResource resource, out string classname)
		{
			classname = null;
			if (!resource.Name.EndsWith(".xaml", StringComparison.InvariantCulture))
				return false;

			using (var resourceStream = resource.GetResourceStream()) {
				var xmlDoc = new XmlDocument();
				xmlDoc.Load(resourceStream);

				var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);

				var root = xmlDoc.SelectSingleNode("/*", nsmgr);
				if (root == null)
					return false;

				var rootClass = root.Attributes ["Class", "http://schemas.microsoft.com/winfx/2006/xaml"] ??
								root.Attributes ["Class", "http://schemas.microsoft.com/winfx/2009/xaml"];
				if (rootClass == null)
					return false;
				classname = rootClass.Value;
				return true;
			}
		}
	}
}