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

		internal static void UpdateTextHtml(this UILabel platformLabel, ILabel label)
		{
			string text = label.Text ?? string.Empty;

			var attr = new NSAttributedStringDocumentAttributes
			{
				DocumentType = NSDocumentType.HTML,
#if IOS17_5_OR_GREATER || MACCATALYST17_5_OR_GREATER
				CharacterEncoding = NSStringEncoding.UTF8
#else
				StringEncoding = NSStringEncoding.UTF8
#endif
			};

			var uiFontAttribute = label?.Handler?.GetRequiredService<IFontManager>()?.GetFont(label.Font, UIFont.LabelFontSize);

			NSError? error;
			var htmlAttributedString = NSAttributedString.Create(text, attr, out _, out error);
			if (htmlAttributedString == null || error != null)
				return;
				
			var attributedString = new NSMutableAttributedString(htmlAttributedString);

			// Enumerate through the attributes in the string and update font size
			attributedString.EnumerateAttributes(new NSRange(0, attributedString.Length), NSAttributedStringEnumeration.None,
				(NSDictionary attrs, NSRange range, ref bool stop) =>
				{
					if (label!.Font.Family == null)
					{
						var font = attrs[UIStringAttributeKey.Font] as UIFont;
						if (font != null)
						{
							font = font.WithSize((nfloat)(label?.Font.Size ?? UIFont.LabelFontSize));
							attributedString.AddAttribute(UIStringAttributeKey.Font, font, range);
						}
					}
					else if (uiFontAttribute != null)
					{
						attributedString.AddAttribute(UIStringAttributeKey.Font, uiFontAttribute, range);
					}

					if(label?.TextColor != null)
					{
						var color = label.TextColor.ToPlatform();
						attributedString.AddAttribute(UIStringAttributeKey.ForegroundColor, color, range);
					}
				});

			platformLabel.AttributedText = attributedString;
		}

		internal static void UpdateTextPlainText(this UILabel platformLabel, IText label)
		{
			platformLabel.Text = label.Text;
		}
	}
}
