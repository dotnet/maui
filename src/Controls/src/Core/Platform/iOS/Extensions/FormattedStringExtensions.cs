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

				attributed.Append(span.ToNSAttributedString(fontManager, defaultLineHeight, defaultHorizontalAlignment,
					defaultFont, defaultColor, defaultTextTransform));
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
			{
				return;
			}

			if (element.TextType == TextType.Html)
			{
				return;
			}

			if (element?.FormattedText?.Spans is null
				|| element.FormattedText.Spans.Count == 0)
			{
				return;
			}

			var finalSize = control.Bounds;

			if (finalSize.Width <= 0 || finalSize.Height <= 0)
			{
				return;
			}

			var inline = control.AttributedText;

			if (inline is null)
			{
				return;
			}

			var attributedText = control.AttributedText;
			if (attributedText is null || attributedText.Length == 0)
			{
				return;
			}

			var spans = element.FormattedText.Spans;

			nint NSMaxRange(NSRange range) => range.Location + range.Length;

			using var textStorage = new NSTextStorage();
			using var layoutManager = new NSLayoutManager();
			using var textContainer = new NSTextContainer { LineFragmentPadding = 0 };

			textStorage.AddLayoutManager(layoutManager);
			layoutManager.AddTextContainer(textContainer);

			textContainer.Size = new(control.Bounds.Width,
				control.Lines == 0 ? nfloat.MaxValue : control.Bounds.Height);

			textStorage.SetString(attributedText);
			layoutManager.EnsureLayoutForTextContainer(textContainer);

			int currentLocation = 0;

			foreach (var span in spans)
			{
				if (string.IsNullOrEmpty(span?.Text))
				{
					continue;
				}

				var spanRects = new List<CGRect>();
				var spanStartIndex = currentLocation;
				var spanEndIndex = currentLocation + span.Text.Length - 1;

				var startGlyphRange = layoutManager.GetGlyphRange(new NSRange(spanStartIndex, 1));
				var endGlyphRange = layoutManager.GetGlyphRange(new NSRange(spanEndIndex, 1));

				void EnumerateLineFragmentCallback(CGRect rect, CGRect usedRect, NSTextContainer container,
					NSRange lineGlyphRange, out bool stop)
				{
					//Whole span is within the line and bigger than the line
					if (lineGlyphRange.Location >= startGlyphRange.Location &&
						NSMaxRange(lineGlyphRange) <= endGlyphRange.Location)
					{
						spanRects.Add(usedRect);
					}
					// Whole span is within the line and smaller than the line
					else if (lineGlyphRange.Location <= startGlyphRange.Location &&
							 endGlyphRange.Location <= NSMaxRange(lineGlyphRange))
					{
						var spanBoundingRect = layoutManager.GetBoundingRect(
							new(startGlyphRange.Location, endGlyphRange.Location - startGlyphRange.Location + 1),
							textContainer);
						spanRects.Add(spanBoundingRect);
					}
					// Span starts on current line and ends on next lines
					else if (lineGlyphRange.Location <= startGlyphRange.Location &&
							 NSMaxRange(lineGlyphRange) <= endGlyphRange.Location)
					{
						var spanBoundingRect = layoutManager.GetBoundingRect(
							new(startGlyphRange.Location, NSMaxRange(lineGlyphRange) - startGlyphRange.Location),
							textContainer);
						spanRects.Add(spanBoundingRect);
					}
					// Span starts on previous lines and ends on current line
					else if (lineGlyphRange.Location >= startGlyphRange.Location &&
							 NSMaxRange(lineGlyphRange) >= endGlyphRange.Location)
					{
						var spanBoundingRect = layoutManager.GetBoundingRect(
							new(lineGlyphRange.Location, endGlyphRange.Location - lineGlyphRange.Location),
							textContainer);
						spanRects.Add(spanBoundingRect);
					}

					stop = false;
				}

				layoutManager.EnumerateLineFragments(
					new(startGlyphRange.Location, endGlyphRange.Location - startGlyphRange.Location + 1),
					EnumerateLineFragmentCallback);

				if (span is ISpatialElement spatialElement)
				{
					var rects = new List<Rect>();
					foreach (var r in spanRects)
					{
						rects.Add(new Rect(r.X, r.Y, r.Width, r.Height));
					}
					spatialElement.Region = Region.FromRectangles(rects);
				}
				currentLocation += span.Text.Length;
			}
		}
	}
}