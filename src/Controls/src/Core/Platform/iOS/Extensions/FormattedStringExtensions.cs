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
		{
			// Resolve effective alignment accounting for FlowDirection (RTL flips Start↔End)
			var alignment = label.HorizontalTextAlignment;
			var effectiveFlowDirection = ((IVisualElementController)label).EffectiveFlowDirection;
			if (effectiveFlowDirection.IsRightToLeft())
			{
				alignment = alignment switch
				{
					TextAlignment.Start => TextAlignment.End,
					TextAlignment.End => TextAlignment.Start,
					_ => alignment
				};
			}

			return ToNSAttributedString(
				label.FormattedText,
				label.RequireFontManager(),
				label.LineHeight,
				alignment,
				label.ToFont(),
				label.TextColor,
				label.TextTransform,
				label.LineBreakMode,
				label.CharacterSpacing);
		}

		public static NSAttributedString ToNSAttributedString(
			this FormattedString formattedString,
			IFontManager fontManager,
			double defaultLineHeight = -1,
			TextAlignment defaultHorizontalAlignment = TextAlignment.Start,
			Font? defaultFont = null,
			Color? defaultColor = null,
			TextTransform defaultTextTransform = TextTransform.Default)
			=> formattedString.ToNSAttributedString(fontManager, defaultLineHeight, defaultHorizontalAlignment, defaultFont, defaultColor, defaultTextTransform, LineBreakMode.WordWrap, defaultCharacterSpacing: 0d);

		internal static NSAttributedString ToNSAttributedString(
			this FormattedString formattedString,
			IFontManager fontManager,
			double defaultLineHeight,
			TextAlignment defaultHorizontalAlignment,
			Font? defaultFont,
			Color? defaultColor,
			TextTransform defaultTextTransform,
			LineBreakMode lineBreakMode,
			double defaultCharacterSpacing = 0d)
		{
			if (formattedString == null)
			{
				return new NSAttributedString(string.Empty);
			}

			var attributed = new NSMutableAttributedString();
			for (int i = 0; i < formattedString.Spans.Count; i++)
			{
				Span span = formattedString.Spans[i];
				if (span.Text == null)
				{
					continue;
				}

				attributed.Append(span.ToNSAttributedString(fontManager, defaultLineHeight, defaultHorizontalAlignment,
					defaultFont, defaultColor, defaultTextTransform, lineBreakMode, defaultCharacterSpacing));
			}

			return attributed;
		}

		public static NSAttributedString ToNSAttributedString(
			this Span span,
			IFontManager fontManager,
			double defaultLineHeight = -1,
			TextAlignment defaultHorizontalAlignment = TextAlignment.Start,
			Font? defaultFont = null,
			Color? defaultColor = null,
			TextTransform defaultTextTransform = TextTransform.Default)
			=> span.ToNSAttributedString(fontManager, defaultLineHeight, defaultHorizontalAlignment, defaultFont, defaultColor, defaultTextTransform, LineBreakMode.WordWrap, defaultCharacterSpacing: 0d);

		internal static NSAttributedString ToNSAttributedString(
			this Span span,
			IFontManager fontManager,
			double defaultLineHeight,
			TextAlignment defaultHorizontalAlignment,
			Font? defaultFont,
			Color? defaultColor,
			TextTransform defaultTextTransform,
			LineBreakMode lineBreakMode,
			double defaultCharacterSpacing = 0d)
		{
			var defaultFontSize = defaultFont?.Size ?? fontManager.DefaultFontSize;

			var transform = span.TextTransform != TextTransform.Default ? span.TextTransform : defaultTextTransform;

			var text = TextTransformUtilities.GetTransformedText(span.Text, transform);
			if (text is null)
			{
				return new NSAttributedString(string.Empty);
			}

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

			style.LineBreakMode = lineBreakMode switch
			{
				LineBreakMode.NoWrap => UILineBreakMode.Clip,
				LineBreakMode.WordWrap => UILineBreakMode.WordWrap,
				LineBreakMode.CharacterWrap => UILineBreakMode.CharacterWrap,
				LineBreakMode.HeadTruncation => UILineBreakMode.HeadTruncation,
				LineBreakMode.TailTruncation => UILineBreakMode.TailTruncation,
				LineBreakMode.MiddleTruncation => UILineBreakMode.MiddleTruncation,
				_ => UILineBreakMode.WordWrap
			};

			var font = span.GetEffectiveFont(defaultFontSize, defaultFont);
			var hasUnderline = false;
			var hasStrikethrough = false;
			if (span.IsSet(Span.TextDecorationsProperty))
			{
				var textDecorations = span.TextDecorations;
				hasUnderline = (textDecorations & TextDecorations.Underline) != 0;
				hasStrikethrough = (textDecorations & TextDecorations.Strikethrough) != 0;
			}

			var platformFont = font.IsDefault ? null : font.ToUIFont(fontManager);

			// CharacterSpacing with validation
			var characterSpacing = span.IsSet(Span.CharacterSpacingProperty) 
				? span.CharacterSpacing 
				: defaultCharacterSpacing;
			characterSpacing = Math.Max(0, characterSpacing);

#if !MACOS
			var attrString = new NSAttributedString(
				text,
				platformFont,
				(span.TextColor ?? defaultColor)?.ToPlatform(),
				span.BackgroundColor?.ToPlatform(),
				underlineStyle: hasUnderline ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				strikethroughStyle: hasStrikethrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				paragraphStyle: style,
				kerning: (float)characterSpacing);
#else
			var attrString = new NSAttributedString(
				text,
				platformFont,
				(span.TextColor ?? defaultColor)?.ToPlatform(),
				span.BackgroundColor?.ToPlatform(),
				underlineStyle: hasUnderline ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				strikethroughStyle: hasStrikethrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
				paragraphStyle: style,
				kerningAdjustment: (float)characterSpacing);
#endif

			return attrString;
		}

		internal static void RecalculateSpanPositions(this UILabel control, Label element, Size size)
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

			var finalSize = size;

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
			// On iOS 16+, NSLayoutManager's default UsesFontLeading=true causes it to include
			// font leading (extra line spacing) from the OS/2 typographic metrics that CoreText
			// uses when a font has an OpenType STAT table. This makes the layout manager compute
			// line heights that don't match what CoreText uses to draw the glyphs, resulting in
			// span tap hitboxes being vertically offset from the rendered text.
			// Disabling UsesFontLeading on iOS 16+ makes NSLayoutManager match CoreText's metrics
			// so the calculated span rects align with the actual rendered text positions.
			// See: https://github.com/dotnet/maui/issues/36505
			using var layoutManager = new NSLayoutManager
			{
				UsesFontLeading = !OperatingSystem.IsIOSVersionAtLeast(16)
			};
			using var textContainer = new NSTextContainer { LineFragmentPadding = 0 };

			textStorage.AddLayoutManager(layoutManager);
			layoutManager.AddTextContainer(textContainer);

			// Always prefer finalSize from MAUI's layout system — it is the authoritative
			// size for this arrange pass. On Mac Catalyst (and iOS 26+ with NavigationPage),
			// control.Bounds may be stale or {0,0,0,0} during ArrangeOverride because UIKit
			// frame updates can lag behind MAUI's layout.
			var containerWidth = (nfloat)finalSize.Width > 0 ? (nfloat)finalSize.Width : control.Bounds.Width;
			var containerHeight = (nfloat)finalSize.Height > 0 ? (nfloat)finalSize.Height : control.Bounds.Height;
			textContainer.Size = new(containerWidth, control.Lines == 0 ? nfloat.MaxValue : containerHeight);

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