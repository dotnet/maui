using System;
using System.Diagnostics;
using SkiaSharp;
using Svg.Skia;

namespace Microsoft.Maui.Resizetizer
{
	internal class SkiaSharpSvgTools : SkiaSharpTools, IDisposable
	{
		SKSvg svg;

		public SkiaSharpSvgTools(ResizeImageInfo info, ILogger logger)
			: this(info.Filename, info.BaseSize, info.Color, info.TintColor, info.Quality, logger)
		{
		}

		public SkiaSharpSvgTools(string filename, SKSize? baseSize, SKColor? backgroundColor, SKColor? tintColor, ResizeQuality quality, ILogger logger)
			: base(filename, baseSize, backgroundColor, tintColor, quality, logger)
		{
			var sw = new Stopwatch();
			sw.Start();

			svg = new SKSvg();
			var pic = svg.Load(filename);

			sw.Stop();
			Logger?.Log($"Open SVG took {sw.ElapsedMilliseconds}ms ({filename})");

			if (pic.CullRect.Size.IsEmpty)
				Logger?.Log($"SVG picture did not have a size and will fail to generate. ({Filename})");
		}

		public override SKSize GetOriginalSize() =>
			svg.Picture.CullRect.Size;

		public override void DrawUnscaled(SKCanvas canvas, float scale)
		{
			var size = GetOriginalSize();
			if (size.IsEmpty)
			{
				throw new InvalidOperationException($"Cannot draw SVG file '{Filename}'. The SVG has no size. Ensure the SVG includes a viewBox attribute or both width and height attributes with valid dimensions.");
			}
			if (scale >= 1 && Quality != ResizeQuality.Fastest)
			{
				// Draw vectors directly for Auto/back-compat and Best/highest fidelity.
				canvas.DrawPicture(svg.Picture, Paint);
			}
			else
			{
				// Rasterize first so the final draw honors the selected sampling options.
				var info = new SKImageInfo(
					Math.Max(1, (int)Math.Ceiling(size.Width)),
					Math.Max(1, (int)Math.Ceiling(size.Height)));
				using var surface = SKSurface.Create(info);
				var cvn = surface.Canvas;

				cvn.Clear(SKColors.Transparent);
				cvn.DrawPicture(svg.Picture);

				// convert it all into an image
				using var img = surface.Snapshot();

				// draw to the main canvas using the correct quality settings
				canvas.DrawImage(img, SKRect.Create(size.Width, size.Height), SamplingOptions, Paint);
			}
		}

		public void Dispose()
		{
			svg?.Dispose();
			svg = null;
		}
	}
}
