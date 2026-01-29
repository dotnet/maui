namespace Microsoft.Maui.Handlers;

using System;

internal static partial class WebViewHelper
{
	/// <summary>
	/// Escapes backslashes, single quotes, and line terminators in a JavaScript string
	/// for use in a single-quoted string literal passed to eval().
	/// </summary>
	internal static string? EscapeJsString(string? js)
	{
		if (js == null)
			return null;

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		bool hasBackslash = js.Contains('\\', StringComparison.Ordinal);
		bool hasSingleQuote = js.Contains('\'', StringComparison.Ordinal);
		bool hasNewline = js.Contains('\n', StringComparison.Ordinal);
		bool hasCarriageReturn = js.Contains('\r', StringComparison.Ordinal);
		bool hasLineSeparator = js.Contains('\u2028', StringComparison.Ordinal);
		bool hasParagraphSeparator = js.Contains('\u2029', StringComparison.Ordinal);
#else
		bool hasBackslash = js.IndexOf('\\') != -1;
		bool hasSingleQuote = js.IndexOf('\'') != -1;
		bool hasNewline = js.IndexOf('\n') != -1;
		bool hasCarriageReturn = js.IndexOf('\r') != -1;
		bool hasLineSeparator = js.IndexOf('\u2028') != -1;
		bool hasParagraphSeparator = js.IndexOf('\u2029') != -1;
#endif

		if (!hasBackslash && !hasSingleQuote && !hasNewline && !hasCarriageReturn && !hasLineSeparator && !hasParagraphSeparator)
			return js;

		var result = js;
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		if (hasBackslash)
			result = result.Replace("\\", "\\\\", StringComparison.Ordinal);
		if (hasSingleQuote)
			result = result.Replace("'", "\\'", StringComparison.Ordinal);

		// Escape line terminators to prevent SyntaxError in eval('...')
		if (hasNewline)
			result = result.Replace("\n", "\\n", StringComparison.Ordinal);
		if (hasCarriageReturn)
			result = result.Replace("\r", "\\r", StringComparison.Ordinal);
		if (hasLineSeparator)
			result = result.Replace("\u2028", "\\u2028", StringComparison.Ordinal);
		if (hasParagraphSeparator)
			result = result.Replace("\u2029", "\\u2029", StringComparison.Ordinal);
#else
		if (hasBackslash)
			result = result.Replace("\\", "\\\\");
		if (hasSingleQuote)
			result = result.Replace("'", "\\'");

		// Escape line terminators to prevent SyntaxError in eval('...')
		if (hasNewline)
			result = result.Replace("\n", "\\n");
		if (hasCarriageReturn)
			result = result.Replace("\r", "\\r");
		if (hasLineSeparator)
			result = result.Replace("\u2028", "\\u2028");
		if (hasParagraphSeparator)
			result = result.Replace("\u2029", "\\u2029");
#endif

		return result;
	}
}
