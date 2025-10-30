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
	public static ITypeSymbol? GetTypeSymbol(this XmlType xmlType, Action<Diagnostic>? reportDiagnostic, Compilation compilation, AssemblyCaches xmlnsCache)
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

	//todo this shouldn't report diagnostic directly, but that require changing GetTypeReference
	public static bool TryResolveTypeSymbol(this XmlType xmlType, Action<Diagnostic>? reportDiagnostic, Compilation compilation, AssemblyCaches xmlnsCache, out INamedTypeSymbol? symbol)
	{
		//TODO we need to cache types symbols at this level

		//This is a shortcut as we don't look for extensions, but used to work in the past, at least for XamlG
		// var ns = GetClrNamespace(xmlType.NamespaceUri);
		// if (ns != null )
		// {
		// 	var ss = compilation.GetTypesByMetadataName($"{ns}.{xmlType.Name}");
		// 	var symbols = ss.Where(t=>t.IsPublicOrVisibleInternal(xmlnsCache.InternalsVisible))
		// 				.ToImmutableArray();
		// 	if (symbols.Length == 1)
		// 	{
		// 		symbol = symbols.Single();
		// 		if (symbol.IsGenericType && xmlType.TypeArguments is not null)
		// 		{
		// 			var typeArguments = xmlType.TypeArguments.Select(typeArg => typeArg.GetTypeSymbol(reportDiagnostic, compilation, xmlnsCache)!).ToArray();
		// 			symbol = symbol.Construct(typeArguments);
		// 		}
		// 		return true;
		// 	}
		// 	if (symbols.Length > 1)
		// 	{
		// 		symbol = null;
		// 		if (reportDiagnostic is not null)
		// 			reportDiagnostic(Diagnostic.Create(Descriptors.DuplicateTypeError, null, $"{xmlType.NamespaceUri}:{xmlType.Name}"));

		// 		return false;
		// 	}
		// }

		var xmlnsDefinitions = xmlnsCache.XmlnsDefinitions;
		var symbols = xmlType.GetTypeReferences(
			xmlnsDefinitions,
			compilation.AssemblyName!,
			typeInfo =>
			{
				var ts = compilation.GetTypesByMetadataName($"{typeInfo.clrNamespace}.{typeInfo.typeName}")
						.Where(t => t.IsPublicOrVisibleInternal(xmlnsCache.InternalsVisible));
				if (ts.Count() == 1)
					return ts.Single();
				if (ts.Count() > 1 && reportDiagnostic is not null)
					reportDiagnostic(Diagnostic.Create(Descriptors.DuplicateTypeError, null, $"{xmlType.NamespaceUri}:{xmlType.Name}"));

				return null;
			}
		).Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>();

		if (symbols is null || !symbols.Any())
		{
			symbol = null;
			return false;
		}

		if (symbols.Count() > 1)
		{
			symbol = null;
			if (reportDiagnostic is not null)
				reportDiagnostic(Diagnostic.Create(Descriptors.DuplicateTypeError, null, $"{xmlType.NamespaceUri}:{xmlType.Name}"));
			return false;
		}

		symbol = symbols.Single();
		if (symbol is null)
			return false;

		if (symbol.IsGenericType && xmlType.TypeArguments is not null)
		{
			var typeArguments = xmlType.TypeArguments.Select(typeArg => typeArg.GetTypeSymbol(reportDiagnostic, compilation, xmlnsCache)!).ToArray();
			symbol = symbol.Construct(typeArguments);
		}
		return true;
	}

	public static ITypeSymbol? GetTypeSymbol(this string nameAndPrefix, Action<Diagnostic>? reportDiagnostic, Compilation compilation, AssemblyCaches xmlnsCache, INode node)
		=> GetTypeSymbol(nameAndPrefix, reportDiagnostic, compilation, xmlnsCache, node.NamespaceResolver, (IXmlLineInfo)node);

	public static ITypeSymbol? GetTypeSymbol(this string nameAndPrefix, Action<Diagnostic>? reportDiagnostic, Compilation compilation, AssemblyCaches xmlnsCache, IXmlNamespaceResolver resolver, IXmlLineInfo lineInfo)
	{
		XmlType xmlType = TypeArgumentsParser.ParseSingle(nameAndPrefix, resolver, lineInfo);
		return xmlType.GetTypeSymbol(reportDiagnostic, compilation, xmlnsCache);
	}

	//FIXME should return a ITypeSymbol, and properly construct it for generics. globalalias param should go away
	// public static ITypeSymbol GetTypeSymbol(this XmlType xmlType, Compilation compilation, AssemblyCaches xmlnsCache, IDictionary<XmlType, string> typeCache, bool globalAlias = true)
	// {
	// 	if (typeCache.TryGetValue(xmlType, out string returnType))
	// 	{
	// 		if (globalAlias)
	// 			returnType = $"global::{returnType}";
	// 		return returnType;
	// 	}

	// 	var ns = GetClrNamespace(xmlType.NamespaceUri);
	// 	if (ns != null)
	// 		returnType = $"{ns}.{xmlType.Name}";
	// 	else
	// 		// It's an external, non-built-in namespace URL.
	// 		returnType = GetTypeNameFromCustomNamespace(xmlType, compilation, xmlnsCache);

	// 	if (xmlType.TypeArguments != null)
	// 		returnType = $"{returnType}<{string.Join(", ", xmlType.TypeArguments.Select(typeArg => GetTypeName(typeArg, compilation, xmlnsCache, typeCache)))}>";

	// 	typeCache[xmlType] = returnType;
	// 	if (globalAlias)
	// 		returnType = $"global::{returnType}";
	// 	return returnType;
	// }

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