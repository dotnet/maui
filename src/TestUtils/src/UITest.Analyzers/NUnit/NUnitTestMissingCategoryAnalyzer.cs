using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UITest.Analyzers.NUnit
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class NUnitTestMissingCategoryAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "NUnitTestMissingCategoryAnalyzer";

		const string Title = "Test methods should have a Category";
		const string MessageFormat = "Test method '{0}' should be marked with a `[Category]` attribute on the method or its parent class";
		const string Description = "Test methods should be marked with a `[Category]` attribute on the method or its parent class.";

		private const string Category = "Testing";

		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			// TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
			// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context)
		{
			var methodSymbol = (IMethodSymbol)context.Symbol;

			// Check if the method has the [Test] attribute.
			var hasTestAttribute = methodSymbol.GetAttributes().Any(attr => attr?.AttributeClass?.Name == "TestAttribute");

			// Check if the method has the [Category] attribute.
			var hasCategoryAttribute = methodSymbol.GetAttributes().Any(attr => attr?.AttributeClass?.Name == "CategoryAttribute");
			if (!hasCategoryAttribute)
			{
				// If the method does not have a [Category] attribute, check the containing class
				var containingClass = methodSymbol.ContainingType;
				hasCategoryAttribute = containingClass.GetAttributes().Any(attr => attr?.AttributeClass?.Name == "CategoryAttribute");
			}

			// If it has [Test] but not [Category], report a diagnostic.
			if (hasTestAttribute && !hasCategoryAttribute)
			{
				var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name);
				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
