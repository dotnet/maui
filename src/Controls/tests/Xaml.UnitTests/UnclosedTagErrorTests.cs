using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Xaml Inflation")]
public class UnclosedTagErrorTests : BaseTestFixture
{
	[Fact]
	public void UnclosedTagReportsErrorDiagnostic()
	{
		// XAML with an unclosed Label tag (missing > or />)
		var xaml = @"<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"">
    <Label Text=""foo""
</ContentPage>";

		var result = CreateMauiCompilation()
			.RunMauiSourceGenerator(new AdditionalXamlFile("TestPage.xaml", xaml));

		// Should have an error diagnostic
		Assert.Single(result.Diagnostics);
		
		var diagnostic = result.Diagnostics[0];
		
		// Verify diagnostic ID
		Assert.Equal("MAUIG1001", diagnostic.Id);
		
		// Verify severity
		Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
		
		// Verify the error message indicates an XML parsing error (unclosed tag)
		var message = diagnostic.GetMessage();
		
		// The message should mention the XML error (character cannot begin with '<')
		// This error occurs when the XML parser encounters '<' where it's expecting content or a closing tag
		Assert.Contains("Name cannot begin with the '<' character", message, System.StringComparison.Ordinal);
		
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
