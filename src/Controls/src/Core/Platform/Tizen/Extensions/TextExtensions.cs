using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls.Internals;
using TEntry = Tizen.UIExtensions.NUI.Entry;
using TEditor = Tizen.UIExtensions.NUI.Editor;
using TLabel = Tizen.UIExtensions.NUI.Label;

namespace Microsoft.Maui.Controls.Platform
{
	public static class TextExtensions
	{
		public static void UpdateText(this TEntry entry, InputView inputView)
		{
			var text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
			if (entry.Text != text)
				entry.Text = text;
		}

		public static void UpdateLineBreakMode(this TLabel platformLabel, Label label)
		{
			platformLabel.LineBreakMode = label.LineBreakMode.ToPlatform();
		}

		public static void UpdateText(this TEditor editor, InputView inputView)
		{
			var text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
			if (editor.Text != text)
				editor.Text = text;
		}

		public static void UpdateText(this TLabel platformLabel, Label label)
		{
			platformLabel.Text = TextTransformUtilites.GetTransformedText(label.Text, label.TextTransform);
		}
	}
}
