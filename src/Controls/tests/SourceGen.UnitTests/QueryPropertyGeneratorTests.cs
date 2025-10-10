using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

[TestFixture]
public class QueryPropertyGeneratorTests
{
	[Test]
	public void SingleStringProperty_GeneratesCorrectImplementation()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	public partial class MyPage : ContentPage
	{
		public string Name { get; set; }
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		Assert.That(result.GeneratedTrees.Length, Is.EqualTo(1));

		var generatedSource = result.GeneratedTrees[0].ToString();

		// Check it implements IQueryAttributable
		Assert.That(generatedSource, Does.Contain("partial class MyPage : global::Microsoft.Maui.Controls.IQueryAttributable"));
		Assert.That(generatedSource, Does.Contain("void global::Microsoft.Maui.Controls.IQueryAttributable.ApplyQueryAttributes(global::System.Collections.Generic.IDictionary<string, object> query)"));

		// Check URL decoding for string
		Assert.That(generatedSource, Does.Contain("global::System.Net.WebUtility.UrlDecode"));
		Assert.That(generatedSource, Does.Contain("Name = global::System.Net.WebUtility.UrlDecode"));
	}

	[Test]
	public void MultipleProperties_GeneratesCorrectImplementation()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	[QueryProperty(nameof(Location), ""location"")]
	public partial class MyPage : ContentPage
	{
		public string Name { get; set; }
		public string Location { get; set; }
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		Assert.That(result.GeneratedTrees.Length, Is.EqualTo(1));

		var generatedSource = result.GeneratedTrees[0].ToString();

		// Check both properties are handled
		Assert.That(generatedSource, Does.Contain("if (query.TryGetValue(\"name\", out var nameValue))"));
		Assert.That(generatedSource, Does.Contain("if (query.TryGetValue(\"location\", out var locationValue))"));
		Assert.That(generatedSource, Does.Contain("Name = global::System.Net.WebUtility.UrlDecode"));
		Assert.That(generatedSource, Does.Contain("Location = global::System.Net.WebUtility.UrlDecode"));
	}

	[Test]
	public void IntProperty_GeneratesTypeConversion()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Count), ""count"")]
	public partial class MyPage : ContentPage
	{
		public int Count { get; set; }
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		Assert.That(result.GeneratedTrees.Length, Is.EqualTo(1));

		var generatedSource = result.GeneratedTrees[0].ToString();

		// Check type conversion
		Assert.That(generatedSource, Does.Contain("Convert.ChangeType"));
		Assert.That(generatedSource, Does.Contain("Count = (int)convertedValue"));

		// Should not use URL decoding for non-string
		Assert.That(generatedSource, Does.Not.Contain("Count = global::System.Net.WebUtility.UrlDecode"));
	}

	[Test]
	public void DoubleProperty_GeneratesTypeConversion()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Price), ""price"")]
	public partial class MyPage : ContentPage
	{
		public double Price { get; set; }
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		var generatedSource = result.GeneratedTrees[0].ToString();

		Assert.That(generatedSource, Does.Contain("Convert.ChangeType"));
		Assert.That(generatedSource, Does.Contain("Price = (double)convertedValue"));
	}

	[Test]
	public void BoolProperty_GeneratesTypeConversion()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(IsActive), ""active"")]
	public partial class MyPage : ContentPage
	{
		public bool IsActive { get; set; }
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		var generatedSource = result.GeneratedTrees[0].ToString();

		Assert.That(generatedSource, Does.Contain("Convert.ChangeType"));
		Assert.That(generatedSource, Does.Contain("IsActive = (bool)convertedValue"));
	}

	[Test]
	public void MixedPropertyTypes_GeneratesCorrectImplementation()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	[QueryProperty(nameof(Age), ""age"")]
	[QueryProperty(nameof(Price), ""price"")]
	public partial class MyPage : ContentPage
	{
		public string Name { get; set; }
		public int Age { get; set; }
		public double Price { get; set; }
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		var generatedSource = result.GeneratedTrees[0].ToString();

		// String uses URL decode
		Assert.That(generatedSource, Does.Contain("Name = global::System.Net.WebUtility.UrlDecode"));

		// Non-strings use Convert.ChangeType
		Assert.That(generatedSource, Does.Contain("Age = (int)convertedValue"));
		Assert.That(generatedSource, Does.Contain("Price = (double)convertedValue"));
	}

	[Test]
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

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		Assert.That(result.GeneratedTrees.Length, Is.EqualTo(0));
	}

	[Test]
	public void DocumentationExample_BearDetailPage()
	{
		// Example from: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	public class Animal
	{
		public string Name { get; set; }
	}

	[QueryProperty(nameof(Bear), ""Bear"")]
	public partial class BearDetailPage : ContentPage
	{
		Animal bear;
		public Animal Bear
		{
			get => bear;
			set
			{
				bear = value;
			}
		}

		public BearDetailPage()
		{
			BindingContext = this;
		}
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		Assert.That(result.GeneratedTrees.Length, Is.EqualTo(1));

		var generatedSource = result.GeneratedTrees[0].ToString();

		Assert.That(generatedSource, Does.Contain("partial class BearDetailPage : global::Microsoft.Maui.Controls.IQueryAttributable"));
		Assert.That(generatedSource, Does.Contain("if (query.TryGetValue(\"Bear\", out var BearValue))"));
	}

	[Test]
	public void DocumentationExample_ElephantDetailPage()
	{
		// Example from: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	[QueryProperty(nameof(Location), ""location"")]
	public partial class ElephantDetailPage : ContentPage
	{
		public string Name
		{
			set
			{
				// Custom logic
			}
		}

		public string Location
		{
			set
			{
				// Custom logic
			}
		}
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		Assert.That(result.GeneratedTrees.Length, Is.EqualTo(1));

		var generatedSource = result.GeneratedTrees[0].ToString();

		Assert.That(generatedSource, Does.Contain("partial class ElephantDetailPage : global::Microsoft.Maui.Controls.IQueryAttributable"));
		Assert.That(generatedSource, Does.Contain("if (query.TryGetValue(\"name\", out var nameValue))"));
		Assert.That(generatedSource, Does.Contain("if (query.TryGetValue(\"location\", out var locationValue))"));
		Assert.That(generatedSource, Does.Contain("Name = global::System.Net.WebUtility.UrlDecode"));
		Assert.That(generatedSource, Does.Contain("Location = global::System.Net.WebUtility.UrlDecode"));
	}

	[Test]
	public void PropertyNotFound_GeneratesNothing()
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

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		// Should not generate source for non-existent properties
		Assert.That(result.GeneratedTrees.Length, Is.EqualTo(0));
	}

	[Test]
	public void NullableStringProperty_GeneratesCorrectImplementation()
	{
		var sourceCode = @"
#nullable enable
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	public partial class MyPage : ContentPage
	{
		public string? Name { get; set; }
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		var generatedSource = result.GeneratedTrees[0].ToString();

		Assert.That(generatedSource, Does.Contain("Name = global::System.Net.WebUtility.UrlDecode"));
	}

	[Test]
	public void NullableIntProperty_GeneratesCorrectImplementation()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Count), ""count"")]
	public partial class MyPage : ContentPage
	{
		public int? Count { get; set; }
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		var generatedSource = result.GeneratedTrees[0].ToString();

		Assert.That(generatedSource, Does.Contain("Count = (int?)convertedValue"));
	}

	[Test]
	public void ClassWithoutNamespace_GeneratesCorrectImplementation()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

[QueryProperty(nameof(Name), ""name"")]
public partial class MyPage : ContentPage
{
	public string Name { get; set; }
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		var generatedSource = result.GeneratedTrees[0].ToString();

		// Should not have namespace declaration
		Assert.That(generatedSource, Does.Not.Contain("namespace "));
		Assert.That(generatedSource, Does.Contain("partial class MyPage : global::Microsoft.Maui.Controls.IQueryAttributable"));
	}

	[Test]
	public void HandlesPropertyClearing()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	public partial class MyPage : ContentPage
	{
		public string Name { get; set; }
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		var generatedSource = result.GeneratedTrees[0].ToString();

		// Check that it tracks previous keys and clears properties
		Assert.That(generatedSource, Does.Contain("var previousKeys = _queryPropertyKeys ?? new global::System.Collections.Generic.HashSet<string>()"));
		Assert.That(generatedSource, Does.Contain("_queryPropertyKeys = new global::System.Collections.Generic.HashSet<string>()"));
		Assert.That(generatedSource, Does.Contain("else if (previousKeys.Contains(\"name\"))"));
		Assert.That(generatedSource, Does.Contain("private global::System.Collections.Generic.HashSet<string>? _queryPropertyKeys"));
	}

	[Test]
	public void ViewCompleteGeneratedSource()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	[QueryProperty(nameof(Age), ""age"")]
	public partial class MyPage : ContentPage
	{
		public string Name { get; set; }
		public int Age { get; set; }
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);
		Assert.That(result.GeneratedTrees.Length, Is.EqualTo(1));

		var generatedSource = result.GeneratedTrees[0].ToString();
		System.Console.WriteLine("===== GENERATED SOURCE =====");
		System.Console.WriteLine(generatedSource);
		System.Console.WriteLine("===========================");
	}

	[Test]
	public void GeneratedImplementation_ContainsExpectedPatterns()
	{
		// Create a compilation with a class using QueryProperty
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	[QueryProperty(nameof(Age), ""age"")]
	public partial class PersonDetailsPage : ContentPage
	{
		public string Name { get; set; }
		public int Age { get; set; }
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty, "Generator should not produce diagnostics");
		Assert.That(result.GeneratedTrees.Length, Is.EqualTo(1), "Should generate exactly one file");

		var generatedSource = result.GeneratedTrees[0].ToString();

		// Verify key aspects of the generated code
		Assert.That(generatedSource, Does.Contain("partial class PersonDetailsPage : global::Microsoft.Maui.Controls.IQueryAttributable"));
		Assert.That(generatedSource, Does.Contain("void global::Microsoft.Maui.Controls.IQueryAttributable.ApplyQueryAttributes(global::System.Collections.Generic.IDictionary<string, object> query)"));

		// Verify string handling with URL decoding
		Assert.That(generatedSource, Does.Contain("global::System.Net.WebUtility.UrlDecode"));

		// Verify int handling with type conversion
		Assert.That(generatedSource, Does.Contain("Convert.ChangeType"));

		// Verify property tracking for clearing
		Assert.That(generatedSource, Does.Contain("_queryPropertyKeys"));
	}

	[Test]
	public void GeneratedCode_CompilesSuccessfully()
	{
		var sourceCode = @"
using Microsoft.Maui.Controls;

namespace MyApp
{
	[QueryProperty(nameof(Name), ""name"")]
	public partial class TestPage : ContentPage
	{
		public string Name { get; set; }
	}
}";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation);

		Assert.That(result.Diagnostics, Is.Empty);

		// Add the generated source to the compilation and verify it compiles
		compilation = compilation.AddSyntaxTrees(result.GeneratedTrees[0]);

		var compilationDiagnostics = compilation.GetDiagnostics();
		var errors = compilationDiagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).ToArray();

		Assert.That(errors, Is.Empty, $"Generated code should compile without errors. Errors: {string.Join(Environment.NewLine, errors.Select(e => e.ToString()))}");
	}
}
