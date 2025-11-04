using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DeviceTests.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class XUnitTestMissingCategoryAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "MAUI1002";

		const string Title = "Test methods should have a Category";
		const string MessageFormat = "Test method '{0}' should be marked with a `[Category]` attribute on the method or its parent class";
		const string Description = "xUnit device test methods should be marked with a `[Category]` attribute on the method or its parent class.";

		private const string Category = "Testing";

		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
			DiagnosticId,
			Title,
			MessageFormat,
			Category,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context)
		{
			var methodSymbol = (IMethodSymbol)context.Symbol;

			// Check if the method has xUnit test attributes ([Fact] or [Theory])
			var hasFactAttribute = methodSymbol.GetAttributes().Any(attr =>
				attr?.AttributeClass?.Name == "FactAttribute" ||
				attr?.AttributeClass?.Name == "TheoryAttribute");

			if (!hasFactAttribute)
				return;

			// Check if the method has the [Category] attribute
			var hasCategoryAttribute = methodSymbol.GetAttributes().Any(attr =>
				attr?.AttributeClass?.Name == "CategoryAttribute");

			if (!hasCategoryAttribute)
			{
				// Check the containing class (declaring class only, NOT base classes)
				// xUnit does NOT inherit [Category] from base classes, so we shouldn't check them
				hasCategoryAttribute = methodSymbol.ContainingType.GetAttributes().Any(attr =>
					attr?.AttributeClass?.Name == "CategoryAttribute");
			}

			// If it has [Fact] or [Theory] but not [Category], report a diagnostic
			if (!hasCategoryAttribute)
			{
				var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name);
				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
