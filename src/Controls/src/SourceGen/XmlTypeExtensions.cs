using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

static class XmlTypeExtensions
{
	public static ITypeSymbol? GetTypeSymbol(this XmlType xmlType, Action<Diagnostic>? reportDiagnostic, Compilation compilation, AssemblyAttributes xmlnsCache)
	{
		if (TryResolveTypeSymbol(xmlType, reportDiagnostic, compilation, xmlnsCache, out var symbol))
			return symbol!;
		if (reportDiagnostic is not null)
		{
			//FIXME report location
			reportDiagnostic(Diagnostic.Create(Descriptors.TypeResolution, null, $"{xmlType.NamespaceUri}:{xmlType.Name}"));
			return null;
		}
		throw new Exception($"Unable to resolve {xmlType.NamespaceUri}:{xmlType.Name}");
	}

	static Dictionary<Compilation, Dictionary<XmlType, INamedTypeSymbol>> s_typeSymbolCache = new();
	public static bool TryResolveTypeSymbol(this XmlType xmlType, Action<Diagnostic>? reportDiagnostic, Compilation compilation, AssemblyAttributes xmlnsCache, out INamedTypeSymbol? symbol)
	{
		if (!s_typeSymbolCache.TryGetValue(compilation, out var xmlTypeCache))
		{
			xmlTypeCache = [];
			s_typeSymbolCache[compilation] = xmlTypeCache;
		}

		if (xmlTypeCache.TryGetValue(xmlType, out symbol))
			return true;

		var name = xmlType.Name.Split(':').Last(); //strip prefix
		var genericSuffix = xmlType.TypeArguments is not null ? $"`{xmlType.TypeArguments.Count}" : string.Empty;

		if (!xmlnsCache.ClrNamespacesForXmlns.TryGetValue(xmlType.NamespaceUri, out var namespaces))
		{
			XmlnsHelper.ParseXmlns(xmlType.NamespaceUri, out _, out var ns, out _, out _);
			namespaces = [ns!];						
		}

		var extsuffixes = (name != "DataTemplate" && !name.EndsWith("Extension", StringComparison.Ordinal)) ? new [] {"Extension", string.Empty} : [string.Empty];
		foreach (var suffix in extsuffixes)
        {		
			var types = namespaces.Select(ns => $"{ns}.{name}{suffix}{genericSuffix}").SelectMany(typeName => compilation.GetTypesByMetadataName(typeName)).Where(ts => ts.IsPublicOrVisibleInternal(xmlnsCache.InternalsVisible)).Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>().ToArray();

			if (types.Length > 1)
			{
				symbol = null;
				if (reportDiagnostic is not null)
					reportDiagnostic(Diagnostic.Create(Descriptors.DuplicateTypeError, null, $"{xmlType.NamespaceUri}:{xmlType.Name}"));
				return false;
			}
			if (types.Length == 1)
			{
				symbol = types[0];
				if (symbol.IsGenericType && xmlType.TypeArguments is not null)
				{
					var typeArgs = xmlType.TypeArguments.Select(typeArg => typeArg.GetTypeSymbol(reportDiagnostic, compilation, xmlnsCache)!).ToArray();
					symbol = symbol.Construct(typeArgs);
				}
				xmlTypeCache[xmlType] = symbol;
				return true;
			}
        }
		symbol = null;
		return false;
	}

	public static ITypeSymbol? GetTypeSymbol(this string nameAndPrefix, Action<Diagnostic>? reportDiagnostic, Compilation compilation, AssemblyAttributes xmlnsCache, INode node)
		=> GetTypeSymbol(nameAndPrefix, reportDiagnostic, compilation, xmlnsCache, node.NamespaceResolver, (IXmlLineInfo)node);

	public static ITypeSymbol? GetTypeSymbol(this string nameAndPrefix, Action<Diagnostic>? reportDiagnostic, Compilation compilation, AssemblyAttributes xmlnsCache, IXmlNamespaceResolver resolver, IXmlLineInfo lineInfo)
	{
		XmlType xmlType = TypeArgumentsParser.ParseSingle(nameAndPrefix, resolver, lineInfo);
		return xmlType.GetTypeSymbol(reportDiagnostic, compilation, xmlnsCache);
	}

	static string? GetClrNamespace(string namespaceuri)
	{
		if (namespaceuri == XamlParser.X2009Uri)
			return "System";

		if (namespaceuri != XamlParser.X2006Uri &&
			!namespaceuri.StartsWith("clr-namespace", StringComparison.InvariantCulture) &&
			!namespaceuri.StartsWith("using:", StringComparison.InvariantCulture))
			return null;

		return XmlnsHelper.ParseNamespaceFromXmlns(namespaceuri);
	}

	public static bool RepresentsType(this XmlType xmlType, string namespaceUri, string name)
	{
		return xmlType.Name == name && xmlType.NamespaceUri == namespaceUri;
	}

	// 	static string GetTypeNameFromCustomNamespace(XmlType xmlType, Compilation compilation, AssemblyCaches xmlnsCache)
	// 	{
	// #nullable disable
	// 		string typeName = xmlType.GetTypeReference<string>(xmlnsCache.XmlnsDefinitions, null,
	// 			(typeInfo) =>
	// 			{
	// 				string typeName = typeInfo.typeName.Replace('+', '/'); //Nested types
	// 				string fullName = $"{typeInfo.clrNamespace}.{typeInfo.typeName}";
	// 				IList<INamedTypeSymbol> types = compilation.GetTypesByMetadataName(fullName);

	// 				if (types.Count == 0)
	// 				{
	// 					return null;
	// 				}

	// 				foreach (INamedTypeSymbol type in types)
	// 				{
	// 					// skip over types that are not in the correct assemblies
	// 					if (type.ContainingAssembly.Identity.Name != typeInfo.assemblyName)
	// 					{
	// 						continue;
	// 					}

	// 					if (!type.IsPublicOrVisibleInternal(xmlnsCache.InternalsVisible))
	// 					{
	// 						continue;
	// 					}

	// 					int i = fullName.IndexOf('`');
	// 					if (i > 0)
	// 					{
	// 						fullName = fullName.Substring(0, i);
	// 					}
	// 					return fullName;
	// 				}

	// 				return null;
	// 			});

	// 		return typeName;
	// #nullable enable
	// 	}
}