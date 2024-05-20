#nullable disable
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

		public static void UpdateText(this UITextView textView, InputView inputView) =>
			UpdateText(textView, inputView, textView.IsFirstResponder);

		public static void UpdateText(this UITextField textField, InputView inputView) =>
			UpdateText(textField, inputView, textField.IsEditing);

		static void UpdateText(this IUITextInput textInput, InputView inputView, bool isEditing)
		{
			// Setting the text causes the cursor to be reset to the end of the IUITextInput.
			// So, let's set back the cursor to the last known position and calculate a new
			// position if needed when the text was modified by a Converter.
			var textRange = textInput.GetTextRange(textInput.BeginningOfDocument, textInput.EndOfDocument);
			var oldText = textInput.TextInRange(textRange) ?? string.Empty;

			// We need this variable because in some cases because of the iOS's
			// auto correction eg. eg '--' => '—' the actual text in the input might have
			// a different length that the one that has been set in the control.
			var newTextLength = !string.IsNullOrWhiteSpace(inputView.Text) ? textInput.TextInRange(textRange)?.Length ?? 0 : 0;

			var newText = TextTransformUtilites.GetTransformedText(
				inputView?.Text,
				textInput.GetSecureTextEntry() ? TextTransform.Default : inputView.TextTransform
				);

			if (!string.IsNullOrWhiteSpace(oldText))
				newTextLength = newText.Length;

			if (oldText != newText)
			{
				// Re-calculate the cursor offset position if the text was modified by a Converter.
				// but if the text is being set by code, let's just move the cursor to the end.
				var cursorOffset = newTextLength - oldText.Length;
				var cursorPosition = isEditing ? textInput.GetCursorPosition(cursorOffset) : newTextLength;

				if (textInput is UITextField tf)
					tf.Text = newText;
				else if (textInput is UITextView tv)
					tv.Text = newText;
				else
					textInput.ReplaceText(textRange, newText);

				textInput.SetTextRange(cursorPosition, 0);
			}
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
			{
				if (label.LineBreakMode == LineBreakMode.TailTruncation)
					maxLines = 1;
				else
					maxLines = 0;
			}

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
					break;
			}

			platformLabel.Lines = maxLines;
		}
	}
}