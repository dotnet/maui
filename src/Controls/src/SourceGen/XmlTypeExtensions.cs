using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;

using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

static class XmlTypeExtensions
{
	public static ITypeSymbol? GetTypeSymbol(this XmlType xmlType, SourceGenContext context)
		=> xmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache, context.TypeCache);

	public static ITypeSymbol? GetTypeSymbol(this XmlType xmlType, Action<Diagnostic>? reportDiagnostic, Compilation compilation, AssemblyAttributes xmlnsCache, IDictionary<XmlType, ITypeSymbol> typeCache)
	{
		if (TryResolveTypeSymbol(xmlType, reportDiagnostic, compilation, xmlnsCache, typeCache, out var symbol))
			return symbol!;
		if (reportDiagnostic is not null)
		{
			//FIXME report location
			reportDiagnostic(Diagnostic.Create(Descriptors.TypeResolution, null, $"{xmlType.NamespaceUri}:{xmlType.Name}"));
			return null;
		}
		throw new Exception($"Unable to resolve {xmlType.NamespaceUri}:{xmlType.Name}");
	}

	public static ITypeSymbol? GetTypeSymbol(this string nameAndPrefix, SourceGenContext context, INode node)
		=> GetTypeSymbol(nameAndPrefix, context.ReportDiagnostic, context.Compilation, context.XmlnsCache, context.TypeCache, node);

	public static ITypeSymbol? GetTypeSymbol(this string nameAndPrefix, Action<Diagnostic>? reportDiagnostic, Compilation compilation, AssemblyAttributes xmlnsCache, IDictionary<XmlType, ITypeSymbol> typeCache, INode node)
		=> GetTypeSymbol(nameAndPrefix, reportDiagnostic, compilation, xmlnsCache, typeCache, node.NamespaceResolver, (IXmlLineInfo)node);

	public static ITypeSymbol? GetTypeSymbol(this string nameAndPrefix, Action<Diagnostic>? reportDiagnostic, Compilation compilation, AssemblyAttributes xmlnsCache, IDictionary<XmlType, ITypeSymbol> typeCache, IXmlNamespaceResolver resolver, IXmlLineInfo lineInfo)
	{
		XmlType xmlType = TypeArgumentsParser.ParseSingle(nameAndPrefix, resolver, lineInfo);
		return xmlType.GetTypeSymbol(reportDiagnostic, compilation, xmlnsCache, typeCache);
	}

	public static bool TryResolveTypeSymbol(this XmlType xmlType, Action<Diagnostic>? reportDiagnostic, Compilation compilation, AssemblyAttributes xmlnsCache, IDictionary<XmlType, ITypeSymbol> typeCache, out INamedTypeSymbol? symbol)
	{
		if (typeCache.TryGetValue(xmlType, out var s))
		{
			symbol = s as INamedTypeSymbol;
			return true;
		}

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
					var typeArgs = xmlType.TypeArguments.Select(typeArg => typeArg.GetTypeSymbol(reportDiagnostic, compilation, xmlnsCache, typeCache)!).ToArray();
					symbol = symbol.Construct(typeArgs);
				}
				typeCache[xmlType] = symbol;
				return true;
			}
        }
		symbol = null;
		return false;
	}

	public static bool RepresentsType(this XmlType xmlType, string namespaceUri, string name)
	{
		return xmlType.Name == name && xmlType.NamespaceUri == namespaceUri;
	}
}