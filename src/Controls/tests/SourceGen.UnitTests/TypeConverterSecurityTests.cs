using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.SourceGen.TypeConverters;
using Microsoft.Maui.Controls.Xaml;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

[TestFixture]
public class TypeConverterSecurityTests : SourceGenXamlInitializeComponentTestBase
{
	[TestCase("\"injection\"; System.Console.WriteLine(\"Injected!\"); //")]
	[TestCase("Red\"; System.Console.WriteLine(\"Injected!\"); //")]
	[TestCase("\x00\x01\x02")] // Control characters
	[TestCase("Red\\\"]; System.Console.WriteLine(\\\"Injected!\\\"); //")]
	public void ColorConverter_ShouldPreventInjectionAttacks(string maliciousInput)
	{
		// Create a test XAML with malicious color value
		var xaml = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             x:Class=""Test.TestPage"">
    <Label BackgroundColor=""{maliciousInput}"" />
</ContentPage>";

		var code = @"using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}";

		var (result, generated) = RunGenerator(xaml, code);
		
		// For dangerous inputs, code generation should either fail completely (null) 
		// or produce safe escaped output without the malicious content
		if (generated != null)
		{
			// If code was generated, it should not contain the malicious input unescaped
			Assert.That(generated, Does.Not.Contain("System.Console.WriteLine"));
			Assert.That(generated, Does.Not.Contain("Injected!"));
			
			// Should not contain control characters that could break code
			Assert.That(generated, Does.Not.Contain("\x00"));
			Assert.That(generated, Does.Not.Contain("\x01"));
			Assert.That(generated, Does.Not.Contain("\x02"));
		}
		else
		{
			// Code generation failed due to security validation - this is acceptable for malicious inputs
			// Verify that there were diagnostic errors reported
			var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
			Assert.That(errors.Any(), Is.True, "Expected diagnostic errors when code generation fails for malicious input");
		}
	}

	[TestCase("test\n\r\t")] // Newlines and tabs - these should be handled safely  
	public void ColorConverter_ShouldEscapeButAllowWhitespace(string inputWithWhitespace)
	{
		// Create a test XAML with whitespace characters that should be handled safely
		var xaml = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             x:Class=""Test.TestPage"">
    <Label BackgroundColor=""{inputWithWhitespace}"" />
</ContentPage>";

		var code = @"using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}";

		var (result, generated) = RunGenerator(xaml, code);
		
		// Should generate code that safely handles whitespace characters
		Assert.IsNotNull(generated, "Generated code should not be null for whitespace input");
		
		// The key security requirement: should not contain raw newlines or carriage returns
		// that could break code structure or enable injection
		Assert.That(generated, Does.Not.Contain("\n\r"));
		Assert.That(generated, Does.Not.Contain("\r\n"));
		
		// Should not contain injection attempts
		Assert.That(generated, Does.Not.Contain("System.Console.WriteLine"));
		Assert.That(generated, Does.Not.Contain("Injected"));
	}	[TestCase("file.png\"; System.Console.WriteLine(\"Injected!\"); //")]
	[TestCase("https://test.com\"; System.Console.WriteLine(\"Injected!\"); //")]
	[TestCase("\x00\x01\x02")] // Control characters
	public void ImageSourceConverter_ShouldPreventInjectionAttacks(string maliciousInput)
	{
		// Create a test XAML with malicious image source value
		var xaml = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             x:Class=""Test.TestPage"">
    <Image Source=""{maliciousInput}"" />
</ContentPage>";

		var code = @"using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}";

		var (result, generated) = RunGenerator(xaml, code);
		
		// For dangerous inputs, code generation should either fail completely (null) 
		// or produce safe escaped output without the malicious content
		if (generated != null)
		{
			// If code was generated, it should not contain the malicious input unescaped
			Assert.That(generated, Does.Not.Contain("System.Console.WriteLine"));
			Assert.That(generated, Does.Not.Contain("Injected!"));
			
			// Should not contain control characters that could break code
			Assert.That(generated, Does.Not.Contain("\x00"));
			Assert.That(generated, Does.Not.Contain("\x01"));
			Assert.That(generated, Does.Not.Contain("\x02"));
		}
		else
		{
			// Code generation failed due to security validation - this is acceptable for malicious inputs
			var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
			Assert.That(errors.Any(), Is.True, "Expected diagnostic errors when code generation fails for malicious input");
		}
	}

	[TestCase("test\n\r\t")] // Newlines and tabs - these should be handled safely
	public void ImageSourceConverter_ShouldEscapeButAllowWhitespace(string inputWithWhitespace)
	{
		// Create a test XAML with whitespace characters that should be handled safely
		var xaml = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             x:Class=""Test.TestPage"">
    <Image Source=""{inputWithWhitespace}"" />
</ContentPage>";

		var code = @"using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}";

		var (result, generated) = RunGenerator(xaml, code);
		
		// Should generate code that safely handles whitespace characters
		Assert.IsNotNull(generated, "Generated code should not be null for whitespace input");
		
		// The key security requirement: should not contain raw newlines or carriage returns
		// that could break code structure or enable injection
		Assert.That(generated, Does.Not.Contain("\n\r"));
		Assert.That(generated, Does.Not.Contain("\r\n"));
		
		// Should not contain injection attempts
		Assert.That(generated, Does.Not.Contain("System.Console.WriteLine"));
		Assert.That(generated, Does.Not.Contain("Injected"));
	}

	[Test]
	public void StringHelpers_EscapeStringLiteral_ShouldEscapeSpecialCharacters()
	{
		// Test that StringHelpers properly escapes various dangerous characters
		var input = "test\"quote\nNewline\tTab\x00Null";
		var escaped = StringHelpers.EscapeStringLiteral(input);
		
		// Should produce a valid C# string literal
		Assert.That(escaped, Does.StartWith("\""));
		Assert.That(escaped, Does.EndWith("\""));
		
		// Should escape quotes and control characters
		Assert.That(escaped, Does.Contain("\\\""));
		Assert.That(escaped, Does.Contain("\\n"));
		Assert.That(escaped, Does.Contain("\\t"));
		Assert.That(escaped, Does.Contain("\\0"));
	}

	[TestCase("normal text", true)]
	[TestCase("", true)]
	[TestCase("\x00", false)] // Null character
	[TestCase("\x01", false)] // Control character
	[TestCase("text\x00more", false)] // Contains null
	[TestCase("text\nmore", false)] // Contains newline without proper context
	[TestCase("text\rmore", false)] // Contains carriage return
	[TestCase("text\tmore", true)] // Tab should be allowed in our revised implementation
	public void StringHelpers_IsSafeForCodeGeneration_ShouldValidateCorrectly(string input, bool expectedSafe)
	{
		var result = StringHelpers.IsSafeForCodeGeneration(input);
		Assert.That(result, Is.EqualTo(expectedSafe));
	}

	[Test]
	public void ComplexInjectionScenario_ShouldBePreventedAcrossAllConverters()
	{
		// Test a complex XAML that attempts injection in multiple converters
		var xaml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             x:Class=""Test.TestPage"">
    <StackLayout>
        <Label Text=""Test"" 
               BackgroundColor=""Red""; System.Console.WriteLine(""Injected from Color!""); //"" />
        <Image Source=""image.png""; System.Console.WriteLine(""Injected from Image!""); //"" />
    </StackLayout>
</ContentPage>";

		var code = @"using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}";

		var (result, generated) = RunGenerator(xaml, code);
		
		// For dangerous inputs, the security system should either prevent code generation
		// or produce safe output without malicious content
		if (generated != null)
		{
			// Verify NO injection was successful across any converter
			Assert.That(generated, Does.Not.Contain("System.Console.WriteLine"));
			Assert.That(generated, Does.Not.Contain("Injected from"));
			
			// Ensure the code still compiles and is syntactically valid
			var compilation = CreateCompilation(generated);
			var diagnostics = compilation.GetDiagnostics();
			var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
			
			// Should have no compilation errors (may have warnings)
			Assert.That(errors, Is.Empty, $"Generated code should not have compilation errors. Diagnostics: {string.Join(", ", errors.Select(d => d.ToString()))}");
		}
		else
		{
			// Code generation failed due to security validation - this is acceptable for malicious inputs
			var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
			Assert.That(errors.Any(), Is.True, "Expected diagnostic errors when code generation fails for malicious input");
		}
	}

	private CSharpCompilation CreateCompilation(string source)
	{
		var references = new[]
		{
			MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(System.Console).Assembly.Location)
		};

		return CSharpCompilation.Create("TestAssembly",
			new[] { CSharpSyntaxTree.ParseText(source) },
			references);
	}
}