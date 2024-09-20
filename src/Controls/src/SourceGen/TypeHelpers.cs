using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

static class TypeHelpers
{
	public static string GetTypeName(XmlType xmlType, SourceGenContext context, bool globalAlias = true)
		=> GetTypeName(xmlType, context.Compilation, context.XmlnsCache, context.TypeCache, globalAlias);

	public static ITypeSymbol? ResolveTypeSymbol(this XmlType xmlType, SourceGenContext context)
	{
		var fqn = GetTypeName(xmlType, context, globalAlias: false);
		return context.Compilation.GetTypeByMetadataName(fqn);
	}

    public static string GetTypeName(XmlType xmlType, Compilation compilation, AssemblyCaches xmlnsCache, IDictionary<XmlType, string> typeCache, bool globalAlias = true)
	{
		if (typeCache.TryGetValue(xmlType, out string returnType))
		{
			if (globalAlias)
				returnType = $"global::{returnType}";
			return returnType;
		}

		var ns = GetClrNamespace(xmlType.NamespaceUri);
		if (ns != null)
		{
			returnType = $"{ns}.{xmlType.Name}";
		}
		else
		{
			// It's an external, non-built-in namespace URL.
			returnType = GetTypeNameFromCustomNamespace(xmlType, compilation, xmlnsCache);
		}

		if (xmlType.TypeArguments != null)
		{
			returnType = $"{returnType}<{string.Join(", ", xmlType.TypeArguments.Select(typeArg => GetTypeName(typeArg, compilation, xmlnsCache, typeCache)))}>";
		}
		typeCache[xmlType] = returnType;
		if (globalAlias)
			returnType = $"global::{returnType}";
		return returnType;
	}

	static string? GetClrNamespace(string namespaceuri)
	{
		if (namespaceuri == XamlParser.X2009Uri)
		{
			return "System";
		}

		if (namespaceuri != XamlParser.X2006Uri &&
			!namespaceuri.StartsWith("clr-namespace", StringComparison.InvariantCulture) &&
			!namespaceuri.StartsWith("using:", StringComparison.InvariantCulture))
		{
			return null;
		}

		return XmlnsHelper.ParseNamespaceFromXmlns(namespaceuri);
	}

	static string GetTypeNameFromCustomNamespace(XmlType xmlType, Compilation compilation, AssemblyCaches xmlnsCache)
	{
#nullable disable
		string typeName = xmlType.GetTypeReference<string>(xmlnsCache.XmlnsDefinitions, null,
			(typeInfo) =>
			{
				string typeName = typeInfo.typeName.Replace('+', '/'); //Nested types
				string fullName = $"{typeInfo.clrNamespace}.{typeInfo.typeName}";
				IList<INamedTypeSymbol> types = compilation.GetTypesByMetadataName(fullName);

				if (types.Count == 0)
				{
					return null;
				}

				foreach (INamedTypeSymbol type in types)
				{
					// skip over types that are not in the correct assemblies
					if (type.ContainingAssembly.Identity.Name != typeInfo.assemblyName)
					{
						continue;
					}

					if (!IsPublicOrVisibleInternal(type, xmlnsCache.InternalsVisible))
					{
						continue;
					}

					int i = fullName.IndexOf('`');
					if (i > 0)
					{
						fullName = fullName.Substring(0, i);
					}
					return fullName;
				}

				return null;
			});

		return typeName;
#nullable enable
	}

	static bool IsPublicOrVisibleInternal(INamedTypeSymbol type, IEnumerable<IAssemblySymbol> internalsVisible)
	{
		// return types that are public
		if (type.DeclaredAccessibility == Accessibility.Public)
		{
			return true;
		}

		// only return internal types if they are visible to us
		if (type.DeclaredAccessibility == Accessibility.Internal && internalsVisible.Contains(type.ContainingAssembly, SymbolEqualityComparer.Default))
		{
			return true;
		}

		return false;
	}

	public static bool CanAdd(this ITypeSymbol type) 
		=> type.AllInterfaces.Any(i => i.ToString() == "System.Collections.IEnumerable")
		&& type.GetAllMethods("Add").Any(m => m.Parameters.Length == 1);
	
}