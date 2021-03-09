using SkiaSharp;
using Svg.Skia;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Microsoft.Maui.Resizetizer
{
	public class Svg2AndroidDrawableConversionException : Exception
	{
		public Svg2AndroidDrawableConversionException(string message, string file) : base ("Failed to Convert SVG to Android Drawable: " + message + " in [" + file + "]")
		{
			Filename = file;
		}

		public string Filename { get; }
	}
	
	internal class SkiaSharpSvgTools : SkiaSharpTools, IDisposable
	{
		SKSvg svg;

		public SkiaSharpSvgTools(SharedImageInfo info, ILogger logger)
			: this(info.Filename, info.BaseSize, info.TintColor, logger)
		{
		}

		public SkiaSharpSvgTools(string filename, Size? baseSize, Color? tintColor, ILogger logger)
			: base(filename, baseSize, tintColor, logger)
		{
			var sw = new Stopwatch();
			sw.Start();

			svg = new SKSvg();
			svg.Load(filename);

			sw.Stop();
			Logger?.Log($"Open SVG took {sw.ElapsedMilliseconds}ms ({filename})");
		}

		public override SKSize GetOriginalSize() =>
			svg.Picture.CullRect.Size;

		public override void DrawUnscaled(SKCanvas canvas, float scale)
		{
			if (scale >= 1)
			{
				// draw using default scaling
				canvas.DrawPicture(svg.Picture, Paint);
			}
			else
			{
				// draw using raster downscaling
				var size = GetOriginalSize();

				// vector scaling has rounding issues, so first draw as intended
				var info = new SKImageInfo((int)size.Width, (int)size.Height);
				using var bmp = new SKBitmap(info);
				using var cvn = new SKCanvas(bmp);

				// draw to a larger canvas first
				cvn.Clear(SKColors.Transparent);
				cvn.DrawPicture(svg.Picture, Paint);

				// set the paint to be the highest quality it can find
				var paint = new SKPaint
				{
					IsAntialias = true,
					FilterQuality = SKFilterQuality.High
				};

				// draw to the main canvas using the correct quality settings
				canvas.DrawBitmap(bmp, 0, 0, paint);
			}
		}

		public void Dispose()
		{
			svg?.Dispose();
			svg = null;
		}
	}
}
