using System;

// copied from: Microsoft.Maui/Core/src/Primitives/LineBreakMode.cs
// merged with: Xwt.Drawing/TextLayout.cs TextTrimming

namespace Microsoft.Maui.Graphics.Platform.Gtk;

[Flags]
public enum LineBreakMode {

	None = 0,

	Wrap = 0x1 << 0,
	Truncation = 0x1 << 1,
	Ellipsis = 0x1 << 2,

	Character = 0x1 << 3,
	Word = 0x1 << 4,

	Head = 0x1 << 5,
	Middle = 0x1 << 6,
	Tail = 0x1 << 7,

	NoWrap = None,

	WordWrap = Wrap | Word | Tail,
	CharacterWrap = Wrap | Character | Tail,
	WordCharacterWrap = Wrap | Word | Character | Tail,

	HeadTruncation = Truncation | Character | Head,
	TailTruncation = Truncation | Character | Tail,
	MiddleTruncation = Truncation | Character | Middle,

	WordElipsis = Ellipsis | Word | Tail,
	CharacterElipsis = Ellipsis | Character | Tail,

}