using CoreGraphics;
using CoreText;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	partial class PlatformCanvas
	{
		public override SizeF GetStringSize(string value, IFont font, float fontSize)
		{
			if (string.IsNullOrEmpty(value)) return new SizeF();

			var nsString = new NSString(value);
			var uiFont = font?.ToPlatformFont(fontSize) ?? FontExtensions.GetDefaultPlatformFont();

			var attributes = new NSMutableDictionary
			{
				{ new NSString("NSFontAttributeName"), uiFont }
			};

			CGSize size;
			if (!UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
			{
				size = nsString.GetBoundingRect(
					CGSize.Empty,
					NSStringDrawingOptions.UsesLineFragmentOrigin,
					new UIStringAttributes { Font = uiFont },
					null).Size;
			}
			else
			{
				size = nsString.StringSize(uiFont, CGSize.Empty);
			}

			uiFont.Dispose();
			return new SizeF((float)size.Width, (float)size.Height);
		}

		public override SizeF GetStringSize(
			string value,
			IFont font,
			float fontSize,
			HorizontalAlignment horizontalAlignment,
			VerticalAlignment verticalAlignment)
		{
			float factor = 1;
			while (fontSize > 10)
			{
				fontSize /= 10;
				factor *= 10;
			}

			var path = new CGPath();
			path.AddRect(new RectangleF(0, 0, 512, 512));
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

			// Create the frame setter with the attributed string.
			var frameSetter = new CTFramesetter(attributedString);

			var textBounds = GetTextSize(frameSetter, path);
			//Logger.Debug("{0} {1}",vSize,aString);

			frameSetter.Dispose();
			attributedString.Dispose();
			paragraphStyle.Dispose();
			//font.Dispose();
			path.Dispose();

			textBounds.Width *= factor;
			textBounds.Height *= factor;

			//size.Width = Math.Ceiling(vSize.Width);
			//size.Height = Math.Ceiling(vSize.Height);

			return textBounds.Size;
		}

		static RectangleF GetTextSize(
			CTFramesetter frameSetter,
			CGPath path)
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
