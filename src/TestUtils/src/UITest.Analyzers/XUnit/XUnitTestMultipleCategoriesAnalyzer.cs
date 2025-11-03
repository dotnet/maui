using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UITest.Analyzers.XUnit
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class XUnitTestMultipleCategoriesAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "MAUI1001";

		const string Title = "Test methods should have only one unique category";
		const string MessageFormat = "Test method '{0}' has {1} unique categories: {2}. Only a single unique category is allowed per test.";
		const string Description = "xUnit device tests should have exactly one unique category. Multiple categories cause tests to be silently skipped.";

		private const string Category = "Testing";

		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
			DiagnosticId,
			Title,
			MessageFormat,
			Category,
			DiagnosticSeverity.Error,
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

			// Collect all categories from method and class
			var allCategories = new HashSet<string>(StringComparer.Ordinal);

			// Get categories from the method
			foreach (var attr in methodSymbol.GetAttributes())
			{
				if (attr?.AttributeClass?.Name == "CategoryAttribute")
				{
					AddCategoriesFromAttribute(attr, allCategories);
				}
			}

			// Get categories from the containing class and its base classes
			var currentType = methodSymbol.ContainingType;
			while (currentType != null)
			{
				foreach (var attr in currentType.GetAttributes())
				{
					if (attr?.AttributeClass?.Name == "CategoryAttribute")
					{
						AddCategoriesFromAttribute(attr, allCategories);
					}
				}
				currentType = currentType.BaseType;
			}

			// If there are multiple unique categories, report a diagnostic
			if (allCategories.Count > 1)
			{
				var categoriesList = string.Join(", ", allCategories.OrderBy(c => c));
				var diagnostic = Diagnostic.Create(
					Rule,
					methodSymbol.Locations[0],
					methodSymbol.Name,
					allCategories.Count,
					categoriesList);
				context.ReportDiagnostic(diagnostic);
			}
		}

		private static void AddCategoriesFromAttribute(AttributeData attr, HashSet<string> categories)
		{
			// Category attribute can have multiple string arguments
			if (attr.ConstructorArguments.Length > 0)
			{
				var arg = attr.ConstructorArguments[0];

				// Handle params string[] - could be a single value or array
				if (arg.Kind == TypedConstantKind.Array)
				{
					foreach (var item in arg.Values)
					{
						if (item.Value is string categoryName)
						{
							categories.Add(categoryName);
						}
					}
				}
				else if (arg.Value is string categoryName)
				{
					categories.Add(categoryName);
				}
			}
		}
	}
}
