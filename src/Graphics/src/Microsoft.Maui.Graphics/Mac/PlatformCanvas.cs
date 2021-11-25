using AppKit;
using CoreGraphics;
using CoreText;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Graphics.Platform
{
	partial class PlatformCanvas
	{
		public override SizeF GetStringSize(string value, IFont font, float fontSize)
		{
			var nativeString = new NSString(value);
			var attributes = new NSMutableDictionary();
			attributes[NSStringAttributeKey.Font] = font?.ToPlatformFont(fontSize) ?? FontExtensions.GetDefaultPlatformFont(fontSize);
			var size = nativeString.StringSize(attributes);
			return size.AsSizeF();
		}

		public override SizeF GetStringSize(string value, IFont font, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
		{
			var actualFontSize = fontSize;
			float factor = 1;
			while (actualFontSize > 10)
			{
				actualFontSize /= 10;
				factor *= 10;
			}

			var path = new CGPath();
			path.AddRect(new CGRect(0, 0, 512, 512));
			path.CloseSubpath();

			var attributedString = new NSMutableAttributedString(value);

			var attributes = new CTStringAttributes();

			// Load the font
			attributes.Font = font?.ToCTFont(actualFontSize) ?? FontExtensions.GetDefaultCTFont(actualFontSize);

			// Set the horizontal alignment
			var paragraphSettings = new CTParagraphStyleSettings();
			switch (horizontalAlignment)
			{
				case HorizontalAlignment.Left:
					paragraphSettings.Alignment = CTTextAlignment.Left;
					break;
				case HorizontalAlignment.Center:
					paragraphSettings.Alignment = CTTextAlignment.Center;
					break;
				case HorizontalAlignment.Right:
					paragraphSettings.Alignment = CTTextAlignment.Right;
					break;
				case HorizontalAlignment.Justified:
					paragraphSettings.Alignment = CTTextAlignment.Justified;
					break;
			}

			var paragraphStyle = new CTParagraphStyle(paragraphSettings);
			attributes.ParagraphStyle = paragraphStyle;

			// Set the attributes for the complete length of the string
			attributedString.SetAttributes(attributes, new NSRange(0, value.Length));

			// Create the framesetter with the attributed string.
			var frameSetter = new CTFramesetter(attributedString);

			var textBounds = GetTextSize(frameSetter, path);

			frameSetter.Dispose();
			attributedString.Dispose();
			paragraphStyle.Dispose();
			path.Dispose();

			textBounds.Width *= factor;
			textBounds.Height *= factor;

			return textBounds.Size;
		}

		static RectangleF GetTextSize(CTFramesetter frameSetter, CGPath path)
		{
			var frame = frameSetter.GetFrame(new NSRange(0, 0), path, null);

			if (frame != null)
			{
				var textSize = GetTextSize(frame);
				frame.Dispose();
				return textSize;
			}

			return new RectangleF(0, 0, 0, 0);
		}

		static RectangleF GetTextSize(CTFrame frame)
		{
			var minY = float.MaxValue;
			var maxY = float.MinValue;
			float width = 0;

			var lines = frame.GetLines();
			var origins = new CGPoint[lines.Length];
			frame.GetLineOrigins(new NSRange(0, 0), origins);

			for (var i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				var lineWidth = (float)line.GetTypographicBounds(out var ascent, out var descent, out var leading);

				if (lineWidth > width)
					width = lineWidth;

				var origin = origins[i];

				minY = (float)Math.Min(minY, origin.Y - ascent);
				maxY = (float)Math.Max(maxY, origin.Y + descent);

				lines[i].Dispose();
			}

			return new RectangleF(0f, minY, width, Math.Max(0, maxY - minY));
		}
	}
}
