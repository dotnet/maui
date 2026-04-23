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
		Assert.Empty(result.GeneratedTrees);
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
		Assert.Empty(result.GeneratedTrees);
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
		Assert.Empty(result.GeneratedTrees);
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
		Assert.Empty(result.GeneratedTrees);
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
		Assert.Single(result.GeneratedTrees);
		var generatedSource = result.GeneratedTrees[0].ToString();
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
		Assert.Empty(result.GeneratedTrees);
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
		Assert.Empty(result.GeneratedTrees);
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
		Assert.Single(result.GeneratedTrees);

		var generatedSource = result.GeneratedTrees[0].ToString();
		// The query string key "my-name" should be used as-is in TryGetValue,
		// but the variable name should be sanitized
		Assert.Contains(@"query.TryGetValue(""my-name""", generatedSource, StringComparison.Ordinal);
		Assert.DoesNotContain("my-nameValue", generatedSource, StringComparison.Ordinal);
		Assert.Contains("my_nameValue", generatedSource, StringComparison.Ordinal);
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
		Assert.Empty(result.GeneratedTrees);
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
}
