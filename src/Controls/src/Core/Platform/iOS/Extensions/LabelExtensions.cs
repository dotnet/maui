using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using UIKit;

namespace Controls.Core.Platform.iOS.Extensions
{
	public static class LabelExtensions
	{
		public static void UpdateText(this UILabel nativeLabel, Label label)
		{
			switch (label.TextType)
			{
				case TextType.Html:
					nativeLabel.UpdateTextHtml(label);
					break;

				default:
					nativeLabel.UpdateTextPlainText(label);
					break;
			}
		}

		public static void UpdateTextDecorations(this UILabel nativeLabel, Label label)
		{
			if (label?.TextType != TextType.Text)
				return;

			var modAttrText = nativeLabel.AttributedText?.WithDecorations(label.TextDecorations);

			if (modAttrText != null)
				nativeLabel.AttributedText = modAttrText;
		}
	}
}
