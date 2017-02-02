using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	static class XmlTypeExtensions
	{
 		public static TypeReference GetTypeReference (string namespaceURI, string typename, ModuleDefinition module, IXmlLineInfo xmlInfo)
 		{
 			return new XmlType (namespaceURI, typename, null).GetTypeReference (module, xmlInfo);
 		}

		static IList<XmlnsDefinitionAttribute> s_xmlnsDefinitions;

		static void GatherXmlnsDefinitionAttributes()
		{
			//this could be extended to look for [XmlnsDefinition] in all assemblies
			var assemblies = new [] {
				typeof(View).Assembly,
				typeof(XamlLoader).Assembly,
			};

			s_xmlnsDefinitions = new List<XmlnsDefinitionAttribute>();

			foreach (var assembly in assemblies)
				foreach (XmlnsDefinitionAttribute attribute in assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), false)) {
					s_xmlnsDefinitions.Add(attribute);
					attribute.AssemblyName = attribute.AssemblyName ?? assembly.FullName;
				}
		}

		public static TypeReference GetTypeReference(this XmlType xmlType, ModuleDefinition module, IXmlLineInfo xmlInfo)
		{
			if (s_xmlnsDefinitions == null)
				GatherXmlnsDefinitionAttributes();

			var namespaceURI = xmlType.NamespaceUri;
			var elementName = xmlType.Name;
			var typeArguments = xmlType.TypeArguments;

			var lookupAssemblies = new List<XmlnsDefinitionAttribute>();

			var lookupNames = new List<string>();

			foreach (var xmlnsDef in s_xmlnsDefinitions) {
				if (xmlnsDef.XmlNamespace != namespaceURI)
					continue;
				lookupAssemblies.Add(xmlnsDef);
			}

			if (lookupAssemblies.Count == 0) {
				string ns;
				string typename;
				string asmstring;
				string targetPlatform;

				XmlnsHelper.ParseXmlns(namespaceURI, out typename, out ns, out asmstring, out targetPlatform);
				asmstring = asmstring ?? module.Assembly.Name.Name;
				lookupAssemblies.Add(new XmlnsDefinitionAttribute(namespaceURI, ns) {
					AssemblyName = asmstring
				});
			}

			lookupNames.Add(elementName);
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

					var assemblydefinition = module.Assembly.Name.Name == asm.AssemblyName ?
												module.Assembly :
												module.AssemblyResolver.Resolve(AssemblyNameReference.Parse(asm.AssemblyName));

					type = assemblydefinition.MainModule.GetType(asm.ClrNamespace, name);
					if (type == null)
					{
						var exportedtype =
							assemblydefinition.MainModule.ExportedTypes.FirstOrDefault(
								(ExportedType arg) => arg.IsForwarder && arg.Namespace == asm.ClrNamespace && arg.Name == name);
						if (exportedtype != null)
							type = exportedtype.Resolve();
					}
				}
			}

			if (type != null && typeArguments != null && type.HasGenericParameters)
			{
				type =
					module.ImportReference(type)
						.MakeGenericInstanceType(typeArguments.Select(x => GetTypeReference(x, module, xmlInfo)).ToArray());
			}

			if (type == null)
				throw new XamlParseException(string.Format("Type {0} not found in xmlns {1}", elementName, namespaceURI), xmlInfo);

			return module.ImportReference(type);
		}
	}
}