namespace Microsoft.Maui.Handlers;

using System;
using System.Text.RegularExpressions;

internal static partial class WebViewHelper
{
	internal static string? EscapeJsString(string js)
	{
		if (js == null)
			return null;

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		if (!js.Contains('\'', StringComparison.Ordinal))
#else
		if (js.IndexOf('\'') == -1)
#endif
			return js;

		return EscapeJsStringRegex().Replace(js, m =>
		{
			int count = m.Groups[1].Value.Length;
			// Replace with doubled backslashes plus one extra backslash, then the quote.
			return new string('\\', (count * 2) + 1) + "'";
		});
	}

#if NET6_0_OR_GREATER
	[GeneratedRegex(@"(\\*)'")]
	private static partial Regex EscapeJsStringRegex();
#else
	static Regex? EscapeJsStringRegexCached;
	private static Regex EscapeJsStringRegex() =>
		EscapeJsStringRegexCached ??= new Regex(@"(\\*)'", RegexOptions.Compiled | RegexOptions.CultureInvariant);
#endif
}
