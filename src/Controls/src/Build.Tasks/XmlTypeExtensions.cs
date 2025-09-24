using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	static class XmlTypeExtensions
	{
		static IList<XmlnsDefinitionAttribute> GatherXmlnsDefinitionAttributes(ModuleDefinition module)
		{
			var xmlnsDefinitions = new List<XmlnsDefinitionAttribute>();

			if (module.AssemblyReferences?.Count > 0)
			{
				// Search for the attribute in the assemblies being
				// referenced.
				foreach (var asmRef in module.AssemblyReferences)
				{
					var asmDef = module.AssemblyResolver.Resolve(asmRef);
					foreach (var ca in asmDef.CustomAttributes)
					{
						if (ca.AttributeType.FullName == typeof(XmlnsDefinitionAttribute).FullName)
						{
							var attr = GetXmlnsDefinition(ca, asmDef);
							xmlnsDefinitions.Add(attr);
						}
					}
				}
			}
			else
			{
				// Use standard XF assemblies
				// (Should only happen in unit tests)
				var requiredAssemblies = new[] {
					typeof(XamlLoader).Assembly,
					typeof(View).Assembly,
				};
				foreach (var assembly in requiredAssemblies)
					foreach (XmlnsDefinitionAttribute attribute in assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), false))
					{
						attribute.AssemblyName = attribute.AssemblyName ?? assembly.FullName;
						xmlnsDefinitions.Add(attribute);
					}
			}

			return xmlnsDefinitions;
		}

		public static TypeReference GetTypeReference(XamlCache cache, string typeName, ModuleDefinition module, BaseNode node, bool expandToExtension = true)
		{
			try
			{
				XmlType xmlType = TypeArgumentsParser.ParseSingle(typeName, node.NamespaceResolver, (IXmlLineInfo)node);
				return GetTypeReference(xmlType, cache, module, node as IXmlLineInfo, expandToExtension: expandToExtension);
			}
			catch (XamlParseException)
			{
				throw new BuildException(BuildExceptionCode.InvalidXaml, node as IXmlLineInfo, null, typeName);
			}
		}

		public static TypeReference GetTypeReference(XamlCache cache, string namespaceURI, string typename, ModuleDefinition module, IXmlLineInfo xmlInfo, bool expandToExtension = true)
		{
			return new XmlType(namespaceURI, typename, null).GetTypeReference(cache, module, xmlInfo, expandToExtension: expandToExtension);
		}

		public static bool TryGetTypeReference(this XmlType xmlType, XamlCache cache, ModuleDefinition module, IXmlLineInfo xmlInfo, bool expandToExtension, out TypeReference typeReference)
		{
			IList<XmlnsDefinitionAttribute> xmlnsDefinitions = cache.GetXmlsDefinitions(module, GatherXmlnsDefinitionAttributes);

			var typeArguments = xmlType.TypeArguments;

			TypeReference type = xmlType.GetTypeReference(xmlnsDefinitions, module.Assembly.Name.Name, (typeInfo) =>
			{
				string typeName = typeInfo.typeName.Replace('+', '/'); //Nested types
				var type = module.GetTypeDefinition(cache, (typeInfo.assemblyName, typeInfo.clrNamespace, typeName));
				if (type is not null && type.IsPublicOrVisibleInternal(module))
					return type;
				return null;
			}, expandToExtension: expandToExtension);

			if (type != null && typeArguments != null && type.HasGenericParameters)
				type = module.ImportReference(type).MakeGenericInstanceType(typeArguments.Select(x => x.GetTypeReference(cache, module, xmlInfo)).ToArray());

			return (typeReference = (type == null) ? null : module.ImportReference(type)) != null;
		}

		public static TypeReference GetTypeReference(this XmlType xmlType, XamlCache cache, ModuleDefinition module, IXmlLineInfo xmlInfo, bool expandToExtension = true)
		{
			if (TryGetTypeReference(xmlType, cache, module, xmlInfo, expandToExtension: expandToExtension, out TypeReference typeReference))
				return typeReference;

			throw new BuildException(BuildExceptionCode.TypeResolution, xmlInfo, null, $"{xmlType.NamespaceUri}:{xmlType.Name}");
		}

		public static XmlnsDefinitionAttribute GetXmlnsDefinition(this CustomAttribute ca, AssemblyDefinition asmDef)
		{
			var attr = new XmlnsDefinitionAttribute(
							ca.ConstructorArguments[0].Value as string,
							ca.ConstructorArguments[1].Value as string);

			string assemblyName = null;
			if (ca.Properties.Count > 0)
				assemblyName = ca.Properties[0].Argument.Value as string;
			attr.AssemblyName = assemblyName ?? asmDef.Name.FullName;
			return attr;
		}
	}
}
