using CoreGraphics;
using System.Collections.Generic;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using System;

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

		internal static void RecalculateSpanPositions(this UILabel control, Label element)
		{
			if (element is null)
				return;

			if (element.TextType == TextType.Html)
				return;

			if (element?.FormattedText?.Spans is null
				|| element.FormattedText.Spans.Count == 0)
				return;

			var finalSize = control.Frame;

			if (finalSize.Width <= 0 || finalSize.Height <= 0)
				return;

			var inline = control.AttributedText;

			if (inline is null)
				return;

			NSTextStorage textStorage = new NSTextStorage();
			textStorage.SetString(inline);

			var layoutManager = new NSLayoutManager();
			textStorage.AddLayoutManager(layoutManager);

			var textContainer = new NSTextContainer(size: finalSize.Size)
			{
				LineFragmentPadding = 0
			};

			layoutManager.AddTextContainer(textContainer);

			var currentLocation = 0;

			for (int i = 0; i < element.FormattedText.Spans.Count; i++)
			{
				var span = element.FormattedText.Spans[i];

				var location = currentLocation;
				var length = span.Text?.Length ?? 0;

				if (length == 0 || span?.Text is null)
					continue;

				var startRect = GetCharacterBounds(new NSRange(location, 1), layoutManager, textContainer);
				var endRect = GetCharacterBounds(new NSRange(location + length, 1), layoutManager, textContainer);

				var defaultLineHeight = control.FindDefaultLineHeight(location, length);

				var yaxis = startRect.Top;
				var lineHeights = new List<double>();

				while ((endRect.Bottom - yaxis) > 0.001)
				{
					double lineHeight;
					if (yaxis == startRect.Top) // First Line
					{
						lineHeight = startRect.Bottom - startRect.Top;
					}
					else if (yaxis != endRect.Top) // Middle Line(s)
					{
						lineHeight = defaultLineHeight;
					}
					else // Bottom Line
					{
						lineHeight = endRect.Bottom - endRect.Top;
					}
					lineHeights.Add(lineHeight);
					yaxis += (float)lineHeight;
				}

				// if the span is multiline, we need to calculate the bounds for each line individually
				if (lineHeights.Count > 1)
				{
					var spanRectangles = GetMultilinedBounds(new NSRange(location, length), layoutManager, textContainer, startRect, endRect, lineHeights, span.Text.EndsWith('\n') || span.Text.EndsWith("\r\n"));
					((ISpatialElement)span).Region = Region.FromRectangles(spanRectangles).Inflate(5);
				}
				else
				{
					((ISpatialElement)span).Region = Region.FromLines(lineHeights.ToArray(), finalSize.Width, startRect.X, endRect.X, startRect.Top).Inflate(5);
				}

				// Update current location
				currentLocation += length;
			}
		}

		static CGRect GetCharacterBounds(NSRange characterRange, NSLayoutManager layoutManager, NSTextContainer textContainer)
		{
			layoutManager.GetCharacterRange(characterRange, out NSRange glyphRange);

			return layoutManager.GetBoundingRect(glyphRange, textContainer);
		}

		static Rect[] GetMultilinedBounds(NSRange characterRange, NSLayoutManager layoutManager, NSTextContainer textContainer, CGRect startRect, CGRect endRect, List<double> lineHeights, bool endsWithNewLine)
		{
			var glyphRange = layoutManager.GetCharacterRange(characterRange);
			var multilineRects = new List<CGRect>();

			layoutManager.EnumerateLineFragments(glyphRange, (CGRect rect, CGRect usedRect, NSTextContainer textContainer, NSRange lineGlyphRange, out bool stop) =>
			{
				multilineRects.Add(usedRect);
				stop = false;
			});

			return CreateSpanRects(startRect, endRect, lineHeights, multilineRects, endsWithNewLine);
		}

		static Rect[] CreateSpanRects(CGRect startRect, CGRect endRect, List<double> lineHeights, List<CGRect> multilineRects, bool endsWithNewLine)
		{
			List<Rect> spanRectangles = new List<Rect>();
			var curHeight = (double)startRect.Top;

			// go through each line and create a Rect for the text contained
			for (int i = 0; i < multilineRects.Count; i++)
			{
				var rect = multilineRects[i];

				// top line
				// The rect.Width measures from the start of the line even if the span does not start
				// at the beginning of the line so we will take the difference for the width.
				if (i == 0)
				{
					spanRectangles.Add(new Rect(startRect.X, startRect.Top, rect.Width - startRect.Left, lineHeights[i]));
				}
				// middle lines
				else if (i < multilineRects.Count - 1)
				{
					spanRectangles.Add(new Rect(0, curHeight, rect.Width, lineHeights[i]));
				}
				// bottom line
				// rect.Width is the width of the entire last line that is not a new line character - including if there is more text after the span we are processing.
				// endRect.X will consider a new line character at the end of a span as a new line and will not give useful information about the end X position in that case.
				// As such, we select which to use as the width based on if the span ends with a new line character.
				else
				{
					spanRectangles.Add(new Rect(0, curHeight, endsWithNewLine ? rect.Width : endRect.X, lineHeights[i]));
				}

				curHeight += lineHeights[i];
			}

			return spanRectangles.ToArray();
		}

		static double FindDefaultLineHeight(this UILabel control, int start, int length)
		{
			if (length == 0)
				return 0.0;

			var textStorage = new NSTextStorage();

			if (control.AttributedText is not null)
				textStorage.SetString(control.AttributedText.Substring(start, length));

			var layoutManager = new NSLayoutManager();
			textStorage.AddLayoutManager(layoutManager);

			var textContainer = new NSTextContainer(size: new SizeF(float.MaxValue, float.MaxValue))
			{
				LineFragmentPadding = 0
			};
			layoutManager.AddTextContainer(textContainer);

			var rect = GetCharacterBounds(new NSRange(0, 1), layoutManager, textContainer);
			return rect.Bottom - rect.Top;
		}
	}
}