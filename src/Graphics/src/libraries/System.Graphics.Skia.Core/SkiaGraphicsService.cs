using System.IO;
using SkiaSharp;

namespace System.Graphics.Skia
{
    public class SkiaGraphicsService : IGraphicsService
    {
        public static readonly SkiaGraphicsService Instance = new SkiaGraphicsService();

        private static SKPaint _strokePaint;
        private static SKPaint _fillPaint;
        private static SKBitmap _b;

        private SkiaGraphicsService()
        {
            if (_strokePaint == null)
            {
                _strokePaint = new SKPaint
                {
                    Color = SKColors.White,
                    IsStroke = true
                };

                _fillPaint = new SKPaint
                {
                    Color = SKColors.White,
                    IsStroke = false
                };

                _b = new SKBitmap(1, 1, SKColorType.Rgba8888, SKAlphaType.Premul);
            }
        }

        public string SystemFontName => "Arial";
        public string BoldSystemFontName => "Arial-Bold";

        public IImage LoadImageFromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
        {
            using (var s = new SKManagedStream(stream))
            {
                using (var codec = SKCodec.Create(s))
                {
                    var info = codec.Info;
                    var bitmap = new SKBitmap(info.Width, info.Height, info.ColorType, info.IsOpaque ? SKAlphaType.Opaque : SKAlphaType.Premul);

                    var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels(out _));
                    if (result == SKCodecResult.Success || result == SKCodecResult.IncompleteInput)
                    {
                        return new SkiaImage(bitmap);
                    }
                }
            }

            return null;
        }

        public SizeF GetStringSize(string value, string fontName, float fontSize)
        {
            if (string.IsNullOrEmpty(value))
                return new SizeF();

            var paint = new SKPaint
            {
                Typeface = SkiaDelegatingFontService.Instance.GetTypeface(fontName),
                TextSize = fontSize
            };
            var width = paint.MeasureText(value);
            paint.Dispose();
            return new SizeF(width, fontSize);
        }

        public SizeF GetStringSize(string value, string fontName, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            if (string.IsNullOrEmpty(value))
                return new SizeF();

            var paint = new SKPaint
            {
                TextSize = fontSize,
                Typeface = SkiaDelegatingFontService.Instance.GetTypeface(fontName)
            };
            var width = paint.MeasureText(value);
            paint.Dispose();
            return new SizeF(width, fontSize);
        }

        public BitmapExportContext CreateBitmapExportContext(int width, int height, float displayScale = 1)
        {
            return new SkiaBitmapExportContext(width, height, displayScale, 72, false);
        }
    }
}