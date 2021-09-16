using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.SourceGen
{
	public static class SyntaxNodeExtensions
	{
		public static ITypeSymbol? GetReturnType(this MethodDeclarationSyntax methodDeclarationSyntax, Compilation compilation)
		{
			var model = compilation.GetSemanticModel(methodDeclarationSyntax.SyntaxTree);
			return model.GetSymbolInfo(methodDeclarationSyntax.ReturnType).Symbol as ITypeSymbol;
		}

		public static string? GetMethodName(this MethodDeclarationSyntax methodDeclarationSyntax, Compilation compilation)
			=> methodDeclarationSyntax.GetSymbol(compilation)?.Name;

		public static ISymbol? GetSymbol(this SyntaxNode source, Compilation compilation)
			=> compilation.GetSemanticModel(source.SyntaxTree).GetDeclaredSymbol(source);

		public static bool TryGetParentSyntax<T>(this SyntaxNode? syntaxNode, out T? result)
			where T : SyntaxNode
		{
			// set defaults
			result = null;

			if (syntaxNode == null)
				return false;

			try
			{
				syntaxNode = syntaxNode.Parent;

				if (syntaxNode == null)
					return false;

				if (syntaxNode.GetType() == typeof(T))
				{
					result = syntaxNode as T;
					return true;
				}

				return TryGetParentSyntax(syntaxNode, out result);
			}
			catch
			{
				return false;
			}
		}
	}
}
