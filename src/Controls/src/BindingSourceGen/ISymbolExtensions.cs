using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.BindingSourceGen;

internal static class ISymbolExtensions
{
	internal static bool IsAccessible(this ISymbol symbol) =>
		symbol.DeclaredAccessibility == Accessibility.Public
		|| symbol.DeclaredAccessibility == Accessibility.Internal
		|| symbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal;

	internal static AccessorKind ToAccessorKind(this ISymbol symbol)
	{
		return symbol switch
		{
			IFieldSymbol _ => AccessorKind.Field,
			IPropertySymbol _ => AccessorKind.Property,
			_ => throw new ArgumentException("Symbol is not a field or property.", nameof(symbol))
		};
	}

	internal static string GetIndexerName(this ISymbol? elementAccessSymbol)
	{
		const string defaultName = "Item";

		if (elementAccessSymbol is not IPropertySymbol propertySymbol)
		{
			return defaultName;
		}

		var containgType = propertySymbol.ContainingType;
		if (containgType == null)
		{
			return defaultName;
		}

		var defaultMemberAttribute = GetAttribute(containgType, "DefaultMemberAttribute");
		if (defaultMemberAttribute != null)
		{
			return GetAttributeValue(defaultMemberAttribute);
		}

		var indexerNameAttr = GetAttribute(propertySymbol, "IndexerNameAttribute");
		if (indexerNameAttr != null)
		{
			return GetAttributeValue(indexerNameAttr);
		}

		return defaultName;

		AttributeData? GetAttribute(ISymbol symbol, string attributeName)
		{
			return symbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.Name == attributeName);
		}

		string GetAttributeValue(AttributeData attribute)
		{
			return (attribute.ConstructorArguments.Length > 0 ? attribute.ConstructorArguments[0].Value as string : null) ?? defaultName;
		}
	}
}

public enum AccessorKind
{
	Field,
	Property,
}
