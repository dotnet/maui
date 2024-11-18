using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace UITest.Analyzers.NUnit
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NUnitTestMissingCategoryMethodCodeFixProvider)), Shared]
	public class NUnitTestMissingCategoryMethodCodeFixProvider : CodeFixProvider
	{
		const string AddCategoryAttributeToMethodCodeFixTitle = "Add [Category] attribute to method";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
			=> ImmutableArray.Create(NUnitTestMissingCategoryAnalyzer.DiagnosticId);

		// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
		public sealed override FixAllProvider GetFixAllProvider()
			=> WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			// TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			// Find the type declaration identified by the diagnostic.
			var declaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()?.OfType<MethodDeclarationSyntax>()?.FirstOrDefault();

			if (declaration is not null && context.Document is not null)
			{
				// Register a code action that will invoke the fix.
				context.RegisterCodeFix(
					CodeAction.Create(
						title: AddCategoryAttributeToMethodCodeFixTitle,
						createChangedDocument: c => AddCategoryAttributeToMethodAsync(context.Document, declaration, c),
						equivalenceKey: nameof(AddCategoryAttributeToMethodCodeFixTitle)),
					diagnostic);
			}
		}

		private async Task<Document> AddCategoryAttributeToMethodAsync(Document document, MethodDeclarationSyntax methodDecl, CancellationToken cancellationToken)
		{
			// Generate the new [Category("Uncategorized")] attribute.
			var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
				SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Category"))
					.WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
						SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("\"PlaceholderCategory\"")))))));

			// Add the attribute to the method.
			var newMethod = methodDecl.AddAttributeLists(attributeList);

			// Replace the old method with the new method in the syntax tree.
			var root = await document.GetSyntaxRootAsync(cancellationToken);
			var newRoot = root?.ReplaceNode(methodDecl, newMethod);

			if (newRoot is null)
			{
				return document;
			}

			// Return the updated document.
			return document.WithSyntaxRoot(newRoot);
		}
	}
}
