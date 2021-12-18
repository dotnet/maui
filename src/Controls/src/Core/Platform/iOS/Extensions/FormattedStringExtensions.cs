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
		public static NSAttributedString ToAttributed(this FormattedString formattedString, Label label)
			=> formattedString.ToAttributed(
				label.GetFontManager(),
				label?.LineHeight ?? 0,
				label?.HorizontalTextAlignment ?? TextAlignment.Start,
				label?.ToFont(),
				label?.TextColor,
				label?.TextTransform ?? TextTransform.Default);


		public static NSAttributedString ToAttributed(this FormattedString formattedString, IFontManager fontManager, double defaultLineHeight = 0d, TextAlignment defaultHorizontalAlignment = TextAlignment.Start, Font? defaultFont = null, Color? defaultColor = null, TextTransform defaultTextTransform = TextTransform.Default)
		{
			if (formattedString == null)
				return new NSAttributedString(string.Empty);

			var attributed = new NSMutableAttributedString();
			for (int i = 0; i < formattedString.Spans.Count; i++)
			{
				Span span = formattedString.Spans[i];
				if (span.Text == null)
					continue;

				attributed.Append(span.ToAttributed(fontManager, defaultLineHeight, defaultHorizontalAlignment, defaultFont, defaultColor, defaultTextTransform));
			}

			return attributed;
		}

		public static NSAttributedString ToAttributed(this Span span, Label label)
			=> span.ToAttributed(
				label.GetFontManager(),
				label?.LineHeight ?? 0,
				label?.HorizontalTextAlignment ?? TextAlignment.Start,
				label?.ToFont(),
				label?.TextColor,
				label?.TextTransform ?? TextTransform.None);

		public static NSAttributedString ToAttributed(this Span span, IFontManager fontManager, double defaultLineHeight = 0d, TextAlignment defaultHorizontalAlignment = TextAlignment.Start, Font? defaultFont = null, Color? defaultColor = null, TextTransform defaultTextTransform = TextTransform.Default)
		{
			var transform = span.TextTransform != TextTransform.Default	? span.TextTransform : defaultTextTransform;

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
			
			var font = span.ToFont();
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
				(span.TextColor ?? defaultColor)?.ToNative(),
				span.BackgroundColor?.ToNative(),
				underlineStyle: hasUnderline ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				strikethroughStyle: hasStrikethrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				paragraphStyle: style,
				kerning: (float)span.CharacterSpacing);
#else
			var attrString = new NSAttributedString(
				text,
				platformFont,
				(span.TextColor ?? defaultColor)?.ToNative(),
				span.BackgroundColor?.ToNative(),
				underlineStyle: hasUnderline ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				strikethroughStyle: hasStrikethrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				paragraphStyle: style,
				kerningAdjustment: (float)span.CharacterSpacing);
#endif

			return attrString;
		}

	}
}