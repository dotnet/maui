﻿using System;
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
			// Setting the text causes the cursor to be reset to the end of the UITextView.
			// So, let's set back the cursor to the last known position and calculate a new
			// position if needed when the text was modified by a Converter.
			var oldText = textView?.Text ?? string.Empty;
			var newText = inputView?.Text ?? string.Empty;

			// Calculate the cursor offset position if the text was modified by a Converter.
			var cursorOffset = newText.Length - oldText.Length;
			var cursorPosition = textView.GetCursorPosition(cursorOffset);

			textView.Text = TextTransformUtilites.GetTransformedText(newText, inputView.TextTransform);

			var textInput = inputView as ITextInput;

			if (textInput.CursorPosition != cursorPosition)
				// Both PlatformView and VirtualView are out of sync.
				// Setting the VirtualView property, will sync both.
				textInput.CursorPosition = cursorPosition;
			else
				// PlatformView and VirtualView were in sync, but due text modified by a Converter
				// PlatformView needs to be synced again, VirtualView is ok.
				textView.SetCursorPosition(cursorPosition);
		}

		public static void UpdateText(this UITextField textField, InputView inputView)
		{
			// Setting the text causes the cursor to be reset to the end of the UITextView.
			// So, let's set back the cursor to the last known position and calculate a new
			// position if needed when the text was modified by a Converter.
			var oldText = textField?.Text ?? string.Empty;
			var newText = inputView?.Text ?? string.Empty;

			// Calculate the cursor offset position if the text was modified by a Converter.
			var cursorOffset = newText.Length - oldText.Length;
			var cursorPosition = textField.GetCursorPosition(cursorOffset);

			textField.Text = TextTransformUtilites.GetTransformedText(newText, inputView.TextTransform);

			var textInput = inputView as ITextInput;

			if (textInput.CursorPosition != cursorPosition)
				// Both PlatformView and VirtualView are out of sync.
				// Setting the VirtualView property, will sync both.
				textInput.CursorPosition = cursorPosition;
			else
				// PlatformView and VirtualView were in sync, but due text modified by a Converter
				// PlatformView needs to be synced again, VirtualView is ok.
				textField.SetCursorPosition(cursorPosition);
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