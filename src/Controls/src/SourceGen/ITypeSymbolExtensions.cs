using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

static partial class ITypeSymbolExtensions
{
	//Allow resolving base type for root elements
	static ITypeSymbol? GetBaseType(this ITypeSymbol type, SourceGenContext? context)
	{
		var baseType = type.BaseType;
		if (context == null)
			return baseType;
		if (baseType != null && !baseType.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default))
			return baseType;

		if (context.RootType != null && type.Equals(context.RootType, SymbolEqualityComparer.Default))
			return context.BaseType;

		return baseType;
	}

	public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol, SourceGenContext? context)
	{
		foreach (var member in symbol.GetMembers())
		{
			yield return member;
		}

		var baseType = symbol.GetBaseType(context);

		while (baseType != null)
		{
			foreach (var member in baseType.GetMembers())
			{
				yield return member;
			}

			baseType = baseType.GetBaseType(context);
		}
		foreach (var iface in symbol.AllInterfaces)
		{
			foreach (var member in iface.GetMembers())
			{
				yield return member;
			}
		}
	}

	public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol, string name, SourceGenContext? context)
	{
		foreach (var member in symbol.GetMembers(name))
		{
			yield return member;
		}

		var baseType = symbol.GetBaseType(context);
		while (baseType != null)
		{
			foreach (var member in baseType.GetMembers(name))
			{
				yield return member;
			}

			baseType = baseType.GetBaseType(context);
		}
		foreach (var iface in symbol.AllInterfaces)
		{
			foreach (var member in iface.GetMembers(name))
			{
				yield return member;
			}
		}
	}

	public static IEnumerable<IPropertySymbol> GetAllProperties(this ITypeSymbol symbol, SourceGenContext? context)
		=> symbol.GetAllMembers(context).OfType<IPropertySymbol>();
	public static IEnumerable<IPropertySymbol> GetAllProperties(this ITypeSymbol symbol, string name, SourceGenContext? context)
		=> symbol.GetAllMembers(name, context).OfType<IPropertySymbol>();

	public static IEnumerable<IMethodSymbol> GetAllMethods(this ITypeSymbol symbol, SourceGenContext? context)
		=> symbol.GetAllMembers(context).OfType<IMethodSymbol>();
	public static IEnumerable<IMethodSymbol> GetAllMethods(this ITypeSymbol symbol, string name, SourceGenContext? context)
		=> symbol.GetAllMembers(name, context).OfType<IMethodSymbol>();

	public static IEnumerable<IFieldSymbol> GetFields(this ITypeSymbol symbol)
		=> symbol.GetMembers().OfType<IFieldSymbol>();
	public static IEnumerable<IFieldSymbol> GetAllFields(this ITypeSymbol symbol, SourceGenContext? context)
		=> symbol.GetAllMembers(context).OfType<IFieldSymbol>();
	public static IEnumerable<IFieldSymbol> GetAllFields(this ITypeSymbol symbol, string name, SourceGenContext? context)
		=> symbol.GetAllMembers(name, context).OfType<IFieldSymbol>();

	public static IEnumerable<IMethodSymbol> GetConstructors(this ITypeSymbol symbol, SourceGenContext? context)
		=> symbol.GetMembers().OfType<IMethodSymbol>().Where(m => m.IsStatic == false && m.MethodKind == MethodKind.Constructor);
	public static IEnumerable<AttributeData> GetAllAttributes(this ITypeSymbol symbol, SourceGenContext? context)
	{
		foreach (var attribute in symbol.GetAttributes())
		{
			yield return attribute;
		}

		var baseType = symbol.GetBaseType(context);
		while (baseType != null)
		{
			foreach (var attribute in baseType.GetAttributes())
			{
				if (attribute.IsInherited())
					yield return attribute;
			}

			baseType = baseType.GetBaseType(context);
		}
	}

	public static IEnumerable<AttributeData> GetAllAttributes(this ITypeSymbol symbol, ITypeSymbol attributeType, SourceGenContext? context)
		=> symbol.GetAllAttributes(context).Where(ad => SymbolEqualityComparer.Default.Equals(ad.AttributeClass, attributeType));
	public static IEnumerable<AttributeData> GetAttributes(this ITypeSymbol symbol, ITypeSymbol attributeType)
		=> symbol.GetAttributes().Where(ad => SymbolEqualityComparer.Default.Equals(ad.AttributeClass, attributeType));

	public static IEnumerable<IEventSymbol> GetAllEvents(this ITypeSymbol symbol, SourceGenContext? context)
		=> symbol.GetAllMembers(context).OfType<IEventSymbol>();
	public static IEnumerable<IEventSymbol> GetAllEvents(this ITypeSymbol symbol, string name, SourceGenContext? context)
		=> symbol.GetAllMembers(name, context).OfType<IEventSymbol>();

	public static bool InheritsFrom(this ITypeSymbol type, ITypeSymbol baseType, SourceGenContext? context)
	{
		if (type == null || baseType == null)
			return false;

		if (SymbolEqualityComparer.Default.Equals(type, baseType))
			return true;

		return type.GetBaseType(context)?.InheritsFrom(baseType, context) ?? false;
	}

	public static bool Implements(this ITypeSymbol type, ITypeSymbol iface)
	{
		if (type == null || iface == null)
			return false;

		return type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iface));
	}

	public static bool IsInterface(this ITypeSymbol type)
		=> type.TypeKind == TypeKind.Interface;

	public static bool ImplementsGeneric(this ITypeSymbol type, ITypeSymbol iface, out ITypeSymbol[] typeArguments)
	{
		typeArguments = [];
		if (type == null || iface == null)
			return false;

		foreach (var i in type.AllInterfaces)
		{
			if (SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, iface))
			{
				typeArguments = [.. i.TypeArguments];
				return true;
			}
		}

		return false;
	}

	public static bool IsPublic(this ISymbol symbol)
		=> symbol.DeclaredAccessibility == Accessibility.Public;
	public static bool IsPublicOrVisibleInternal(this ITypeSymbol type, IEnumerable<IAssemblySymbol> internalsVisible)
	{
		// return types that are public
		if (type.DeclaredAccessibility == Accessibility.Public)
			return true;

		// only return internal types if they are visible to us
		if (type.DeclaredAccessibility == Accessibility.Internal && internalsVisible.Contains(type.ContainingAssembly, SymbolEqualityComparer.Default))
			return true;

		return false;
	}

	public static bool CanAdd(this ITypeSymbol type, SourceGenContext? context)
		=> type.AllInterfaces.Any(i => i.ToString() == "System.Collections.IEnumerable")
		&& type.GetAllMethods("Add", context).Any(m => m.Parameters.Length == 1);
}
