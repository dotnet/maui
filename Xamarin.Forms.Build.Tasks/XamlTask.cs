using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Mono.Cecil;

using Xamarin.Forms.Xaml;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using Mono.Cecil.Mdb;

namespace Xamarin.Forms.Build.Tasks
{
	[LoadInSeparateAppDomain]
	public abstract class XamlTask : MarshalByRefObject, ITask
	{
		[Required]
		public string Assembly { get; set; }
		public string DependencyPaths { get; set; }
		public string ReferencePath { get; set; }
		[Obsolete("this is no longer used")]
		public int Verbosity { get; set; }
		public bool DebugSymbols { get; set; }
		public string DebugType { get; set; }

		TaskLoggingHelper _log;

		internal XamlTask()
		{
			_log = new TaskLoggingHelper(this);
		}

		public IBuildEngine BuildEngine { get; set; }
		public ITaskHost HostObject { get; set; }

		protected Logger Logger { get; set; }

		public bool Execute()
		{
			Logger = new Logger(_log);
			IList<Exception> _;
			return Execute(out _);
		}

		public abstract bool Execute(out IList<Exception> thrownExceptions);

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
		public static bool IsXaml(this EmbeddedResource resource, ModuleDefinition module, out string classname)
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

				var rootClass = root.Attributes["Class", XamlParser.X2006Uri] ??
								root.Attributes["Class", XamlParser.X2009Uri];
				if (rootClass != null) {
					classname = rootClass.Value;
					return true;
				}

				//no x:Class, but it might be a RD without x:Class and with <?xaml-comp compile="true" ?>
				//in that case, it has a XamlResourceIdAttribute
				var typeRef = GetTypeForResourceId(module, resource.Name);
				if (typeRef != null) {
					classname = typeRef.FullName;
					return true;
				}

				return false;
			}
		}

		static TypeReference GetTypeForResourceId(ModuleDefinition module, string resourceId)
		{
			foreach (var ca in module.GetCustomAttributes()) {
				if (!TypeRefComparer.Default.Equals(ca.AttributeType, module.ImportReference(typeof(XamlResourceIdAttribute))))
					continue;
				if (ca.ConstructorArguments[0].Value as string != resourceId)
					continue;
				return ca.ConstructorArguments[2].Value as TypeReference;
			}
			return null;
		}
	}
}