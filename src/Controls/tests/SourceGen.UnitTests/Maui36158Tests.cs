using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

// https://github.com/dotnet/maui/issues/36158
// When a markup / XAML C# Expression fails to parse, MAUIG1003 (and sibling parse errors)
// must report the line/column of the offending expression, not the file root (1,1).
public class Maui36158Tests : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void UnclosedExpressionReportsExpressionLineNotFileRoot()
	{
		// The malformed expression (missing trailing '}') is on line 7 (1-indexed).
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="Hello" />
	<Label Text="{IsActive ? 'Status: Active' : 'Status: Idle'" />
</ContentPage>
""";

		var (result, _) = RunGenerator(xaml, string.Empty, assertNoCompilationErrors: false);

		var diagnostic = Assert.Single(result.Diagnostics.Where(d => d.Id == "MAUIG1003"));

		var location = diagnostic.Location.GetLineSpan();

		// Line 7 (1-indexed) == line 6 (0-indexed). Before the fix this was the file root (line 0).
		Assert.Equal(6, location.StartLinePosition.Line);
		// And the column must point into the expression, not column 0.
		Assert.NotEqual(0, location.StartLinePosition.Character);
	}

	[Fact]
	public void UnclosedMarkupExtensionReportsExpressionLineNotFileRoot()
	{
		// An unclosed markup extension (missing trailing '}') on line 8 (1-indexed).
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="Hello" />
	<Label Text="World" />
	<Label Text="{StaticResource MissingBrace" />
</ContentPage>
""";

		var (result, _) = RunGenerator(xaml, string.Empty, assertNoCompilationErrors: false);

		var diagnostic = Assert.Single(result.Diagnostics.Where(d => d.Id == "MAUIG1003"));

		var location = diagnostic.Location.GetLineSpan();

		// Line 8 (1-indexed) == line 7 (0-indexed). Before the fix this was the file root (line 0).
		Assert.Equal(7, location.StartLinePosition.Line);
		Assert.NotEqual(0, location.StartLinePosition.Character);
	}
}
