using System.IO;
using CoreGraphics;
using CoreText;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Graphics.CoreGraphics
{
    public class NativeGraphicsService : IGraphicsService
    {
        public static readonly NativeGraphicsService Instance = new NativeGraphicsService();
        
        private NativeGraphicsService()
        {
            var systemFont = UIFont.SystemFontOfSize(UIFont.SystemFontSize);
            SystemFontName = systemFont.Name;
            systemFont.Dispose();

            var boldSystemFont = UIFont.BoldSystemFontOfSize(UIFont.SystemFontSize);
            BoldSystemFontName = boldSystemFont.Name;
            boldSystemFont.Dispose();
        }

        public IImage LoadImageFromStream(Stream stream, ImageFormat format = ImageFormat.Png)
        {
            var data = NSData.FromStream(stream);
            var image = UIImage.LoadFromData(data);
            return new NativeImage(image);
        }
        
        public string SystemFontName { get; }
        public string BoldSystemFontName { get; }

        public SizeF GetStringSize(string value, string fontName, float fontSize)
        {
            if (string.IsNullOrEmpty(value)) return new SizeF();

            var finalFontName = fontName ?? SystemFontName;
            var nsString = new NSString(value);
            UIFont uiFont;  
            if (finalFontName == SystemFontName)
                uiFont = UIFont.SystemFontOfSize(fontSize);
            else if (finalFontName == BoldSystemFontName)
                uiFont = UIFont.BoldSystemFontOfSize(fontSize);
            else
                uiFont = UIFont.FromName(finalFontName, fontSize);

            var size = nsString.StringSize(uiFont);
            uiFont.Dispose();
            return new SizeF((float) size.Width, (float) size.Height);
        }

        public SizeF GetStringSize(
            string value,
            string fontName,
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
            path.AddRect(new Drawing.RectangleF(0, 0, 512, 512));
            path.CloseSubpath();

            var attributedString = new NSMutableAttributedString(value);

            var attributes = new CTStringAttributes();

            // Load the font
            var font = NativeFontService.Instance.LoadFont(fontName ?? SystemFontName, fontSize);
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

        private static RectangleF GetTextSize(
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

        public BitmapExportContext CreateBitmapExportContext(int width, int height, float displayScale = 1)
        {
            return new NativeBitmapExportContext(width, height, displayScale);
        }
    }
}