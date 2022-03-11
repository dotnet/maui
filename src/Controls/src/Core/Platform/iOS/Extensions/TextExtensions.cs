using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls.Internals;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	public static class TextExtensions
	{
		public static void UpdateText(this UITextView textView, InputView inputView)
		{
			textView.Text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
		}

		public static void UpdateText(this UITextField textField, InputView inputView)
		{
			textField.Text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
		}
	}
}
