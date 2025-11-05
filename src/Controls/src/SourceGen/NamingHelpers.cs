using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.Maui.Controls.SourceGen;

#nullable enable
static class NamingHelpers
{
	public static string CreateUniqueVariableName(SourceGenContext context, ISymbol symbol)
	{
		while (context.ParentContext != null)
			context = context.ParentContext;

		return CreateUniqueVariableNameImpl(context, symbol, lowFirst: true);
	}

	public static string CreateUniqueTypeName(SourceGenContext context, string baseName)
	{
		while (context.ParentContext != null)
			context = context.ParentContext;

		return CreateUniqueVariableNameImpl(context, baseName, lowFirst: false);
	}

	internal static string CreateUniqueVariableNameImpl(SourceGenContext context, ISymbol symbol, bool lowFirst = true)
	{
		string suffix = "";
		if (symbol is IArrayTypeSymbol arrayTypeSymbol)
		{
			symbol = arrayTypeSymbol.ElementType;
			suffix = "Array";
		}

		SymbolDisplayFormat ShortFormat =
		new(
			globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
			typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
			propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
			parameterOptions: SymbolDisplayParameterOptions.IncludeName,
			miscellaneousOptions:
				SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
				SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

#pragma warning disable RS0030 // Do not use banned APIs
		return CreateUniqueVariableNameImpl(context, symbol.ToDisplayString(ShortFormat) + suffix, lowFirst);
#pragma warning restore RS0030 // Do not use banned APIs
	}

	static string CreateUniqueVariableNameImpl(SourceGenContext context, string baseName, bool lowFirst)
	{
		baseName = CamelCase(baseName, lowFirst);
		var lastIdForContext = context.lastIdForName;
		var lastId = lastIdForContext.AddOrUpdate(baseName, 0, (key, oldValue) => oldValue + 1);
		return lastId == 0 && SyntaxFacts.GetKeywordKind(baseName) == SyntaxKind.None ? baseName : $"{baseName}{lastId}";
	}

	static string CamelCase(string name, bool lowFirst)
	{
		name = name.Replace(".", "_");
		if (string.IsNullOrEmpty(name))
			return name;
		name = Regex.Replace(name, "([A-Z])([A-Z]+)($|[A-Z])", m => m.Groups[1].Value + m.Groups[2].Value.ToLowerInvariant() + m.Groups[3].Value);

		return lowFirst ? char.ToLowerInvariant(name[0]) + name.Substring(1) : name;
	}
}