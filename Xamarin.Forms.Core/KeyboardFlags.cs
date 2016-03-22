using System;

namespace Xamarin.Forms
{
	[Flags]
	public enum KeyboardFlags
	{
		CapitalizeSentence = 1,
		Spellcheck = 1 << 1,
		Suggestions = 1 << 2,
		All = ~0
	}
}