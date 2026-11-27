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
					// An opening bracket can start a new indexer only after a path separator following a close.
					bool hasSeparator = false;
					bool afterClose = true;
					for (int j = close + 1; j < path.Length; j++)
					{
						char c = path[j];
						if (c == '[' && hasSeparator)
							break;
						if (c == ']')
						{
							close = j;
							hasSeparator = false;
							afterClose = true;
						}
						else if (afterClose && !char.IsWhiteSpace(c))
						{
							hasSeparator = c == '.';
							afterClose = false;
						}
					}
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
