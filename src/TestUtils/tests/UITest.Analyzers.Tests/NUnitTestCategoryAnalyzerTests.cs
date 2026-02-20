using UITest.Analyzers.NUnit;
using Xunit;

namespace UITest.Analyzers.Tests;

public class NUnitTestCategoryAnalyzerTests
{
	#region MAUI0001 - Missing Category Tests

	[Fact]
	public async Task TestMethod_WithNoCategory_ReportsMissingCategoryDiagnostic()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				public class TestClass
				{
					[NUnit.Framework.Test]
					public void TestMethod() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(
			source, NUnitTestMissingCategoryAnalyzer.MissingCategoryDiagnosticId);

		Assert.Single(diagnostics);
		Assert.Equal("MAUI0001", diagnostics[0].Id);
		Assert.Contains("TestMethod", diagnostics[0].GetMessage(), StringComparison.Ordinal);
	}

	[Fact]
	public async Task TestMethod_WithCategoryOnMethod_ReportsNoDiagnostic()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				public class TestClass
				{
					[NUnit.Framework.Test]
					[NUnit.Framework.Category("Button")]
					public void TestMethod() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(source);

		Assert.Empty(diagnostics);
	}

	[Fact]
	public async Task TestMethod_WithCategoryOnClass_ReportsNoDiagnostic()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				[NUnit.Framework.Category("Button")]
				public class TestClass
				{
					[NUnit.Framework.Test]
					public void TestMethod() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(source);

		Assert.Empty(diagnostics);
	}

	[Fact]
	public async Task NonTestMethod_WithNoCategory_ReportsNoDiagnostic()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				public class TestClass
				{
					public void RegularMethod() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(source);

		Assert.Empty(diagnostics);
	}

	[Fact]
	public async Task MultipleTestMethods_WithNoCategory_ReportsMultipleDiagnostics()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				public class TestClass
				{
					[NUnit.Framework.Test]
					public void TestMethod1() { }

					[NUnit.Framework.Test]
					public void TestMethod2() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(
			source, NUnitTestMissingCategoryAnalyzer.MissingCategoryDiagnosticId);

		Assert.Equal(2, diagnostics.Length);
	}

	#endregion

	#region MAUI0002 - Multiple Categories Tests

	[Fact]
	public async Task TestMethod_WithTwoCategoriesOnMethod_ReportsMultipleCategoriesDiagnostic()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				public class TestClass
				{
					[NUnit.Framework.Test]
					[NUnit.Framework.Category("Button")]
					[NUnit.Framework.Category("Compatibility")]
					public void TestMethod() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(
			source, NUnitTestMissingCategoryAnalyzer.MultipleCategoriesDiagnosticId);

		Assert.Single(diagnostics);
		Assert.Equal("MAUI0002", diagnostics[0].Id);
		Assert.Contains("TestMethod", diagnostics[0].GetMessage(), StringComparison.Ordinal);
		Assert.Contains("2", diagnostics[0].GetMessage(), StringComparison.Ordinal);
	}

	[Fact]
	public async Task TestMethod_WithThreeCategoriesOnMethod_ReportsMultipleCategoriesDiagnostic()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				public class TestClass
				{
					[NUnit.Framework.Test]
					[NUnit.Framework.Category("Maps")]
					[NUnit.Framework.Category("Performance")]
					[NUnit.Framework.Category("Compatibility")]
					public void TestMethod() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(
			source, NUnitTestMissingCategoryAnalyzer.MultipleCategoriesDiagnosticId);

		Assert.Single(diagnostics);
		Assert.Equal("MAUI0002", diagnostics[0].Id);
		Assert.Contains("3", diagnostics[0].GetMessage(), StringComparison.Ordinal);
	}

	[Fact]
	public async Task TestMethod_WithCategoryOnMethodAndClass_ReportsMultipleCategoriesDiagnostic()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				[NUnit.Framework.Category("ClassCategory")]
				public class TestClass
				{
					[NUnit.Framework.Test]
					[NUnit.Framework.Category("MethodCategory")]
					public void TestMethod() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(
			source, NUnitTestMissingCategoryAnalyzer.MultipleCategoriesDiagnosticId);

		Assert.Single(diagnostics);
		Assert.Equal("MAUI0002", diagnostics[0].Id);
		Assert.Contains("2", diagnostics[0].GetMessage(), StringComparison.Ordinal);
	}

	[Fact]
	public async Task TestMethod_WithTwoCategoriesOnClass_ReportsMultipleCategoriesDiagnostic()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				[NUnit.Framework.Category("Category1")]
				[NUnit.Framework.Category("Category2")]
				public class TestClass
				{
					[NUnit.Framework.Test]
					public void TestMethod() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(
			source, NUnitTestMissingCategoryAnalyzer.MultipleCategoriesDiagnosticId);

		Assert.Single(diagnostics);
		Assert.Equal("MAUI0002", diagnostics[0].Id);
	}

	[Fact]
	public async Task MultipleTestMethods_WithClassCategory_OneMethodHasAdditional_ReportsOneDiagnostic()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				[NUnit.Framework.Category("ClassCategory")]
				public class TestClass
				{
					[NUnit.Framework.Test]
					public void TestMethod1() { }

					[NUnit.Framework.Test]
					[NUnit.Framework.Category("MethodCategory")]
					public void TestMethod2() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(
			source, NUnitTestMissingCategoryAnalyzer.MultipleCategoriesDiagnosticId);

		// Only TestMethod2 should have the diagnostic (class + method = 2 categories)
		Assert.Single(diagnostics);
		Assert.Contains("TestMethod2", diagnostics[0].GetMessage(), StringComparison.Ordinal);
	}

	#endregion

	#region Custom Attributes Derived from CategoryAttribute Tests

	[Fact]
	public async Task TestMethod_WithCustomCategoryAttribute_ReportsNoDiagnostic()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				public class CustomCategoryAttribute : NUnit.Framework.CategoryAttribute
				{
					public CustomCategoryAttribute() : base("Custom") { }
				}

				public class TestClass
				{
					[NUnit.Framework.Test]
					[CustomCategory]
					public void TestMethod() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(source);

		Assert.Empty(diagnostics);
	}

	[Fact]
	public async Task TestMethod_WithCustomCategoryAndRegularCategory_ReportsMultipleCategoriesDiagnostic()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				public class CustomCategoryAttribute : NUnit.Framework.CategoryAttribute
				{
					public CustomCategoryAttribute() : base("Custom") { }
				}

				public class TestClass
				{
					[NUnit.Framework.Test]
					[CustomCategory]
					[NUnit.Framework.Category("Regular")]
					public void TestMethod() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(
			source, NUnitTestMissingCategoryAnalyzer.MultipleCategoriesDiagnosticId);

		Assert.Single(diagnostics);
		Assert.Equal("MAUI0002", diagnostics[0].Id);
	}

	#endregion

	#region Platform-Specific Ignore Attributes Tests

	[Theory]
	[InlineData("FailsOnAndroidWhenRunningOnXamarinUITest")]
	[InlineData("FailsOnIOSWhenRunningOnXamarinUITest")]
	[InlineData("FailsOnMacWhenRunningOnXamarinUITest")]
	[InlineData("FailsOnWindowsWhenRunningOnXamarinUITest")]
	[InlineData("FailsOnAllPlatformsWhenRunningOnXamarinUITest")]
	public async Task TestMethod_WithPlatformIgnoreAttributeDerivedFromCategory_DoesNotCountAsCategory(string attributeName)
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + $$"""

			namespace TestNamespace
			{
				// Simulates platform ignore attributes that derive from CategoryAttribute
				public class {{attributeName}}Attribute : NUnit.Framework.CategoryAttribute
				{
					public {{attributeName}}Attribute() : base("{{attributeName}}") { }
					public {{attributeName}}Attribute(string reason) : base(reason) { }
				}

				public class TestClass
				{
					[NUnit.Framework.Test]
					[NUnit.Framework.Category("Button")]
					[{{attributeName}}("Some reason")]
					public void TestMethod() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(source);

		// Should not report multiple categories because the platform ignore attribute should be excluded
		Assert.Empty(diagnostics);
	}

	[Fact]
	public async Task TestMethod_WithOnlyPlatformIgnoreAttribute_ReportsMissingCategory()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				// Simulates platform ignore attribute that derives from CategoryAttribute
				public class FailsOnAndroidWhenRunningOnXamarinUITestAttribute : NUnit.Framework.CategoryAttribute
				{
					public FailsOnAndroidWhenRunningOnXamarinUITestAttribute() : base("FailsOnAndroid") { }
				}

				public class TestClass
				{
					[NUnit.Framework.Test]
					[FailsOnAndroidWhenRunningOnXamarinUITest]
					public void TestMethod() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(
			source, NUnitTestMissingCategoryAnalyzer.MissingCategoryDiagnosticId);

		// Should report missing category because platform ignore attributes don't count
		Assert.Single(diagnostics);
		Assert.Equal("MAUI0001", diagnostics[0].Id);
	}

	#endregion

	#region Edge Cases

	[Fact]
	public async Task TestMethod_SameCategoryOnMethodAndClass_ReportsMultipleCategoriesDiagnostic()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				[NUnit.Framework.Category("Button")]
				public class TestClass
				{
					[NUnit.Framework.Test]
					[NUnit.Framework.Category("Button")]
					public void TestMethod() { }
				}
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(
			source, NUnitTestMissingCategoryAnalyzer.MultipleCategoriesDiagnosticId);

		// Even though it's the same category, there are 2 category attributes
		Assert.Single(diagnostics);
		Assert.Equal("MAUI0002", diagnostics[0].Id);
	}

	[Fact]
	public async Task EmptyClass_ReportsNoDiagnostic()
	{
		var source = AnalyzerTestHelpers.NUnitAttributeStubs + """

			namespace TestNamespace
			{
				public class TestClass { }
			}
			""";

		var diagnostics = await AnalyzerTestHelpers.GetDiagnosticsAsync<NUnitTestMissingCategoryAnalyzer>(source);

		Assert.Empty(diagnostics);
	}

	#endregion
}
