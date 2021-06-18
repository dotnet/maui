using System;
using System.IO;
using AppKit;
using CoreGraphics;
using CoreText;
using Foundation;

namespace Microsoft.Maui.Graphics.Native
{
	public class NativeGraphicsService : IGraphicsService
	{
		public static NativeGraphicsService Instance = new NativeGraphicsService();

		private readonly string _boldSystemFontName;
		private readonly string _systemFontName;

		private NativeGraphicsService()
		{
			var systemFont = NSFont.SystemFontOfSize(NSFont.SystemFontSize);
			_systemFontName = systemFont.FontName;
			systemFont.Dispose();

			var boldSystemFont = NSFont.BoldSystemFontOfSize(NSFont.SystemFontSize);
			_boldSystemFontName = boldSystemFont.FontName;
			boldSystemFont.Dispose();
		}

		public IImage LoadImageFromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			if (stream == null)
				return null;

			var previous = NSApplication.CheckForIllegalCrossThreadCalls;
			NSApplication.CheckForIllegalCrossThreadCalls = false;
			var data = NSData.FromStream(stream);
			var image = new NSImage(data);
			NSApplication.CheckForIllegalCrossThreadCalls = previous;
			return new NativeImage(image);
		}

		public string SystemFontName => _systemFontName;
		public string BoldSystemFontName => _boldSystemFontName;

		public SizeF GetStringSize(string value, string fontName, float fontSize)
		{
			var actualFontName = fontName ?? _systemFontName;
			var nativeString = new NSString(value);
			var font = NSFont.FromFontName(actualFontName, (float) fontSize);
			if (font == null)
				font = NSFont.FromFontName(_systemFontName, (float) fontSize);
			var attributes = new NSMutableDictionary();
			attributes[NSStringAttributeKey.Font] = font;
			var size = nativeString.StringSize(attributes);
			font.Dispose();
			return size.AsSizeF();
		}

		public SizeF GetStringSize(string value, string fontName, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
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
			var font = NativeFontService.Instance.LoadFont(fontName ?? _systemFontName, (float) actualFontSize);
			attributes.Font = font;

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

		public static RectangleF GetTextSize(CTFramesetter frameSetter, CGPath path)
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

		public static RectangleF GetTextSize(CTFrame frame)
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
				var lineWidth = (float) line.GetTypographicBounds(out var ascent, out var descent, out var leading);

				if (lineWidth > width)
					width = lineWidth;

				var origin = origins[i];

				minY = (float) Math.Min(minY, origin.Y - ascent);
				maxY = (float) Math.Max(maxY, origin.Y + descent);

				lines[i].Dispose();
			}

			return new RectangleF(0f, minY, width, Math.Max(0, maxY - minY));
		}

		public RectangleF GetPathBounds(PathF path)
		{
			var nativePath = path.NativePath as CGPath;

			if (nativePath == null)
			{
				nativePath = path.AsCGPath();
				path.NativePath = nativePath;
			}

			var bounds = nativePath.PathBoundingBox;
			return bounds.AsRectangleF();
		}

		public BitmapExportContext CreateBitmapExportContext(int width, int height, float displayScale = 1)
		{
			return new NativeBitmapExportContext(width, height, displayScale);
		}
	}
}
