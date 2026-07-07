using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public class WebViewHelperTests
{
	[Fact]
	public void EscapeJsString_NullInput_ReturnsNull()
	{
		var result = WebViewHelper.EscapeJsString(null);
		Assert.Null(result);
	}

	[Fact]
	public void EscapeJsString_NoSpecialChars_ReturnsSameString()
	{
		// No backslashes or single quotes - returns unchanged
		const string input = """console.log("Hello, world!");""";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(input, result);
	}

	[Fact]
	public void EscapeJsString_SingleQuote_EscapesCorrectly()
	{
		// Single quotes should be escaped
		const string input = """console.log('Hello, world!');""";
		const string expected = """console.log(\'Hello, world!\');""";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_Backslash_EscapesCorrectly()
	{
		// Backslashes should be escaped to prevent double-evaluation in eval()
		const string input = @"console.log(""Hello\\World"");";
		const string expected = @"console.log(""Hello\\\\World"");";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_BackslashAndQuote_EscapesBothCorrectly()
	{
		// Both backslashes and single quotes must be escaped
		// Backslashes first, then quotes
		const string input = @"var str = 'Don\'t do that';";
		// \ -> \\, then ' -> \'
		// Input has: ' D o n \ ' t   (quote, backslash, quote)
		// After escaping \: ' D o n \\ ' t
		// After escaping ': \' D o n \\ \' t
		const string expected = @"var str = \'Don\\\'t do that\';";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_MultipleBackslashes()
	{
		// Multiple backslashes should each be doubled
		const string input = @"path\\to\\file";
		const string expected = @"path\\\\to\\\\file";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_XssAttackPrevention()
	{
		const string input = """console.log("\\");alert('xss');(\\"");""";
		const string expected = """console.log("\\\\");alert(\'xss\');(\\\\"");""";
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
	public void EscapeJsString_OnlyBackslash()
	{
		const string input = @"\";
		const string expected = @"\\";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_TrailingBackslashBeforeQuote()
	{
		// Edge case: backslash immediately before quote
		const string input = @"test\'";
		// \ -> \\, then ' -> \'
		const string expected = @"test\\\'";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_EmptyString_ReturnsSameString()
	{
		const string input = "";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(input, result);
	}

	[Fact]
	public void EscapeJsString_Newlines_EscapesCorrectly()
	{
		const string input = "line1\nline2\rline3";
		const string expected = "line1\\nline2\\rline3";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_UnicodeLineSeparators_EscapesCorrectly()
	{
		const string input = "line1\u2028line2\u2029line3";
		const string expected = "line1\\u2028line2\\u2029line3";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void EscapeJsString_MultilineWithQuotesAndBackslashes()
	{
		const string input = "var x = 'test\\path';\nalert('done');";
		const string expected = "var x = \\'test\\\\path\\';\\nalert(\\'done\\');";
		var result = WebViewHelper.EscapeJsString(input);
		Assert.Equal(expected, result);
	}
}
