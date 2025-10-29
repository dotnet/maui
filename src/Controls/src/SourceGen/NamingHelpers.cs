using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.Maui.Controls.SourceGen;

#nullable enable
static class NamingHelpers
{
	static IDictionary<object, IDictionary<string, int>> _lastId = new Dictionary<object, IDictionary<string, int>>();

	public static string CreateUniqueVariableName(SourceGenContext context, ISymbol symbol)
	{
		while (context.ParentContext != null)
			context = context.ParentContext;

		return CreateUniqueVariableNameImpl(context, symbol);
	}

	internal static string CreateUniqueVariableNameImpl(object context, ISymbol symbol)
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
		return CreateUniqueVariableNameImpl(context, symbol.ToDisplayString(ShortFormat) + suffix);
#pragma warning restore RS0030 // Do not use banned APIs
	}

	static string CreateUniqueVariableNameImpl(object context, string baseName)
	{
		baseName = CamelCase(baseName);
		if (!_lastId.TryGetValue(context, out var lastIdForContext))
		{
			lastIdForContext = new Dictionary<string, int>();
			_lastId[context] = lastIdForContext;
		}
		if (!lastIdForContext.TryGetValue(baseName, out var lastId))
			lastId = 0;

		lastIdForContext[baseName] = lastId + 1;
		return lastId == 0 && SyntaxFacts.GetKeywordKind(baseName) == SyntaxKind.None ? baseName : $"{baseName}{lastId}";
	}

	static string CamelCase(string name)
	{
		name = name.Replace(".", "_");
		if (string.IsNullOrEmpty(name))
			return name;
		name = Regex.Replace(name, "([A-Z])([A-Z]+)($|[A-Z])", m => m.Groups[1].Value + m.Groups[2].Value.ToLowerInvariant() + m.Groups[3].Value);
		return char.ToLowerInvariant(name[0]) + name.Substring(1);
	}
}