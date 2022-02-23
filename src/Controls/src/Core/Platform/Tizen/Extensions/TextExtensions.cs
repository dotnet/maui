using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls.Internals;
using TEntry = Tizen.UIExtensions.NUI.Entry;
using TEditor = Tizen.UIExtensions.NUI.Editor;

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
		
		public static void UpdateText(this TEditor editor, InputView inputView)
		{
			editor.Text = TextTransformUtilites.GetTransformedText(editor.Text, inputView.TextTransform);
		}
	}
}
