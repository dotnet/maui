using System;

namespace Xamarin.Forms
{
	[Flags]
	public enum TextDecorations
	{
		None = 0,
		Underline = 1 << 0,
		Strikethrough = 1 << 1,
	}
}
