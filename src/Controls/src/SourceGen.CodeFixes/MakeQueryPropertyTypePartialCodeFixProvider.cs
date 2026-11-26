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
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.Maui.Controls.SourceGen.CodeFixes;

/// <summary>
/// Fixes MAUI1200 by adding the <c>partial</c> modifier to a <c>[QueryProperty]</c> type and its containing types.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeQueryPropertyTypePartialCodeFixProvider)), Shared]
public sealed class MakeQueryPropertyTypePartialCodeFixProvider : CodeFixProvider
{
	// Keep in sync with Descriptors.QueryPropertyRequiresReflection and QueryPropertyGenerator.MissingPartialProperty.
	internal const string DiagnosticId = "MAUI1200";
	internal const string MissingPartialProperty = "MissingPartial";
	internal const string EquivalenceKey = nameof(MakeQueryPropertyTypePartialCodeFixProvider);

	public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticId);

	public override FixAllProvider GetFixAllProvider() =>
		FixAllProvider.Create(static async (context, document, diagnostics) =>
			document is null ? null : await MakePartialAsync(document, diagnostics, context.CancellationToken).ConfigureAwait(false));

	public override Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		foreach (var diagnostic in context.Diagnostics.Where(IsFixable))
		{
			context.RegisterCodeFix(
				CodeAction.Create(
					"Make type partial",
					cancellationToken => MakePartialAsync(context.Document, [diagnostic], cancellationToken),
					EquivalenceKey),
				diagnostic);
		}

		return Task.CompletedTask;
	}

	static bool IsFixable(Diagnostic diagnostic) =>
		diagnostic.Id == DiagnosticId && diagnostic.Properties.ContainsKey(MissingPartialProperty);

	static async Task<Document> MakePartialAsync(Document document, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root is null)
			return document;

		var declarations = new HashSet<TypeDeclarationSyntax>();
		foreach (var diagnostic in diagnostics.Where(IsFixable))
		{
			var type = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
			for (; type is not null; type = type.Parent?.FirstAncestorOrSelf<TypeDeclarationSyntax>())
			{
				if (!type.Modifiers.Any(SyntaxKind.PartialKeyword))
					declarations.Add(type);
			}
		}

		if (declarations.Count == 0)
			return document;

		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		foreach (var declaration in declarations)
			editor.SetModifiers(declaration, editor.Generator.GetModifiers(declaration).WithPartial(true));

		return editor.GetChangedDocument();
	}
}
