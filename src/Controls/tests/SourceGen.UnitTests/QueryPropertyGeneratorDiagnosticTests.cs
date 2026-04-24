using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class QueryPropertyGeneratorDiagnosticTests : QueryPropertyGeneratorTestBase
{
	[Fact]
	public void NonPartialClass_ReportsDiagnostic()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	public class MyPage : ContentPage
	{
		public string Name { get; set; }
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		Assert.NotEmpty(result.Diagnostics);
		Assert.True(result.Diagnostics.Any(d => d.Id == "MAUI1200"));
		Assert.Empty(GetQueryPropertyTrees(result));
	}

	[Fact]
	public void PropertyNotFound_ReportsDiagnostic()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(""NonExistentProperty"", ""name"")]
	public partial class MyPage : ContentPage
	{
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		Assert.NotEmpty(result.Diagnostics);
		Assert.True(result.Diagnostics.Any(d => d.Id == "MAUI1201"));
		Assert.Empty(GetQueryPropertyTrees(result));
	}

	[Fact]
	public void PropertyWithPrivateSetter_ReportsDiagnostic()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	public partial class MyPage : ContentPage
	{
		public string Name { get; private set; }
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		Assert.NotEmpty(result.Diagnostics);
		Assert.True(result.Diagnostics.Any(d => d.Id == "MAUI1202"));
		Assert.Empty(GetQueryPropertyTrees(result));
	}

	[Fact]
	public void PropertyWithNoSetter_ReportsDiagnostic()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	public partial class MyPage : ContentPage
	{
		public string Name { get; }
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		Assert.NotEmpty(result.Diagnostics);
		Assert.True(result.Diagnostics.Any(d => d.Id == "MAUI1202"));
		Assert.Empty(GetQueryPropertyTrees(result));
	}

	[Fact]
	public void MixedValidAndInvalidProperties_GeneratesForValidOnly()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	[QueryProperty(""NonExistent"", ""missing"")]
	public partial class MyPage : ContentPage
	{
		public string Name { get; set; }
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		// Should have diagnostic for non-existent property
		Assert.True(result.Diagnostics.Any(d => d.Id == "MAUI1201"));

		// Should still generate code for the valid property
		Assert.Single(GetQueryPropertyTrees(result));
		var generatedSource = GetQueryPropertyTrees(result)[0].ToString();
		Assert.Contains(@"query.TryGetValue(""name""", generatedSource, StringComparison.Ordinal);
		Assert.DoesNotContain(@"query.TryGetValue(""missing""", generatedSource, StringComparison.Ordinal);
	}

	[Fact]
	public void NoQueryPropertyAttribute_GeneratesNothing()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	public partial class MyPage : ContentPage
	{
		public string Name { get; set; }
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		Assert.Empty(result.Diagnostics);
		Assert.Empty(GetQueryPropertyTrees(result));
	}

	[Fact]
	public void ClassAlreadyImplementsIQueryAttributable_SkipsGeneration()
	{
		var sourceCode = @"
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	public partial class MyPage : ContentPage, IQueryAttributable
	{
		public string Name { get; set; }

		public void ApplyQueryAttributes(IDictionary<string, object> query)
		{
			// Custom implementation
		}
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		Assert.Empty(result.Diagnostics);
		Assert.Empty(GetQueryPropertyTrees(result));
	}

	[Fact]
	public void QueryIdWithSpecialCharacters_GeneratesValidCode()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""my-name"")]
	public partial class MyPage : ContentPage
	{
		public string Name { get; set; }
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		Assert.Empty(result.Diagnostics);
		Assert.Single(GetQueryPropertyTrees(result));

		var generatedSource = GetQueryPropertyTrees(result)[0].ToString();
		// The query string key "my-name" should be used as-is in TryGetValue
		Assert.Contains(@"query.TryGetValue(""my-name""", generatedSource, StringComparison.Ordinal);
		// Variable name uses the property name (Name), not the sanitized query ID
		Assert.Contains("Name__value", generatedSource, StringComparison.Ordinal);
	}

	[Fact]
	public void NonPartialClass_IsWarning_NotError()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	public class MyPage : ContentPage
	{
		public string Name { get; set; }
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		var diagnostic = result.Diagnostics.Single(d => d.Id == "MAUI1200");
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
	}

	[Fact]
	public void NonPartialClass_ReflectionDisabled_ReportsError()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	public class MyPage : ContentPage
	{
		public string Name { get; set; }
	}
}";

		var result = RunQueryPropertyGeneratorWithGlobalOptions(sourceCode,
			("build_property.MauiQueryPropertyAttributeSupport", "false"));

		Assert.NotEmpty(result.Diagnostics);
		var diagnostic = result.Diagnostics.Single(d => d.Id == "MAUI1204");
		Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
		Assert.Empty(GetQueryPropertyTrees(result));
	}

	[Fact]
	public void NonPartialClass_ReflectionEnabled_ReportsWarning()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	public class MyPage : ContentPage
	{
		public string Name { get; set; }
	}
}";

		var result = RunQueryPropertyGeneratorWithGlobalOptions(sourceCode,
			("build_property.MauiQueryPropertyAttributeSupport", "true"));

		Assert.NotEmpty(result.Diagnostics);
		Assert.True(result.Diagnostics.Any(d => d.Id == "MAUI1200"));
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUI1204");
	}

	[Fact]
	public void NestedClass_GeneratesNestedPartialChain()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	public partial class OuterPage : ContentPage
	{
		[QueryProperty(nameof(Name), ""name"")]
		public partial class InnerPage : ContentPage
		{
			public string Name { get; set; }
		}
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		Assert.Empty(result.Diagnostics);
		Assert.Single(GetQueryPropertyTrees(result));

		var generatedSource = GetQueryPropertyTrees(result)[0].ToString();
		// Should emit nested partial chain: partial class OuterPage { partial class InnerPage : IQueryAttributable { ... } }
		Assert.Contains("partial class OuterPage", generatedSource, StringComparison.Ordinal);
		Assert.Contains("partial class InnerPage", generatedSource, StringComparison.Ordinal);
		Assert.Contains(@"query.TryGetValue(""name""", generatedSource, StringComparison.Ordinal);
	}

	[Fact]
	public void NestedClass_NonPartialOuter_ReportsError()
	{
		// Reproduces the Issue17521 CI failure: nested [QueryProperty] class
		// inside a non-partial outer class would generate broken code (CS0260).
		// The generator must detect this and report MAUI1205 instead.
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	public class OuterPage : Shell
	{
		[QueryProperty(nameof(Name), ""name"")]
		public partial class InnerPage : ContentPage
		{
			public string Name { get; set; }
		}
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		Assert.NotEmpty(result.Diagnostics);
		Assert.True(result.Diagnostics.Any(d => d.Id == "MAUI1205"));
		Assert.Empty(GetQueryPropertyTrees(result));
	}

	[Fact]
	public void NestedClass_DeeplyNested_NonPartialMiddle_ReportsError()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	public partial class Outer : ContentPage
	{
		public class Middle
		{
			[QueryProperty(nameof(Name), ""name"")]
			public partial class Inner : ContentPage
			{
				public string Name { get; set; }
			}
		}
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		Assert.NotEmpty(result.Diagnostics);
		Assert.True(result.Diagnostics.Any(d => d.Id == "MAUI1205"));
		Assert.Empty(GetQueryPropertyTrees(result));
	}

	[Fact]
	public void InheritedProperty_IsFound()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	public class BasePage : ContentPage
	{
		public string Name { get; set; }
	}

	[QueryProperty(nameof(Name), ""name"")]
	public partial class DerivedPage : BasePage
	{
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		// Should find the inherited property — no MAUI1201 warning
		Assert.Empty(result.Diagnostics);
		Assert.Single(GetQueryPropertyTrees(result));

		var generatedSource = GetQueryPropertyTrees(result)[0].ToString();
		Assert.Contains(@"query.TryGetValue(""name""", generatedSource, StringComparison.Ordinal);
	}

	[Fact]
	public void MultipleClasses_GenerateSeparateFiles()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	public partial class PageA : ContentPage
	{
		public string Name { get; set; }
	}

	[QueryProperty(nameof(Id), ""id"")]
	public partial class PageB : ContentPage
	{
		public int Id { get; set; }
	}
}";

		var result = RunQueryPropertyGenerator(sourceCode);

		Assert.Empty(result.Diagnostics);

		// Should produce 2 QueryProperty files (one per class)
		var queryTrees = GetQueryPropertyTrees(result);
		Assert.Equal(2, queryTrees.Length);

		// Each file should be for a different class
		var allSource = string.Join("\n", queryTrees.Select(t => t.ToString()));
		Assert.Contains("partial class PageA", allSource, StringComparison.Ordinal);
		Assert.Contains("partial class PageB", allSource, StringComparison.Ordinal);

		// Verify hint names are unique (namespace-qualified)
		var hintNames = result.GeneratedTrees.Select(t => t.FilePath).ToArray();
		Assert.Equal(hintNames.Distinct(StringComparer.Ordinal).Count(), hintNames.Length);
	}
}
