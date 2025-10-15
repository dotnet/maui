using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Convenience extensions so we have a single place to accommodate the fact we have to support .NET Standard 2.0
	/// </summary>
	internal static class StringExtensions
	{
		public static bool ContainsChar(this string toSearch, char character)
		{
#if NETSTANDARD2_0
			return toSearch.Contains(character.ToString());
#else
			return toSearch.Contains(character, StringComparison.Ordinal);
#endif
		}

		public static int IndexOfChar(this string toSearch, char character)
		{
			if (toSearch == null)
			{
				throw new ArgumentNullException(nameof(toSearch));
			}

#if NETSTANDARD2_0
			return toSearch.IndexOf(character);
#else
			return toSearch.IndexOf(character, StringComparison.Ordinal);
#endif
		}
	}
}