using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.SourceGen
{
	public static class TypeSyntaxExtensions
	{
		public static string GetMethodName(this MethodDeclarationSyntax methodDeclarationSyntax)
			=> methodDeclarationSyntax.Identifier.Text;
	}
}
