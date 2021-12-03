#nullable enable
using Foundation;
using Microsoft.Maui.Controls.Internals;
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
		{
			if (formattedString == null)
				return new NSAttributedString(string.Empty);

			var attributed = new NSMutableAttributedString();
			for (int i = 0; i < formattedString.Spans.Count; i++)
			{
				Span span = formattedString.Spans[i];
				if (span.Text == null)
					continue;

				attributed.Append(span.ToAttributed(label));
			}

			return attributed;
		}

		internal static NSAttributedString ToAttributed(this Span span, Label label)
		{
			var text = TextTransformUtilites.GetTransformedText(span.Text, span.TextTransform);
			if (text is null)
				return new NSAttributedString(string.Empty);

			var style = new NSMutableParagraphStyle();
			var lineHeight = span.LineHeight >= 0 
				? span.LineHeight 
				: label?.LineHeight ?? -1;

			if (lineHeight >= 0)
			{
				style.LineHeightMultiple = new nfloat(lineHeight);
			}

			if (label is not null)
			{
				style.Alignment = label.HorizontalTextAlignment switch
				{
					TextAlignment.Start => UITextAlignment.Left,
					TextAlignment.Center => UITextAlignment.Center,
					TextAlignment.End => UITextAlignment.Right,
					_ => UITextAlignment.Left
				};
			}

			var font = span.ToFont();
			if (font.IsDefault && label is not null)
				font = label.ToFont();

			var hasUnderline = false;
			var hasStrikethrough = false;
			if (span.IsSet(Span.TextDecorationsProperty))
			{
				var textDecorations = span.TextDecorations;
				hasUnderline = (textDecorations & TextDecorations.Underline) != 0;
				hasStrikethrough = (textDecorations & TextDecorations.Strikethrough) != 0;
			}
#if !MACOS
			var attrString = new NSAttributedString(
				text,
				font.IsDefault ? null : font.ToUIFont(),
				(span.TextColor ?? label?.TextColor)?.ToNative(),
				span.BackgroundColor?.ToNative(),
				underlineStyle: hasUnderline ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				strikethroughStyle: hasStrikethrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				paragraphStyle: style,
				kerning: (float)span.CharacterSpacing);
#else
			var attrString = new NSAttributedString(
				text,
				font.IsDefault ? null : font.ToNSFont(),
				(span.TextColor ?? label?.TextColor)?.ToNative(),
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