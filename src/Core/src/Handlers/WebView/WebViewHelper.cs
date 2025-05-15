namespace Microsoft.Maui.Handlers;

using System;
using System.Text.RegularExpressions;

internal static partial class WebViewHelper
{
	internal static string? EscapeJsString(string js)
	{
		if (js == null)
			return null;

#if NET6_0_OR_GREATER
		if (!js.Contains('\'', StringComparison.Ordinal))
#else
		if (!js.Contains('\''))
#endif
			return js;

#if NET6_0_OR_GREATER
		return EscapeJsStringRegex().Replace(js, m =>
#else
		return Regex.Replace(js, @"(\\*)'", m =>
#endif
		{
			int count = m.Groups[1].Value.Length;
			// Replace with doubled backslashes plus one extra backslash, then the quote.
			return new string('\\', (count * 2) + 1) + "'";
		});
	}
	
#if NET6_0_OR_GREATER
	[GeneratedRegex(@"(\\*)'")]
	private static partial Regex EscapeJsStringRegex();
#endif
}
