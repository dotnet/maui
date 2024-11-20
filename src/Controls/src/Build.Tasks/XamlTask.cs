using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	public abstract class XamlTask : MarshalByRefObject, ITask
	{
		[Required]
		public string Assembly { get; set; }
		public string[] ReferencePath { get; set; }
		public bool DebugSymbols { get; set; }
		public string DebugType { get; set; }

		protected TaskLoggingHelper LoggingHelper { get; }

		internal XamlTask()
		{
			LoggingHelper = new TaskLoggingHelper(this);
		}

		public IBuildEngine BuildEngine { get; set; }
		public ITaskHost HostObject { get; set; }

		public bool Execute()
		{
			IList<Exception> _;
			return Execute(out _);
		}

		public abstract bool Execute(out IList<Exception> thrownExceptions);

		internal static ILRootNode ParseXaml(Stream stream, TypeReference typeReference)
		{
			using (var reader = XmlReader.Create(stream))
			{
				while (reader.Read())
				{
					//Skip until element
					if (reader.NodeType == XmlNodeType.Whitespace)
						continue;
					if (reader.NodeType != XmlNodeType.Element)
					{
						Debug.WriteLine("Unhandled node {0} {1} {2}", reader.NodeType, reader.Name, reader.Value);
						continue;
					}

					var xmlType = new XmlType(reader.NamespaceURI, reader.Name, XamlParser.GetTypeArguments(reader));
					var rootnode = new ILRootNode(xmlType, typeReference, reader as IXmlNamespaceResolver, ((IXmlLineInfo)reader).LineNumber, ((IXmlLineInfo)reader).LinePosition);
					XamlParser.ParseXaml(rootnode, reader);
					return rootnode;
				}
			}
			return null;
		}
	}

	static class CecilExtensions
	{
		public static bool IsXaml(this EmbeddedResource resource, XamlCache cache, ModuleDefinition module, out string classname)
		{
			classname = null;
			if (!resource.Name.EndsWith(".xaml", StringComparison.InvariantCulture))
				return false;

			using (var resourceStream = resource.GetResourceStream())
			using (var reader = XmlReader.Create(resourceStream))
			{
				// Read to the first Element
				while (reader.Read() && reader.NodeType != XmlNodeType.Element)
					;

				if (reader.NodeType != XmlNodeType.Element)
					return false;

				classname = reader.GetAttribute("Class", XamlParser.X2009Uri) ??
							reader.GetAttribute("Class", XamlParser.X2006Uri);
				if (classname != null)
					return true;

				//no x:Class, but it might be a RD without x:Class and with <?xaml-comp compile="true" ?>
				//in that case, it has a XamlResourceIdAttribute
				var typeRef = GetTypeForResourceId(cache, module, resource.Name);
				if (typeRef != null)
				{
					classname = typeRef.FullName;
					return true;
				}

				return false;
			}
		}

		static TypeReference GetTypeForResourceId(XamlCache cache, ModuleDefinition module, string resourceId)
		{
			foreach (var ca in module.GetCustomAttributes())
			{
				if (!TypeRefComparer.Default.Equals(ca.AttributeType, module.ImportReference(cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Xaml", "XamlResourceIdAttribute"))))
					continue;
				if (ca.ConstructorArguments[0].Value as string != resourceId)
					continue;
				return ca.ConstructorArguments[2].Value as TypeReference;
			}
			return null;
		}
	}
}
