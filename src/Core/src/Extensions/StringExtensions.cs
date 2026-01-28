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
#if NETSTANDARD2_0
			return toSearch.IndexOf(character);
#else
			return toSearch.IndexOf(character, StringComparison.Ordinal);
#endif
		}

		public static int LastIndexOfChar(this string toSearch, char character)
		{
			// The char overload of LastIndexOf doesn't have a StringComparison parameter.
			// Unlike IndexOf and Contains, there's no LastIndexOf(char, StringComparison).
			// The char comparison is always ordinal.
#pragma warning disable CA1307 // Specify StringComparison for clarity
			return toSearch.LastIndexOf(character);
#pragma warning restore CA1307
		}
	}
}