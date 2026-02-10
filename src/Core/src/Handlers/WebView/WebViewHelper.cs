namespace Microsoft.Maui.Handlers;

using System;
using System.Text.RegularExpressions;

internal static partial class WebViewHelper
{
	const string NewlineMarker = "##NL##";

	internal static string? EscapeJsString(string js)
	{
		if (string.IsNullOrEmpty(js))
			return js;

		// Normalize line endings
		js = Regex.Replace(js, @"\r\n|\r", "\n");

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		bool hasBacktick = js.Contains('`', StringComparison.Ordinal);
#else
		bool hasBacktick = js.IndexOf('`') != -1;
#endif

		// Escape sequence marker
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		js = js.Replace("\\n", NewlineMarker, StringComparison.Ordinal);
#else
		js = js.Replace("\\n", NewlineMarker);
#endif
		// Escape backticks if present
		if (hasBacktick)
		{
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
			js = js.Replace("`", "\\`", StringComparison.Ordinal);
#else
			js = js.Replace("`", "\\`");
#endif
		}

		// Remove backslash-newline continuation
		js = Regex.Replace(js, @"\\[ \t]*\n", string.Empty);

		// Replace literal newlines with \n
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		js = js.Replace("\n", "\\n", StringComparison.Ordinal);
#else
		js = js.Replace("\n", "\\n");
#endif

		// Restore original escape sequences
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		js = js.Replace(NewlineMarker, "\\\\n", StringComparison.Ordinal);
#else
		js = js.Replace(NewlineMarker, "\\\\n");
#endif

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
