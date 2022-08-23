using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Platform;
using TEntry = Tizen.UIExtensions.ElmSharp.Entry;
using TLabel = Tizen.UIExtensions.ElmSharp.Label;

namespace Microsoft.Maui.Controls.Platform
{
	public static class TextExtensions
	{
		public static void UpdateText(this TEntry entry, InputView inputView)
		{
			entry.Text = TextTransformUtilites.GetTransformedText(entry.Text, inputView.TextTransform);
		}

		public static void UpdateLineBreakMode(this TLabel platformLabel, Label label)
		{
			platformLabel.LineBreakMode = label.LineBreakMode.ToPlatform();
		}
	}
}
