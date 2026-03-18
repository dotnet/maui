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
		public const string MissingCategoryDiagnosticId = "MAUI0001";
		public const string MultipleCategoriesDiagnosticId = "MAUI0002";

		const string MissingCategoryTitle = "Test methods should have exactly one Category";
		const string MissingCategoryMessageFormat = "Test method '{0}' should be marked with exactly one `[Category]` attribute on the method or its parent class";
		const string MissingCategoryDescription = "Test methods should be marked with exactly one `[Category]` attribute on the method or its parent class.";

		const string MultipleCategoriesTitle = "Test methods should have exactly one Category";
		const string MultipleCategoriesMessageFormat = "Test method '{0}' has {1} `[Category]` attributes but should have exactly one";
		const string MultipleCategoriesDescription = "Test methods should have exactly one `[Category]` attribute, either on the method or its parent class.";

		private const string Category = "Testing";

		private static readonly DiagnosticDescriptor MissingCategoryRule = new DiagnosticDescriptor(
			MissingCategoryDiagnosticId,
			MissingCategoryTitle,
			MissingCategoryMessageFormat,
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: MissingCategoryDescription);

		private static readonly DiagnosticDescriptor MultipleCategoriesRule = new DiagnosticDescriptor(
			MultipleCategoriesDiagnosticId,
			MultipleCategoriesTitle,
			MultipleCategoriesMessageFormat,
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: MultipleCategoriesDescription);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(MissingCategoryRule, MultipleCategoriesRule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context)
		{
			var methodSymbol = (IMethodSymbol)context.Symbol;

			// Check if the method has the [Test] attribute.
			var hasTestAttribute = methodSymbol.GetAttributes().Any(attr => attr?.AttributeClass?.Name == "TestAttribute");

			if (!hasTestAttribute)
			{
				return;
			}

			// Count category attributes on the method
			int methodCategoryCount = CountCategoryAttributes(methodSymbol.GetAttributes());

			// Count category attributes on the containing class
			var containingClass = methodSymbol.ContainingType;
			int classCategoryCount = CountCategoryAttributes(containingClass.GetAttributes());

			int totalCategoryCount = methodCategoryCount + classCategoryCount;

			// If it has [Test] but no [Category], report missing category diagnostic.
			if (totalCategoryCount == 0)
			{
				var diagnostic = Diagnostic.Create(MissingCategoryRule, methodSymbol.Locations[0], methodSymbol.Name);
				context.ReportDiagnostic(diagnostic);
			}
			// If it has more than one [Category], report multiple categories diagnostic.
			else if (totalCategoryCount > 1)
			{
				var diagnostic = Diagnostic.Create(MultipleCategoriesRule, methodSymbol.Locations[0], methodSymbol.Name, totalCategoryCount);
				context.ReportDiagnostic(diagnostic);
			}
		}

		/// <summary>
		/// Counts the number of Category attributes, considering both direct [Category] attributes
		/// and attributes that derive from CategoryAttribute (but excluding conditional ignore attributes).
		/// </summary>
		private static int CountCategoryAttributes(ImmutableArray<AttributeData> attributes)
		{
			int count = 0;
			foreach (var attr in attributes)
			{
				if (attr?.AttributeClass == null)
				{
					continue;
				}

				// Check if it's a direct [Category] attribute
				if (attr.AttributeClass.Name == "CategoryAttribute")
				{
					count++;
					continue;
				}

				// Check if it derives from CategoryAttribute (but exclude platform-specific ignore attributes)
				// These attributes conditionally derive from CategoryAttribute or IgnoreAttribute based on platform,
				// so we should not count them as category attributes
				var attributeName = attr.AttributeClass.Name;
				if (IsPlatformIgnoreAttribute(attributeName))
				{
					continue;
				}

				// Check the base type hierarchy for CategoryAttribute
				var baseType = attr.AttributeClass.BaseType;
				while (baseType != null)
				{
					if (baseType.Name == "CategoryAttribute")
					{
						count++;
						break;
					}
					baseType = baseType.BaseType;
				}
			}
			return count;
		}

		/// <summary>
		/// Returns true if the attribute is a platform-specific ignore attribute that conditionally
		/// derives from CategoryAttribute based on the target platform.
		/// </summary>
		private static bool IsPlatformIgnoreAttribute(string attributeName)
		{
			return attributeName == "FailsOnAndroidWhenRunningOnXamarinUITestAttribute" ||
				   attributeName == "FailsOnIOSWhenRunningOnXamarinUITestAttribute" ||
				   attributeName == "FailsOnMacWhenRunningOnXamarinUITestAttribute" ||
				   attributeName == "FailsOnWindowsWhenRunningOnXamarinUITestAttribute" ||
				   attributeName == "FailsOnAllPlatformsWhenRunningOnXamarinUITestAttribute" ||
				   // Also check without the "Attribute" suffix
				   attributeName == "FailsOnAndroidWhenRunningOnXamarinUITest" ||
				   attributeName == "FailsOnIOSWhenRunningOnXamarinUITest" ||
				   attributeName == "FailsOnMacWhenRunningOnXamarinUITest" ||
				   attributeName == "FailsOnWindowsWhenRunningOnXamarinUITest" ||
				   attributeName == "FailsOnAllPlatformsWhenRunningOnXamarinUITest";
		}
	}
}
