using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public class WebViewHelperTests
{
	[Fact]
	public void EscapeJsString_NullInput_ReturnsNull()
	{
		const string input = null;
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Null(result);
	}

	[Fact]
	public void EscapeJsString_NoSingleQuote_ReturnsSameString()
	{
		const string input = """console.log("Hello, world!");""";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(input, result);
	}

	[Fact]
	public void EscapeJsString_UnescapedQuote_EscapesCorrectly()
	{
		// Each unescaped single quote should be preceded by one backslash.
		const string input = """console.log('Hello, world!');""";
		// Expected: each occurrence of "'" becomes "\'"
		const string expected = """console.log(\'Hello, world!\');""";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_AlreadyEscapedQuote_EscapesFurther()
	{
		const string input = """var str = 'Don\'t do that';""";
		const string expected = """var str = \'Don\\\'t do that\';""";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_MultipleLinesAndMixedQuotes()
	{
		const string input = """
			function test() {
				console.log('Test "string" with a single quote');
				var example = 'It\\'s tricky!';
			}
			""";
		const string expected = """
			function test() {
				console.log(\'Test "string" with a single quote\');
				var example = \'It\\\\\'s tricky!\';
			}
			""";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_MultipleBackslashesBeforeQuote()
	{
		const string input = @"var tricky = 'Backslash: \\\' tricky!';";
		const string expected = @"var tricky = \'Backslash: \\\\\\\' tricky!\';";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_QuoteAtBeginning()
	{
		const string input = @"'Start with quote";
		const string expected = @"\'Start with quote";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_QuoteAtEnd()
	{
		const string input = @"Ends with a quote'";
		const string expected = @"Ends with a quote\'";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_OnlyQuote()
	{
		const string input = @"'";
		const string expected = @"\'";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_RepeatedEscapedQuotes()
	{
		const string input = @"'Quote' and again \'Quote\'";
		const string expected = @"\'Quote\' and again \\\'Quote\\\'";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}
}
