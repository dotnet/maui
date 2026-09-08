using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class LabelExtensions
	{
		public static void UpdateTextColor(this UILabel platformLabel, ITextStyle textStyle, UIColor? defaultColor = null)
		{
			// Default value of color documented to be black in iOS docs
			var textColor = textStyle.TextColor;
			platformLabel.TextColor = textColor.ToPlatform(defaultColor ?? ColorExtensions.LabelColor);
		}

		public static void UpdateCharacterSpacing(this UILabel platformLabel, ITextStyle textStyle)
		{
			var textAttr = platformLabel.AttributedText?.WithCharacterSpacing(textStyle.CharacterSpacing);

			if (textAttr != null)
				platformLabel.AttributedText = textAttr;
		}

		public static void UpdateFont(this UILabel platformLabel, ITextStyle textStyle, IFontManager fontManager) =>
			platformLabel.UpdateFont(textStyle, fontManager, UIFont.LabelFontSize);

		public static void UpdateFont(this UILabel platformLabel, ITextStyle textStyle, IFontManager fontManager, double defaultSize)
		{
			var uiFont = fontManager.GetFont(textStyle.Font, defaultSize);
			platformLabel.Font = uiFont;
		}

		public static void UpdateHorizontalTextAlignment(this UILabel platformLabel, ILabel label)
		{
			platformLabel.TextAlignment = label.HorizontalTextAlignment.ToPlatformHorizontal(platformLabel.EffectiveUserInterfaceLayoutDirection);
		}

		// Don't use this method, it doesn't work. But we can't remove it.
		public static void UpdateVerticalTextAlignment(this UILabel platformLabel, ILabel label)
		{
			if (!platformLabel.Bounds.IsEmpty)
				platformLabel.InvalidateMeasure(label);
		}

		internal static void UpdateVerticalTextAlignment(this MauiLabel platformLabel, ILabel label)
		{
			platformLabel.VerticalAlignment = label.VerticalTextAlignment.ToPlatformVertical();
		}

		public static void UpdatePadding(this MauiLabel platformLabel, ILabel label)
		{
			platformLabel.TextInsets = new UIEdgeInsets(
				(float)label.Padding.Top,
				(float)label.Padding.Left,
				(float)label.Padding.Bottom,
				(float)label.Padding.Right);
		}

		public static void UpdateTextDecorations(this UILabel platformLabel, ILabel label)
		{
			var modAttrText = platformLabel.AttributedText?.WithDecorations(label.TextDecorations);

			if (modAttrText != null)
				platformLabel.AttributedText = modAttrText;
		}

		public static void UpdateLineHeight(this UILabel platformLabel, ILabel label)
		{
			var modAttrText = platformLabel.AttributedText?.WithLineHeight(label.LineHeight);

			if (modAttrText != null)
				platformLabel.AttributedText = modAttrText;
		}

		internal static void UpdateTextHtml(this UILabel platformLabel, string text)
		{
			var attr = new NSAttributedStringDocumentAttributes
			{
				DocumentType = NSDocumentType.HTML,
#if IOS17_5_OR_GREATER || MACCATALYST17_5_OR_GREATER
				CharacterEncoding = NSStringEncoding.UTF8
#else
				StringEncoding = NSStringEncoding.UTF8
#endif
			};

			NSError nsError = new();

			// NOTE: Sometimes this will crash with some sort of consistency error.
			// https://github.com/dotnet/maui/issues/25946
			// The caller should ensure that this extension is dispatched. We cannot
			// do it here as we need to re-apply the formatting and we cannot call
			// into Controls from Core.
			// This is observed with CarouselView 1 but not with 2, so hopefully this
			// will be just disappear once we switch.
#pragma warning disable CS8601
#pragma warning disable CS0618
			platformLabel.AttributedText = new NSAttributedString(text, attr, ref nsError);
#pragma warning restore CS0618
#pragma warning restore CS8601
		}

		internal static void UpdateTextPlainText(this UILabel platformLabel, IText label)
		{
			platformLabel.Text = label.Text;
		}
	}
}
