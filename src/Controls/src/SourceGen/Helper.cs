using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.Controls.SourceGen
{
	internal static class Helper
	{
		internal const string Singleton = nameof(Singleton);
		internal const string Scoped = nameof(Scoped);
		internal const string Transient = nameof(Transient);

		// Determine the namespace the class/enum/struct/record is declared in, if any
		internal static string GetNamespace(BaseTypeDeclarationSyntax syntax)
		{
			// If we don't have a namespace at all we'll return an empty string
			// This accounts for the "default namespace" case
			string @namespace = string.Empty;

			// Get the containing syntax node for the type declaration
			// (could be a nested type, for example)
			SyntaxNode? potentialNamespaceParent = syntax.Parent;

			// Keep moving "out" of nested classes etc until we get to a namespace
			// or until we run out of parents
			while (potentialNamespaceParent is not null
				&& potentialNamespaceParent is not NamespaceDeclarationSyntax
				&& potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
			{
				potentialNamespaceParent = potentialNamespaceParent.Parent;
			}

			// Build up the final namespace by looping until we no longer have a namespace declaration
			if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
			{
				// We have a namespace. Use that as the type
				@namespace = namespaceParent.Name.ToString();

				// Keep moving "out" of the namespace declarations until we 
				// run out of nested namespace declarations
				while (namespaceParent.Parent is NamespaceDeclarationSyntax parent)
				{
					// Add the outer namespace as a prefix to the final namespace
					@namespace = $"{parent.Name}.{@namespace}";
					namespaceParent = parent;
				}
			}

			// return the final namespace
			return @namespace;
		}
	}
}
