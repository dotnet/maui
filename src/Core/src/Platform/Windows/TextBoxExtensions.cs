using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform.Windows
{
	public static class TextBoxExtensions
	{
		public static void UpdateText(this TextBox nativeControl, string text, TextTransform textTransform) =>
			nativeControl.Text = text.GetTransformedText(textTransform);
	}
}
