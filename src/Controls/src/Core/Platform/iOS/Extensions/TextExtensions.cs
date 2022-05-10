using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Platform;
using UIKit;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Entry;

namespace Microsoft.Maui.Controls.Platform
{
	public static class TextExtensions
	{
		public static void UpdateCursorColor(this UITextField textField, Entry entry)
		{
			if (entry.IsSet(Specifics.CursorColorProperty))
			{
				var color = entry.OnThisPlatform().GetCursorColor();

				if (color != null)
					textField.TintColor = color.ToPlatform();
			}
		}

		public static void UpdateAdjustsFontSizeToFitWidth(this UITextField textField, Entry entry)
		{
			textField.AdjustsFontSizeToFitWidth = entry.OnThisPlatform().AdjustsFontSizeToFitWidth();
		}

		public static void UpdateText(this UITextView textView, InputView inputView)
		{
			textView.Text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
		}

		public static void UpdateText(this UITextField textField, InputView inputView)
		{
			var startRange = textField.GetPosition(textField.BeginningOfDocument, 0);
			var selectedTextRange = textField.SelectedTextRange ?? textField.GetTextRange(startRange, startRange);

			// Save the the cursor offset position if the text was modified by a Convertor
			var cursorOffset = inputView?.Text?.Length - textField.Text?.Length ?? 0;

			textField.Text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);

			var newCursorPosition = textField.GetPosition(selectedTextRange.Start, cursorOffset);
			textField.SelectedTextRange = textField.GetTextRange(newCursorPosition, newCursorPosition);
		}

		public static void UpdateLineBreakMode(this UILabel platformLabel, Label label)
		{
			platformLabel.SetLineBreakMode(label);
		}

		public static void UpdateMaxLines(this UILabel platformLabel, Label label)
		{
			platformLabel.SetLineBreakMode(label);
		}

		internal static void SetLineBreakMode(this UILabel platformLabel, Label label)
		{
			int maxLines = label.MaxLines;
			if (maxLines < 0)
				maxLines = 0;

			switch (label.LineBreakMode)
			{
				case LineBreakMode.NoWrap:
					platformLabel.LineBreakMode = UILineBreakMode.Clip;
					maxLines = 1;
					break;
				case LineBreakMode.WordWrap:
					platformLabel.LineBreakMode = UILineBreakMode.WordWrap;
					break;
				case LineBreakMode.CharacterWrap:
					platformLabel.LineBreakMode = UILineBreakMode.CharacterWrap;
					break;
				case LineBreakMode.HeadTruncation:
					platformLabel.LineBreakMode = UILineBreakMode.HeadTruncation;
					maxLines = 1;
					break;
				case LineBreakMode.MiddleTruncation:
					platformLabel.LineBreakMode = UILineBreakMode.MiddleTruncation;
					maxLines = 1;
					break;
				case LineBreakMode.TailTruncation:
					platformLabel.LineBreakMode = UILineBreakMode.TailTruncation;
					maxLines = 1;
					break;
			}

			platformLabel.Lines = maxLines;
		}
	}
}