using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.BindingSourceGen;

internal static class BindingGenerationUtilities
{

    internal static bool IsNullableValueType(ITypeSymbol typeInfo) =>
        typeInfo is INamedTypeSymbol namedTypeSymbol
            && namedTypeSymbol.IsGenericType
            && namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;
            
    internal static bool IsTypeNullable(ITypeSymbol typeInfo, bool enabledNullable)
    {
        if (!enabledNullable && typeInfo.IsReferenceType)
        {
            return true;
        }

        if (typeInfo.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return true;
        }

        return IsNullableValueType(typeInfo);
    }

    internal static TypeDescription CreateTypeNameFromITypeSymbol(ITypeSymbol typeSymbol, bool enabledNullable)
    {
        var isNullable = IsTypeNullable(typeSymbol, enabledNullable);
        return new TypeDescription(
            GlobalName: GetGlobalName(typeSymbol, isNullable, typeSymbol.IsValueType),
            IsNullable: isNullable,
            IsGenericParameter: typeSymbol.Kind == SymbolKind.TypeParameter,
            IsValueType: typeSymbol.IsValueType);
    }

    internal static TypeDescription CreateTypeDescriptionForExplicitCast(ITypeSymbol typeSymbol, bool enabledNullable)
    {
        var isNullable = IsTypeNullable(typeSymbol, enabledNullable);
        var name = GetGlobalName(typeSymbol, isNullable, typeSymbol.IsValueType);

        return new TypeDescription(
            GlobalName: name,
            IsNullable: isNullable,
            IsGenericParameter: typeSymbol.Kind == SymbolKind.TypeParameter,
            IsValueType: typeSymbol.IsValueType);
    }

    internal static TypeDescription CreateTypeDescriptionForAsCast(ITypeSymbol typeSymbol)
    {
        // We can cast to nullable value type or non-nullable reference type
        var name = typeSymbol.IsValueType ?
            ((INamedTypeSymbol)typeSymbol).TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) :
            typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return new TypeDescription(
            GlobalName: name,
            IsNullable: typeSymbol.IsValueType,
            IsGenericParameter: typeSymbol.Kind == SymbolKind.TypeParameter,
            IsValueType: typeSymbol.IsValueType);
    }

    internal static string GetGlobalName(ITypeSymbol typeSymbol, bool IsNullable, bool IsValueType)
    {
        if (IsNullable && IsValueType)
        {
            return ((INamedTypeSymbol)typeSymbol).TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}
