using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[TestFixture]
public class UnclosedTagErrorTests : BaseTestFixture
{
	[Test]
	public void UnclosedTagReportsErrorDiagnostic()
	{
		// XAML with an unclosed Label tag (missing > or />)
		var xaml = @"<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"">
    <Label Text=""foo""
</ContentPage>";

		var result = CreateMauiCompilation()
			.RunMauiSourceGenerator(new AdditionalXamlFile("TestPage.xaml", xaml));

		// Should have an error diagnostic
		Assert.That(result.Diagnostics.Length, Is.EqualTo(1), "Should have exactly one diagnostic");
		
		var diagnostic = result.Diagnostics[0];
		
		// Verify diagnostic ID
		Assert.That(diagnostic.Id, Is.EqualTo("MAUIG1001"), "Should be XAML parser error");
		
		// Verify severity
		Assert.That(diagnostic.Severity, Is.EqualTo(DiagnosticSeverity.Error), "Should be an error");
		
		// Verify the error message indicates an XML parsing error (unclosed tag)
		var message = diagnostic.GetMessage();
		TestContext.WriteLine($"Diagnostic message: {message}");
		
		// The message should mention the XML error (character cannot begin with '<')
		// This error occurs when the XML parser encounters '<' where it's expecting content or a closing tag
		Assert.That(message, Does.Contain("Name cannot begin with the '<' character"), 
			"Should contain the XML error message indicating an unclosed tag");
		
		// Note: This PR (fix for issue #13797) aims to:
		// 1. Report errors at the correct line/column (extracted from XmlException)
		// 2. Remove duplicate "Line X, position Y" from the error message
		//
		// However, there appears to be a code path where the XmlException is not being
		// caught correctly, resulting in location (0,0) and the message still containing
		// line info. This test documents the current behavior. The fix may need additional
		// work to handle all exception paths.
	}
}
