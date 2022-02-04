using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class LabelExtensions
	{
		public static void UpdateTextColor(this UILabel nativeLabel, ITextStyle textStyle, UIColor? defaultColor = null)
		{
			// Default value of color documented to be black in iOS docs
			var textColor = textStyle.TextColor;
			nativeLabel.TextColor = textColor.ToPlatform(defaultColor ?? ColorExtensions.LabelColor);
		}

		public static void UpdateCharacterSpacing(this UILabel nativeLabel, ITextStyle textStyle)
		{
			var textAttr = nativeLabel.AttributedText?.WithCharacterSpacing(textStyle.CharacterSpacing);

			if (textAttr != null)
				nativeLabel.AttributedText = textAttr;
		}

		public static void UpdateFont(this UILabel nativeLabel, ITextStyle textStyle, IFontManager fontManager) =>
			nativeLabel.UpdateFont(textStyle, fontManager, UIFont.LabelFontSize);

		public static void UpdateFont(this UILabel nativeLabel, ITextStyle textStyle, IFontManager fontManager, double defaultSize)
		{
			var uiFont = fontManager.GetFont(textStyle.Font, defaultSize);
			nativeLabel.Font = uiFont;
		}

		public static void UpdateHorizontalTextAlignment(this UILabel nativeLabel, ILabel label)
		{
			nativeLabel.TextAlignment = label.HorizontalTextAlignment.ToPlatform(label);
		}

		public static void UpdateLineBreakMode(this UILabel nativeLabel, ILabel label)
		{
			nativeLabel.SetLineBreakMode(label);
		}

		public static void UpdateMaxLines(this UILabel nativeLabel, ILabel label)
		{
			nativeLabel.SetLineBreakMode(label);
		}

		public static void UpdatePadding(this MauiLabel nativeLabel, ILabel label)
		{
			nativeLabel.TextInsets = new UIEdgeInsets(
				(float)label.Padding.Top,
				(float)label.Padding.Left,
				(float)label.Padding.Bottom,
				(float)label.Padding.Right);
		}

		public static void UpdateTextDecorations(this UILabel nativeLabel, ILabel label)
		{
			var modAttrText = nativeLabel.AttributedText?.WithDecorations(label.TextDecorations);

			if (modAttrText != null)
				nativeLabel.AttributedText = modAttrText;
		}

		public static void UpdateLineHeight(this UILabel nativeLabel, ILabel label)
		{
			var modAttrText = nativeLabel.AttributedText?.WithLineHeight(label.LineHeight);

			if (modAttrText != null)
				nativeLabel.AttributedText = modAttrText;
		}

		internal static void UpdateTextHtml(this UILabel nativeLabel, ILabel label)
		{
			string text = label.Text ?? string.Empty;

			var attr = new NSAttributedStringDocumentAttributes
			{
				DocumentType = NSDocumentType.HTML,
				StringEncoding = NSStringEncoding.UTF8
			};

			NSError? nsError = null;

			nativeLabel.AttributedText = new NSAttributedString(text, attr, ref nsError);
		}

		internal static void UpdateTextPlainText(this UILabel nativeLabel, IText label)
		{
			nativeLabel.Text = label.Text;
		}

		internal static void SetLineBreakMode(this UILabel nativeLabel, ILabel label)
		{
			int maxLines = label.MaxLines;
			if (maxLines < 0)
				maxLines = 0;

			switch (label.LineBreakMode)
			{
				case LineBreakMode.NoWrap:
					nativeLabel.LineBreakMode = UILineBreakMode.Clip;
					maxLines = 1;
					break;
				case LineBreakMode.WordWrap:
					nativeLabel.LineBreakMode = UILineBreakMode.WordWrap;
					break;
				case LineBreakMode.CharacterWrap:
					nativeLabel.LineBreakMode = UILineBreakMode.CharacterWrap;
					break;
				case LineBreakMode.HeadTruncation:
					nativeLabel.LineBreakMode = UILineBreakMode.HeadTruncation;
					maxLines = 1;
					break;
				case LineBreakMode.MiddleTruncation:
					nativeLabel.LineBreakMode = UILineBreakMode.MiddleTruncation;
					maxLines = 1;
					break;
				case LineBreakMode.TailTruncation:
					nativeLabel.LineBreakMode = UILineBreakMode.TailTruncation;
					maxLines = 1;
					break;
			}

			nativeLabel.Lines = maxLines;
		}
	}
}