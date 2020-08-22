using System;
using System.Collections.Generic;
using System.Maui;
using System.Text;
using Xamarin.Forms;

namespace Xamarin.Forms
{
	public partial class Entry : ITextInput
	{
		TextType IText.TextType => TextType.Text;

		Color IText.Color => TextColor;
	}
}
