using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.SourceGen
{
	public static class GeneratorSyntaxContextExtensions
	{
		public static string? GetFullName(this ISymbol symbol, string? ending = null)
		{
			//TODO: loop through and get parent
			var name = Combine(symbol.Name, ending);
			if (symbol.ContainingNamespace != null)
				return GetFullName(symbol.ContainingNamespace, name);

			return name;
		}

		static string? Combine(string? first, string? second)
		{
			if (string.IsNullOrWhiteSpace(second))
				return first;
			if (string.IsNullOrWhiteSpace(first))
				return second;
			return $"{first}.{second}";
		}

		public static string? GetReturnType(this GeneratorSyntaxContext context, MethodDeclarationSyntax methodDeclarationSyntax)
		{
			var retType = context.SemanticModel.GetSymbolInfo(methodDeclarationSyntax.ReturnType);

			return retType.Symbol?.GetFullName();
		}

		public static INamedTypeSymbol? GetTypeForSyntax(this GeneratorSyntaxContext context, TypeOfExpressionSyntax expression)
		{
			var interfaceType = context.SemanticModel.GetSymbolInfo(expression.Type);
			if (interfaceType.Symbol == null)
				return null;
			var s = GetFullName(interfaceType.Symbol);
			if (s != null && !string.IsNullOrEmpty(s))
				return context.SemanticModel.Compilation.GetTypeByMetadataName(s);
			return null;
		}
	}
}
