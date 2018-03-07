using System;

namespace Xamarin.Forms
{
	[Flags]
	public enum KeyboardFlags
	{
		None = 0,
		CapitalizeSentence = 1,
		Spellcheck = 1 << 1,
		Suggestions = 1 << 2,
		CapitalizeWord = 1 << 3,
		CapitalizeCharacter = 1 << 4,
		CapitalizeNone = 1 << 5,
		All = ~0
	}
}