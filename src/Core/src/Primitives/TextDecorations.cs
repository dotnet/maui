using System;

namespace Microsoft.Maui
{
	[Flags]
	public enum TextDecorations
	{
		None = 0,
		Underline = 1 << 0,
		Strikethrough = 1 << 1,
	}
}
