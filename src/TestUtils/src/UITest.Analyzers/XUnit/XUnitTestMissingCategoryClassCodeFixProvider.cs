using System;
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

namespace UITest.Analyzers.XUnit
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(XUnitTestMissingCategoryClassCodeFixProvider)), Shared]
	public class XUnitTestMissingCategoryClassCodeFixProvider : CodeFixProvider
	{
		const string AddCategoryAttributeToClassCodeFixTitle = "Add [Category] attribute to class";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
			=> ImmutableArray.Create(XUnitTestMissingCategoryAnalyzer.DiagnosticId);

		public sealed override FixAllProvider GetFixAllProvider()
			=> WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			// Find the method declaration identified by the diagnostic.
			var declaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()?.OfType<MethodDeclarationSyntax>()?.FirstOrDefault();

			if (declaration is not null && context.Document is not null)
			{
				// Register a code action that will invoke the fix.
				context.RegisterCodeFix(
					CodeAction.Create(
						title: AddCategoryAttributeToClassCodeFixTitle,
						createChangedDocument: c => AddCategoryAttributeToClassAsync(context.Document, declaration, c),
						equivalenceKey: nameof(AddCategoryAttributeToClassCodeFixTitle)),
					diagnostic);
			}
		}

		private async Task<Document> AddCategoryAttributeToClassAsync(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			// Find the containing class declaration for the method
			var classDeclaration = methodDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
			if (classDeclaration == null)
			{
				return document;
			}

			// Check if the class already has a [Category] attribute
			var hasCategoryAttribute = classDeclaration.AttributeLists
				.SelectMany(attrList => attrList.Attributes)
				.Any(attr => attr.Name.ToString() == "Category");

			if (hasCategoryAttribute)
			{
				// The class already has a [Category] attribute, no need to add another
				return document;
			}

			// Create a new [Category] attribute with a placeholder category name
			var newAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("Category"))
				.WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
					SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("\"PlaceholderCategory\"")))));

			var newAttributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(newAttribute))
				.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

			// Add the new [Category] attribute to the class
			var newClassDeclaration = classDeclaration.AddAttributeLists(newAttributeList);

			// Replace the old class declaration with the new one in the syntax tree
			var newRoot = root?.ReplaceNode(classDeclaration, newClassDeclaration);

			if (newRoot is null)
			{
				return document;
			}

			// Return a new document with the updated syntax tree
			return document.WithSyntaxRoot(newRoot);
		}
	}
}
