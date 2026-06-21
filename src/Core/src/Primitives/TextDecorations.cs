using System;

namespace Microsoft.Maui;

/// <summary>
/// Flagging enumeration defining text decorations.
/// </summary>
[Flags]
public enum TextDecorations
{
	/// <summary>
	/// No text decoration.
	/// </summary>
	None = 0,

	/// <summary>
	/// A text underline.
	/// </summary>
	Underline = 1 << 0,

	/// <summary>
	/// A single-line strikethrough.
	/// </summary>
	Strikethrough = 1 << 1,
}
