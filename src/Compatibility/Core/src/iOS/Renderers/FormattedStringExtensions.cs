using Foundation;
using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
#if __MOBILE__
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using AppKit;
using UIColor = AppKit.NSColor;
using UITextAlignment = AppKit.NSTextAlignment;
namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public static class FormattedStringExtensions
	{
		public static NSAttributedString ToAttributed(this Span span, Font defaultFont, Color defaultForegroundColor)
		{
			if (span == null)
				return null;

			var fgcolor = span.TextColor;
			if (fgcolor == null)
				fgcolor = defaultForegroundColor;
			if (fgcolor == null)
				fgcolor = ColorExtensions.LabelColor.ToColor();

#if __MOBILE__
			return new NSAttributedString(span.Text, null, fgcolor.ToUIColor(),
				span.BackgroundColor.ToUIColor(), kerning: (float)span.CharacterSpacing);
#else
			return new NSAttributedString(span.Text, null, fgcolor.ToNSColor(),
				span.BackgroundColor.ToNSColor(), kerningAdjustment: (float)span.CharacterSpacing);
#endif
		}

		public static NSAttributedString ToAttributed(this FormattedString formattedString, Font defaultFont,
			Color defaultForegroundColor)
		{
			if (formattedString == null)
				return null;
			var attributed = new NSMutableAttributedString();
			for (int i = 0; i < formattedString.Spans.Count; i++)
			{
				Span span = formattedString.Spans[i];
				if (span.Text == null)
					continue;

				attributed.Append(span.ToAttributed(defaultFont, defaultForegroundColor));
			}

			return attributed;
		}

		internal static NSAttributedString ToAttributed(this Span span, BindableObject owner, Color defaultForegroundColor, TextAlignment textAlignment, double lineHeight = -1.0)
		{
			if (span == null)
				return null;

			var text = span.Text;
			if (text == null)
				return null;

			NSMutableParagraphStyle style = new NSMutableParagraphStyle();
			lineHeight = span.LineHeight >= 0 ? span.LineHeight : lineHeight;
			if (lineHeight >= 0)
			{
				style.LineHeightMultiple = new nfloat(lineHeight);
			}

			switch (textAlignment)
			{
				case TextAlignment.Start:
					style.Alignment = UITextAlignment.Left;
					break;
				case TextAlignment.Center:
					style.Alignment = UITextAlignment.Center;
					break;
				case TextAlignment.End:
					style.Alignment = UITextAlignment.Right;
					break;
				default:
					style.Alignment = UITextAlignment.Left;
					break;
			}


#if __MOBILE__
			UIFont targetFont;
			if (span.IsDefault())
				targetFont = ((IFontElement)owner).ToUIFont();
			else
				targetFont = span.ToUIFont();
#else
			NSFont targetFont;
			if (span.IsDefault())
				targetFont = ((IFontElement)owner).ToNSFont();
			else
				targetFont = span.ToNSFont();
#endif
			var fgcolor = span.TextColor;

			if (fgcolor == null)
				fgcolor = defaultForegroundColor;

			if (owner is Entry && fgcolor != null)
				fgcolor = defaultForegroundColor;

#if __MOBILE__
			if (fgcolor == null)
				fgcolor = ColorExtensions.LabelColor.ToColor();
			UIColor spanFgColor;
			UIColor spanBgColor = null;
			spanFgColor = fgcolor.ToUIColor();

			spanBgColor = span.BackgroundColor?.ToUIColor();
#else

			if (fgcolor.IsDefault)
				fgcolor = ColorExtensions.LabelColor.ToColor(NSColorSpace.GenericRGBColorSpace);
			NSColor spanFgColor;
			NSColor spanBgColor;
			spanFgColor = fgcolor.ToNSColor();
			spanBgColor = span.BackgroundColor.ToNSColor();
#endif

			bool hasUnderline = false;
			bool hasStrikethrough = false;
			if (span.IsSet(Span.TextDecorationsProperty))
			{
				var textDecorations = span.TextDecorations;
				hasUnderline = (textDecorations & TextDecorations.Underline) != 0;
				hasStrikethrough = (textDecorations & TextDecorations.Strikethrough) != 0;
			}
#if __MOBILE__
			var attrString = new NSAttributedString(text, targetFont, spanFgColor, spanBgColor,
				underlineStyle: hasUnderline ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				strikethroughStyle: hasStrikethrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None, paragraphStyle: style, kerning: (float)span.CharacterSpacing);
#else
			var attrString = new NSAttributedString(text, targetFont, spanFgColor, spanBgColor,
				underlineStyle: hasUnderline ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				strikethroughStyle: hasStrikethrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None, paragraphStyle: style, kerningAdjustment: (float)span.CharacterSpacing);
#endif

			return attrString;
		}

		internal static NSAttributedString ToAttributed(this FormattedString formattedString, BindableObject owner,
			Color defaultForegroundColor, TextAlignment textAlignment = TextAlignment.Start, double lineHeight = -1.0)
		{
			if (formattedString == null)
				return null;

			var attributed = new NSMutableAttributedString();

			for (int i = 0; i < formattedString.Spans.Count; i++)
			{
				Span span = formattedString.Spans[i];

				var attributedString = span.ToAttributed(owner, defaultForegroundColor, textAlignment, lineHeight);

				if (attributedString == null)
					continue;

				attributed.Append(attributedString);
			}

			return attributed;
		}
	}
}