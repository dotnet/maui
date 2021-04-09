using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform.Windows
{
	public static class TextBoxExtensions
	{
		public static void UpdateText(this TextBox nativeControl, string? text, TextTransform textTransform)
		{
			// TODO: WinUI throws layout cycle exception if the text is big enough to go out of the boundries in sample app.
			// This must be removed after the layout measurement calls is corrected for WinUI.

			if (text?.Length > 5)
				text = text.TrimToMaxLength(5);

			nativeControl.Text = text.GetTransformedText(textTransform);
		}
	}
}
