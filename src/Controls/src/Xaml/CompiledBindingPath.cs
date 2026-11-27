using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Xaml;

internal static class CompiledBindingPath
{
	internal static string[] Split(string path, bool indexerEndsAtLastBracket = false)
	{
		var parts = new List<string>();
		int start = 0;
		for (int i = 0; i < path.Length; i++)
		{
			if (path[i] == '[')
			{
				// Opening brackets inside the key are literal, not nested delimiters.
				int close = path.IndexOf("]", i + 1, StringComparison.Ordinal);
				if (close < 0)
					break; // Leave the unclosed indexer for the caller to diagnose.

				if (indexerEndsAtLastBracket)
				{
					// XamlC allows literal "]." in a key, but must not consume the next indexer.
					int nextOpen = path.IndexOf("[", close + 1, StringComparison.Ordinal);
					int end = nextOpen < 0 ? path.Length - 1 : nextOpen - 1;
					close = path.LastIndexOf("]", end, end - close + 1, StringComparison.Ordinal);
				}

				i = path.IndexOf(".", close + 1, StringComparison.Ordinal);
				if (i < 0)
					break;
			}
			else if (path[i] != '.')
				continue;

			if (i > start)
				parts.Add(path.Substring(start, i - start));
			start = i + 1;
		}

		if (path.Length > start)
			parts.Add(path.Substring(start));

		return parts.ToArray();
	}
}
