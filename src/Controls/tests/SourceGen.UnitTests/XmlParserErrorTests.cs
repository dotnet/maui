using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class XmlParserErrorTests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn);

	[Fact]
	public void UnclosedTagReportsErrorWithCorrectLocationAndMessage()
	{
		// XAML with an unclosed Label tag (missing > or /> or </Label>)
		// This simulates a common XML syntax error
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="foo"
</ContentPage>
""";

		var compilation = CreateMauiCompilation();
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		// Should have an error diagnostic
		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.Single(errors);

		var diagnostic = errors[0];

		// Verify diagnostic ID for XAML parser error
		Assert.Equal("MAUIG1001", diagnostic.Id);

		// Get the location information
		var location = diagnostic.Location.GetLineSpan();
		var message = diagnostic.GetMessage();

		// Output for debugging
		System.Console.WriteLine($"Line (0-indexed): {location.StartLinePosition.Line}");
		System.Console.WriteLine($"Character (0-indexed): {location.StartLinePosition.Character}");
		System.Console.WriteLine($"Message: {message}");

		// The error message should mention the XML parsing error
		// The XmlException says: "Name cannot begin with the '<' character, hexadecimal value 0x3C. Line 7, position 1."
		Assert.Contains("Name cannot begin with the '<' character", message, System.StringComparison.Ordinal);

		// PR #13797 fix: The location should be extracted from XmlException
		// and the duplicate "Line X, position Y" should be stripped from the message
		Assert.Equal(6, location.StartLinePosition.Line); // Line 7 in 1-indexed = line 6 in 0-indexed
		Assert.Equal(1, location.StartLinePosition.Character); // Position 1 from XmlException (not converted to 0-indexed by LocationHelpers)
		Assert.DoesNotContain("Line 7, position 1", message, System.StringComparison.Ordinal);
	}

	[Fact]
	public void MissingClosingTagReportsErrorWithCorrectLocation()
	{
		// Another common XML error: tag opened but never closed
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<StackLayout>
		<Label Text="Hello"/>
""";

		var compilation = CreateMauiCompilation();
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		// Should have an error diagnostic
		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.Single(errors);

		var diagnostic = errors[0];
		Assert.Equal("MAUIG1001", diagnostic.Id);

		var message = diagnostic.GetMessage();
		var location = diagnostic.Location.GetLineSpan();

		System.Console.WriteLine($"Message: {message}");
		System.Console.WriteLine($"Location: Line {location.StartLinePosition.Line}, Char {location.StartLinePosition.Character}");

		// The error should mention unexpected end of file
		Assert.Contains("Unexpected end of file", message, System.StringComparison.Ordinal);

		// And should NOT have duplicate line position info
		// The message should not end with " Line X, position Y."
		Assert.DoesNotContain("Line ", message.Substring(message.Length - 20), System.StringComparison.Ordinal);
	}
}
