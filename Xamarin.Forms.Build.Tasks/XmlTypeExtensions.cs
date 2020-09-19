using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Build.Tasks
{
	static class XmlTypeExtensions
	{
		static Dictionary<ModuleDefinition, IList<XmlnsDefinitionAttribute>> s_xmlnsDefinitions = 
			new Dictionary<ModuleDefinition, IList<XmlnsDefinitionAttribute>>();
		static object _nsLock = new object();

		static IList<XmlnsDefinitionAttribute> GatherXmlnsDefinitionAttributes(ModuleDefinition module)
		{
			var xmlnsDefinitions = new List<XmlnsDefinitionAttribute>();

			if (module.AssemblyReferences?.Count > 0) {
				// Search for the attribute in the assemblies being
				// referenced.
				foreach (var asmRef in module.AssemblyReferences) {
					var asmDef = module.AssemblyResolver.Resolve(asmRef);
					foreach (var ca in asmDef.CustomAttributes) {
						if (ca.AttributeType.FullName == typeof(XmlnsDefinitionAttribute).FullName) {
							var attr = GetXmlnsDefinition(ca, asmDef);
							xmlnsDefinitions.Add(attr);
						}
					}
				}
			} else {
				// Use standard XF assemblies
				// (Should only happen in unit tests)
				var requiredAssemblies = new[] {
					typeof(XamlLoader).Assembly,
					typeof(View).Assembly,
				};
				foreach (var assembly in requiredAssemblies)
					foreach (XmlnsDefinitionAttribute attribute in assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), false)) {
						attribute.AssemblyName = attribute.AssemblyName ?? assembly.FullName;
						xmlnsDefinitions.Add(attribute);
					}
			}

			s_xmlnsDefinitions[module] = xmlnsDefinitions;
			return xmlnsDefinitions;
		}

		public static TypeReference GetTypeReference(string xmlType, ModuleDefinition module, BaseNode node)
		{
			var split = xmlType.Split(':');
			if (split.Length > 2)
				throw new BuildException(BuildExceptionCode.InvalidXaml, node as IXmlLineInfo, null, xmlType);

			string prefix, name;
			if (split.Length == 2) {
				prefix = split[0];
				name = split[1];
			} else {
				prefix = "";
				name = split[0];
			}
			var namespaceuri = node.NamespaceResolver.LookupNamespace(prefix) ?? "";
			return GetTypeReference(new XmlType(namespaceuri, name, null), module, node as IXmlLineInfo);
		}

		public static TypeReference GetTypeReference(string namespaceURI, string typename, ModuleDefinition module, IXmlLineInfo xmlInfo)
		{
			return new XmlType(namespaceURI, typename, null).GetTypeReference(module, xmlInfo);
		}

		public static bool TryGetTypeReference(this XmlType xmlType, ModuleDefinition module, IXmlLineInfo xmlInfo, out TypeReference typeReference)
		{
			IList<XmlnsDefinitionAttribute> xmlnsDefinitions = null;
			lock (_nsLock) {
				if (!s_xmlnsDefinitions.TryGetValue(module, out xmlnsDefinitions))
					xmlnsDefinitions = GatherXmlnsDefinitionAttributes(module);
			}

			var typeArguments = xmlType.TypeArguments;

			TypeReference type = xmlType.GetTypeReference(xmlnsDefinitions, module.Assembly.Name.Name, (typeInfo) => {
					string typeName = typeInfo.TypeName.Replace('+', '/'); //Nested types
					return module.GetTypeDefinition((typeInfo.AssemblyName, typeInfo.ClrNamespace, typeName));
				},
				out IList<XamlLoader.FallbackTypeInfo> potentialTypes);

			if (type != null && typeArguments != null && type.HasGenericParameters)
				type = module.ImportReference(type).MakeGenericInstanceType(typeArguments.Select(x => GetTypeReference(x, module, xmlInfo)).ToArray());

			return (typeReference = (type == null) ? null : module.ImportReference(type)) != null;
		}

		public static TypeReference GetTypeReference(this XmlType xmlType, ModuleDefinition module, IXmlLineInfo xmlInfo)
		{
			if (TryGetTypeReference(xmlType, module, xmlInfo, out TypeReference typeReference))
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
