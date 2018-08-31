using Foundation;
using System;
using Xamarin.Forms.Internals;
#if __MOBILE__
using UIKit;

namespace Xamarin.Forms.Platform.iOS
#else
using AppKit;
using UIColor = AppKit.NSColor;

namespace Xamarin.Forms.Platform.MacOS
#endif
{
	public static class FormattedStringExtensions
	{
		public static NSAttributedString ToAttributed(this Span span, Font defaultFont, Color defaultForegroundColor)
		{
			if (span == null)
				return null;

#pragma warning disable 0618 //retaining legacy call to obsolete code
			var font = span.Font != Font.Default ? span.Font : defaultFont;
#pragma warning restore 0618
			var fgcolor = span.TextColor;
			if (fgcolor.IsDefault)
				fgcolor = defaultForegroundColor;
			if (fgcolor.IsDefault)
				fgcolor = Color.Black; // as defined by apple docs		

#if __MOBILE__
			return new NSAttributedString(span.Text, font == Font.Default ? null : font.ToUIFont(), fgcolor.ToUIColor(), span.BackgroundColor.ToUIColor());
#else
			return new NSAttributedString(span.Text, font == Font.Default ? null : font.ToNSFont(), fgcolor.ToNSColor(),
				span.BackgroundColor.ToNSColor());
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

		internal static NSAttributedString ToAttributed(this Span span, Element owner, Color defaultForegroundColor, double lineHeight = -1.0)
		{
			if (span == null)
				return null;

			var text = span.Text;
			if (text == null)
				return null;

			NSMutableParagraphStyle style = null;
			lineHeight = span.LineHeight >= 0 ? span.LineHeight : lineHeight;
			if (lineHeight >= 0)
			{
				style = new NSMutableParagraphStyle();
				style.LineHeightMultiple = new nfloat(lineHeight);
			}

#if __MOBILE__
			UIFont targetFont;
			if (span.IsDefault())
				targetFont = ((IFontElement)owner).ToUIFont();
			else
				targetFont = span.ToUIFont();

			var fgcolor = span.TextColor;
			if (fgcolor.IsDefault)
				fgcolor = defaultForegroundColor;
			if (fgcolor.IsDefault)
				fgcolor = Color.Black; // as defined by apple docs

			return new NSAttributedString(text, targetFont, fgcolor.ToUIColor(), span.BackgroundColor.ToUIColor(), null, style);
#else
			NSFont targetFont;
			if (span.IsDefault())
				targetFont = ((IFontElement)owner).ToNSFont();
			else
				targetFont = span.ToNSFont();

			var fgcolor = span.TextColor;
			if (fgcolor.IsDefault)
				fgcolor = defaultForegroundColor;
			if (fgcolor.IsDefault)
				fgcolor = Color.Black; // as defined by apple docs

			return new NSAttributedString(text, targetFont, fgcolor.ToNSColor(), span.BackgroundColor.ToNSColor(),
										  null, null, null, NSUnderlineStyle.None, NSUnderlineStyle.None, style);
#endif
		}

		internal static NSAttributedString ToAttributed(this FormattedString formattedString, Element owner,
			Color defaultForegroundColor, double lineHeight = -1.0)
		{
			if (formattedString == null)
				return null;
			var attributed = new NSMutableAttributedString();

			for (int i = 0; i < formattedString.Spans.Count; i++)
			{
				Span span = formattedString.Spans[i];
				var attributedString = span.ToAttributed(owner, defaultForegroundColor, lineHeight);
				if (attributedString == null)
					continue;

				attributed.Append(attributedString);
			}

			return attributed;
		}
	}
}