using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Xaml;

internal static class CompiledBindingPath
{
	internal static string[] Split(string path)
	{
		var parts = new List<string>();
		int start = 0;
		for (int i = 0; i < path.Length; i++)
		{
			if (path[i] == '[')
			{
				// Opening brackets inside the key are literal, not nested delimiters.
				int close = path.IndexOf(']', i + 1);
				if (close < 0)
					break; // Leave the unclosed indexer for the caller to diagnose.

				// Keep the rest of this part: XamlC uses the last ']' before the separator.
				i = path.IndexOf('.', close + 1);
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
