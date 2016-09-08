using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	static class XmlTypeExtensions
	{
		public static TypeReference GetTypeReference(this XmlType xmlType, ModuleDefinition module, IXmlLineInfo xmlInfo)
		{
			var namespaceURI = xmlType.NamespaceUri;
			var elementName = xmlType.Name;
			var typeArguments = xmlType.TypeArguments;

			List<Tuple<string, string>> lookupAssemblies = new List<Tuple<string, string>>(); //assembly, namespace
			List<string> lookupNames = new List<string>();

			if (!XmlnsHelper.IsCustom(namespaceURI))
			{
				lookupAssemblies.Add(new Tuple<string, string>("Xamarin.Forms.Core", "Xamarin.Forms"));
				lookupAssemblies.Add(new Tuple<string, string>("Xamarin.Forms.Xaml", "Xamarin.Forms.Xaml"));
			}
			else if (namespaceURI == "http://schemas.microsoft.com/winfx/2009/xaml" ||
			         namespaceURI == "http://schemas.microsoft.com/winfx/2006/xaml")
			{
				lookupAssemblies.Add(new Tuple<string, string>("Xamarin.Forms.Xaml", "Xamarin.Forms.Xaml"));
				lookupAssemblies.Add(new Tuple<string, string>("mscorlib", "System"));
				lookupAssemblies.Add(new Tuple<string, string>("System", "System"));
			}
			else
			{
				string ns;
				string typename;
				string asmstring;
				string targetPlatform;

				XmlnsHelper.ParseXmlns(namespaceURI, out typename, out ns, out asmstring, out targetPlatform);
				asmstring = asmstring ?? module.Assembly.Name.Name;
				lookupAssemblies.Add(new Tuple<string, string>(asmstring, ns));
			}

			lookupNames.Add(elementName);
			if (namespaceURI == "http://schemas.microsoft.com/winfx/2009/xaml")
				lookupNames.Add(elementName + "Extension");
			for (var i = 0; i < lookupNames.Count; i++)
			{
				var name = lookupNames[i];
				if (name.Contains(":"))
					name = name.Substring(name.LastIndexOf(':') + 1);
				if (typeArguments != null)
					name += "`" + typeArguments.Count; //this will return an open generic Type
				lookupNames[i] = name;
			}

			TypeReference type = null;
			foreach (var asm in lookupAssemblies)
			{
				if (type != null)
					break;
				foreach (var name in lookupNames)
				{
					if (type != null)
						break;

					var assemblydefinition = module.Assembly.Name.Name == asm.Item1
						? module.Assembly
						: module.AssemblyResolver.Resolve(asm.Item1);
					type = assemblydefinition.MainModule.GetType(asm.Item2, name);
					if (type == null)
					{
						var exportedtype =
							assemblydefinition.MainModule.ExportedTypes.FirstOrDefault(
								(ExportedType arg) => arg.IsForwarder && arg.Namespace == asm.Item2 && arg.Name == name);
						if (exportedtype != null)
							type = exportedtype.Resolve();
					}
				}
			}

			if (type != null && typeArguments != null && type.HasGenericParameters)
			{
				type =
					module.Import(type)
						.MakeGenericInstanceType(typeArguments.Select(x => GetTypeReference(x, module, xmlInfo)).ToArray());
			}

			if (type == null)
				throw new XamlParseException(string.Format("Type {0} not found in xmlns {1}", elementName, namespaceURI), xmlInfo);

			return module.Import(type);
		}
	}
}