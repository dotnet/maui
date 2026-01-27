namespace Microsoft.Maui.Handlers;

using System;

internal static partial class WebViewHelper
{
	/// <summary>
	/// Escapes a JavaScript string for safe inclusion in a single-quoted string literal.
	/// </summary>
	internal static string? EscapeJsString(string? js)
	{
		if (js == null)
			return null;

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		bool hasBackslash = js.Contains('\\', StringComparison.Ordinal);
		bool hasSingleQuote = js.Contains('\'', StringComparison.Ordinal);
#else
		bool hasBackslash = js.IndexOf('\\') != -1;
		bool hasSingleQuote = js.IndexOf('\'') != -1;
#endif

		if (!hasBackslash && !hasSingleQuote)
			return js;

		var result = js;
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		if (hasBackslash)
			result = result.Replace("\\", "\\\\", StringComparison.Ordinal);
		if (hasSingleQuote)
			result = result.Replace("'", "\\'", StringComparison.Ordinal);
#else
		if (hasBackslash)
			result = result.Replace("\\", "\\\\");
		if (hasSingleQuote)
			result = result.Replace("'", "\\'");
#endif

		return result;
	}
}
