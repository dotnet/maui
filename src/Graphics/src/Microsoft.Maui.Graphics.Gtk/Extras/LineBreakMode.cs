using System;

// copied from: Microsoft.Maui/Core/src/Primitives/LineBreakMode.cs
// merged with: Xwt.Drawing/TextLayout.cs TextTrimming

namespace Microsoft.Maui.Graphics.Extras {

	[Flags]
	public enum LineBreakMode {

		None = 0,

		Wrap = 0x1 << 0,
		Truncation = 0x1 << 1,
		Elipsis = 0x1 << 2,

		Character = 0x1 << 3,
		Word = 0x1 << 4,

		Start = 0x1 << 5,
		Center = 0x1 << 6,
		End = 0x1 << 7,

		NoWrap = None,

		WordWrap = Wrap | Word | End,
		CharacterWrap = Wrap | Character | End,
		WordCharacterWrap = Wrap | Word | Character | End,

		StartTruncation = Truncation | Character | Start,
		EndTruncation = Truncation | Character | End,
		CenterTruncation = Truncation | Character | Center,

		HeadTruncation = StartTruncation,
		TailTruncation = EndTruncation,
		MiddleTruncation = CenterTruncation,

		WordElipsis = Elipsis | Word | End,
		CharacterElipsis = Elipsis | Character | End,

	}

}
