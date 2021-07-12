using System;
using System.Collections.Generic;
using System.Text;
using Android.Widget;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Android.Text;
using static Android.Widget.TextView;

namespace Controls.Core.Platform.Android.Extensions
{
	public static class TextViewExtensions
	{
		public static void UpdateText(this TextView textView, Label label)
		{
			textView.UpdateText(label.Text, label.TextType);
		}

		public static void UpdateText(this TextView textView, string newText, TextType textType = TextType.Text)
		{
			newText ??= string.Empty;
			var oldText = textView.Text ?? string.Empty;

			switch (textType)
			{
				case TextType.Html:
					if (NativeVersion.IsAtLeast(24))
						textView.SetText(Html.FromHtml(newText, FromHtmlOptions.ModeCompact), BufferType.Spannable);
					else
#pragma warning disable CS0618 // Type or member is obsolete
						textView.SetText(Html.FromHtml(newText), BufferType.Spannable);
#pragma warning restore CS0618 // Type or member is obsolete
					break;

				default:
					textView.Text = newText;
					break;
			}
		}
	}
}
