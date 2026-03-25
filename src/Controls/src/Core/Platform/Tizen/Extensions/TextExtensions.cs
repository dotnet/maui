#nullable disable
using Microsoft.Maui.Controls.Internals;
using Tizen.UIExtensions.NUI;
using TEditor = Tizen.UIExtensions.NUI.Editor;
using TEntry = Tizen.UIExtensions.NUI.Entry;
using TFormattedString = Tizen.UIExtensions.Common.FormattedString;
using TLabel = Tizen.UIExtensions.NUI.Label;
using TSpan = Tizen.UIExtensions.Common.Span;

namespace Microsoft.Maui.Controls.Platform
{
	public static class TextExtensions
	{
		public static void UpdateText(this TEntry entry, InputView inputView)
		{
			var text = TextTransformUtilities.GetTransformedText(inputView.Text, inputView.TextTransform);
			if (entry.Text != text)
				entry.Text = text;
		}

		public static void UpdateLineBreakMode(this TLabel platformLabel, Label label)
		{
			platformLabel.LineBreakMode = label.LineBreakMode.ToPlatform();
		}

		public static void UpdateText(this TEditor editor, InputView inputView)
		{
			var text = TextTransformUtilities.GetTransformedText(inputView.Text, inputView.TextTransform);
			if (editor.Text != text)
				editor.Text = text;
		}

		public static void UpdateText(this TLabel platformLabel, Label label)
		{
			switch (label.TextType)
			{
				case TextType.Text:
					if (label.FormattedText != null)
						platformLabel.FormattedText = label.ToFormattedString();
					else
						platformLabel.Text = TextTransformUtilities.GetTransformedText(label.Text, label.TextTransform);
					break;
				case TextType.Html:
					platformLabel.UpdateTextHtml(label);
					break;
			}
		}

		static void UpdateTextHtml(this TLabel platformLabel, Label label)
		{
			var formattedText = new TFormattedString();
			var htmlSpan = new TSpan() { Text = label.Text };
			formattedText.Spans.Add(htmlSpan);
			platformLabel.EnableMarkup = true;
			platformLabel.Text = formattedText.ToMarkupText();
		}
	}
}
