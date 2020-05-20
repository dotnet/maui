using Foundation;
using UIKit;

namespace System.Maui.Platform
{
	public static class ViewExtensions
	{
		public static void SetText(this UILabel label, string text)
			=> label.Text = text;

		public static void SetText(this UILabel label, NSAttributedString text)
			=> label.AttributedText = text;

		public static void SetBackgroundColor (this UIView view, UIColor color)
			=> view.BackgroundColor = color;
		public static UIColor GetBackgroundColor (this UIView view) =>
			view.BackgroundColor;


		public static void SetText(this UIButton view, string text)
			=> view.SetTitle(text, UIControlState.Normal);

		public static void SetText(this UIButton view, NSAttributedString text)
			=> view.SetAttributedTitle(text, UIControlState.Normal);

		public static void SetTextColor(this UIButton button, Color color, Color defaultColor)
		{
			button.SetTitleColor(color.Cleanse(defaultColor).ToNativeColor(), UIControlState.Normal);
		}

		static Color Cleanse(this Color color, Color defaultColor) => color.IsDefault ? defaultColor : color;
	}
}
