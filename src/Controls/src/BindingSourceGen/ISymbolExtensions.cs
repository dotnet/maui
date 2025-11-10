using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.BindingSourceGen;

internal static class ISymbolExtensions
{
	internal static bool IsAccessible(this ISymbol symbol) =>
		symbol.DeclaredAccessibility == Accessibility.Public
		|| symbol.DeclaredAccessibility == Accessibility.Internal
		|| symbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal;

	/// <summary>
	/// Determines if a property's getter is accessible.
	/// For properties, checks the GetMethod accessibility.
	/// For other symbols, uses the default IsAccessible check.
	/// </summary>
	internal static bool IsGetterAccessible(this ISymbol symbol)
	{
		if (symbol is IPropertySymbol propertySymbol)
		{
			// If there's no getter, it's not accessible for reading
			if (propertySymbol.GetMethod == null)
				return false;
			
			return propertySymbol.GetMethod.IsAccessible();
		}
		
		// For fields and other symbols, use the default check
		return symbol.IsAccessible();
	}

	/// <summary>
	/// Determines if a property's setter is accessible.
	/// For properties, checks the SetMethod accessibility.
	/// Returns true if there's no setter (read-only property) or if the setter is accessible.
	/// Returns false only if a setter exists but is inaccessible.
	/// For other symbols, uses the default IsAccessible check.
	/// </summary>
	internal static bool IsSetterAccessible(this ISymbol symbol)
	{
		if (symbol is IPropertySymbol propertySymbol)
		{
			// If there's no setter, treat it as accessible (read-only is OK)
			// We only care about cases where a setter exists but is not accessible
			if (propertySymbol.SetMethod == null)
				return true;
			
			return propertySymbol.SetMethod.IsAccessible();
		}
		
		// For fields, use the default check (fields don't have separate getter/setter)
		return symbol.IsAccessible();
	}

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
