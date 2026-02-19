using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.BindingSourceGen;

internal static class ISymbolExtensions
{
	private const string INotifyPropertyChangedFullName = "System.ComponentModel.INotifyPropertyChanged";

	internal static bool IsAccessible(this ISymbol symbol) =>
		symbol.DeclaredAccessibility == Accessibility.Public
		|| symbol.DeclaredAccessibility == Accessibility.Internal
		|| symbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal;

	/// <summary>
	/// Determines if a type definitively implements INotifyPropertyChanged.
	/// Returns true only if the type directly implements INPC and this is known at compile-time.
	/// Returns false for all other cases, including when the implementation status is unknown at compile-time.
	/// </summary>
	internal static bool ImplementsINPC(this ITypeSymbol? typeSymbol)
	{
		if (typeSymbol == null)
			return false;

		// Check if the type directly implements INPC
		return typeSymbol.AllInterfaces.Any(i => i.ToDisplayString() == INotifyPropertyChangedFullName);
	}

	/// <summary>
	/// Determines if a type might implement INotifyPropertyChanged at runtime.
	/// Returns true for types where the INPC implementation status cannot be determined at compile-time:
	/// - Type parameters (generic types) since they could be constrained to INPC at runtime
	/// - Interfaces (implementations could implement INPC)
	/// - Unsealed classes (derived types could implement INPC)
	/// Returns false for sealed types and definitively known non-INPC types.
	/// </summary>
	internal static bool MaybeImplementsINPC(this ITypeSymbol? typeSymbol)
	{
		if (typeSymbol == null)
			return true; // Be conservative if we can't determine the type

		// If we know it implements INPC, it's not "maybe", it definitely does
		if (typeSymbol.ImplementsINPC())
			return false;

		// Type parameters could be constrained to INPC types at runtime
		if (typeSymbol.TypeKind == TypeKind.TypeParameter)
			return true;

		// Interfaces could have implementations that implement INPC
		if (typeSymbol.TypeKind == TypeKind.Interface)
			return true;

		// Unsealed classes could have derived types that implement INPC
		if (typeSymbol.TypeKind == TypeKind.Class && !typeSymbol.IsSealed)
			return true;

		// For sealed types (including string, arrays, structs, etc.) that don't implement INPC,
		// we know they can never implement it
		return false;
	}

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
	/// Determines if a property's setter is accessible for binding purposes.
	/// For properties, checks the SetMethod accessibility.
	/// Returns true if there's no setter (read-only properties don't need setter access).
	/// Returns true if the setter exists and is accessible.
	/// Returns false only if a setter exists but is inaccessible.
	/// For other symbols, uses the default IsAccessible check.
	/// </summary>
	internal static bool IsSetterAccessible(this ISymbol symbol)
	{
		if (symbol is IPropertySymbol propertySymbol)
		{
			// Read-only properties (no setter) are OK - we don't need to write to them
			// so we return true to avoid marking them as inaccessible
			if (propertySymbol.SetMethod == null)
				return true;
			
			// Check if the setter method itself is accessible
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
