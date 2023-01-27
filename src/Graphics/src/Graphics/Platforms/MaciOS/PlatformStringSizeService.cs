using System;
using CoreGraphics;
using CoreText;
using Foundation;

namespace Microsoft.Maui.Graphics.Platform
{
	partial class PlatformStringSizeService : IStringSizeService
	{
		public SizeF GetStringSize(string value, IFont font, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
		{
			float factor = 1;
			while (fontSize > 10)
			{
				fontSize /= 10;
				factor *= 10;
			}

			var path = new CGPath();
			path.AddRect(new RectF(0, 0, 512, 512));
			path.CloseSubpath();

			var attributedString = new NSMutableAttributedString(value);

			var attributes = new CTStringAttributes();

			attributes.Font = font?.ToCTFont(fontSize) ?? FontExtensions.GetDefaultCTFont(fontSize);

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

		internal static RectF GetTextSize(CTFramesetter frameSetter, CGPath path)
		{
			var frame = frameSetter.GetFrame(new NSRange(0, 0), path, null);

			if (frame != null)
			{
				var textSize = GetTextSize(frame);
				frame.Dispose();
				return textSize;
			}

			return new RectF(0, 0, 0, 0);
		}

		internal static RectF GetTextSize(CTFrame frame)
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

			return new RectF(0f, minY, width, Math.Max(0, maxY - minY));
		}
	}
}
