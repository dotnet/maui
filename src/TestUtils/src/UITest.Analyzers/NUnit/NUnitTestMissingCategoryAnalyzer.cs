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
		public const string ShardedCategoryDiagnosticId = "MAUI0003";

		const string MissingCategoryTitle = "Test methods should have exactly one Category";
		const string MissingCategoryMessageFormat = "Test method '{0}' should be marked with exactly one `[Category]` attribute on the method or its parent class";
		const string MissingCategoryDescription = "Test methods should be marked with exactly one `[Category]` attribute on the method or its parent class.";

		const string MultipleCategoriesTitle = "Test methods should have exactly one Category";
		const string MultipleCategoriesMessageFormat = "Test method '{0}' has {1} `[Category]` attributes but should have exactly one";
		const string MultipleCategoriesDescription = "Test methods should have exactly one `[Category]` attribute, either on the method or its parent class.";

		const string ShardedCategoryTitle = "Sharded test categories should use ShardedTestCategory";
		const string ShardedCategoryMessageFormat = "Test method '{0}' uses the sharded category '{1}' directly; use `[ShardedTestCategory(UITestCategories.{1}, shard: ...)]` so both the umbrella and CI shard categories are registered";
		const string ShardedCategoryDescription = "Categories split across CI jobs must use ShardedTestCategory so every test remains addressable through both its umbrella category and exactly one shard category.";

		private const string Category = "Testing";

		// Keep this list in sync through the rebalance-ui-test-categories skill.
		private static readonly ImmutableArray<string> CiShardCategoryPrefixes =
			ImmutableArray.Create("CollectionView");

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

		private static readonly DiagnosticDescriptor ShardedCategoryRule = new DiagnosticDescriptor(
			ShardedCategoryDiagnosticId,
			ShardedCategoryTitle,
			ShardedCategoryMessageFormat,
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: ShardedCategoryDescription);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(MissingCategoryRule, MultipleCategoriesRule, ShardedCategoryRule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context)
		{
			var methodSymbol = (IMethodSymbol)context.Symbol;
			var methodAttributes = methodSymbol.GetAttributes();
			var testAttributes = methodAttributes.Where(IsTestAttribute).ToImmutableArray();

			if (testAttributes.IsEmpty)
			{
				return;
			}

			foreach (var attribute in methodAttributes.Concat(methodSymbol.ContainingType.GetAttributes()))
			{
				if (TryGetDirectCategory(attribute, out var category) &&
					CiShardCategoryPrefixes.Contains(category))
				{
					context.ReportDiagnostic(Diagnostic.Create(
						ShardedCategoryRule,
						methodSymbol.Locations[0],
						methodSymbol.Name,
						category));
				}

				if (TryGetTestCaseCategory(attribute, out category) &&
					CiShardCategoryPrefixes.Contains(category))
				{
					context.ReportDiagnostic(Diagnostic.Create(
						ShardedCategoryRule,
						methodSymbol.Locations[0],
						methodSymbol.Name,
						category));
				}
			}

			// Count category attributes on the method
			int methodCategoryCount = CountCategoryAttributes(methodAttributes);

			// Count category attributes on the containing class
			var containingClass = methodSymbol.ContainingType;
			int classCategoryCount = CountCategoryAttributes(containingClass.GetAttributes());

			int sharedCategoryCount = methodCategoryCount + classCategoryCount;
			var parameterizedTestAttributes = testAttributes
				.Where(IsParameterizedTestAttribute)
				.ToImmutableArray();

			var categoryCounts = parameterizedTestAttributes.IsEmpty
				? ImmutableArray.Create(sharedCategoryCount)
				: parameterizedTestAttributes
					.Select(attribute => sharedCategoryCount + CountTestCaseCategory(attribute))
					.ToImmutableArray();

			if (categoryCounts.Any(count => count == 0))
			{
				var diagnostic = Diagnostic.Create(MissingCategoryRule, methodSymbol.Locations[0], methodSymbol.Name);
				context.ReportDiagnostic(diagnostic);
			}
			else if (categoryCounts.Any(count => count > 1))
			{
				var diagnostic = Diagnostic.Create(
					MultipleCategoriesRule,
					methodSymbol.Locations[0],
					methodSymbol.Name,
					categoryCounts.Max());
				context.ReportDiagnostic(diagnostic);
			}
		}

		private static bool IsTestAttribute(AttributeData attribute)
		{
			return attribute.AttributeClass?.Name is
				"TestAttribute" or
				"TestCaseAttribute" or
				"TestCaseSourceAttribute" or
				"TheoryAttribute";
		}

		private static bool IsParameterizedTestAttribute(AttributeData attribute)
		{
			return attribute.AttributeClass?.Name is "TestCaseAttribute" or "TestCaseSourceAttribute";
		}

		private static int CountTestCaseCategory(AttributeData attribute)
		{
			return TryGetTestCaseCategory(attribute, out var category) && !IsCiShardCategory(category)
				? 1
				: 0;
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
					if (IsCiShardCategory(attr))
					{
						continue;
					}

					count++;
					continue;
				}

				if (attr.AttributeClass.Name is "ShardedTestCategoryAttribute" or "ShardedTestCategory")
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

		private static bool IsCiShardCategory(AttributeData attribute)
		{
			if (!TryGetDirectCategory(attribute, out var category))
			{
				return false;
			}

			return IsCiShardCategory(category);
		}

		private static bool IsCiShardCategory(string category)
		{
			foreach (var prefix in CiShardCategoryPrefixes)
			{
				if (category.Length > prefix.Length &&
					category.StartsWith(prefix, StringComparison.Ordinal) &&
					category.Skip(prefix.Length).All(char.IsDigit))
				{
					return true;
				}
			}

			return false;
		}

		private static bool TryGetDirectCategory(AttributeData attribute, out string category)
		{
			category = string.Empty;
			if (attribute.AttributeClass?.Name != "CategoryAttribute" ||
				attribute.ConstructorArguments.Length != 1 ||
				attribute.ConstructorArguments[0].Value is not string value)
			{
				return false;
			}

			category = value;
			return true;
		}

		private static bool TryGetTestCaseCategory(AttributeData attribute, out string category)
		{
			category = string.Empty;
			if (!IsParameterizedTestAttribute(attribute))
			{
				return false;
			}

			foreach (var namedArgument in attribute.NamedArguments)
			{
				if (namedArgument.Key == "Category" &&
					namedArgument.Value.Value is string value &&
					!string.IsNullOrWhiteSpace(value))
				{
					category = value;
					return true;
				}
			}

			return false;
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
