using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.BindingSourceGen;

public static class ITypeSymbolExtensions
{
	public static bool IsTypeNullable(this ITypeSymbol typeInfo, bool enabledNullable)
	{
		if (!enabledNullable && typeInfo.IsReferenceType)
		{
			return true;
		}

		return typeInfo.IsNullableValueType() || typeInfo.IsNullableReferenceType();
	}

	public static TypeDescription CreateTypeDescription(this ITypeSymbol typeSymbol, bool enabledNullable)
	{
		var isNullable = IsTypeNullable(typeSymbol, enabledNullable);
		return new TypeDescription(
			GlobalName: GetGlobalName(typeSymbol, isNullable, typeSymbol.IsValueType),
			IsNullable: isNullable,
			IsGenericParameter: typeSymbol.Kind == SymbolKind.TypeParameter, //TODO: Add support for generic parameters
			IsValueType: typeSymbol.IsValueType);
	}

	private static bool IsNullableValueType(this ITypeSymbol typeInfo) =>
		typeInfo is INamedTypeSymbol namedTypeSymbol
			&& namedTypeSymbol.IsGenericType
			&& namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;

	private static bool IsNullableReferenceType(this ITypeSymbol typeInfo) =>
		typeInfo.IsReferenceType && typeInfo.NullableAnnotation == NullableAnnotation.Annotated;


	private static string GetGlobalName(this ITypeSymbol typeSymbol, bool isNullable, bool isValueType)
	{
		if (isNullable && isValueType)
		{
			// Strips the "?" from the type name
			return ((INamedTypeSymbol)typeSymbol).TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		}

		return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
	}
}
