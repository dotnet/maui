using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Enumerates keyboard option flags that controls capitalization, spellcheck, and suggestion behavior.
	/// </summary>
	[Flags]
	public enum KeyboardFlags
	{
		/// <summary>
		/// Indicates that nothing will be automatically capitalized.
		/// </summary>
		None = 0,

		/// <summary>
		/// Indicates that the first letters of the first words of each sentence will be automatically capitalized.
		/// </summary>
		CapitalizeSentence = 1,

		/// <summary>
		/// Perform spellcheck on text that the user enters.
		/// </summary>
		Spellcheck = 1 << 1,

		/// <summary>
		/// Offer suggested word completions on text that the user enters.
		/// </summary>
		Suggestions = 1 << 2,

		/// <summary>
		/// Indicates that the first letter of each word will be automatically capitalized.
		/// </summary>
		CapitalizeWord = 1 << 3,

		/// <summary>
		/// Indicates that every character will be automatically capitalized.
		/// </summary>
		CapitalizeCharacter = 1 << 4,

		/// <summary>
		/// Indicates that nothing will be automatically capitalized.
		/// </summary>
		CapitalizeNone = 1 << 5,

		/// <summary>
		/// Capitalize the first letter of the first words of sentences, perform spellcheck, and offer suggested word completions on text that the user enters.
		/// </summary>
		All = ~0
	}
}