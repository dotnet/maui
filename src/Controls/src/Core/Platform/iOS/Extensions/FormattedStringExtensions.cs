#nullable enable
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
#if !MACOS
using ObjCRuntime;
using UIKit;
#else
using AppKit;
using UIColor = AppKit.NSColor;
using UITextAlignment = AppKit.NSTextAlignment;
#endif

namespace Microsoft.Maui.Controls.Platform
{
	public static class FormattedStringExtensions
	{
		public static NSAttributedString? ToNSAttributedString(this Label label)
			=> ToNSAttributedString(
				label.FormattedText,
				label.RequireFontManager(),
				label.LineHeight,
				label.HorizontalTextAlignment,
				label.ToFont(),
				label.TextColor,
				label.TextTransform);

		public static NSAttributedString ToNSAttributedString(
			this FormattedString formattedString,
			IFontManager fontManager,
			double defaultLineHeight = 0d, // TODO: NET8 should be -1, but too late to change for net6
			TextAlignment defaultHorizontalAlignment = TextAlignment.Start,
			Font? defaultFont = null,
			Color? defaultColor = null,
			TextTransform defaultTextTransform = TextTransform.Default)
		{
			if (formattedString == null)
				return new NSAttributedString(string.Empty);

			var attributed = new NSMutableAttributedString();
			for (int i = 0; i < formattedString.Spans.Count; i++)
			{
				Span span = formattedString.Spans[i];
				if (span.Text == null)
					continue;

				attributed.Append(span.ToNSAttributedString(fontManager, defaultLineHeight, defaultHorizontalAlignment, defaultFont, defaultColor, defaultTextTransform));
			}

			return attributed;
		}

		public static NSAttributedString ToNSAttributedString(
			this Span span,
			IFontManager fontManager,
			double defaultLineHeight = 0d, // TODO: NET8 should be -1, but too late to change for NET8
			TextAlignment defaultHorizontalAlignment = TextAlignment.Start,
			Font? defaultFont = null,
			Color? defaultColor = null,
			TextTransform defaultTextTransform = TextTransform.Default)
		{
			var defaultFontSize = defaultFont?.Size ?? fontManager.DefaultFontSize;

			var transform = span.TextTransform != TextTransform.Default ? span.TextTransform : defaultTextTransform;

			var text = TextTransformUtilites.GetTransformedText(span.Text, transform);
			if (text is null)
				return new NSAttributedString(string.Empty);

			var style = new NSMutableParagraphStyle();
			var lineHeight = span.LineHeight >= 0
				? span.LineHeight
				: defaultLineHeight;

			if (lineHeight >= 0)
			{
				style.LineHeightMultiple = new nfloat(lineHeight);
			}

			style.Alignment = defaultHorizontalAlignment switch
			{
				TextAlignment.Start => UITextAlignment.Left,
				TextAlignment.Center => UITextAlignment.Center,
				TextAlignment.End => UITextAlignment.Right,
				_ => UITextAlignment.Left
			};

			var font = span.ToFont(defaultFontSize);
			if (font.IsDefault && defaultFont.HasValue)
				font = defaultFont.Value;

			var hasUnderline = false;
			var hasStrikethrough = false;
			if (span.IsSet(Span.TextDecorationsProperty))
			{
				var textDecorations = span.TextDecorations;
				hasUnderline = (textDecorations & TextDecorations.Underline) != 0;
				hasStrikethrough = (textDecorations & TextDecorations.Strikethrough) != 0;
			}

			var platformFont = font.IsDefault ? null : font.ToUIFont(fontManager);

#if !MACOS
			var attrString = new NSAttributedString(
				text,
				platformFont,
				(span.TextColor ?? defaultColor)?.ToPlatform(),
				span.BackgroundColor?.ToPlatform(),
				underlineStyle: hasUnderline ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				strikethroughStyle: hasStrikethrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				paragraphStyle: style,
				kerning: (float)span.CharacterSpacing);
#else
			var attrString = new NSAttributedString(
				text,
				platformFont,
				(span.TextColor ?? defaultColor)?.ToPlatform(),
				span.BackgroundColor?.ToPlatform(),
				underlineStyle: hasUnderline ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				strikethroughStyle: hasStrikethrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				paragraphStyle: style,
				kerningAdjustment: (float)span.CharacterSpacing);
#endif

			return attrString;
		}
	}
}