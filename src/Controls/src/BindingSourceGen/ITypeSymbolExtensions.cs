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
		var isAccessible = typeSymbol.IsAccessible();
		return new TypeDescription(
			GlobalName: GetGlobalName(typeSymbol, isNullable, typeSymbol.IsValueType),
			IsNullable: isNullable,
			IsGenericParameter: typeSymbol.Kind == SymbolKind.TypeParameter, //TODO: Add support for generic parameters
			IsValueType: typeSymbol.IsValueType,
			IsAccessible: isAccessible,
			AssemblyQualifiedName: isAccessible ? null : GetAssemblyQualifiedName(typeSymbol));
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

	private static string GetAssemblyQualifiedName(this ITypeSymbol typeSymbol)
	{
		// For UnsafeAccessorType, we need a format like:
		// "FullTypeName, AssemblyName, Version=X.X.X.X, Culture=neutral, PublicKeyToken=null"
		// However, for source-generated code in the same assembly, we can use a simpler format
		
		var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
			.Replace("global::", ""); // Remove global:: prefix for assembly-qualified name
		
		var containingAssembly = typeSymbol.ContainingAssembly;
		if (containingAssembly != null)
		{
			// Simple format for same-assembly access
			return $"{typeName}, {containingAssembly.Name}";
		}
		
		return typeName;
	}
}
