using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

static class ISymbolExtensions
{
	public static ImmutableArray<AttributeData> GetAttributes(this ISymbol symbol, string name)
			=> symbol.GetAttributes().Where(attribute => attribute.AttributeClass?.ToString() == name).ToImmutableArray();

	public static string ToFQDisplayString(this ISymbol symbol)
	{
#pragma warning disable RS0030 //we banned ToDisplayString in favor of this
		if (symbol is IFieldSymbol field)
			return field.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType));
		if (symbol is IPropertySymbol property)
			return property.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType));
		if (symbol is ITypeSymbol type)
			return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		if (symbol is IMethodSymbol method && method.IsStatic)
			return method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType));
#pragma warning restore RS0030
		throw new System.NotImplementedException();
	}

	public static string GetMetadataFullName(this INamedTypeSymbol type)
	{
		var typeNames = new Stack<string>();
		for (var current = type; current is not null; current = current.ContainingType)
			typeNames.Push(current.MetadataName);

		var namespaceNames = new Stack<string>();
		for (var current = type.ContainingNamespace; !current.IsGlobalNamespace; current = current.ContainingNamespace)
			namespaceNames.Push(current.MetadataName);

		return $"{string.Join(".", namespaceNames)}{(namespaceNames.Count > 0 ? "." : string.Empty)}{string.Join("+", typeNames)}";
	}

	public static string GetAssemblyQualifiedName(this INamedTypeSymbol type)
	{
		var metadataName = type.GetMetadataFullName();
		var typeArguments = new List<INamedTypeSymbol>();
		var hasUnsupportedTypeArgument = false;
		for (var current = type; current is not null; current = current.ContainingType)
		{
			if (!current.IsGenericType)
				continue;

			var currentTypeArguments = current.TypeArguments.OfType<INamedTypeSymbol>().ToArray();
			if (currentTypeArguments.Length != current.TypeArguments.Length)
			{
				hasUnsupportedTypeArgument = true;
				break;
			}

			typeArguments.InsertRange(0, currentTypeArguments);
		}

		if (typeArguments.Count > 0 && !hasUnsupportedTypeArgument)
			metadataName += $"[{string.Join(",", typeArguments.Select(typeArgument => $"[{typeArgument.GetAssemblyQualifiedName()}]"))}]";

		return $"{metadataName}, {type.ContainingAssembly.Identity}";
	}
}
