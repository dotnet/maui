using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls.Internals;
using TEntry = Tizen.UIExtensions.ElmSharp.Entry;

namespace Microsoft.Maui.Controls.Platform
{
	public static class TextExtensions
	{
		public static void UpdateText(this TEntry entry, InputView inputView)
		{
			entry.Text = TextTransformUtilites.GetTransformedText(entry.Text, inputView.TextTransform);
		}
	}
}
