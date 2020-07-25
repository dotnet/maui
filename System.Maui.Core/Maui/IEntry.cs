using System;
using System.Collections.Generic;
using System.Text;

namespace System.Maui
{
	public partial class Entry : ITextInput
	{
		TextType IText.TextType => TextType.Text;

		Color IText.Color => TextColor;
	}
}
