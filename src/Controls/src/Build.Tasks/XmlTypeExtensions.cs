using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	static class XmlTypeExtensions
	{
		static readonly string _xmlnsDefinitionName = typeof(XmlnsDefinitionAttribute).FullName;

		static IList<XmlnsDefinitionAttribute> GatherXmlnsDefinitionAttributes(ModuleDefinition module)
		{
			var xmlnsDefinitions = new List<XmlnsDefinitionAttribute>();

			if (module.AssemblyReferences?.Count > 0)
			{
				// Search for the attribute in the assemblies being
				// referenced.
				GatherXmlnsDefinitionAttributes(xmlnsDefinitions, module.Assembly, module.Assembly);

				foreach (var asmRef in module.AssemblyReferences)
				{
					var asmDef = module.AssemblyResolver.Resolve(asmRef);
					GatherXmlnsDefinitionAttributes(xmlnsDefinitions, asmDef, module.Assembly);
				}
			}
			else
			{
				// Use standard MAUI assemblies
				// (Should only happen in unit tests)
				var requiredAssemblies = new[] {
					typeof(XamlLoader).Assembly,
					typeof(View).Assembly,
				};
				foreach (var assembly in requiredAssemblies)
					foreach (XmlnsDefinitionAttribute attribute in assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), false))
					{
						attribute.AssemblyName ??= assembly.FullName;
						ValidateProtectedXmlns(attribute.XmlNamespace, attribute.AssemblyName);
						xmlnsDefinitions.Add(attribute);
					}
			}

			return xmlnsDefinitions;
		}
		static void ValidateProtectedXmlns(string xmlNamespace, string assemblyName)
		{
			//maui, and x: xmlns are protected
			if (xmlNamespace != XamlParser.MauiUri && xmlNamespace != XamlParser.X2009Uri)
				return;

			//we know thos assemblies, they are fine in maui or x xmlns
			if (assemblyName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase)
				|| assemblyName.StartsWith("System", StringComparison.OrdinalIgnoreCase)
				|| assemblyName.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase))
				return;

			throw new BuildException(BuildExceptionCode.InvalidXaml, null, null,
				$"Protected Xmlns {xmlNamespace}. Can't add assembly {assemblyName}.");

		}
		static void GatherXmlnsDefinitionAttributes(List<XmlnsDefinitionAttribute> xmlnsDefinitions, AssemblyDefinition asmDef, AssemblyDefinition currentAssembly)
		{
			foreach (var ca in asmDef.CustomAttributes)
			{
				if (ca.AttributeType.FullName == _xmlnsDefinitionName)
				{
					var attr = GetXmlnsDefinition(ca, asmDef);
					//only add globalxmlns definition from the current assembly
					if (attr.XmlNamespace == XamlParser.MauiGlobalUri
						&& asmDef != currentAssembly)
						continue;
					ValidateProtectedXmlns(attr.XmlNamespace, attr.AssemblyName);
					xmlnsDefinitions.Add(attr);
				}
			}
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
			return new XmlType(namespaceURI, typename, null).GetTypeReference(cache, module, xmlInfo);
		}

		public static bool TryGetTypeReference(this XmlType xmlType, XamlCache cache, ModuleDefinition module, IXmlLineInfo xmlInfo, bool expandToExtension, out TypeReference typeReference)
		{
			IList<XmlnsDefinitionAttribute> xmlnsDefinitions = cache.GetXmlnsDefinitions(module, GatherXmlnsDefinitionAttributes);

			var typeArguments = xmlType.TypeArguments;

			IEnumerable<TypeReference> types = xmlType.GetTypeReferences(xmlnsDefinitions, module.Assembly.Name.Name, (typeInfo) =>
			{
				if (typeInfo.clrNamespace.StartsWith("http")) //aggregated xmlns, might result in a typeload exception
					return null;
				string typeName = typeInfo.typeName.Replace('+', '/'); //Nested types
				var type = module.GetTypeDefinition(cache, (typeInfo.assemblyName, typeInfo.clrNamespace, typeName));
				if (type is not null && type.IsPublicOrVisibleInternal(module))
					return type;
				return null;
			}, expandToExtension: expandToExtension);

			if (types.Distinct(TypeRefComparer.Default).Skip(1).Any())
			{
				typeReference = null;
				return false;
			}

			var type = types.Distinct(TypeRefComparer.Default).FirstOrDefault();
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

		static XmlnsDefinitionAttribute GetXmlnsDefinition(this CustomAttribute ca, AssemblyDefinition asmDef)
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

		public static IList<XmlnsPrefixAttribute> GetXmlnsPrefixAttributes(ModuleDefinition module)
		{
			var xmlnsPrefixes = new List<XmlnsPrefixAttribute>();
			foreach (var ca in module.Assembly.CustomAttributes)
			{
				if (ca.AttributeType.FullName == typeof(XmlnsPrefixAttribute).FullName)
				{
					var attr = new XmlnsPrefixAttribute(
						ca.ConstructorArguments[0].Value as string,
						ca.ConstructorArguments[1].Value as string);
					xmlnsPrefixes.Add(attr);
				}
			}

			if (module.AssemblyReferences?.Count > 0)
			{
				// Search for the attribute in the assemblies being
				// referenced.
				foreach (var asmRef in module.AssemblyReferences)
				{
					try
					{
						var asmDef = module.AssemblyResolver.Resolve(asmRef);
						foreach (var ca in asmDef.CustomAttributes)
						{
							if (ca.AttributeType.FullName == typeof(XmlnsPrefixAttribute).FullName)
							{
								var attr = new XmlnsPrefixAttribute(
									ca.ConstructorArguments[0].Value as string,
									ca.ConstructorArguments[1].Value as string);
								xmlnsPrefixes.Add(attr);
							}
						}
					}
					catch (System.Exception)
					{
						// Ignore assembly resolution errors
					}
				}
			}
			return xmlnsPrefixes;
		}
	}
}
