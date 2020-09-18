using Foundation;
using AppKit;
using Xamarin.Forms;

namespace Xamarin.Platform
{
	public static class ViewExtensions
	{
		public static void SetText(this NSTextField label, string text)
			=> label.StringValue = text;

		public static void SetText(this NSTextField label, NSAttributedString text)
			=> label.AttributedStringValue = text;

		public static void SetText(this NSButton view, string text)
			=> view.StringValue = text;

		public static void SetText(this NSButton view, NSAttributedString text)
			=> view.AttributedStringValue = text;

		public static void SetBackgroundColor(this NSView view, NSColor color)
		{
			view.WantsLayer = true;
			view.Layer.BackgroundColor = color.CGColor;
		}

		public static NSColor GetBackgroundColor(this NSView view) =>
			NSColor.FromCGColor(view.Layer.BackgroundColor);

		public static CoreGraphics.CGSize SizeThatFits(this NSView view, CoreGraphics.CGSize size) =>
			(view as NSControl)?.SizeThatFits(size) ?? view.FittingSize;

		public static void SetTextColor(this NSButton button, Color color, Color defaultColor) =>
			button.ContentTintColor = color.Cleanse(defaultColor).ToNative();

		static Color Cleanse(this Color color, Color defaultColor) => color.IsDefault ? defaultColor : color;
	}
}
