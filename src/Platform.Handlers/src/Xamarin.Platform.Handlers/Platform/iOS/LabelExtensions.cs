using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using UIKit;

namespace Xamarin.Platform
{
	public static class LabelExtensions
	{
		public static void UpdateText(this UILabel label, string text)
			=> label.Text = text;

		public static void UpdateText(this UILabel label, NSAttributedString text)
			=> label.AttributedText = text;

		public static void UpdateText(this UILabel label, ILabel text)
			=> label.UpdateText(text.Text);
	}
}
